using Prism.Events;
using Prism.Services.Dialogs;

namespace OEP520G.Core
{
    /// <summary>
    /// 資料處理分類
    /// </summary>
    public enum ECrudInstruction
    {
        Create,
        Read,
        Update,
        Delete,
        Append,
        Modify,
        Copy,
        Rename,
        None
    }

    public class CRUD
    {
        /// <summary>
        /// 解析資料處理命令，將字串形式轉為 ECrudCommand 形式
        /// </summary>
        /// <param name="instruction">輸入的指令</param>
        /// <returns>EDataBaseCommand命令格式</returns>
        public static ECrudInstruction InstructionParse(string instruction)
        {
            return instruction switch
            {
                "Create" => ECrudInstruction.Create,
                "Read" => ECrudInstruction.Read,
                "Update" => ECrudInstruction.Update,
                "Delete" => ECrudInstruction.Delete,
                "Append" => ECrudInstruction.Append,
                "Modify" => ECrudInstruction.Modify,
                "Copy" => ECrudInstruction.Copy,
                "Rename" => ECrudInstruction.Rename,
                _ => ECrudInstruction.None,
            };
        }
    }

    /********************
     // 對話框
     *******************/
    /// <summary>
    /// CRUD資料傳回的事件聚合器
    /// </summary>
    public class CrudDialogReceiver : PubSubEvent<CrudDialogData> { }

    /// <summary>
    /// CRUD對話框的回傳資料
    /// </summary>
    public class CrudDialogData
    {
        public ButtonResult Result { get; set; }
        public ECrudInstruction Command { get; set; }

        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
    }
}
