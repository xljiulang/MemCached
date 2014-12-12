using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace MemCachedLib
{
    /// <summary>   
    /// 缓存客户端连接池
    /// 线程安全类型
    /// </summary> 
    internal class CachedClientPool : IDisposable
    {
        /// <summary>
        /// 服务器IP和端口
        /// </summary>
        private IPEndPoint ip;

        /// <summary>
        /// 客户端集合
        /// </summary>
        private ConcurrentStack<CachedClient> clientStack = new ConcurrentStack<CachedClient>();

        /// <summary>
        /// 自旋等待对象
        /// </summary>
        private SpinWait spinWait = new SpinWait();

        /// <summary>
        /// 获取最大客户端数量
        /// </summary>
        public int MaxClient { get; private set; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="ip">服务器IP和端口</param>    
        public CachedClientPool(IPEndPoint ip)
            : this(ip, 10)
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="ip">服务器IP和端口</param>    
        /// <param name="maxClient">最大客户端数量</param>
        public CachedClientPool(IPEndPoint ip, int maxClient)
        {
            this.ip = ip;
            this.MaxClient = maxClient;

            for (var i = 0; i < maxClient; i++)
            {
                this.Push(new CachedClient(ip));
            }
        }

        /// <summary>
        /// 从池中取出一个缓存客户端      
        /// </summary>       
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public CachedClient Pop()
        {
            CachedClient client = null;
            while (!this.clientStack.TryPop(out client))
            {
                this.spinWait.SpinOnce();
            }
            return client;
        }

        /// <summary>
        /// 将不现使用的缓存客户端放回池中
        /// </summary>
        /// <param name="client">缓存客户端</param>
        public void Push(CachedClient client)
        {
            this.clientStack.Push(client);
        }

        /// <summary>
        /// 清理并释放资源
        /// </summary>
        public void Dispose()
        {
            CachedClient client = null;
            while (this.clientStack.TryPop(out client))
            {
                client.Dispose();
            }
        }
    }
}
