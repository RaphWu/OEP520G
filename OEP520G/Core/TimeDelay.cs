//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace OEP520G.Core
//{
//    /// <summary>
//    /// 延遲時間
//    /// </summary>
//    /// <see cref="http://slashlook.com/archive2016/20160201.html"/>
//    public static class TimeDelay
//    {
//        /// <summary>
//        /// 時間延遲
//        /// </summary>
//        /// <remarks>使用語法: await Delay(時間);</remarks>
//        /// <param name="ms">要延遲的時間，單位ms</param>
//        public static System.Threading.Tasks.Task Delay(int ms)
//            => DelayAsync(ms);

//        /// <summary>
//        /// 時間延遲
//        /// </summary>
//        /// <remarks>使用語法: await Delay(時間);</remarks>
//        /// <param name="ms">要延遲的時間，單位ms</param>
//        /// <param name="callBack">延遲時間到達後，要呼叫的函式</param>
//        public static System.Threading.Tasks.Task Delay(int ms, object callBack)
//            => DelayAsync(ms, callBack);

//        /***** 延遲實體部分 *****/

//        private static async System.Threading.Tasks.Task DelayAsync(int ms)
//            => await System.Threading.Tasks.Task.Delay(ms);

//        private static async System.Threading.Tasks.Task DelayAsync(int ms, object callBack)
//            => await System.Threading.Tasks.Task.Delay(ms).ContinueWith(_ => callBack);
//    }
//}
