using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.SessionState;

namespace MemCachedLib.Session
{
    /// <summary>
    /// Session数据序列化和反序列化
    /// </summary>
    internal static class SessionSerializer
    {
        /// <summary>
        /// 序列化为二进制数据
        /// </summary>
        /// <param name="items">选项</param>
        /// <returns></returns>
        public static byte[] Serialize(SessionStateItemCollection items)
        {
            if (items == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    items.Serialize(writer);
                    writer.Close();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 反序列为集合
        /// </summary>
        /// <param name="binary">数据</param>
        /// <returns></returns>
        public static SessionStateItemCollection Deserialize(byte[] binary)
        {
            if (binary == null || binary.Length == 0)
            {
                return new SessionStateItemCollection();
            }

            using (var ms = new MemoryStream(binary))
            {
                using (var reader = new BinaryReader(ms))
                {
                    return SessionStateItemCollection.Deserialize(reader);
                }
            }
        }
    }
}
