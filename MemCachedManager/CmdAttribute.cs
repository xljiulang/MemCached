using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedManager
{
    /// <summary>
    /// 命令特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CmdAttribute : Attribute
    {
        /// <summary>
        /// 用法代码
        /// </summary>
        public string CodeText { get; set; }
        /// <summary>
        /// 参数和功能描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 命令特性
        /// </summary>
        /// <param name="codeText">用法代码</param>
        /// <param name="desription">参数和功能描述</param>
        public CmdAttribute(string codeText, string desription)
        {
            this.CodeText = codeText;
            this.Description = desription;
        }
    }
}
