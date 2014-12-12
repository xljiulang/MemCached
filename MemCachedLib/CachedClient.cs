using MemCachedLib.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


namespace MemCachedLib
{
    /// <summary>
    /// 缓存客户端
    /// </summary>
    internal class CachedClient : IDisposable
    {
        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 服务器地址
        /// </summary>
        private IPEndPoint ipEndPoint { get; set; }

        /// <summary>
        /// 提供序列化对象
        /// </summary>
        private BinaryFormatter serializer;

        /// <summary>
        /// 接收缓冲区
        /// </summary>
        private byte[] socketBuffer = new byte[1024 * 8];

        /// <summary>
        /// 接收数据对象
        /// </summary>
        private ByteBuilder byteBuilder = new ByteBuilder(1024 * 8);


        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="ipEndPoint">服务器IP</param>       
        public CachedClient(IPEndPoint ipEndPoint)
        {
            this.ipEndPoint = ipEndPoint;
            this.serializer = new BinaryFormatter();
        }

        /// <summary>
        /// 反序列化得到对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="binary">数据</param>
        /// <returns></returns>
        public T ToEntity<T>(byte[] binary)
        {
            if (binary == null || binary.Length == 0)
            {
                return default(T);
            }

            using (var stream = new MemoryStream())
            {
                try
                {
                    stream.Write(binary, 0, binary.Length);
                    stream.Position = 0;
                    return (T)serializer.UnsafeDeserialize(stream, null);
                }
                catch
                {
                    return default(T);
                }
            }
        }


        /// <summary>
        /// 将对象序列化
        /// </summary>
        /// <param name="value">实体</param>
        /// <returns></returns>
        public byte[] ToBinary(object value)
        {
            if (value == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                try
                {
                    this.serializer.Serialize(ms, value);
                    return ms.ToArray();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 发送指令
        /// 只取其中第一个响应包
        /// </summary>
        /// <param name="request">请求指令</param>
        /// <returns></returns>
        public ResponseHeader Send(RequestHeader request)
        {
            return this.Sends(request).FirstOrDefault();
        }

        /// <summary>
        /// 发送指令
        /// 接收多个响应包
        /// </summary>
        /// <param name="request">请求指令</param>
        /// <returns></returns>
        public IEnumerable<ResponseHeader> Sends(RequestHeader request)
        {           
            if (this.socket == null)
            {
                this.socket = new Socket(this.ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            if (this.socket.Connected == false)
            {
                this.socket.Connect(this.ipEndPoint);
            }

            // 将指令包解析发送
            this.socket.Send(request.ToByteArray());
            // 集合恢复
            this.byteBuilder.Clear();
            // 响应包集合
            var responseList = new List<ResponseHeader>();

            while (true)
            {
                // 接收数据
                int readByte = this.socket.Receive(this.socketBuffer);
                // 放到数据区
                this.byteBuilder.Add(this.socketBuffer, 0, readByte);
                // 包长
                int packetLength = 0;

                // 把接收到的数据(也许还不完整)拆分为包
                while (this.byteBuilder.Length >= 12 && (packetLength = this.byteBuilder.ToInt32(8) + 24) <= this.byteBuilder.Length)
                {
                    // 把数据包取出
                    var packetBytes = this.byteBuilder.ReadRange(packetLength);
                    var response = new ResponseHeader(packetBytes);
                    responseList.Add(response);

                    // 非stat操作都是只有一个响应包
                    // stat操作的最后一个包是空包，占24字节
                    // stat操作有误的话也只返回一个响应包，为非No_Error状态
                    if (response.OpCode != OpCodes.Stat || packetLength == 24 || response.Status != OprationStatus.No_Error)
                    {
                        return responseList;
                    }
                }
            }
        }


        /// <summary>
        /// 清理并释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.socket != null)
            {
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Dispose();
                this.socket = null;
            }
        }
    }
}
