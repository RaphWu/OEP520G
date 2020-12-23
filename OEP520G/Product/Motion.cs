using CSharpCore;
using CSharpCore.FileSystem;
using OEP520G.Parameter;

namespace OEP520G.Product
{
    public class MotionObj
    {
        // .ini存檔時檔案名稱及Section名稱
        private readonly string FILE_NAME = FileList.INI_MACHINE;

        /// <summary>
        /// 建構函式
        /// </summary>
        public MotionObj()
        {
            ReadParameter();
            //SaveParameter();
        }

        /// <summary>
        /// 從參數檔讀取參數
        /// [屬性名稱] = [Type].Parse(iniFile.ReadIniFile(SectionName, "[屬性名稱]", "[預設值]"));
        /// </summary>
        public void ReadParameter()
        {
            IniFile iniFile = new IniFile(FILE_NAME);

            // TODO
        }

        /// <summary>
        /// 將參數寫入參數檔
        /// iniFile.WriteIniFile(SectionName, "[屬性名稱]", [屬性值]));
        /// </summary>
        public void SaveParameter()
        {
            IniFile iniFile = new IniFile(FILE_NAME);

            // TODO
        }

        /********************
         * 屬性定義
         * *****************/
        // 機台辨識
        public string MachineId { get; set; }
        public string MachineName { get; set; }


    }
}
