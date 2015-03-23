using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.SessionState;

namespace MemCachedLib.Session
{
    /// <summary>
    /// Session数据项目
    /// </summary>
    [Serializable]
    internal class SessionItem
    {
        /// <summary>
        /// 初始化标识
        /// </summary>
        public SessionStateActions ActionFlag { get; set; }

        /// <summary>
        /// 锁ID
        /// </summary>
        public int LockId { get; set; }

        /// <summary>
        /// 锁时间
        /// </summary>
        public DateTime LockTime { get; set; }

        /// <summary>
        /// 记录过期时间(分)
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// 是否已锁
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Session有效数据
        /// </summary>
        public byte[] Binary { get; set; }
    }
}
