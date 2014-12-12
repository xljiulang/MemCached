using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemCachedLib.Request
{
    /// <summary>
    /// 删除缓存命令
    /// <remarks>MUST NOT have extras.MUST have key.MUST NOT have value.</remarks>
    /// </summary>
    internal class DeleteRequest : RequestHeader
    {
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="key">键值</param>
        public DeleteRequest(string key)
        {
            this.Key = Encoding.ASCII.GetBytes(key);
        }

        /// <summary>
        /// 操作指令
        /// </summary>
        protected override OpCodes OpCode
        {
            get { return OpCodes.Delete; }
        }
    }
}
