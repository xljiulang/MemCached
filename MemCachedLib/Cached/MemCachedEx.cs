using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using MemCachedLib.Request;

namespace MemCachedLib.Cached
{
    /// <summary>
    /// 分布式MemCached缓存操作对象
    /// 线程安全类型
    /// </summary>
    public sealed class MemCachedEx : IDisposable
    {
        /// <summary>
        /// 生成对应服务器实例的缓存操作对象
        /// </summary>
        /// <param name="ips">服务器ip列表</param>
        /// <returns></returns>
        public static MemCachedEx Create(params IPEndPoint[] ips)
        {
            return new MemCachedEx(ips);
        }

        /// <summary>
        /// 服务器缓存列表
        /// </summary>
        private IEnumerable<MemCached> memCacheds;

        /// <summary>
        /// 服务缓存搜索
        /// </summary>
        private ConsistentHash<MemCached> searcher;


        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="ips">列表器IP列表</param>       
        private MemCachedEx(IPEndPoint[] ips)
        {
            this.memCacheds = ips.Select(item => MemCached.Create(item));
            this.searcher = new ConsistentHash<MemCached>(this.memCacheds);
        }


        /// <summary>
        /// 通过Key来查询数据保存到的服务
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        private MemCached this[string key]
        {
            get
            {
                var memCached = this.searcher.GetNode(key);
                return memCached;
            }
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
            return this[key].Get<T>(key);
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
            return this[key].Set(key, value, expiry, cas);
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
            return this[key].Add(key, value, expiry, cas);
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
            return this[key].Replace(key, value, expiry, cas);
        }


        /// <summary>
        /// 删除缓存
        /// </summary>               
        /// <param name="key">键值</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public OprationStatus Delete(string key)
        {
            return this[key].Delete(key);
        }

        /// <summary>
        /// 刷新所有记录的过期时间
        /// </summary>
        /// <param name="expiry">经过一定时间后过期，0秒表示保持不过期</param>
        /// <exception cref="SocketException"></exception>
        public void Flush(TimeSpan expiry)
        {
            foreach (var m in this.memCacheds)
            {
                m.Flush(expiry);
            }
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
            return this[key].Touch(key, expiry);
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
            return this[key].GAT<T>(key, expiry);
        }

        /// <summary>
        /// 获取服务器版本
        /// </summary>
        /// <param name="ip">服务器IP</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public CachedReault<string> Version(IPEndPoint ip)
        {
            return this.memCacheds.FirstOrDefault(item => item.IPEndPoint == ip).Version();
        }

        /// <summary>
        /// 获取服务器状态信息
        /// </summary>
        /// <param name="ip">服务器IP</param>
        /// <param name="item">选项</param>
        /// <exception cref="SocketException"></exception>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> Stat(IPEndPoint ip, StatItems item = StatItems.nothing)
        {
            return this.memCacheds.FirstOrDefault(m => m.IPEndPoint == ip).Stat(item);
        }

        /// <summary>
        /// 清理并释放资源
        /// </summary>
        public void Dispose()
        {
            foreach (var m in this.memCacheds)
            {
                m.Dispose();
            }
        }
    }
}
