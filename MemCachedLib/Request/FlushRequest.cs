using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib.Request
{
    /// <summary>
    /// Flush指令
    /// <remarks>MAY have extras.MUST NOT have key.MUST NOT have value.</remarks>
    /// </summary>
    internal class FlushRequest : RequestHeader
    {
        /// <summary>
        /// Flush
        /// </summary>
        /// <param name="expiry">过期时间</param>
        public FlushRequest(TimeSpan expiry)
        {
            this.Expiry = (int)expiry.TotalSeconds;
        }

        /// <summary>
        /// 操作指令
        /// </summary>
        protected override OpCodes OpCode
        {
            get { return OpCodes.Flush; }
        }
    }
}
