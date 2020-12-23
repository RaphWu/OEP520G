using Prism.Commands;

namespace OEP520G.Core
{
    /********************
     * 共用命令 - 使用Prism的Composite Command定義全域共用函數
     * 註：這裡採用的是靜態物件的寫法，而不是比較好的相依性注入寫法
     *    原因是相依性注入寫法需在建構式傳入參數，影響單例物件的建立，目前還不確定解法
     *    
     * 1. 全域指令：存檔
     *    定義存檔命令，讓所有須做存檔作業的物件註冊
     *    執行時，會呼叫全部有註冊的物件各自執行存檔動作
     *    在須要註冊的物件中(通常是ViewModel)，使用以下方式：
     *    
     *    public DelegateCommand SaveCommandDelegate { get; private set; }
     *    public MyViewModel()
     *    {
     *        SaveCommandDelegate = new DelegateCommand(MySaveFunction);
     *        ApplicationCommands.WriteCommand.RegisterCommand(SaveCommandDelegate);
     *    }
     *    private void MySaveFunction()
     *    {
     *        ...
     *    }
     *    
     *    參數存取流程
     *       a. View初始化時，各自的ViewModel呼叫Module的ReadParameter()自參數檔(磁碟檔案)讀取參數至Module
     *          並同時做一次儲存動作，防止無參數檔或無該參數狀況，確保參數檔為正確狀態
     *       b. 當View被顯示時，ViewModel的GetParameter()由Module取得資料，View則自動取出資料做顯示
     *       c. 當參數在View被修改時，修改的資料只暫留在ViewModel中，還不須要存回Module
     *          若此時切換頁面，修改的資料應該被放棄；頁面切換回來時，ViewModel應由Module重新取得未修改前的資料
     *       d. 當按下儲存鍵時，ViewModel的PutParameter()將修改的資料存回Module
     *          修改的資料再由Module的WriteParameter()存至參數檔(磁碟檔案)
     *******************/
    /// <summary>
    /// CompositeCommand介面
    /// </summary>
    /// <example>參閱: https://prismlibrary.com/docs/composite-commands.html </example>
    public static class ApplicationCommands
    {
        /// <summary>
        /// 參數存檔頂層命令
        /// </summary>
        public static CompositeCommand WriteCommand = new CompositeCommand();
    }
}
