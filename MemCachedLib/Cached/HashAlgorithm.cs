using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MemCachedLib.Cached
{
    /// <summary>
    /// 哈希值算法
    /// </summary>
    internal class HashAlgorithm
    {
        private const uint m = 0x5bd1e995;
        private const int r = 24;

        /// <summary>
        /// byte转换为uint
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct Byte2Uint
        {
            [FieldOffset(0)]
            public byte[] Bytes;

            [FieldOffset(0)]
            public uint[] UInts;
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <param name="key">文本</param>
        /// <returns></returns>
        public static int GetHashCode(string key)
        {
            return (int)HashAlgorithm.GetHashCode(Encoding.ASCII.GetBytes(key));
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <param name="bytes">二进制数据</param>
        /// <returns></returns>
        public static uint GetHashCode(byte[] bytes)
        {
            return HashAlgorithm.GetHashCode(bytes, 0xc58f1a7b);
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        /// <param name="bytes">二进制数据</param>
        /// <param name="seed">种子</param>
        /// <returns></returns>
        public static uint GetHashCode(byte[] bytes, uint seed)
        {
            int length = bytes.Length;
            if (length == 0)
            {
                return 0;
            }
            uint h = seed ^ (uint)length;
            int currentIndex = 0;

            uint[] hackArray = new Byte2Uint { Bytes = bytes }.UInts;
            while (length >= 4)
            {
                uint k = hackArray[currentIndex++];
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                length -= 4;
            }
            currentIndex *= 4;
            switch (length)
            {
                case 3:
                    h ^= (ushort)(bytes[currentIndex++] | bytes[currentIndex++] << 8);
                    h ^= (uint)bytes[currentIndex] << 16;
                    h *= m;
                    break;
                case 2:
                    h ^= (ushort)(bytes[currentIndex++] | bytes[currentIndex] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= bytes[currentIndex];
                    h *= m;
                    break;
                default:
                    break;
            }


            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}
