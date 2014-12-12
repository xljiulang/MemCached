using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib.Request
{
    /// <summary>
    /// 存储指令
    /// <remarks>MUST have extras.MUST have key.MAY have value.</remarks>
    /// </summary>
    internal class StoreRequest : RequestHeader
    {
        /// <summary>
        /// 存储方式
        /// </summary>
        private OpCodes opCode;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="code">存储方式</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expiry">时间</param>
        /// <param name="cas">版本号验证值(0表示忽略)</param>
        public StoreRequest(OpCodes code, string key, byte[] value, TimeSpan expiry, long cas = 0)
        {
            this.opCode = code;
            this.Key = Encoding.ASCII.GetBytes(key);
            this.Value = value;
            this.Expiry = (int)expiry.TotalSeconds;
            this.Flags = 0;
            this.CAS = cas;
        }

        /// <summary>
        /// 操作指令
        /// </summary>
        protected override OpCodes OpCode
        {
            get { return this.opCode; }
        }
    }
}
