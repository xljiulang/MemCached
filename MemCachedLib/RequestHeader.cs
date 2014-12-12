using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemCachedLib
{
    /// <summary>
    /// 请求指令基础类   
    /// </summary>
    internal abstract class RequestHeader
    {
        /// <summary>
        /// 指令码(必须)
        /// </summary>
        protected abstract OpCodes OpCode { get; }
        /// <summary>
        /// 键
        /// </summary>
        protected byte[] Key;
        /// <summary>
        /// 版本号验证值
        /// </summary>
        protected long CAS = 0L;
        /// <summary>
        /// 标记
        /// </summary>
        protected int? Flags;
        /// <summary>
        /// 过期时间
        /// </summary>
        protected int? Expiry;
        /// <summary>
        /// 值
        /// </summary>
        protected byte[] Value;

        /// <summary>
        /// 转换为数据包
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            // 额外数据长度
            var extraLength = 0;
            if (Flags.HasValue)
            {
                extraLength = extraLength + 4;
            }
            if (Expiry.HasValue)
            {
                extraLength = extraLength + 4;
            }

            // 键长度
            var keyLength = this.Key == null ? 0 : this.Key.Length;
            // 值长度
            var valueLenth = this.Value == null ? 0 : this.Value.Length;
            // 数据包长度
            var totalBodyLength = extraLength + keyLength + valueLenth;

            #region 头部数据
            var packet = new byte[24 + totalBodyLength];
            packet[0] = 0x80;
            packet[1] = (byte)this.OpCode;
            if (keyLength > 0)
            {
                var keyLenBytes = BitConverter.GetBytes(keyLength);
                packet[2] = keyLenBytes[1];
                packet[3] = keyLenBytes[0];
            }
            packet[4] = (byte)extraLength;
            #endregion

            // 设置数据总长度
            if (totalBodyLength > 0)
            {
                var bodyLenBytes = BitConverter.GetBytes(totalBodyLength);
                packet[8] = bodyLenBytes[3];
                packet[9] = bodyLenBytes[2];
                packet[10] = bodyLenBytes[1];
                packet[11] = bodyLenBytes[0];
            }

            // 设置版本号验证值
            if (this.CAS > 0)
            {
                var casBytes = BitConverter.GetBytes(this.CAS).Reverse().ToArray();
                Array.Copy(casBytes, 0, packet, 16, 8);
            }

            // 设置额外数据
            if (extraLength == 4)
            {
                var extra = this.Expiry.HasValue ? this.Expiry.Value : this.Flags.Value;
                var extraBytes = BitConverter.GetBytes(extra);
                packet[24] = extraBytes[3];
                packet[25] = extraBytes[2];
                packet[26] = extraBytes[1];
                packet[27] = extraBytes[0];
            }
            else if (extraLength == 8)
            {
                var flagsBytes = BitConverter.GetBytes(this.Flags.Value);
                var expiryBytes = BitConverter.GetBytes(this.Expiry.Value);
                packet[24] = flagsBytes[3];
                packet[25] = flagsBytes[2];
                packet[26] = flagsBytes[1];
                packet[27] = flagsBytes[0];
                packet[28] = expiryBytes[3];
                packet[29] = expiryBytes[2];
                packet[30] = expiryBytes[1];
                packet[31] = expiryBytes[0];
            }

            // 设置键数据
            if (keyLength > 0)
            {
                Array.Copy(this.Key, 0, packet, 24 + extraLength, keyLength);
            }

            // 设置值数据
            if (valueLenth > 0)
            {
                Array.Copy(this.Value, 0, packet, 24 + extraLength + keyLength, valueLenth);
            }

            return packet;
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.OpCode.ToString();
        }
    }
}
