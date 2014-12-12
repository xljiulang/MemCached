using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib.Request
{
    /// <summary>
    /// Stat指令
    /// <remarks>MUST NOT have extras.MAY have key.MUST NOT have value.</remarks>
    /// </summary>
    internal class StatRequest : RequestHeader
    {
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="key">参数</param>
        public StatRequest(StatItems key = StatItems.nothing)
        {
            if (key != StatItems.nothing)
            {
                this.Key = Encoding.ASCII.GetBytes(key.ToString());
            }
        }

        /// <summary>
        /// 操作指令
        /// </summary>
        protected override OpCodes OpCode
        {
            get { return OpCodes.Stat; }
        }
    }
}
