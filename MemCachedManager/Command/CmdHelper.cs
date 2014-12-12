using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MemCachedLib;
using System.Net;
using MemCachedLib.Cached;

namespace MemCachedManager.Command
{
    /// <summary>
    /// 命令指令帮助类
    /// </summary>
    public static class CmdHelper
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="cmdLine">命令行</param>
        /// <param name="port">服务器端口</param>
        /// <returns></returns>
        public static string Exec(string cmdLine, int port)
        {
            CmdEnum cmd;
            List<string> args;

            if (CheckCmd(cmdLine, out cmd, out args) == false)
            {
                return GetHelpString();
            }

            var error = "Command Error";
            try
            {
                var value = Exec(cmd, args, port);
                if (string.IsNullOrEmpty(value))
                {
                    return error;
                }
                return value;
            }
            catch
            {
                return error;
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="cmd">指令</param>
        /// <param name="args">参数</param>
        /// <param name="port">服务器端口</param>
        /// <returns></returns>
        private static string Exec(CmdEnum cmd, List<string> args, int port)
        {
            using (var memCached = MemCached.Create(new IPEndPoint(IPAddress.Loopback, port)))
            {
                switch (cmd)
                {
                    case CmdEnum.get:
                        if (args.Count != 1)
                        {
                            return string.Empty;
                        }
                        try
                        {
                            var result = memCached.Get(args[0]);
                            if (result.Status != OprationStatus.No_Error)
                            {
                                return result.Status.ToString();
                            }
                            return result.Value.ToString();
                        }
                        catch
                        {
                            return "The Value Is A Unknow Object";
                        }

                    case CmdEnum.set:
                        if (args.Count != 3)
                        {
                            return string.Empty;
                        }

                        var state = memCached.Set(args[0], args[1], TimeSpan.FromSeconds(int.Parse(args[2])));
                        return state.ToString();

                    case CmdEnum.add:
                        if (args.Count != 3)
                        {
                            return string.Empty;
                        }

                        state = memCached.Add(args[0], args[1], TimeSpan.FromSeconds(int.Parse(args[2])));
                        return state.ToString();

                    case CmdEnum.replace:
                        if (args.Count != 3)
                        {
                            return string.Empty;
                        }

                        state = memCached.Replace(args[0], args[1], TimeSpan.FromSeconds(int.Parse(args[2])));
                        return state.ToString();

                    case CmdEnum.delete:
                        if (args.Count != 1)
                        {
                            return string.Empty;
                        }
                        state = memCached.Delete(args[0]);
                        return state.ToString();

                    case CmdEnum.flush:
                        if (args.Count != 1)
                        {
                            return string.Empty;
                        }
                        memCached.Flush(TimeSpan.FromSeconds(int.Parse(args[0])));
                        return "OK";

                    case CmdEnum.touch:
                        if (args.Count != 2)
                        {
                            return string.Empty;
                        }
                        state = memCached.Touch(args[0], TimeSpan.FromSeconds(int.Parse(args[1])));
                        return state.ToString();

                    case CmdEnum.stat:
                        var lines = memCached.Stat().Select(item => string.Format("{0} => {1}", item.Key, item.Value));
                        return string.Join(Environment.NewLine, lines);

                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// 获取帮助信息
        /// </summary>
        /// <returns></returns>
        private static string GetHelpString()
        {
            Func<FieldInfo, string> descriptionFunc = (item) =>
            {
                var format = "{0}\r\n用法：{1}\r\n说明：{2}";
                var cmd = Attribute.GetCustomAttributes(item, typeof(CmdAttribute)).FirstOrDefault() as CmdAttribute;
                return string.Format(format, item.Name.ToLower(), cmd.CodeText, cmd.Description);
            };

            var descriptions = typeof(CmdEnum)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(item => descriptionFunc(item));

            return "cls\r\n清屏\r\n\r\n" + string.Join("\r\n\r\n", descriptions);
        }

        /// <summary>
        /// 检测命令参数
        /// </summary>
        /// <param name="cmdLine">命令行</param>
        /// <param name="cmd">命令</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        private static bool CheckCmd(string cmdLine, out CmdEnum cmd, out List<string> args)
        {
            cmd = default(CmdEnum);

            if (string.IsNullOrEmpty(cmdLine))
            {
                args = new List<string>();
                return false;
            }

            args = cmdLine.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var c = args.FirstOrDefault();
            args.RemoveAt(0);

            try
            {
                cmd = (CmdEnum)Enum.Parse(typeof(CmdEnum), c);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
