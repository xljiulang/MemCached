using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace MemCachedManager
{
    /// <summary>
    /// MemCached服务
    /// </summary>
    public class MemService
    {

        /// <summary>
        /// 服务名前缀
        /// </summary>
        private static readonly string servicePreName = "Memcached Server";

        /// <summary>
        /// 获取所有MemCached服务
        /// </summary>
        /// <returns></returns>
        public static MemService[] GetMemService()
        {
            return ServiceController
                     .GetServices()
                     .Where(item => item.ServiceName.StartsWith(servicePreName))
                     .Select(item => new MemService(item.ServiceName))
                     .ToArray();
        }


        public static MemService AddService(int port)
        {
            var name = string.Format("{0} {1}", servicePreName, port);
            var service = new MemService(name);
            return service;
        }


        /// <summary>
        /// 服务文件
        /// </summary>
        private readonly string serviceFileName = IntPtr.Size == 4 ? "Memcached_x86.exe" : "Memcached_x64.exe";

        /// <summary>
        /// 服务名称
        /// </summary>
        private string serviceName;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="serviceName"></param>
        private MemService(string serviceName)
        {
            this.serviceName = serviceName;
        }

        /// <summary>
        /// 获取服务对象
        /// </summary>
        private ServiceController Service
        {
            get
            {
                return ServiceController
                    .GetServices()
                    .FirstOrDefault(item => item.ServiceName == this.serviceName);
            }
        }

        /// <summary>
        /// 获取服务是否在运行中
        /// </summary>
        public bool HasRunnig
        {
            get
            {
                var service = this.Service;
                if (service != null)
                {
                    return service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StartPending;
                }
                return false;
            }
        }

        /// <summary>
        /// 从注册表取出参数的值
        /// </summary>
        /// <param name="arg">参数名</param>
        /// <returns></returns>
        private string GetArgValue(string arg)
        {
            if (this.Service != null)
            {
                using (var key = this.GetServiceKey())
                {
                    var imagePath = key.GetValue("imagePath").ToString();
                    var args = imagePath.Split(' ').ToList();
                    var index = args.FindIndex(item => item == arg);
                    if (index > -1 && index < args.Count - 1)
                    {
                        return args[index + 1];
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取服务端口
        /// </summary>
        public int Port
        {
            get
            {
                var portString = this.GetArgValue("-p");
                int port = 0;
                if (int.TryParse(portString, out port))
                {
                    return port;
                }
                return 11211;
            }
        }

        /// <summary>
        /// 获取使用最大内存限制
        /// </summary>
        public int MaxMemory
        {
            get
            {
                var memString = this.GetArgValue("-m");
                int mem = 0;
                if (int.TryParse(memString, out mem))
                {
                    return mem;
                }
                return 64;
            }
        }

        /// <summary>
        /// 获取服务注册表键
        /// </summary>
        /// <returns></returns>
        private RegistryKey GetServiceKey()
        {
            string key = @"SYSTEM\CurrentControlSet\services\" + this.serviceName;
            return Registry.LocalMachine.OpenSubKey(key, true);
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            if (this.Service.CanStop)
            {
                this.Service.Stop();
            }
        }

        public void Delete()
        {
            if (this.Service.CanStop)
            {
                this.Service.Stop();
            }
            this.RunCmd("sc.exe", "delete \"" + this.serviceName + "\"");
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="maxMemory">最大使用内存限制</param>
        /// <param name="port">服务端口</param>
        public void Start(int maxMemory, int port)
        {
            var format = " \"{0}\" binpath= \"{1} -d runservice -m {2} -p {3}\" start= auto displayname= \"{0}\"";
            var arg = string.Format(format, this.serviceName, Path.GetFullPath(this.serviceFileName), maxMemory, port);
            var create = "create" + arg;
            var config = "config" + arg;

            this.RunCmd("sc.exe", create);
            this.RunCmd("sc.exe", config);
            this.Service.Start();
        }

        /// <summary>
        /// 通过参数运行指定可执行文件
        /// </summary>
        /// <param name="fileName">可执行文件</param>
        /// <param name="arg">参数</param>
        private void RunCmd(string fileName, string arg)
        {
            ProcessStartInfo info = new ProcessStartInfo(fileName);
            info.UseShellExecute = false;
            info.Arguments = arg;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            Process.Start(info).WaitForExit();
        }

        /// <summary>
        /// 名称
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.serviceName;
        }
    }
}
