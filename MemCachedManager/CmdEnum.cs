using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedManager
{
    /// <summary>
    /// 指令枚举
    /// </summary>
    public enum CmdEnum
    {
        /// <summary>
        /// get指令
        /// </summary>
        [Cmd("get [key]", "获取指定键的数据，[key]：键值")]
        get,

        /// <summary>
        /// set指令
        /// </summary>
        [Cmd("set [key] [value] [expity]", "添加或更新指定键的数据，[key]：键值 [value]：值 [expity]：过期时间(秒)")]
        set,

        /// <summary>
        /// add指令
        /// </summary>
        [Cmd("add [key] [value] [expity]", "添加指定键的数据，[key]：键值 [value]：值 [expity]：过期时间(秒)")]
        add,

        /// <summary>
        /// replace指令
        /// </summary>
        [Cmd("replace [key] [value] [expity]", "更新指定键的数据，[key]：键值 [value]：值 [expity]：过期时间(秒)")]
        replace,

        /// <summary>
        /// delete指令
        /// </summary>
        [Cmd("delete [key]", "删除指定键及相关数据，[key]：键值")]
        delete,

        /// <summary>
        /// flush指令
        /// </summary>
        [Cmd("flush [expity]", "将所有记录过期时间重置，[expity]：过期时间(秒)")]
        flush
    }
}
