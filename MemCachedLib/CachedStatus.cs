using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib
{
    /// <summary>
    /// 操作结果状态
    /// </summary>
    public enum CachedStatus
    {
        /// <summary>
        /// 无任何错误
        /// </summary>
        No_Error = 0x0000,
        /// <summary>
        /// 指定的键不存在
        /// </summary>
        Key_Not_Found = 0x0001,
        /// <summary>
        /// 指定的键已存在
        /// </summary>
        Key_Exists = 0x0002,
        /// <summary>
        /// 保存的实体过大
        /// </summary>
        Value_Too_Large = 0x0003,
        /// <summary>
        /// 参数无效
        /// </summary>
        Invalid_Arguments = 0x0004,
        /// <summary>
        /// 实体不被保存
        /// </summary>
        Item_Not_Stored = 0x0005,
        /// <summary>
        /// 自加或自减的对象不是数字类型
        /// </summary>
        IncrOrDecr_On_Non_Numeric_Value = 0x0006,
        /// <summary>
        /// Vbucket属在其它服务器
        /// </summary>
        The_Vbucket_Belongs_To_Another_Server = 0x0007,
        /// <summary>
        /// 身份验证错误
        /// </summary>
        Authentication_Error = 0x0008,
        /// <summary>
        /// 身份验证进行中
        /// </summary>
        Authentication_Continue = 0x0009,
        /// <summary>
        /// 未知指令
        /// </summary>
        Unknow_Command = 0x0081,
        /// <summary>
        /// 服务器内存溢出
        /// </summary>
        Out_Of_Memory = 0x0082,
        /// <summary>
        /// 不支持的操作
        /// </summary>
        Not_Supported = 0x0083,
        /// <summary>
        /// 服务器内部错误
        /// </summary>
        Internal_Error = 0x0084,
        /// <summary>
        /// 服务器繁忙
        /// </summary>
        Busy = 0x0085,
        /// <summary>
        /// 暂时性操作失败
        /// </summary>
        Temporary_Failure = 0x0086
    }
}
