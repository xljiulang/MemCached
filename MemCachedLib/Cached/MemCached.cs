using MemCachedLib.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MemCachedLib.Cached
{
    /// <summary>
    /// MemCached缓存操作对象
    /// 线程安全类型
    /// </summary>
    public sealed class MemCached : IDisposable
    {
        #region 静态方法
        /// <summary>
        /// 生成对应服务器实例的缓存操作对象        
        /// </summary>
        /// <param name="ip">服务器ip和端口</param>
        /// <returns></returns>
        public static MemCached Create(IPEndPoint ip)
        {
            return new MemCached(ip);
        }
        #endregion

        /// <summary>
        /// 确保哈希是与IP变动而变动
        /// </summary>
        private int hashCode;

        /// <summary>
        /// 客户端池
        /// </summary>
        private CachedClientPool clientPool;

        /// <summary>
        /// 获取服务器ip和端口
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="ip">服务器ip</param>      
        private MemCached(IPEndPoint ip)
        {
            this.IPEndPoint = ip;
            this.clientPool = new CachedClientPool(ip);
            this.hashCode = HashAlgorithm.GetHashCode(ip.ToString());
        }

        /// <summary>
        /// 表示公共的请求方法
        /// 避免CachedClient漏收
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="func">委托</param>
        /// <returns></returns>
        private T Request<T>(Func<CachedClient, T> func)
        {
            var client = this.clientPool.Pop();
            T result = func.Invoke(client);
            this.clientPool.Push(client);
            return result;
        }

        /// <summary>
        /// 表示公共的请求方法
        /// 避免CachedClient漏收
        /// </summary>
        /// <param name="action">回调</param>
        private void Request(Action<CachedClient> action)
        {
            var client = this.clientPool.Pop();
            action.Invoke(client);
            this.clientPool.Push(client);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>        
        /// <typeparam name="T">值类型</typeparam>        
        /// <param name="key">键值</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public CachedReault<T> Get<T>(string key)
        {
            return this.Request<CachedReault<T>>(client =>
            {
                var response = client.Send(new GetRequest(key));
                var value = client.ToEntity<T>(response.Value);
                return new CachedReault<T>(response.Status, response.CAS, value);
            });
        }

        /// <summary>
        /// 获取缓存
        /// </summary>         
        /// <param name="key">键值</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public CachedReault<dynamic> Get(string key)
        {
            return this.Get<dynamic>(key);
        }

        /// <summary>
        /// 存储缓存
        /// </summary>         
        /// <param name="code">存储方式</param>
        /// <param name="key">键值</param>       
        /// <param name="value">缓存对象(需要可序列化)</param>
        /// <param name="expiry">缓存时长，0秒表示永久</param>
        /// <param name="cas">版本号(0表示忽略)</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        private OprationStatus Store(OpCodes code, string key, object value, TimeSpan expiry, long cas = 0)
        {
            return this.Request<OprationStatus>(client =>
            {
                var valueBytes = client.ToBinary(value);
                var request = new StoreRequest(code, key, valueBytes, expiry, cas);
                return client.Send(request).Status;
            });
        }


        /// <summary>
        /// 设置缓存
        /// </summary>         
        /// <param name="key">键值</param>       
        /// <param name="value">缓存对象(需要可序列化)</param>
        /// <param name="expiry">缓存时长，0秒表示永久</param>
        /// <param name="cas">版本号验证值(0表示忽略验证)</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public OprationStatus Set(string key, object value, TimeSpan expiry, long cas = 0)
        {
            return this.Store(OpCodes.Set, key, value, expiry, cas);
        }

        /// <summary>
        /// 添加缓存       
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">缓存对象(需要可序列化)</param>
        /// <param name="expiry">过期时间，0秒表示永久</param>
        /// <param name="cas">版本号验证值(0表示忽略验证)</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public OprationStatus Add(string key, object value, TimeSpan expiry, long cas = 0)
        {
            return this.Store(OpCodes.Add, key, value, expiry, cas);
        }

        /// <summary>
        /// 替换缓存    
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">缓存对象(需要可序列化)</param>
        /// <param name="expiry">过期时间，0秒表示永久</param>
        /// <param name="cas">版本号验证值(0表示忽略验证)</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public OprationStatus Replace(string key, object value, TimeSpan expiry, long cas = 0)
        {
            return this.Store(OpCodes.Replace, key, value, expiry, cas);
        }


        /// <summary>
        /// 删除缓存
        /// </summary>               
        /// <param name="key">键值</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public OprationStatus Delete(string key)
        {
            return this.Request<OprationStatus>(client =>
            {
                return client.Send(new DeleteRequest(key)).Status;
            });
        }

        /// <summary>
        /// 刷新所有记录的过期时间        
        /// </summary>
        /// <param name="expiry">经过一定时间后过期，0秒表示保持不过期</param>
        /// <exception cref="SocketException"></exception>
        public void Flush(TimeSpan expiry)
        {
            this.Request(client => client.Send(new FlushRequest(expiry)));
        }

        /// <summary>
        /// 重置记录的过期时间
        /// <remarks>此功能一些服务器实现有bug，部分服务器不支持</remarks>
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="expiry">过期时间</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public OprationStatus Touch(string key, TimeSpan expiry)
        {
            return this.Request<OprationStatus>(client =>
            {
                var request = new TouchReqeuest(key, expiry);
                return client.Send(request).Status;
            });
        }


        /// <summary>
        /// GetAndTouch
        /// 获取并重置过期时间
        /// <remarks>此功能一些服务器实现有bug，部分服务器不支持</remarks>
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public CachedReault<T> GAT<T>(string key, TimeSpan expiry)
        {
            return this.Request<CachedReault<T>>(client =>
            {
                var res = client.Send(new GATRequest(key, expiry));
                var value = client.ToEntity<T>(res.Value);
                return new CachedReault<T>(res.Status, res.CAS, value);
            });
        }

        /// <summary>
        /// 获取服务器版本
        /// </summary>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public CachedReault<string> Version()
        {
            return this.Request<CachedReault<string>>(client =>
            {
                var response = client.Send(new VersionRequest());
                var version = Encoding.ASCII.GetString(response.Value);
                return new CachedReault<string>(response.Status, response.CAS, version);
            });
        }

        /// <summary>
        /// 获取服务器状态信息
        /// </summary>
        /// <param name="item">选项</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> Stat(StatItems item = StatItems.nothing)
        {
            return this.Request<List<KeyValuePair<string, string>>>(client =>
            {
                var dic = new List<KeyValuePair<string, string>>();
                var responses = client.Sends(new StatRequest(item));

                foreach (var res in responses)
                {
                    if (res.TotalBody > 0)
                    {
                        var k = Encoding.ASCII.GetString(res.Key);
                        var v = Encoding.ASCII.GetString(res.Value);
                        dic.Add(new KeyValuePair<string, string>(k, v));
                    }
                }
                return dic;
            });
        }

        /// <summary>
        /// 确保哈希是与IP变动而变动
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.hashCode;
        }

        /// <summary>
        /// 清理并释放资源
        /// </summary>
        public void Dispose()
        {
            this.clientPool.Dispose();
        }


    }
}
