using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MemCachedLib;
using System.Net;

namespace MemCachedManager
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
            var keyNotExists = "The Key Is Not Exists";
            var keyHasExists = "The Key Has Exists";
            var ok = "OK";

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
                            if (result.Status == false)
                            {
                                return keyNotExists;
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
                        return state ? ok : keyNotExists;

                    case CmdEnum.add:
                        if (args.Count != 3)
                        {
                            return string.Empty;
                        }

                        state = memCached.Add(args[0], args[1], TimeSpan.FromSeconds(int.Parse(args[2])));
                        return state ? ok : keyHasExists;

                    case CmdEnum.replace:
                        if (args.Count != 3)
                        {
                            return string.Empty;
                        }

                        state = memCached.Replace(args[0], args[1], TimeSpan.FromSeconds(int.Parse(args[2])));
                        return state ? ok : keyNotExists;

                    case CmdEnum.delete:
                        if (args.Count != 1)
                        {
                            return string.Empty;
                        }
                        state = memCached.Delete(args[0]);
                        return state ? ok : keyNotExists;

                    case CmdEnum.flush:
                        if (args.Count != 1)
                        {
                            return string.Empty;
                        }
                        memCached.Flush(TimeSpan.FromSeconds(int.Parse(args[0])));
                        return ok;

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
