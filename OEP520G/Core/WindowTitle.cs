using Prism.Events;

namespace OEP520G.Core
{
    /********************
     * 主視窗Title標題
     * 1. 欲顯示在標題列的訊息，使用Prism的Event Aggregator功能傳入，
     *    Aggregator名稱為: WindowTitleSetter
     * 2. 傳入資料須使用物件: WindowTitleData
     *    Key為識別鍵值，傳入訊息的Key不同時，會附加在標題裡；若Key值相同則會覆蓋
     *    Key開頭可標示2位數字表示排列順序:
     *      00: 主系統資訊
     *      01~10: 顯示版本訊息
     *        01: 視覺版本
     *        02: EPCIO版本
     *      11~20: 顯示系統資訊
     *        11: 機台ID
     *        12: 作業品種
     *      21~: 其他訊息
     *    Title則為要顯示在標題的訊息
     * 3. 傳送訊息方法為:
     *    Oep520Core.EA.GetEvent<WindowTitleSetter>().Publish(new WindowTitleData()
     *    {
     *        Key = "Key",
     *        Title = "Title"
     *    });
     *    
     *    或
     *    
     *    WindowTitleData wtd = new WindowTitleData();
     *    wtd.Key = "Key";
     *    wtd.Title = "Title";
     *    Oep520Core.EA.GetEvent<WindowTitleSetter>().Publish(wtd);
     *******************/
    /// <summary>
    /// 主視窗Title標題資料
    /// </summary>
    public class WindowTitleData
    {
        public string Key { get; set; }
        public string Title { get; set; }
    }
}
