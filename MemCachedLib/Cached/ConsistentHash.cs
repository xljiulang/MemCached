using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MemCachedLib.Cached
{
    /// <summary>
    /// 一致性哈希
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    public class ConsistentHash<T>
    {
        /// <summary>
        /// 缓存所有哈希键
        /// </summary>        
        private int[] hashKeys;
        /// <summary>
        /// 初始环大小
        /// </summary>
        private int defaultReplicate = 100;
        /// <summary>
        /// 哈希键与节点字典
        /// </summary>
        private SortedDictionary<int, T> keyHashNodeDic;

        /// <summary>
        /// 一致性哈希
        /// </summary>
        public ConsistentHash()
            : this(null)
        {
        }

        /// <summary>
        /// 一致性哈希
        /// </summary>
        /// <param name="nodes">节点</param>
        public ConsistentHash(IEnumerable<T> nodes)
        {
            this.keyHashNodeDic = new SortedDictionary<int, T>();
            if (nodes != null)
            {
                foreach (T node in nodes)
                {
                    this.Add(node, false);
                }
                this.hashKeys = keyHashNodeDic.Keys.ToArray();
            }
        }

        /// <summary>
        /// 添加一个节点
        /// </summary>
        /// <param name="node">节点</param>
        public void Add(T node)
        {
            this.Add(node, true);
        }

        /// <summary>
        /// 添加一个节点
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="updateKeyArray">是否把键更新到局部变量进行缓存</param>
        private void Add(T node, bool updateKeyArray)
        {
            for (int i = 0; i < defaultReplicate; i++)
            {
                int hash = HashAlgorithm.GetHashCode(node.GetHashCode().ToString() + i);
                keyHashNodeDic[hash] = node;
            }

            if (updateKeyArray)
            {
                this.hashKeys = keyHashNodeDic.Keys.ToArray();
            }
        }

        /// <summary>
        /// 删除一个节点
        /// </summary>
        /// <param name="node">节点</param>
        public bool Remove(T node)
        {
            for (int i = 0; i < defaultReplicate; i++)
            {
                int hash = HashAlgorithm.GetHashCode(node.GetHashCode().ToString() + i);
                if (this.keyHashNodeDic.Remove(hash) == false)
                {
                    return false;
                }
            }

            // 重新加载节点
            this.hashKeys = this.keyHashNodeDic.Keys.ToArray();
            return true;
        }

        /// <summary>
        /// 获取键对应的节点
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public T GetNode(string key)
        {
            int firstNode = this.GetHaskKeyIndex(key);
            return this.keyHashNodeDic[hashKeys[firstNode]];
        }

        /// <summary>
        /// 获取键所对应的键哈希表的索引
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns></returns>
        private int GetHaskKeyIndex(string key)
        {
            int hashCode = HashAlgorithm.GetHashCode(key);

            int begin = 0;
            int end = this.hashKeys.Length - 1;

            if (this.hashKeys[end] < hashCode || this.hashKeys[0] > hashCode)
            {
                return 0;
            }

            int mid = begin;
            while (end - begin > 1)
            {
                mid = (end + begin) / 2;
                if (this.hashKeys[mid] >= hashCode)
                {
                    end = mid;
                }
                else
                {
                    begin = mid;
                }
            }

            return end;
        }
    }

}
