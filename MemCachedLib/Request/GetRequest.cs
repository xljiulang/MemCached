using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemCachedLib.Request
{
    /// <summary>
    /// 获取缓存命令
    /// </summary>
    internal class GetRequest : RequestHeader
    {
        /// <summary>
        /// 构造器
        /// <remarks>MUST NOT have extras.MUST have key.MUST NOT have value.</remarks>
        /// </summary>
        /// <param name="key">键</param>
        public GetRequest(string key)
        {
            this.Key = Encoding.ASCII.GetBytes(key);
        }

        /// <summary>
        /// 操作指令
        /// </summary>
        protected override OpCodes OpCode
        {
            get { return OpCodes.Get; }
        }
    }
}
