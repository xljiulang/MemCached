using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib.Request
{
    /// <summary>
    /// Touch指令
    /// <remarks>MUST have extras.MUST have key.MUST NOT have value.</remarks>
    /// </summary>
    internal class TouchReqeuest : RequestHeader
    {
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expiry">过期时间</param>
        public TouchReqeuest(string key, TimeSpan expiry)
        {
            this.Key = Encoding.ASCII.GetBytes(key);
            this.Expiry = (int)expiry.TotalSeconds;
        }

        /// <summary>
        /// 操作指令
        /// </summary>
        protected override OpCodes OpCode
        {
            get { return OpCodes.Touch; }
        }
    }
}
