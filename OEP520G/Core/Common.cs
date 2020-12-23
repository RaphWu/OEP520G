using CSharpCore;
using CSharpCore.Logger;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

/********************
 * OEP520G共用核心物件
 *******************/
namespace OEP520G.Core
{
    public static class Common
    {
        /// <summary>
        /// Logger
        /// </summary>
        public readonly static Logger log = Logger.Instance;

        /********************
         * 提供一些無法做建構式相依注入的物件，一個共用取得Prism功能的全域變數
         * 初始化在MainWindowViewModel.cs的建構式執行
         *******************/
        public static IEventAggregator EA { get; set; }
        public static IDialogService DS { get; set; }

        ///// <summary>
        ///// 深層複製物件
        ///// </summary>
        ///// <typeparam name="T">物件型別</typeparam>
        ///// <param name="source">被複製物件(必需標記 [Serializable] 標籤)</param>
        ///// <returns>複製的物件</returns>
        ///// <see cref="http://aminggo.idv.tw/blog/?p=543"/>
        ///// <see cref="https://dotblogs.com.tw/wasichris/2015/12/03/152540"/>
        //public static T DeepClone<T>(T source)
        //{
        //    if (!typeof(T).IsSerializable)
        //        throw new ArgumentException("The type must be serializable.", nameof(source));

        //    if (source != null)
        //    {
        //        using (MemoryStream stream = new MemoryStream())
        //        {
        //            var formatter = new BinaryFormatter();
        //            formatter.Serialize(stream, source);
        //            stream.Seek(0, SeekOrigin.Begin);
        //            return (T)formatter.Deserialize(stream);
        //        }
        //    }

        //    return default;
        //}
    }
}