using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib.Request
{
    /// <summary>
    /// Version指令
    /// <remarks>MUST NOT have extras.MUST NOT have key.MUST NOT have value.</remarks>
    /// </summary>
    internal class VersionRequest : RequestHeader
    {
        /// <summary>
        /// 操作指令
        /// </summary>
        protected override OpCodes OpCode
        {
            get { return OpCodes.Version; }
        }
    }
}
