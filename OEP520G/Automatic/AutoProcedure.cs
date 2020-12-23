using CSharpCore;
using CSharpCore.Logger;
using EPCIO;
using EpcioSeries;
using Imageproject.Contracts;
using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Parameter;
using OEP520G.Product;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OEP520G.Automatic
{
    /// <summary>
    /// 自動作業程序
    /// </summary>
    public class AutoProcedure : ActionGroup
    {
        private readonly Logger log = Logger.Instance;

        private readonly Epcio epcio = Epcio.Instance;
        private readonly ProductManager pm = new ProductManager();
        private readonly Dump dump = new Dump();
        private readonly AutoSequence autoSequence = AutoSequence.Instance;
        private readonly PickUpPart pickUpPart = new PickUpPart();
        private readonly Assemble assemble = new Assemble();
        private readonly Tray tray = Tray.Instance;
        private readonly ObjectMotion objectMotion = new ObjectMotion();
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Stage stage = Stage.Instance;
        private readonly Clamp clamp = Clamp.Instance;

        /// <summary>
        /// 目標產出數量
        /// </summary>
        private int _targetQuantity;

        /// <summary>
        /// 目前產出數量
        /// </summary>
        private int _outputQuantity;

        // 取消權杖；中斷作業；機台遺留半成品
        private CancellationTokenSource _cts = null;

        private readonly IImage _image;

        /// <summary>
        /// 建構函式
        /// </summary>
        public AutoProcedure(IImage image)
        {
            _image = image;

            SetRunning(false);
        }

        /// <summary>
        /// 取得目前產出數量
        /// </summary>
        /// <returns></returns>
        public int GetQuantity()
            => _outputQuantity;

        /// <summary>
        /// 執行自動作業
        /// </summary>
        /// <param name="mode">自動模式</param>
        /// <param name="targetQuantity">目標產出數量</param>
        /// <param name="cts">取消權杖</param>
        public async Task StartRunning(EAutomaticOperationMode mode,
                                       int targetQuantity,
                                       CancellationTokenSource cts)
        {
            // 伺服軸停止狀態確認
            if (!ProductManager.HasProductActived
                || !epcio.IsAllServoMotionStop()
                || targetQuantity <= 0)
                return;

            var fly = new FlyCorrect(_image, false);
            Random rnd = new Random();

            EAutomaticOperationMode _mode = mode;
            _targetQuantity = targetQuantity;
            _cts = cts;

            await Task.Run(async () =>
            {
                try
                {
                    // 步驟Index
                    int stepIndex = 0;

                    _outputQuantity = 0;

                    // Flag設定
                    SetRunning(true);

                    /******************
                     * 作業前準備
                     *****************/
                    // 組裝順序
                    List<SequenceData> assemblySequence = new List<SequenceData>();

                    // 取料目標
                    List<SequenceData> pickUpSequence = new List<SequenceData>();

                    for (int index = 0; index < autoSequence.SequenceDataList.Count; index++)
                    {
                        var proc = autoSequence.SequenceDataList[index];

                        if (proc.Effective
                            && !string.IsNullOrEmpty(proc.SelectedHead)
                            && !string.IsNullOrEmpty(proc.SelectedAction)
                            && !string.IsNullOrEmpty(proc.SelectedTarget)
                            )
                        {
                            if (proc.SelectedAction[0..^2] == EAction.PickUp01.ToString()[0..^2])
                                pickUpSequence.Add(autoSequence.SequenceDataList[index]);
                            else
                                assemblySequence.Add(autoSequence.SequenceDataList[index]);
                        }
                    }

                    /*========================================
                      * X-Tray Clamp-Y 組動作
                      ========================================*/
                    // 全抛料
                    await SwitchActionGroup(EActionGroup.XTray_ClampY);
                    //await dump.DiscardAll();

                    bool _isNozzleGroupFinished; // 吸嘴組動作是否結束
                    bool _isChampGroupFinished;  // 夾爪組動作是否結束
                    bool _isProductGood;         // 台車上的成品/半成品是否為良品

                    // 組裝狀態旗標
                    bool _isNozzleHavePart = false;   // 吸嘴上是否有部品
                    bool _isStageHasPart = false;     // 台車上是否有部品
                    bool _isClamp1HasBarrel = false;  // 夾爪1上是否有Barrel
                    bool _isClamp2HasProduct = false; // 夾爪2上是否有成品/半成品

                    // 判斷生產數量
                    while ((_outputQuantity < _targetQuantity) || _isClamp2HasProduct)
                    {
                        _isNozzleGroupFinished = false;
                        _isChampGroupFinished = false;
                        _isProductGood = false;

                        /*========================================
                         * X-Y Clamp-Tray 組動作
                         ========================================*/
                        // 檢查是否強制中斷作業
                        if (_cts.Token.IsCancellationRequested)
                            _cts.Token.ThrowIfCancellationRequested();

                        await SwitchActionGroup(EActionGroup.XY_ClampTray);

                        /********************
                         * 吸嘴組動作
                         ********************/
                        _ = Task.Run(async () =>
                        {
                            // CameraTest
                            try
                            {
                                // TODO: 判斷條件有點不夠確實
                                if (_isStageHasPart)
                                {
                                    /********************
                                     * 組裝流程
                                     ********************/
                                    // 取得步驟
                                    foreach (var proc in assemblySequence)
                                    {
                                        /********************
                                         * 執行條件判斷
                                         ********************/
                                        if (proc.Effective
                                            && !string.IsNullOrEmpty(proc.SelectedHead)
                                            && !string.IsNullOrEmpty(proc.SelectedAction)
                                            && !string.IsNullOrEmpty(proc.SelectedTarget)
                                            )
                                        {
                                            string actionFullName = proc.SelectedAction;
                                            string actionNameNoId = actionFullName[0..^2];

                                            var nozzleId = Enum.Parse<ENozzleId>(proc.SelectedHead);
                                            int nozzleNo = (int)nozzleId;
                                            //int.TryParse(proc.SelectedTarget[^2..], out int feederId);

                                            /********************
                                             * 組裝前處理
                                             ********************/
                                            // TODO: Barrel拍照


                                            // 3:略過搭載(Carry)前對位處理
                                            if (!proc.SkipAlignmentBeforeCarry)
                                            {
                                            }

                                            /********************
                                             * 置件
                                             ********************/
                                            if (actionFullName == EAction.PlacePart.ToString())
                                            {
                                                epcio.SetSpeed(servoXSpeed: EServoSpeed.High,
                                                               servoYSpeed: EServoSpeed.High,
                                                               servoRSpeed: EServoSpeed.High);

                                                await objectMotion.NozzleToStage(nozzleId,
                                                                                 0, 0,
                                                                                 waitingForMotionStop: false);
                                                epcio.MoveTo(degreeR: 0);
                                                await epcio.WaitingForMotionStop(waitingServoX: true,
                                                                                 waitingServoY: true,
                                                                                 waitingServoR: true);

                                                // 組裝動作
                                                MCCL.MCC_EnableBlend(epcio.ServoZ.Group);

                                                nozzle.NozzleDown(nozzleId);

                                                epcio.SetSpeed(servoZSpeed: EServoSpeed.High);
                                                epcio.MoveTo(positionZ: 25);
                                                epcio.SetSpeed(servoZSpeed: EServoSpeed.Low);
                                                epcio.MoveTo(positionZ: 30);
                                                await epcio.WaitingForMotionStop(waitingServoZ: true);

                                                await Task.Delay(300);
                                                nozzle.NozzleBlowOn(nozzleId);
                                                await Task.Delay(200);
                                                nozzle.NozzleOff(nozzleId);
                                                await Task.Delay(300);

                                                epcio.SetSpeed(servoZSpeed: EServoSpeed.Low);
                                                epcio.MoveTo(positionZ: 25);
                                                epcio.SetSpeed(servoZSpeed: EServoSpeed.High);
                                                epcio.MoveTo(positionZ: epcio.SafetyZ);
                                                await epcio.WaitingForMotionStop(waitingServoZ: true);

                                                nozzle.NozzleUp(nozzleId);

                                                MCCL.MCC_DisableBlend(epcio.ServoZ.Group);
                                            }

                                            /******************
                                             * 組裝後處理
                                             *****************/
                                            // 影像處理
                                            else if (actionFullName == EAction.ImageCheck.ToString())
                                            {
                                            }

                                            // 點膠
                                            else if ((actionFullName == EAction.Dispensing.ToString())
                                                  || (actionFullName == EAction.GlueAmountCheck.ToString())
                                                  || (actionFullName == EAction.PreDispensing.ToString())
                                                  || (actionFullName == EAction.ClearGlue.ToString())
                                                  )
                                            {
                                            }
                                            else
                                            {
                                                goto AssebmlyFinished;
                                            }

                                            /******************
                                             * 組裝後處理
                                             *****************/
                                            // 1:畫像處理(飛行，步驟後執行)
                                            if (proc.ImageProcessing)
                                            {
                                            }

                                            // 2:推出小車(步驟後執行)
                                            if (proc.LaunchStageAfterProcedure)
                                            {
                                            }

                                            // 4:置件(Assembly)時張開夾爪
                                            if (proc.OpenClampWhenAssembly)
                                            {
                                            }

                                            // 5:搭載完成後，將小車轉回0度
                                            if (proc.StageReturn0AfterCarry)
                                            {
                                                epcio.SetSpeed(servoRSpeed: EServoSpeed.UltraHigh);
                                                epcio.MoveTo(degreeR: 0);
                                                await epcio.WaitingForMotionStop(waitingServoR: true);
                                            }

                                            // 6:置件時不檢查下壓到位
                                            if (!proc.SkipPositionCheckWhenAssembly)
                                            {
                                            }

                                            // 7:於台車取置件時，不使用夾片
                                            if (proc.OpenClampWhenSingleProcedure)
                                            {
                                            }

                                            // 8:組裝完成後測高
                                            if (proc.MeasureHighAfterAssembly)
                                            {
                                            }

                                            // 9:絕對組裝前畫像對位處理
                                            if (proc.GetCenterAfterStageRotate)
                                            {
                                            }
                                        }

                                    AssebmlyFinished:
                                        /******************
                                         * 步驟結束
                                         *****************/
                                        stepIndex++;
                                    }

                                    // TODO: 組裝失敗判斷
                                    _isProductGood = true;
                                    _isNozzleHavePart = false;
                                }
                                _isNozzleGroupFinished = true;
                            }
                            catch (Exception e)
                            {
                                log.PrintFullInfo($"CameraTest: IsMotionStop(), {e.Message}");
                            }
                        });

                        /********************
                         * 夾爪組動作
                         ********************/
                        _ = Task.Run(async () =>
                        {
                            // CameraTest
                            try
                            {
                                if (_isClamp2HasProduct)
                                {
                                    // 放回成品
                                    await clamp.PlaceProduct(12);
                                    _isClamp2HasProduct = false;
                                }

                                // 產出數(包含台車上正在組裝那顆)是否已達成?
                                // TODO: 台車上是否有仍在組裝的產品？
                                if (_outputQuantity + (_isStageHasPart ? 1 : 0) < _targetQuantity)
                                {
                                    // 取空Barrel
                                    await pickUpPart.ClampPickUpBarrel(11);
                                    _isClamp1HasBarrel = true;

                                    //if (_isStageHasPart)
                                    //    epcio.MoveTo(positionClamp: clamp.Clamp2.StageCoordination.X);
                                }

                                //epcio.MoveTo(positionClamp: _isStageHasPart
                                //   ? clamp.Clamp2.StageCoordination.X
                                //   : clamp.Clamp1.StageCoordination.X);

                                _isChampGroupFinished = true;
                            }
                            catch (Exception e)
                            {
                                log.PrintFullInfo($"CameraTest: IsMotionStop(), {e.Message}");
                            }
                        });

                        // 檢查動作結束
                        await Task.Yield();
                        while (!(_isNozzleGroupFinished && _isChampGroupFinished))
                        { }

                        /*========================================
                         * X-Tray Clamp-Y 組動作
                         ========================================*/
                        // 檢查是否強制中斷作業
                        if (_cts.Token.IsCancellationRequested)
                            _cts.Token.ThrowIfCancellationRequested();

                        await SwitchActionGroup(EActionGroup.XTray_ClampY);

                        _isNozzleGroupFinished = false;
                        _isChampGroupFinished = false;

                        /********************
                         * 吸嘴組動作
                         ********************/
                        _ = Task.Run(async () =>
                        {
                            // CameraTest
                            try
                            {
                                if (_isClamp1HasBarrel)
                                {
                                    // 取料前拍照
                                    epcio.SetSpeed(servoXSpeed: EServoSpeed.High,
                                                   servoTraySpeed: EServoSpeed.High);

                                    foreach (var sequence in pickUpSequence)
                                    {
                                        //Common.log.WriteLine($"拍照{sequence}");

                                        int nozzleNo = (int)Enum.Parse<ENozzleId>(sequence.SelectedHead);
                                        int feederId = int.Parse(sequence.SelectedTarget[^2..]);
                                        string trayName = tray.GetTrayNameFromFeederId(feederId);

                                        // 拍照
                                        bool pictureOK;
                                        do
                                        {
                                            tray.MoveNext(trayName);
                                            await objectMotion.MoveCameraToTray(trayName);

                                            // TODO: 拍照
                                            await Task.Delay(500);
                                            pictureOK = true;

                                            // 模擬拍照失敗
                                            if (rnd.Next(1, 10) <= -1)
                                                pictureOK = false;
                                        } while (!pictureOK);

                                        //// TODO: Offset
                                        //image.Nozzles[nozzleNo] = new ImageData()
                                        //{
                                        //    Effective = true,
                                        //    X = rnd.Next(-30, 30) / 1000,
                                        //    Y = rnd.Next(-30, 30) / 1000,
                                        //    Angle = rnd.Next(0, 3590) / 10
                                        //};
                                    }

                                    // 各吸嘴取料
                                    epcio.SetSpeed(servoXSpeed: EServoSpeed.High,
                                                   servoTraySpeed: EServoSpeed.High,
                                                   servoZSpeed: EServoSpeed.High);

                                    foreach (var proc in pickUpSequence)
                                    {
                                        var nozzleId = Enum.Parse<ENozzleId>(proc.SelectedHead);
                                        int feederId = int.Parse(proc.SelectedTarget[^2..]);
                                        string trayName = tray.GetTrayNameFromFeederId(feederId);

                                        await objectMotion.NozzleToTray(nozzleId, trayName);

                                        // 取料
                                        // TODO: 未加參數
                                        epcio.MoveTo(positionZ: 30);
                                        nozzle.NozzleDown(nozzleId);
                                        await nozzle.WaitingForNozzleDown(nozzleId);
                                        await epcio.WaitingForMotionStop(waitingServoZ: true);

                                        nozzle.NozzleVaccumOn(nozzleId);
                                        await Task.Delay(500);

                                        if (!nozzle.IsNozzleVaccumOn(nozzleId))
                                            throw new Exception($"Nozzle {nozzleId} Vaccum sensor not actived.");

                                        epcio.MoveTo(positionZ: epcio.SafetyZ);
                                        await epcio.WaitingForMotionStop(waitingServoZ: true);

                                        nozzle.NozzleUp(nozzleId);
                                    }
                                    _isNozzleHavePart = true;

                                    /********************
                                     * 整位
                                     ********************/

                                    /********************
                                     * 飛拍
                                     ********************/
                                    await fly.StartCorrect(EServoSpeed.UltraHigh, "POS", _cts);

                                    nozzle.ALlNozzleUp();
                                    await epcio.MoveServoZToSafety();
                                }

                                _isNozzleGroupFinished = true;
                            }
                            catch (Exception e)
                            {
                                log.PrintFullInfo($"CameraTest: IsMotionStop(), {e.Message}");
                            }
                        });

                        /********************
                         * 夾爪組動作
                         ********************/
                        _ = Task.Run(async () =>
                        {
                            // CameraTest
                            try
                            {
                                if (_isStageHasPart)
                                {
                                    await pickUpPart.ClampPickUpProduct(false);
                                    _outputQuantity++;
                                    _isClamp2HasProduct = true;
                                    _isStageHasPart = false;

                                    // 不良品抛料
                                    if (!_isProductGood)
                                    {
                                        await dump.ClampDump(clamp2: true);
                                        _isClamp2HasProduct = false;
                                    }
                                }

                                if (_isClamp1HasBarrel)
                                {
                                    await assemble.ClampPlaceBarrel();
                                    _isStageHasPart = true;
                                    _isClamp1HasBarrel = false;
                                }

                                _isChampGroupFinished = true;
                            }
                            catch (Exception e)
                            {
                                log.PrintFullInfo($"CameraTest: IsMotionStop(), {e.Message}");
                            }
                        });

                        // 檢查動作結束
                        await Task.Yield();
                        while (!(_isNozzleGroupFinished && _isChampGroupFinished))
                        { }

                        /******************
                         * 成品/半成品 輸出或抛料
                         *****************/
                    }

                    // 機台歸位
                    epcio.SetSpeed(EServoSpeed.Middle);
                    await epcio.SafetyPosition();
                    epcio.MoveTo(positionX: 0,
                                 positionY: 0,
                                 positionZ: 0,
                                 degreeR: 0,
                                 positionClamp: 0,
                                 positionTray: 0);
                    stage.StageClampClose();
                }
                catch (OperationCanceledException e)
                {
                    MessageBox.Show(e.Message);
                    await epcio.MoveServoZToSafety();
                }

                await epcio.WaitingForAllServoMotionStop();
            });

            // 自動作業結束
            SetRunning(false);
        }

        /// <summary>
        /// 結束自動運轉，重設目標產出數，且繼續完成組裝
        /// <list type="number">
        /// <item>最少結束產出數 = 目前產出數 + 4</item>
        /// <item>if (最少結束產出數 &gt;= 原目標產出數) => 目標產出數不變<br/>
        ///       else 目標產出數 = 取較大者(最少結束產出數, 欲重設的產出數)</item>
        /// </list>
        /// </summary>
        /// <param name="targetQuantity">欲重設的產出數</param>
        /// <returns>重設後的目標產出數</returns>
        public int StopAutoOperation(int targetQuantity = 0)
        {
            // 最少結束產出數
            int minimumTarget = _outputQuantity + 4;

            // 欲重設的產出數不可為負數
            if (targetQuantity < 0)
                targetQuantity = 0;

            if (minimumTarget < _targetQuantity)
                _targetQuantity = Math.Max(minimumTarget, targetQuantity);

            return _targetQuantity;
        }
    }
}