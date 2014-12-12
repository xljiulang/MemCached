using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib
{
    /// <summary>
    /// 接收的一个完整响应数据包   
    /// </summary>
    internal class ResponseHeader
    {
        /// <summary>
        /// 数据
        /// </summary>
        private ByteBuilder builder;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="binary">接收到的数据</param>
        public ResponseHeader(byte[] binary)
        {
            this.builder = new ByteBuilder(binary);
        }

        /// <summary>
        /// 指令
        /// </summary>
        public OpCodes OpCode
        {
            get
            {
                return (OpCodes)this.builder.ToByte(1);
            }
        }
        /// <summary>
        /// 键长
        /// </summary>
        public short KeyLength
        {
            get
            {
                return this.builder.ToInt16(2);
            }
        }
        /// <summary>
        /// 额外数据长
        /// </summary>
        public byte ExtraLength
        {
            get
            {
                return this.builder.ToByte(4);
            }
        }
        /// <summary>
        /// 操作状态
        /// </summary>
        public OprationStatus Status
        {
            get
            {
                return (OprationStatus)this.builder.ToInt16(6);
            }
        }
        /// <summary>
        /// 数据体长度
        /// </summary>
        public int TotalBody
        {
            get
            {
                return this.builder.ToInt32(8);
            }
        }
        /// <summary>
        /// 版本号验证值
        /// </summary>
        public long CAS
        {
            get
            {
                return this.builder.ToInt64(16);
            }
        }

        public byte[] Key
        {
            get
            {
                var index = 24 + this.ExtraLength;
                return this.builder.ToArray(index, this.KeyLength);
            }
        }

        /// <summary>
        /// 有效数据
        /// </summary>
        public byte[] Value
        {
            get
            {
                var index = 24 + this.ExtraLength + this.KeyLength;
                return this.builder.ToArray(index);
            }
        }
    }
}
