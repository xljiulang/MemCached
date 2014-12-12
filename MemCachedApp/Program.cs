using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemCachedLib;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using MemCachedLib.Cached;
using System.Diagnostics;

namespace MemCachedApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = typeof(Program).Namespace;

            var ips = new IPEndPoint[] 
            { 
                new IPEndPoint(IPAddress.Loopback, 8877),
                new IPEndPoint(IPAddress.Loopback, 8866)
            };

            var cached = MemCachedEx.Create(ips[0]);

            while (true)
            {             
                Console.ReadLine();
            }
        }
    }

    /// <summary>
    /// 实体
    /// </summary>
    [Serializable]
    class Entity
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// 文本显示方式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("name={0} and age={1}", this.Name, this.Age);
        }
    }
}
