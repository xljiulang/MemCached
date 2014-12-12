using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib
{
    /// <summary>
    /// 缓存查询结果
    /// </summary>
    public class CachedReault<T>
    {
        /// <summary>
        /// 获取是否成功状态
        /// </summary>
        public OprationStatus Status { get; private set; }

        /// <summary>
        /// 获取版本号验证值
        /// </summary>
        public long CAS { get; private set; }

        /// <summary>
        /// 获取操作结果
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// 缓存查询结果
        /// </summary>
        /// <param name="status">状态</param>
        /// <param name="cas">数据版本号</param>
        /// <param name="value">值</param>
        internal CachedReault(OprationStatus status, long cas, T value)
        {
            this.Status = status;
            this.Value = value;
            this.CAS = cas;
        }

        /// <summary>
        /// 文本显示方式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Value == null ? null : this.Value.ToString();
        }
    }
}
