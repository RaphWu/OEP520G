using Dapper;

// https://dotblogs.com.tw/supershowwei/2019/08/12/232213
namespace OEP520G.Core
{
    /// <summary>
    /// Dapper 的 string 擴充方法，讓 DbString 更方便使用
    /// 1.必須有安裝Dapper
    /// 2.藉由 IsAnsi、IsFixedLength 這兩個屬性組合出 varchar、nvarchar、char、nchar 四種 SQL Data Type
    /// </summary>
    public static class DapperHelper
    {
        /// <summary>
        /// Length of the string is default 4000
        /// </summary>
        public static DbString ToVarchar(this string me)
            => new DbString { Value = me, IsAnsi = true };

        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public static DbString ToVarchar(this string me, int length)
            => new DbString { Value = me, Length = length, IsAnsi = true };

        /// <summary>
        /// Length of the string is default 4000
        /// </summary>
        public static DbString ToChar(this string me)
            => new DbString { Value = me, IsAnsi = true, IsFixedLength = true };

        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public static DbString ToChar(this string me, int length)
            => new DbString { Value = me, Length = length, IsAnsi = true, IsFixedLength = true };

        /// <summary>
        /// Length of the string is default 4000
        /// </summary>
        public static DbString ToNVarchar(this string me)
            => new DbString { Value = me };

        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public static DbString ToNVarchar(this string me, int length)
            => new DbString { Value = me, Length = length };

        /// <summary>
        /// Length of the string is default 4000
        /// </summary>
        public static DbString ToNChar(this string me)
            => new DbString { Value = me, IsFixedLength = true };

        /// <summary>
        /// Length of the string -1 for max
        /// </summary>
        public static DbString ToNChar(this string me, int length)
            => new DbString { Value = me, Length = length, IsFixedLength = true };
    }
}
