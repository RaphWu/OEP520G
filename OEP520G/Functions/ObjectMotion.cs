using CSharpCore.Logger;
using EPCIO;
using OEP520G.Automatic;
using OEP520G.Core;
using OEP520G.Parameter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OEP520G.Functions
{
    /// <summary>
    /// 物件之間的伺服移動
    /// <list type="number">
    /// <item>以「第一校正點」為機台基準點。</item>
    /// <item>物件名稱：<br/>
    ///       a. 基準點<br/>
    ///       b. 固定物件：固定相機<br/>
    ///       c. 可動物件：移動相機、吸嘴、台車、夾爪、膠針</item>
    /// <item>實體軸座標系(各實體馬達軸的絕對座標)：<br/>
    ///       a. 第一校正點 / 基準點 -> 基準點座標<br/>
    ///       b. 物件所在的絕對軸座標 -> 軸座標<br/>
    ///       c. 物件欲移動的目的地點 -> 目的地座標</item>
    /// <item>基準點座標系(以第一校正點為原點的座標系)：<br/>
    ///       馬達X軸物件(吸嘴、膠針)相對於基準點距離->基準相對座標(可能為負值)</item>
    /// <item>取料座標系(TRAY盤相對於基準點)：<br/>
    ///       取料座標 -> 取料座標</item>
    /// <item>計算：(=>表示轉換方向)<br/>
    ///       基準點座標 = 移動相機直接定位的軸座標絕對值<br/>基準相對座標 = 軸座標 - 基準點座標<br/>
    ///       基準相對座標 => 目的地座標 = 基準點座標 - 基準相對座標 + 目的地座標</item>
    /// <item>【重要】此物件的運動，均不預設運動速度，由呼叫者決定。</item>
    /// <item>【重要】此物件的運動，預設會等待運動的軸停止，才會返回呼叫者；<br/>
    ///       若要不等待即返回，請使用waitingForMotionStop參數。</item>
    /// </list>
    /// </summary>
    public class ObjectMotion
    {
        private readonly Logger log = Logger.Instance;

        private readonly Epcio epcio = Epcio.Instance;
        private readonly Machine machine = Machine.Instance;
        private readonly Camera camera = Camera.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Stage stage = Stage.Instance;
        private readonly Clamp clamp = Clamp.Instance;
        private readonly Dispenser dispenser = Dispenser.Instance;
        private readonly Tray tray = Tray.Instance;

        // 運動前，Z軸是否在安全位置
        private bool _isAxisZNotSafety;

        // 運動前記錄Z軸原高度
        private double? _positionZBackup;

        //private readonly DatumPointInfo refPoint1 = new DatumPointInfo();
        private readonly List<NozzleObject> _nozItem;

        // 物件Code
        public const string REFERENCE_POINT = "RP";
        public const string MOVE_CAMERA = "FLY";
        public const string FIX_CAMERA = "FIX";
        public const string STAGE = "STAGE";
        public const string NOZZLE_01 = "N1";
        public const string NOZZLE_02 = "N2";
        public const string NOZZLE_03 = "N3";
        public const string NOZZLE_04 = "N4";
        public const string NOZZLE_05 = "N5";
        public const string NOZZLE_06 = "N6";
        public const string NOZZLE_07 = "N7";
        public const string NOZZLE_08 = "N8";
        public const string NOZZLE_09 = "N9";
        public const string NOZZLE_10 = "N10";
        public const string NOZZLE_11 = "N11";
        public const string CLAMP_1 = "C1";
        public const string CLAMP_2 = "C2";

        /// <summary>
        /// 建構函式
        /// </summary>
        public ObjectMotion()
        {
            //refPoint1.X = machine.DatumPoint1.X + camera.MoveCamera.OriginX;
            //refPoint1.Y = machine.DatumPoint1.Y + camera.MoveCamera.OriginY;
            _nozItem = nozzle.NozzleList;
        }

        /********************
         * 運動前及運動後的判斷與處置
         *******************/
        /// <summary>
        /// 運動前檢查
        /// </summary>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動<br/>注意：小心撞機，謹慎使用！</param>
        private async Task PreMotion(double? standbyZ, bool moveingAtSafetyZ)
        {
            /****************
             * 1.有要求在安全高度運動：
             *   1.Z軸在安全位置　 => Z軸不移動：
             *   2.Z軸未在安全位置 => 先記錄目前Z座標，然後再：
             *     a.有指定結束後Z座標 => 運動結束後，移動至指定Z座標
             *     b.無指定結束後Z座標 => 運動結束後，Z軸移至原座標
             * 2.無要求在安全高度運動 => 以目前高度運動。小心撞機，謹慎使用
             ***************/

            // Z軸是否不在安全高度
            _isAxisZNotSafety = !epcio.IsServoZSafety();

            // 有要求停止高度=>運動結束後停至指定高度
            if (standbyZ != null)
                _positionZBackup = standbyZ;
            else
                _positionZBackup = null;

            // 有要求檢查安全高度
            if (moveingAtSafetyZ && _isAxisZNotSafety)
            {
                // 高度未指定=>運動結束要停至原高度
                if (_positionZBackup == null)
                    _positionZBackup = epcio.ServoZ.GetCurrentPosition();

                await epcio.MoveServoZToSafety();
                await epcio.WaitingForMotionStop(waitingServoZ: true);
            }
        }

        /// <summary>
        /// 運動結束後處理
        /// </summary>
        private async Task MotionFinished()
        {
            // 有指定停止高度
            if (_positionZBackup != null)
            {
                epcio.MoveTo(positionZ: _positionZBackup);
                await epcio.WaitingForMotionStop(waitingServoZ: true);
            }
        }

        /********************
         * 物件移動(目的地:Tray)
         *******************/
        /// <summary>
        /// 移動相機->Tray
        /// </summary>
        /// <param name="trayName">Tray Name</param>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        /// <returns></returns>
        public async Task MoveCameraToTray(string trayName,
                                           double? standbyZ = null,
                                           bool moveingAtSafetyZ = true,
                                           bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: standbyZ, moveingAtSafetyZ);

            (double trayToDpX, double trayToDpY) = tray.GetDistanceToDatumPoint(trayName);

            double posX = machine.DatumPoint1.Position.X + trayToDpX;
            double posY = machine.DatumPoint1.Position.Y + trayToDpY;

            epcio.MoveTo(positionX: posX, positionTray: posY, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoTray: true);

            await MotionFinished();
        }

        /// <summary>
        /// 吸嘴->Tray
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <param name="trayName">Tray Name</param>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        /// <returns></returns>
        public async Task NozzleToTray(ENozzleId nozzleId,
                                       string trayName,
                                       double? standbyZ = null,
                                       bool moveingAtSafetyZ = true,
                                       bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: standbyZ, moveingAtSafetyZ);

            int nozzleNo = (int)nozzleId;
            var noz = nozzle.NozzleList[nozzleNo];

            // DP到TRAY點位的相對位置
            (double trayToDpX, double trayToDpY) = tray.GetDistanceToDatumPoint(trayName);

            // TRAY點位絕對位置+吸嘴到DP絕對位置+拍照Offset
            double posX = machine.DatumPoint1.Position.X + trayToDpX
                        + noz.DistanceToMoveCamera.X;
            //+ image.Nozzles[nozzleNo].X;

            double posY = machine.DatumPoint1.Position.Y + trayToDpY
                        + noz.DistanceToMoveCamera.Y;
            //+ image.Nozzles[nozzleNo].Y;

            epcio.MoveTo(positionX: posX, positionTray: posY, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoTray: true);

            await MotionFinished();
        }

        /// <summary>
        /// Clamp->Tray
        /// </summary>
        /// <param name="clampId">夾爪ID</param>
        /// <param name="trayName">Tray Name</param>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        /// <returns></returns>
        public async Task ClampToTray(EClampId clampId,
                                      string trayName,
                                      double? standbyZ = null,
                                      bool moveingAtSafetyZ = true,
                                      bool waitingForMotionStop = true)
        {
            //await PreMotion(standbyZ: standbyZ, moveingAtSafetyZ);

            var clp = clamp.ClampList[(int)clampId];

            // DP到TRAY點位的相對位置
            (double trayToDpX, double trayToDpY) = tray.GetDistanceToDatumPoint(trayName);

            double posX = clp.StageCoordination.X
                        - (trayToDpX + (machine.DatumPoint1.Position.X - stage.RotateCenter.X));

            double posY = machine.DatumPoint1.Position.Y + trayToDpY + clp.ConvertToMoveCamera.Y;

            epcio.MoveTo(positionClamp: posX, positionTray: posY, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoClamp: true, waitingServoTray: true);

            //await MotionFinished();
        }

        /********************
         * 物件移動(目的地:吸嘴)
         *******************/
        /// <summary>
        /// 移動相機->吸嘴
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task MoveCameraToNozzle(ENozzleId nozzleId,
                                             double? standbyZ = null,
                                             bool moveingAtSafetyZ = true,
                                             bool waitingForMotionStop = true)
        {
            int _nozzleId = (int)nozzleId;
            await PreMotion(standbyZ: standbyZ, moveingAtSafetyZ);

            PointXY dis = _nozItem[_nozzleId].DistanceToMoveCamera;
            epcio.MoveTo(positionX: epcio.ServoX.GetCurrentPosition() + dis.X,
                         positionY: epcio.ServoY.GetCurrentPosition() + dis.Y,
                         checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

            await MotionFinished();
        }

        /********************
         * 物件移動(目的地:移動相機)
         *******************/
        /// <summary>
        /// 吸嘴->移動相機
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task NozzleToMoveCamera(ENozzleId nozzleId,
                                             double? standbyZ = null,
                                             bool moveingAtSafetyZ = true,
                                             bool waitingForMotionStop = true)
        {
            int _nozzleId = (int)nozzleId;
            await PreMotion(standbyZ: standbyZ, moveingAtSafetyZ);

            PointXY dis = _nozItem[_nozzleId].DistanceToMoveCamera;
            epcio.MoveTo(positionX: epcio.ServoX.GetCurrentPosition() - dis.X,
                         positionY: epcio.ServoY.GetCurrentPosition() - dis.Y, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

            await MotionFinished();
        }

        /// <summary>
        /// 夾爪->移動相機
        /// </summary>
        public async Task ClampToMoveCamera()
        {

        }

        /********************
         * 物件移動(目的地:固定相機)
         *******************/
        /// <summary>
        /// 吸嘴->固定相機
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task NozzleToFixCamera(ENozzleId nozzleId,
                                            double? standbyZ = null,
                                            bool moveingAtSafetyZ = true,
                                            bool waitingForMotionStop = true)
        {
            // CameraTest
            try
            {
                int _nozzleId = (int)nozzleId;
                await PreMotion(standbyZ: standbyZ, moveingAtSafetyZ);

                log.WriteLine($"\t\tCameraTest: NozzleToFixCamera() Nozzle=>{nozzleId} positionX: {_nozItem[_nozzleId].Position.X}");

                epcio.MoveTo(positionX: _nozItem[_nozzleId].Position.X, checkSafetyZ: moveingAtSafetyZ);
                if (waitingForMotionStop)
                    await epcio.WaitingForMotionStop(waitingServoX: true);

                await MotionFinished();
            }
            catch (Exception e)
            {
                log.PrintFullInfo($"CameraTest: IsMotionStop(), {e.Message}");
            }
        }

        /// <summary>
        /// 膠針->固定相機
        /// </summary>
        /// <param name="standbyZ">運動結束後Z軸停止高度</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task DispenserToFixCamera(double? standbyZ = null,
                                               bool moveingAtSafetyZ = true,
                                               bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: standbyZ, moveingAtSafetyZ);

            epcio.MoveTo(positionX: dispenser.Position.X, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true);

            await MotionFinished();
        }

        /********************
         * 物件移動(目的地:台車)
         *******************/
        /// <summary>
        /// 移動相機->台車
        /// </summary>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task MoveCameraToStage(bool moveingAtSafetyZ = true,
                                            bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: null, moveingAtSafetyZ);

            epcio.MoveTo(positionX: stage.RotateCenter.X,
                         positionY: stage.RotateCenter.Y, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

            await MotionFinished();
        }

        /// <summary>
        /// 吸嘴->台車
        /// </summary>
        /// <param name="nozzleId">吸嘴ID(0~10)</param>
        /// <param name="offsetX">附加的位移值X</param>
        /// <param name="offsetY">附加的位移值Y</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task NozzleToStage(ENozzleId nozzleId,
                                        double offsetX,
                                        double offsetY,
                                        bool moveingAtSafetyZ = true,
                                        bool waitingForMotionStop = true)
        {
            PointXY dis = _nozItem[(int)nozzleId].DistanceToMoveCamera;
            await PreMotion(standbyZ: null, moveingAtSafetyZ);

            epcio.MoveTo(positionX: stage.RotateCenter.X + dis.X + offsetX,
                         positionY: stage.RotateCenter.Y + dis.Y + offsetY, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

            await MotionFinished();
        }

        /// <summary>
        /// 吸嘴->台車
        /// </summary>
        /// <param name="nozzleId">吸嘴ID(0~10)</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        public async Task NozzleToStage(ENozzleId nozzleId, bool moveingAtSafetyZ = true)
            => await NozzleToStage(nozzleId, 0, 0, moveingAtSafetyZ);

        /// <summary>
        /// 基準吸嘴->台車
        /// </summary>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        public async Task DatumNozzleToStage(bool moveingAtSafetyZ = true)
            => await NozzleToStage(nozzle.DatumNozzleId, moveingAtSafetyZ);

        /// <summary>
        /// 夾爪->台車
        /// </summary>
        /// <param name="clampId">夾爪ID</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task ClampToStage(EClampId clampId, bool waitingForMotionStop = true)
        {
            if (clampId == EClampId.Clamp1)
                epcio.MoveTo(positionClamp: clamp.Clamp1.StageCoordination.X,
                             positionY: clamp.Clamp1.StageCoordination.Y);
            else if (clampId == EClampId.Clamp2)
                epcio.MoveTo(positionClamp: clamp.Clamp2.StageCoordination.X,
                             positionY: clamp.Clamp2.StageCoordination.Y);
            else
                return;

            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoClamp: true, waitingServoY: true);
        }

        /// <summary>
        /// 膠針->台車
        /// </summary>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task DispenserToStage(bool moveingAtSafetyZ = true,
                                           bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: null, moveingAtSafetyZ);

            epcio.MoveTo(positionX: stage.RotateCenter.X + dispenser.Distance.X,
                         positionY: stage.RotateCenter.Y + dispenser.Distance.Y, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

            await MotionFinished();
        }

        /********************
         * 物件移動(目的地:夾爪)
         *******************/
        /// <summary>
        /// 移動相機->夾爪
        /// </summary>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        public void MoveCameraToClamp(bool moveingAtSafetyZ = true)
        {
        }

        /********************
         * 物件移動(目的地:測高平台)
         *******************/
        /// <summary>
        /// 吸嘴->測高平台
        /// </summary>
        /// <param name="nozzleId">吸嘴ID(0~10)</param>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task NozzleToMeasurePlatform(ENozzleId nozzleId,
                                                  bool moveingAtSafetyZ = true,
                                                  bool waitingForMotionStop = true)
        {
            int _nozzleId = (int)nozzleId;
            await PreMotion(standbyZ: null, moveingAtSafetyZ);

            PointXY dis = nozzle.NozzleList[_nozzleId].DistanceToMoveCamera;

            epcio.MoveTo(positionX: machine.AssembleMeasureHeightPlatform.X - dis.X,
                         positionY: machine.AssembleMeasureHeightPlatform.Y - dis.Y, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

            await MotionFinished();
        }

        /// <summary>
        /// 移動相機->測高平台
        /// </summary>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task MoveCameraToPlatform(bool moveingAtSafetyZ = true,
                                               bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: null, moveingAtSafetyZ);

            epcio.MoveTo(positionX: machine.AssembleMeasureHeightPlatform.X,
                         positionY: machine.AssembleMeasureHeightPlatform.Y, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoY: true);

            await MotionFinished();
        }

        /********************
         * 物件移動(目的地:抛料盒)
         *******************/
        /// <summary>
        /// 吸嘴->組裝側抛料盒
        /// </summary>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task NozzleToDiscardBox(bool moveingAtSafetyZ = true,
                                             bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: machine.AssembleDiscardBox.Z, moveingAtSafetyZ);

            epcio.MoveTo(positionX: machine.AssembleDiscardBox.X, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoX: true);

            await MotionFinished();
        }

        /// <summary>
        /// 夾爪->夾爪側抛料盒
        /// </summary>
        /// <param name="moveingAtSafetyZ">是否要求在安全高度運動</param>
        /// <param name="waitingForMotionStop">是否等運動完成才返回</param>
        public async Task ClampToDiscardBox(bool moveingAtSafetyZ = true,
                                            bool waitingForMotionStop = true)
        {
            await PreMotion(standbyZ: null, moveingAtSafetyZ);

            epcio.MoveTo(
                positionClamp: machine.SemiFinishedDiscardBox.X,
                positionY: machine.SemiFinishedDiscardBox.Y, checkSafetyZ: moveingAtSafetyZ);
            if (waitingForMotionStop)
                await epcio.WaitingForMotionStop(waitingServoClamp: true, waitingServoY: true);

            await MotionFinished();
        }
    }
}
