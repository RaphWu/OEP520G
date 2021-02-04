/****************************
 * 測高作業
 * 
 * 此物件僅負責Z軸測高，作業前須先將物件移至測高位置、吸嘴伸出。
 * 測高到位後，測高結果須讀取變數: Result
 ***************************/
using EPCIO;
using EPCIO.IoSystem;
using OEP520G.Parameter;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// TODO: 無裝吸嘴檢測

namespace OEP520G.Functions
{
    /// <summary>
    /// 測高作業
    /// </summary>
    public class MeasureHeight
    {
        private readonly Machine machine = Machine.Instance;
        private readonly Epcio epcio = Epcio.Instance;

        // 測高參數(見Start()說明)
        private List<double> _resolution;
        private RemoteIo _sensor;
        private bool _negative;
        private CancellationTokenSource measureCts;

        /// <summary>
        /// 測高結果
        /// </summary>
        public double Result { get; private set; }

        /// <summary>
        /// 測高作業是否正常結束
        /// </summary>
        public bool IsFinished { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public MeasureHeight()
        {
        }

        /// <summary>
        /// 測高作業啟動
        /// </summary>
        /// <param name="resolution">下降步進解析度</param>
        /// <param name="sensor">用來檢知下降到位的Sensor</param>
        /// <param name="negative">Sensor觸發是否須反相<br/>false: Sensor觸發時為有效<br/>true: Sensor未觸發時為有效</param>
        public void Start(List<double> resolution,
                          RemoteIo sensor,
                          CancellationTokenSource cts,
                          bool negative = false)
        {
            _resolution = resolution;
            _sensor = sensor;
            _negative = negative;
            measureCts = cts;

            StepMotion();
        }

        ///// <summary>
        ///// 測高作業中斷
        ///// </summary>
        //public void Cancel()
        //{
        //    if (measureCts != null && !measureCts.IsCancellationRequested)
        //        measureCts.Cancel();
        //}

        /// <summary>
        /// 測高作業
        /// </summary>
        private void StepMotion()
        {
            Task.Run(async () =>
            {
                bool firstStep = true; // 第一步不做上升
                const int WAIT_FOR_STABLE = 300; // 下降動作完等待穩定時間

                IsFinished = false;

                try
                {
                    foreach (double res in _resolution)
                    {
                        // 速度
                        int speed;
                        if (res >= 0.1)
                            speed = epcio.ServoZ.HighSpeedRate;
                        else
                            speed = epcio.ServoZ.MiddleSpeedRate;

                        if (!firstStep)
                        {
                            // 升至未觸及Sensor
                            while (_sensor.Value ^ _negative)
                            {
                                await epcio.JogTo(EServoId.Z, res * -1, speed);
                                await epcio.WaitingForMotionStop(waitingServoZ: true);
                                await Task.Delay(WAIT_FOR_STABLE);
                            }
                        }

                        // 若未觸及Sensor，持續步進下降
                        while (!_sensor.Value ^ _negative)
                        {
                            // 降一段
                            await epcio.JogTo(EServoId.Z, res, speed);

                            // 確認軸停止並Delay
                            await epcio.WaitingForMotionStop(waitingServoZ: true);
                            await Task.Delay(WAIT_FOR_STABLE);

                            // 作業是否取消
                            if (measureCts.Token.IsCancellationRequested)
                                measureCts.Token.ThrowIfCancellationRequested();
                        }

                        // 第一步結束
                        if (firstStep)
                            firstStep = false;
                    }

                    IsFinished = true;
                }
                catch (OperationCanceledException e)
                {
                    if (e.CancellationToken == measureCts.Token)
                        IsFinished = false;
                }
                finally
                {
                    Result = IsFinished ? epcio.ServoZ.GetCurrentPosition() - machine.DatumPoint1.Position.Z : 0;
                }
            });
        }
    }
}
