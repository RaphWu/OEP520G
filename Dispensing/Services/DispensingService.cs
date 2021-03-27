using OEP520G.Camera.Contracts;
using OEP520G.Core.Contracts;
using OEP520G.Core.Helpers;
using OEP520G.Core.Models;
using OEP520G.Core.SystemEvent;
using OEP520G.Database.Contracts;
using OEP520G.Dispensing.Constants;
using OEP520G.Dispensing.Contracts;
using OEP520G.Dispensing.Models;
using OEP520G.EPCIO6000.Constants;
using OEP520G.EPCIO6000.Contracts;
using OEP520G.ProductManager.Contracts;
using OEP520G.Stage.Contracts;
using Prism.Events;
using System;
using System.Collections.Generic;

namespace OEP520G.Dispensing.Services
{
    public partial class DispensingService : IDispensing
    {
        private readonly IEventAggregator _ea;
        private readonly IStatusBar _statusBar;
        private readonly ISqliteService _sqlite;
        private readonly IProductManager _pm;
        private readonly IServo _servo;
        private readonly IIo _io;
        private readonly ICamera _camera;
        private readonly IStage _stage;

        // ctor
        public DispensingService(IEventAggregator ea,
                                 IStatusBar statusBar,
                                 ISqliteService sqlite,
                                 IProductManager pm,
                                 IServo servo,
                                 IIo io,
                                 ICamera camera,
                                 IStage stage)
        {
            _ea = ea;
            _statusBar = statusBar;
            _sqlite = sqlite;
            _pm = pm;
            _servo = servo;
            _io = io;
            _camera = camera;
            _stage = stage;

            if (DispensingParameters.Dispenser == null)
            {
                ReadFromDb();
            }

            // 訂閱品種切換事件
            _ea.GetEvent<OnProductChangeover>().Subscribe(ProductChangeover);
        }

        public DispenserDefine Dispenser => DispensingParameters.Dispenser;
        public List<DispensingShapeDefine> Shape => DispensingParameters.Shape;
        public List<DispensingSequenceDefine> Sequence => DispensingParameters.Sequence;
        public List<DispensingGroupDefine> Group => DispensingParameters.Group;

        /********************
         * 品種切換事件
         ********************/
        /// <inheritdoc/>
        public void ProductChangeover(string productName)
        {
            ReadFromDb();
        }

        /********************
         * Database
         ********************/
        /// <inheritdoc/>
        public bool WriteToDb()
        {
            if (!_pm.IsProductActive)
                return false;

            using var conn = _sqlite.OpenConnection(_pm.DB_NAME_PRODUCT);
            if (conn == null)
                return false;

            string msg = "Dispensing parameters are being written...";
            _statusBar.SystemMessage(msg);

            try
            {
                using var tran = conn.BeginTransaction();
                WriteDispenserToDb(conn);
                WriteShapeToDb(conn);
                WriteSequenceToDb(conn);
                WriteGroupToDb(conn);
                tran.Commit();
            }
            catch (Exception e)
            {
                msg = $"Failed to write dispensing data.\nMessage:{e.Message}";
                _statusBar.SystemMessage(msg);
                return false;
            }

            msg = "The dispensing parameters are written successfully.";
            _statusBar.SystemMessage(msg);
            return true;
        }

        /// <inheritdoc/>
        public bool ReadFromDb()
        {
            if (!_pm.IsProductActive)
                return false;

            using var conn = _sqlite.OpenConnection(_pm.DB_NAME_PRODUCT);
            if (conn == null)
                return false;

            string msg = "Dispensing parameters are being read...";
            _statusBar.SystemMessage(msg);

            try
            {
                using var tran = conn.BeginTransaction();
                ReadDispenserFromDb(conn);
                ReadShapeFromDb(conn);
                ReadSequenceFromDb(conn);
                ReadGroupFromDb(conn);
                tran.Commit();
            }
            catch (Exception e)
            {
                msg = $"Failed to read the dispensing data.\nMessage:{e.Message}";
                _statusBar.SystemMessage(msg);
                return false;
            }

            msg = "The dispensing parameters are read successfully.";
            _statusBar.SystemMessage(msg);
            return true;
        }

        /********************
         * 點膠功能
         ********************/
        /// <inheritdoc/>
        public void ExecuteDispensing(int dispenserId, int shapeId)
        {
            if (!_pm.IsProductActive)
                return;

            if (DispensingParameters.Shape.Exists(x => x.Id == shapeId))
            {
                var dispenser = DispensingParameters.Dispenser;

                // TODO: 改LINQ?
                var sequences = DispensingParameters.Sequence.FindAll(x => x.ShapeId == shapeId);
                if (sequences == null)
                    return;

                var groups = DispensingParameters.Group.FindAll(x => x.ShapeId == shapeId);
                if (groups == null)
                    return;

                /***** 點膠資料 *****/
                // reference position
                PointXY refPos = new PointXY
                {
                    X = dispenser.DatumX + _stage.StageCenter.RotateCenterX,
                    Y = dispenser.DatumY + _stage.StageCenter.RotateCenterY
                };

                // 是否點膠中?
                bool isDispensing = false;

                /***** 前置動作 *****/
                // 移至standby position
                _servo.MoveZAxisToSafety();
                _servo.MoveTo(positionX: refPos.X, positionY: refPos.Y);
                _servo.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

                // 動作開始，點膠氣缸下降、Z軸下降
                _io.WriteOutputIo(IoId.DispensingCylinder, true);
                _servo.BlendOn();
                _servo.MoveTo(positionZ: dispenser.DatumZ);
                _io.WaitingForSensorOn(IoId.DispensingDownLs);

                /***** 依Sequence執行動作 *****/
                int lastGroup = -1;
                DispensingGroupDefine group = default;

                foreach (var sequence in sequences)
                {
                    if (lastGroup != sequence.GroupNo)
                    {
                        lastGroup = sequence.GroupNo;
                        group = groups.Find(x => x.GroupNo == lastGroup);

                        // Speed
                        _servo.SetSpeed(servoSpeedX: group.DspSpeed,
                                        servoSpeedY: group.DspSpeed,
                                        servoSpeedZ: group.DspSpeed,
                                        servoSpeedR: group.SpeedR,
                                        servoSpeedClamp: group.DspSpeed,
                                        servoSpeedTray: group.DspSpeed);
                    }

                    // 動作類型
                    switch ((ActionType)sequence.Type)
                    {
                        case ActionType.Move:
                            if (isDispensing)
                            {
                                _io.WriteOutputIo(IoId.DispensingOnOff, false);
                                isDispensing = false;
                            }

                            _servo.MoveTo(positionX: refPos.X + sequence.OffsetX,
                                          positionY: refPos.Y + sequence.OffsetY,
                                          positionZ: sequence.OffsetZ,
                                          degreeR: sequence.OffsetR);
                            break;

                        case ActionType.Rotate:
                            if (!isDispensing)
                            {
                                _io.WriteOutputIo(IoId.DispensingOnOff, true);
                                isDispensing = true;
                            }

                            _servo.MoveTo(positionX: refPos.X + sequence.OffsetX,
                                          positionY: refPos.Y + sequence.OffsetY,
                                          positionZ: sequence.OffsetZ,
                                          degreeR: sequence.OffsetR);

                            break;

                        //case ActionType.Dot:
                        //    break;

                        //case ActionType.Line:
                        //    break;

                        //case ActionType.Arc:
                        //    break;

                        //case ActionType.Midpoint:
                        //    break;

                        //case ActionType.RtCntr:
                        //    break;

                        //case ActionType.RtImg:
                        //    break;

                        default:
                            break;
                    }
                }

                // 動作結束，移至standby position



                // 點膠氣缸上升，Z軸至安全高度
                _io.WriteOutputIo(IoId.DispensingCylinder, false);
                _io.WaitingForSensorOn(IoId.DispensingUpLs);
                _servo.MoveZAxisToSafety();
                _servo.BlendOff();
            }
        }
    }
}
