using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib
{
    /// <summary>
    /// Stat指令的选项枚举
    /// <remarks>二进制协议支持的stat选项不如文本协议的丰富</remarks>
    /// </summary>
    public enum StatItems
    {
        /// <summary>
        /// 表示空选项
        /// </summary>
        nothing,
        /// <summary>
        /// items选项
        /// </summary>
        items,
        /// <summary>
        /// slabs选项
        /// </summary>
        slabs,
        /// <summary>
        /// sizes选项
        /// </summary>
        sizes
    }
}
