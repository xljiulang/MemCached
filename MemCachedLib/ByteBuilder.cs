using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MemCachedLib
{
    /// <summary>
    /// 可变长byte集合
    /// 读取数据后剩余数据非自动前移
    /// 非线程安全类型
    /// GetInt等操作为高位在前
    /// </summary>
    internal class ByteBuilder
    {
        /// <summary>
        /// 原始数据
        /// </summary>
        private byte[] binary;

        /// <summary>
        /// 首次设置的默认容量
        /// </summary>
        private int capacity;

        /// <summary>
        /// 当前容量
        /// </summary>
        private int Capacity;

        /// <summary>
        /// 获取当前的有效数量长度
        /// 随着Read操作而减少
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 当前数据的指针位置
        /// 初始为0
        /// 随着Read操作而增加
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// 可变长byte集合
        /// </summary>
        /// <param name="capacity">容量[乘2倍数增长]</param>
        public ByteBuilder(int capacity)
        {
            this.capacity = capacity;
            this.Capacity = capacity;
            this.binary = new byte[capacity];
        }

        /// <summary>
        /// 可变长byte集合
        /// </summary>
        /// <param name="binary">初始数据</param>
        public ByteBuilder(byte[] binary)
        {
            this.capacity = binary.Length;
            this.Capacity = binary.Length;
            this.Length = binary.Length;
            this.binary = binary;
        }

        /// <summary>
        /// 将指定数据源的数据添加到集合
        /// </summary>
        /// <param name="srcArray">数据源</param>
        /// <returns></returns>
        public void Add(byte[] srcArray)
        {
            this.Add(srcArray, 0, srcArray.Length);
        }

        /// <summary>
        /// 将指定数据源的数据添加到集合
        /// </summary>
        /// <param name="srcArray">数据源</param>
        /// <param name="index">数据源的起始位置</param>
        /// <param name="length">复制的长度</param>
        public void Add(byte[] srcArray, int index, int length)
        {
            if (srcArray == null)
            {
                return;
            }

            int newCapacity = this.Position + this.Length + length;
            if (newCapacity > this.Capacity)
            {
                while (newCapacity > this.Capacity)
                {
                    this.Capacity = this.Capacity * 2;
                }

                byte[] newBuffer = new byte[this.Capacity];
                this.binary.CopyTo(newBuffer, 0);
                this.binary = newBuffer;
            }

            Array.Copy(srcArray, index, this.binary, this.Position + this.Length, length);
            this.Length = this.Length + length;
        }



        /// <summary>
        /// 从指针位置清除指定长度的字节
        /// </summary>
        /// <param name="length">长度，要小于Length</param>       
        public void RemoveRange(int length)
        {
            this.Position = this.Position + length;
            this.Length = this.Length - length;
        }

        /// <summary>
        /// 从相对指针偏移地址清除指定长度的字节
        /// </summary>        
        /// <param name="length">长度，要小于Length</param>
        /// <param name="offset">偏移量</param>
        public void RemoveRange(int length, int offset)
        {
            if (offset == 0)
            {
                this.RemoveRange(length);
                return;
            }
            int destIndex = this.Position + offset;
            int srcIndex = destIndex + length;
            length = this.Length - length - offset;
            Array.Copy(this.binary, srcIndex, this.binary, destIndex, length);
            this.Length = this.Length - length;
        }

        /// <summary>
        /// 从指针位置将数据复制到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">复制长度,要小于Length</param>       
        public void CopyTo(byte[] destArray, int index, int length)
        {
            this.CopyTo(destArray, index, length, 0);
        }

        /// <summary>
        /// 将数据复制到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">复制长度</param>
        /// <param name="offset">相对指针的偏移量</param>
        public void CopyTo(byte[] destArray, int index, int length, int offset)
        {
            var srcIndex = this.Position + offset;
            Array.Copy(this.binary, srcIndex, destArray, index, length);
        }


        /// <summary>
        /// 从指针位置将数据剪切到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">剪切长度,要小于Length</param>      
        public void CutTo(byte[] destArray, int index, int length)
        {
            this.CopyTo(destArray, index, length);
            this.RemoveRange(length);
        }

        /// <summary>
        /// 将数据剪切到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">剪切长度</param>
        /// <param name="offset">相对指针的偏移量</param>
        public void CutTo(byte[] destArray, int index, int length, int offset)
        {
            this.CopyTo(destArray, index, length, offset);
            this.RemoveRange(length, offset);
        }

        /// <summary>
        /// 读取指定位置一个字节
        /// </summary>
        /// <param name="offset">相对指针的偏移索引</param>
        /// <returns></returns>       
        public byte ToByte(int offset)
        {
            var index = offset + this.Position;
            return this.binary[index];
        }

        /// <summary>
        /// 读取指定位置4个字节，返回其Int16表示类型
        /// </summary>
        /// <param name="offset">相对指针的偏移索引</param>
        /// <returns></returns>      
        public Int16 ToInt16(int offset)
        {
            var index = offset + this.Position;
            var value = BitConverter.ToInt16(this.binary, index);
            return IPAddress.HostToNetworkOrder(value);
        }

        /// <summary>
        /// 读取指定位置4个字节，返回其Int32表示类型
        /// </summary>
        /// <param name="offset">相对指针的偏移索引</param>
        /// <returns></returns>       
        public int ToInt32(int offset)
        {
            var index = offset + this.Position;
            var value = BitConverter.ToInt32(this.binary, index);
            return IPAddress.HostToNetworkOrder(value);
        }

        /// <summary>
        /// 读取指定位置8个字节，返回其Int64表示类型
        /// </summary>
        /// <param name="offset">相对指针的偏移索引</param>
        /// <returns></returns>      
        public long ToInt64(int offset)
        {
            var index = offset + this.Position;
            var value = BitConverter.ToInt64(this.binary, index);
            return IPAddress.HostToNetworkOrder(value);
        }


        /// <summary>
        /// 返回有效数据的数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return this.ToArray(0);
        }

        /// <summary>
        /// 返回有效数据的数组
        /// </summary>
        /// <param name="offset">相对指针的偏移索引</param>
        /// <returns></returns>        
        public byte[] ToArray(int offset)
        {
            var length = this.Length - offset;
            return this.ToArray(offset, length);
        }

        /// <summary>
        /// 返回有效数据的数组
        /// </summary>
        /// <param name="offset">相对指针的偏移索引</param>
        /// <param name="length">获取长度,要小于Length</param>
        /// <returns></returns>      
        public byte[] ToArray(int offset, int length)
        {
            var index = offset + this.Position;
            byte[] buffer = new byte[length];
            Array.Copy(this.binary, index, buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// 从指针位置读取并清除指定长度的字节
        /// </summary>
        /// <param name="length">长度,要小于Length</param>
        /// <returns></returns>       
        public byte[] ReadRange(int length)
        {
            byte[] buffer = new byte[length];
            this.CutTo(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// 从相对指针位置读取并清除指定长度的字节
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="offset">指针偏移量</param>
        /// <returns></returns>       
        public byte[] ReadRange(int length, int offset)
        {
            byte[] buffer = new byte[length];
            this.CutTo(buffer, 0, length, offset);
            return buffer;
        }

        /// <summary>
        /// 重置集合指针和有效数据长度为0 
        /// 容量不受到影响
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            this.Position = 0;
            this.Length = 0;
        }

        /// <summary>
        /// 重置集合为初始状态
        /// </summary>
        public void ReSet()
        {
            this.Position = 0;
            this.Length = 0;
            this.Capacity = this.capacity;
            this.binary = new byte[this.capacity];
        }

        /// <summary>
        /// 字节数
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Length.ToString();
        }
    }
}
