using CSharpCore.Logger;
using EPCIO;
using EpcioSeries;
using Imageproject.Constants;
using Imageproject.Contracts;
using Imageproject.Models;
using OEP520G.Core;
using OEP520G.Parameter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TIS.Imaging;
using Unity;

namespace OEP520G.Functions
{
    public class FlyCorrect
    {
        private const bool USE_DAC_TRIGGER = true;

        private readonly Logger log = Logger.Instance;

        private readonly Epcio epcio = Epcio.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly ObjectMotion objectMotion = new ObjectMotion();

        //private readonly ITakePictureRequest takePicture;

        // 飛行資料
        public List<FlyData> FlyDatas { get; set; }

        // 時間標記法用Timer
        //private readonly Timer FlyTimer = new Timer() { Interval = 1 };

        // 目前吸嘴ID
        private EImageTargetId flyNozzle;
        private int flyNozzleId;
        private EImageTargetId firstNozzle;

        // 取消權杖
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly IImage _image;

        /// <summary>
        /// 建構函式
        /// </summary>
        public FlyCorrect(IImage image, bool hasPartOnNozzle)
        {
            _image = image;

            FlyDatas = new List<FlyData>();
            firstNozzle = hasPartOnNozzle
                ? EImageTargetId.NozzleComponent01
                : EImageTargetId.Nozzle01;

            string nozName = "吸嘴";
            for (int speed = 0; speed <= 2; speed++)
            {
                for (int noz = 1; noz <= Nozzle.MAX_NOZZLE; noz++)
                {
                    FlyDatas.Add(new FlyData()
                    {
                        SpeedName = GlobalString.SpeedName[speed],
                        NozzleName = nozName + noz.ToString()
                    });
                }
            }

            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
            {
                NozzleObject ni = nozzle.NozzleList[noz];

                FlyDatas[noz].X = ni.UltraHighTimeMarker.X; // 原補正值X
                FlyDatas[noz].Y = ni.UltraHighTimeMarker.Y; // 原補正值Y
                FlyDatas[noz].Update = false;               // 更新
                FlyDatas[noz].NewX = 0;                     // 讀取的X軸座標
                FlyDatas[noz].NewY = 0;                     // 讀取的Y軸座標

                FlyDatas[noz + Nozzle.MAX_NOZZLE].X = ni.HighTimeMarker.X;
                FlyDatas[noz + Nozzle.MAX_NOZZLE].Y = ni.HighTimeMarker.Y;
                FlyDatas[noz + Nozzle.MAX_NOZZLE].Update = false;
                FlyDatas[noz + Nozzle.MAX_NOZZLE].NewX = 0;
                FlyDatas[noz + Nozzle.MAX_NOZZLE].NewY = 0;

                FlyDatas[noz + Nozzle.MAX_NOZZLE * 2].X = ni.MiddleTimeMarker.X;
                FlyDatas[noz + Nozzle.MAX_NOZZLE * 2].Y = ni.MiddleTimeMarker.Y;
                FlyDatas[noz + Nozzle.MAX_NOZZLE * 2].Update = false;
                FlyDatas[noz + Nozzle.MAX_NOZZLE * 2].NewX = 0;
                FlyDatas[noz + Nozzle.MAX_NOZZLE * 2].NewY = 0;

                /***** 測試資料，正式版本須刪除 *****/
                FlyDatas[noz].NozzleX = ni.Position.X;
                FlyDatas[noz].NozzleY = ni.Position.Y;

                FlyDatas[noz + Nozzle.MAX_NOZZLE].NozzleX = ni.Position.X;
                FlyDatas[noz + Nozzle.MAX_NOZZLE].NozzleY = ni.Position.Y;

                FlyDatas[noz + Nozzle.MAX_NOZZLE * 2].NozzleX = ni.Position.X;
                FlyDatas[noz + Nozzle.MAX_NOZZLE * 2].NozzleY = ni.Position.Y;
                /***** 測試資料，正式版本須刪除 *****/
            }

            //FlyTimer.Elapsed += new ElapsedEventHandler(FlyTimerEvent);
            //FlyTimer.AutoReset = true;
        }

        /********************
         * 飛行
         *******************/
        private EServoSpeed _flySpeed;       // 飛行速度
        private int flyDataIndex = default; // FlyDatas Index倍數: 最高速=0 高速=1 中速=2
        private int flyDataBaseIndex;       // FlyDatas基底Index = Nozzle.MAX_NOZZLE * flyDataIndex

        /// <summary>
        /// 飛行校正前置作業
        /// </summary>
        /// <param name="flySpeed">飛行速度</param>
        /// <param name="flyWaySelected">飛行模式("ENC"/"TIMER")</param>
        /// <param name="cts">取消權杖</param>
        public async Task StartCorrect(EServoSpeed flySpeed,
                                       string flyWaySelected,
                                       CancellationTokenSource cts)
        {
            _flySpeed = EServoSpeed.Low;
            //_flySpeed = flySpeed;
            _cts = cts;

            // 安全高度
            await epcio.MoveServoZToSafety();

            // 飛行速度 & Index
            switch (_flySpeed)
            {
                case EServoSpeed.UltraHigh:
                    flyDataIndex = 0;
                    break;
                case EServoSpeed.High:
                    flyDataIndex = 1;
                    break;
                case EServoSpeed.Middle:
                    flyDataIndex = 2;
                    break;
            }
            flyDataBaseIndex = Nozzle.MAX_NOZZLE * flyDataIndex;

            // 吸嘴定位
            nozzle.ALlNozzleDown();

            // 關閉Polling
            epcio.StopPolling();

            flyNozzle = firstNozzle - 1;

            // 飛行方法分支
            if (flyWaySelected == "ENC")
                await FlyByEncoder();
            else if (flyWaySelected == "POS")
                await FlyByPosition();
            else if (flyWaySelected == "TIME")
                await FlyByTime();

            await epcio.WaitingForMotionStop(waitingServoX: true);

            // 安全高度
            await epcio.MoveServoZToSafety();

            // 回復Polling
            epcio.StartPolling();

            // 圖片存檔
            IFrameQueueBuffer[] frame = ImageParameters.FrameList;
            for (int cnt = 0; cnt < ImageParameters.MAX_IMAGE_COUNT; cnt++)
            {
                _image.SaveImageToFile($"Nozzle{cnt}", frame[cnt]);
            }
        }

        /********************
         * 飛行校正-座標中斷法
         * X軸定位: Z上升 -> X最大值 -> Z降吸嘴高度
         * 飛行: X飛至原點
         * 取像: Encoder中斷 -> 拍照 -> 設定下一支吸嘴座標
         *******************/
        private async Task FlyByPosition()
        {
            try
            {
                // 作業是否取消
                if (_cts.Token.IsCancellationRequested)
                    _cts.Token.ThrowIfCancellationRequested();

                // 變數初始設定
                //epcio.SetSpeed(EServoSpeed.UltraHigh);
                epcio.SetSpeed(EServoSpeed.Low);

                // 飛行前Z軸定位    
                await epcio.MoveServoZToSafety();

                // X軸就定位
                epcio.MoveTo(positionX: nozzle.NozzleList[0].Position.X);
                await epcio.WaitingForMotionStop(waitingServoX: true);

                ////// Z軸下降至吸嘴高度
                //epcio.MoveTo(positionZ: nozzle.NozzleList[0].Position.Z);
                //await epcio.WaitingForMotionStop(waitingServoZ: true);

                // 確定相機開啟
                _image.FixCameraOn();

                for (int flyNozzleId = 0; flyNozzleId < 11; flyNozzleId++)
                {
                    flyNozzle = firstNozzle + flyNozzleId;

                    //// 移動至下一支吸嘴
                    //MCCL.MCC_Line(nozzle.NozzleList[flyNozzleId].Position.X, 0, 0, 0, 0, 0, 0, MCCL.EPCIO_AXIS_ALL);
                    //while (MCCL.MCC_GetMotionStatus(0) != MCCL.GMS_STOP)
                    //{
                    //    Task.Delay(10).Wait();
                    //}

                    //var noz = nozzle.NozzleList[flyNozzleId];
                    await objectMotion.NozzleToFixCamera(Enum.Parse<ENozzleId>(flyNozzleId.ToString()),
                                                         moveingAtSafetyZ: false,
                                                         waitingForMotionStop: false);
                    //epcio.MoveTo(positionX: nozzle.NozzleList[flyNozzleId].Position.X);
                    //await epcio.WaitingForMotionStop(waitingServoX: true);

                    epcio.MoveTo(positionZ: nozzle.NozzleList[flyNozzleId].Position.Z);
                    await epcio.WaitingForMotionStop(waitingServoX: true, waitingServoZ: true);

                    //log.WriteLine($"FlyByPosition -> Call TakePicture: {flyNozzle}");

                    _image.FrameList[flyNozzleId + 1] = _image.TakePictureWithFixCamera();

                    //flyNozzle++;
                }

                // home
                await epcio.MoveServoZToSafety();
                epcio.MoveTo(positionX: 0, checkSafetyZ: false);
                await epcio.WaitingForMotionStop(waitingServoX: true);
            }
            catch (Exception e)
            {
                MessageBox.Show($"FlyByPosition Error: {flyNozzle}, {flyNozzleId}");
            }
        }

        /********************
         * 飛行校正-座標中斷法
         * X軸定位: Z上升 -> X最大值 -> Z降吸嘴高度
         * 飛行: X飛至原點
         * 取像: Encoder中斷 -> 拍照 -> 設定下一支吸嘴座標
         *******************/
        private MCCL.ENCISR_EX ENC_ISR_Function;

        /// <summary>
        /// 座標中斷法: 飛行主流程
        /// </summary>
        private async Task FlyByEncoder()
        {
            // 中斷設定
            ENC_ISR_Function = new MCCL.ENCISR_EX(EncIsrFunction);
            MCCL.MCC_SetENCRoutineEx(ENC_ISR_Function, Epcio.CARD_INDEX);

            Servo servoX = epcio.ServoX;

            try
            {
                // 作業是否取消
                if (_cts.Token.IsCancellationRequested)
                    _cts.Token.ThrowIfCancellationRequested();

                // 變數初始設定
                flyNozzleId = 0;
                //epcio.SetSpeed(EServoSpeed.UltraHigh);
                epcio.SetSpeed(EServoSpeed.Low);

                // 飛行前Z軸定位
                await epcio.MoveServoZToSafety();

                // X軸就定位
                epcio.MoveTo(positionX: servoX.MaxPosition);
                await epcio.WaitingForMotionStop(waitingServoX: true);

                // Z軸下降至吸嘴高度
                epcio.MoveTo(positionZ: nozzle.NozzleList[flyNozzleId].Position.Z);
                await epcio.WaitingForMotionStop(waitingServoZ: true);

                // 設定ISR第1支吸嘴觸發座標
                MCCL.MCC_SetENCCompValue(nozzle.NozzleList[flyNozzleId].Encoder.X,
                                         servoX.Channel,
                                         Epcio.CARD_INDEX);

                // 啟動中斷
                MCCL.MCC_EnableENCCompTrigger(servoX.Channel, Epcio.CARD_INDEX);

                // DAC觸發設定
                // dwSource值參閱 MCCL.DAC_TRIG_ENC0~7
                if (USE_DAC_TRIGGER)
                {
                    //MCCL.MCC_SetDACTriggerOutput(5.0F, servoX.Channel, Epcio.CARD_INDEX);
                    //MCCL.MCC_SetDACTriggerSource((uint)(0x01 << servoX.Channel), servoX.Channel, Epcio.CARD_INDEX);
                    //MCCL.MCC_EnableDACTriggerMode(servoX.Channel, Epcio.CARD_INDEX);
                    //MCCL.MCC_StartDACConv(Epcio.CARD_INDEX);
                    MCCL.MCC_SetDACTriggerOutput(5.0F, servoX.Channel, Epcio.CARD_INDEX);
                    MCCL.MCC_SetDACTriggerSource((uint)MCCL.DAC_TRIG_ENC0, servoX.Channel, Epcio.CARD_INDEX);
                    MCCL.MCC_EnableDACTriggerMode(servoX.Channel, Epcio.CARD_INDEX);
                    MCCL.MCC_StartDACConv(Epcio.CARD_INDEX);
                }

                // 相機設定
                _image.FixCameraOff();
                _image.FixCameraHwTriggerOn();

                // 飛行至原點
                epcio.SetSpeed(servoXSpeed: _flySpeed);
                epcio.MoveTo(positionX: 0, checkSafetyZ: false);
            }
            catch (Exception e)
            {
                MessageBox.Show($"FlyByEncoder Error: {e.Message}");
            }

            await epcio.WaitingForAllServoMotionStop();
            _image.FixCameraHwTriggerOff();

            // 關閉ISR
            MCCL.MCC_DisableENCCompTrigger(servoX.Channel, Epcio.CARD_INDEX);
            if (USE_DAC_TRIGGER)
            {
                MCCL.MCC_StopDACConv(Epcio.CARD_INDEX);
            }
        }

        /// <summary>
        /// 座標中斷法: EPCIO ISR
        /// </summary>
        private void EncIsrFunction(ref ENCINT_EX pstINTSource)
        {
            try
            {
                // COMP0=Channel 0
                if (pstINTSource.COMP0 == 1)
                {
                    //flyNozzle++;

                    //log.WriteLine($"\tFlyByEncoder -> Call TakePicture: {flyNozzle}");

                    //// 拍照請求
                    //if (!USE_DAC_TRIGGER)
                    //{
                    //    //_ = Task.Run(async () =>
                    //    //{
                    //    //_image.TakePictures(flyNozzle);
                    //    //});
                    //}

                    //// 讀取座標
                    //long pulseX = 0, pulseY = 0, pulseZ = 0, pulseU = 0, pulseV = 0, pulseW = 0;

                    //MCCL.MCC_GetPulsePos(ref pulseX, ref pulseY, ref pulseZ, ref pulseU, ref pulseV, ref pulseW, epcio.ServoX.Group);
                    //FlyDatas[flyDataBaseIndex + flyNozzleId].NewX
                    //    = FlyDatas[flyDataBaseIndex + flyNozzleId].NozzleX - epcio.PulseToPosition(EServoId.X, pulseX);

                    //MCCL.MCC_GetPulsePos(ref pulseX, ref pulseY, ref pulseZ, ref pulseU, ref pulseV, ref pulseW, epcio.ServoY.Group);
                    //FlyDatas[flyDataBaseIndex + flyNozzleId].NewY
                    //    = FlyDatas[flyDataBaseIndex + flyNozzleId].NozzleY - epcio.PulseToPosition(EServoId.Y, pulseX);

                    //FlyDatas[flyDataBaseIndex + flyNozzleId].Update = true;

                    // 下一支吸嘴
                    if (++flyNozzleId < Nozzle.MAX_NOZZLE)
                    {
                        int nextTriggerValue = nozzle.NozzleList[flyNozzleId].Encoder.X;
                        MCCL.MCC_SetENCCompValue(nextTriggerValue,
                                                 epcio.ServoX.Channel,
                                                 Epcio.CARD_INDEX);
                        //log.WriteLine($"\tFlyByEncoder -> Call Next TakePicture: {flyNozzle}, {nextTriggerValue}");
                    }
                    //flyNozzleId++;

                    Thread.Sleep(10);
                    MCCL.MCC_SetDACOutput(0, epcio.ServoX.Channel, Epcio.CARD_INDEX);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"EncIsrFunction() Exception: {e.Message}");
            }
        }

        private FrameQueuedResult HwTriggerPicture(IFrameQueueBuffer frame)
        {

            return FrameQueuedResult.ReQueue;
        }

        /********************
         * 時間標記法
         * X軸定位: Z上升 -> X最大值 -> Z降吸嘴高度
         * 飛行: 開啟Timer -> X飛至原點
         * 取像: 吸嘴時間到 -> 拍照(循環)
         *******************/
        // 時間標記法用計時器
        private readonly Stopwatch flyTimer = new Stopwatch();

        // 飛行開始時間
        private DateTime startTime;

        // 時間標記
        private double[] timeMarker = new double[Nozzle.MAX_NOZZLE];

        /// <summary>
        /// 時間標記法數據推算
        /// </summary>
        /// <remarks>讀取各吸嘴座標回原值，重新推算各吸嘴的時間標記。</remarks>
        /// <returns>計算是否完成</returns>
        private void TimeIntegralCalculate()
        {
            var servoX = epcio.ServoX;

            // 加速段t0
            double accTime = servoX.Axis.AccelerationTime;

            // 飛行速度(mm/sec)
            // CS8509警告可忽略，因為飛行只有3種速度
            double speedV = _flySpeed switch
            {
                EServoSpeed.UltraHigh => servoX.UltraHighSpeed,
                EServoSpeed.High => servoX.HighSpeed,
                EServoSpeed.Middle => servoX.MiddleSpeed,
            };

            // 飛行速度(mm/ms)
            speedV /= 1000.0;

            // 加速段Pulse數
            double A0 = accTime * speedV / 2.0 * servoX.PulsePerMm;

            // 計算各吸嘴的t
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
            {
                // 等速移動部分的Pulse數
                double pulseInConstantVelocity
                    = Convert.ToDouble(servoX.MaxPulse - nozzle.NozzleList[noz].Pulse.X) - A0;

                // 等速移動部分的時間
                double timeInConstantVelocity
                    = pulseInConstantVelocity / servoX.PulsePerMm * speedV;

                // 時間標記
                FlyDatas[flyDataBaseIndex + noz].TimeMarker = timeInConstantVelocity + accTime;
            }
        }

        /// <summary>
        /// 時間標記法 前置作業
        /// </summary>
        private async Task FlyByTime()
        {
            TimeIntegralCalculate();

            Servo servoX = epcio.ServoX;

            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                timeMarker[noz] = FlyDatas[flyDataBaseIndex + noz].TimeMarker;

            // 變數初始設定
            flyNozzleId = 0;
            epcio.SetSpeed(EServoSpeed.UltraHigh);

            // 飛行前Z軸定位
            await epcio.MoveServoZToSafety();

            // X軸就定位
            epcio.MoveTo(positionX: servoX.MaxPosition);
            await epcio.WaitingForMotionStop(waitingServoX: true);

            // Z軸下降至吸嘴高度
            epcio.MoveTo(positionZ: nozzle.NozzleList[flyNozzleId].Position.Z);
            await epcio.WaitingForMotionStop(waitingServoZ: true);

            // 飛行至原點
            epcio.SetSpeed(servoXSpeed: _flySpeed);
            epcio.MoveTo(positionX: 0, checkSafetyZ: false);
            await epcio.WaitingForMotionStop(waitingServoX: true);

            // 記錄飛行啟始時間
            startTime = DateTime.Now;

            // 計時器啟動
            flyTimer.Reset();
            flyTimer.Start();

            try
            {
                // 作業是否取消
                if (_cts.Token.IsCancellationRequested)
                    _cts.Token.ThrowIfCancellationRequested();

                await Task.Run(() =>
                {
                    // 遍歷吸嘴
                    while (flyNozzleId < Nozzle.MAX_NOZZLE)
                    {
                        // 下支吸嘴的時間標記
                        double tm = FlyDatas[flyDataBaseIndex + flyNozzleId].TimeMarker;

                        // 等待時間標記觸發
                        while (flyTimer.Elapsed.TotalMilliseconds < tm)
                        { }

                        // TODO: 拍照，暫時以讀座標代替
                        long pulseX = 0, pulseY = 0, pulseZ = 0, pulseU = 0, pulseV = 0, pulseW = 0;

                        MCCL.MCC_GetPulsePos(ref pulseX, ref pulseY, ref pulseZ, ref pulseU, ref pulseV, ref pulseW, epcio.ServoX.Group);
                        FlyDatas[flyDataBaseIndex + flyNozzleId].NewX
                            = FlyDatas[flyDataBaseIndex + flyNozzleId].NozzleX - epcio.PulseToPosition(EServoId.X, pulseX);

                        MCCL.MCC_GetPulsePos(ref pulseX, ref pulseY, ref pulseZ, ref pulseU, ref pulseV, ref pulseW, epcio.ServoY.Group);
                        FlyDatas[flyDataBaseIndex + flyNozzleId].NewY
                            = FlyDatas[flyDataBaseIndex + flyNozzleId].NozzleY - epcio.PulseToPosition(EServoId.Y, pulseX);

                        // 下一支吸嘴
                        ++flyNozzleId;
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }

            flyTimer.Stop();
        }
    }
}
