using MemCachedLib.Cached;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace MemCachedLib.Session
{
    /// <summary>
    /// 通过MemCached实现的Session提供者
    /// </summary>
    public class MemSessionProvider : SessionStateStoreProviderBase
    {
        /// <summary>
        /// 缓存操作对象
        /// </summary>
        private MemCachedEx cachedEx;

        /// <summary>
        /// 描述
        /// </summary>
        public override string Description
        {
            get
            {
                return base.Description;
            }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public override string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// 创建要用于当前请求的新 SessionState.SessionStateStoreData 对象
        /// </summary>
        /// <param name="context">当前请求的 HttpContext</param>
        /// <param name="timeout">SessionState.HttpSessionState.Timeout值</param>
        /// <returns></returns>
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            var staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
            return new SessionStateStoreData(new SessionStateItemCollection(), staticObjects, timeout);
        }


        /// <summary>
        /// 将新的会话状态项添加到数据存储区中
        /// </summary>
        /// <param name="context">当前请求的 HttpContext</param>
        /// <param name="id">当前请求的 SessionState.HttpSessionState.SessionID</param>
        /// <param name="timeout">当前请求的会话 SessionState.HttpSessionState.Timeout</param>
        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            var session = new SessionItem()
            {
                ActionFlag = SessionStateActions.InitializeItem,
                TimeOut = timeout
            };
            this.cachedEx.Set(id, session, TimeSpan.FromMinutes(session.TimeOut));
        }

        /// <summary>
        /// 请求结束时
        /// </summary>
        /// <param name="context">上下文</param>
        public override void EndRequest(HttpContext context)
        {
        }

        /// <summary>
        /// 从会话数据存储区中返回只读会话状态数据
        /// </summary>
        /// <param name="isExclusive">是否是竞争锁的</param>
        /// <param name="context">当前请求的 HttpContext</param>
        /// <param name="id">当前请求的 SessionState.HttpSessionState.SessionID</param>
        /// <param name="locked">如果请求的会话项在会话数据存储区被锁定，请包含一个设置为 true 的布尔值；否则请包含一个设置为 false 的布尔值</param>
        /// <param name="lockAge">请包含一个设置为会话数据存储区中的项锁定时间的 System.TimeSpan 对象</param>
        /// <param name="lockId">请包含一个设置为当前请求的锁定标识符的对象</param>
        /// <param name="actions">请包含 SessionState.SessionStateActions 值之一，指示当前会话是否为未初始化的无Cookie会话</param>
        /// <returns></returns>
        private SessionStateStoreData GetItem(bool isExclusive, HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            locked = false;
            lockAge = TimeSpan.Zero;
            lockId = null;
            actions = SessionStateActions.None;

            var session = this.cachedEx.Get<SessionItem>(id).Value;
            if (session == null)
            {
                return null;
            }

            actions = session.ActionFlag;

            if (session.Locked)
            {
                locked = true;
                lockId = session.LockId;
                lockAge = DateTime.UtcNow - session.LockTime;
                return null;
            }

            if (isExclusive == true)
            {
                locked = session.Locked = true;
                session.LockTime = DateTime.UtcNow;
                lockAge = TimeSpan.Zero;
                lockId = ++session.LockId;
            }

            session.ActionFlag = SessionStateActions.None;
            this.cachedEx.Set(id, session, TimeSpan.FromMinutes(session.TimeOut));

            var staticObjects = SessionStateUtility.GetSessionStaticObjects(context);
            var sessionCollection = actions == SessionStateActions.InitializeItem ? new SessionStateItemCollection() : SessionSerializer.Deserialize(session.Binary);
            return new SessionStateStoreData(sessionCollection, staticObjects, session.TimeOut);
        }

        /// <summary>
        /// 从会话数据存储区中返回只读会话状态数据
        /// </summary>
        /// <param name="context">当前请求的 HttpContext</param>
        /// <param name="id">当前请求的 SessionState.HttpSessionState.SessionID</param>
        /// <param name="locked">如果请求的会话项在会话数据存储区被锁定，请包含一个设置为 true 的布尔值；否则请包含一个设置为 false 的布尔值</param>
        /// <param name="lockAge">请包含一个设置为会话数据存储区中的项锁定时间的 System.TimeSpan 对象</param>
        /// <param name="lockId">请包含一个设置为当前请求的锁定标识符的对象</param>
        /// <param name="actions">请包含 SessionState.SessionStateActions 值之一，指示当前会话是否为未初始化的无Cookie会话</param>
        /// <returns></returns>
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return this.GetItem(false, context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// 从会话数据存储区中返回只读会话状态数据
        /// </summary>
        /// <param name="context">当前请求的 HttpContext</param>
        /// <param name="id">当前请求的 SessionState.HttpSessionState.SessionID</param>
        /// <param name="locked">如果成功获得锁定，请包含一个设置为 true 的布尔值；否则请包含一个设置为 false 的布尔值</param>
        /// <param name="lockAge">请包含一个设置为会话数据存储区中的项锁定时间的 System.TimeSpan 对象</param>
        /// <param name="lockId">请包含一个设置为当前请求的锁定标识符的对象</param>
        /// <param name="actions">请包含 SessionState.SessionStateActions 值之一，指示当前会话是否为未初始化的无Cookie会话</param>
        /// <returns></returns>
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return this.GetItem(false, context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// 请求初始化时
        /// </summary>
        /// <param name="context">上下文</param>
        public override void InitializeRequest(HttpContext context)
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="config">配置</param>
        public override void Initialize(string name, NameValueCollection config)
        {
            var ips = config["server"]
                .Split(';')
                .Select(item => item.Trim())
                .Where(item => item.Contains(':'))
                .Select(item => item.Split(':'))
                .Select(item => new IPEndPoint(IPAddress.Parse(item[0]), int.Parse(item[1])))
                .ToArray();

            this.cachedEx = MemCachedEx.Create(ips);
            base.Initialize(name, config);
        }

        /// <summary>
        /// 释放对会话数据存储区中项的锁定
        /// </summary>
        /// <param name="context">当前请求的 HttpContext</param>
        /// <param name="id">当前请求的会话标识符</param>
        /// <param name="lockId">lockId</param>
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            var session = this.cachedEx.Get<SessionItem>(id).Value;
            if (session != null && session.Locked == true)
            {
                session.Locked = false;
                this.cachedEx.Set(id, session, TimeSpan.FromMinutes(session.TimeOut));
            }
        }

        /// <summary>
        /// 移除sesstion选项
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="id">SessionID</param>
        /// <param name="lockId">锁ID</param>
        /// <param name="item">选项</param>
        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            this.cachedEx.Delete(id);
        }


        /// <summary>
        /// 重新计时Session选项的超时时间
        /// 以保持Session不被释放
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="id">SessionID</param>
        public override void ResetItemTimeout(HttpContext context, string id)
        {
            var session = this.cachedEx.Get<SessionItem>(id).Value;
            this.cachedEx.Set(id, session, TimeSpan.FromMinutes(session.TimeOut));
        }

        /// <summary>
        /// 使用当前请求中的值更新会话状态数据存储区中的会话项信息，并清除对数据的锁定
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="id">SessionID</param>
        /// <param name="item">Session选项</param>
        /// <param name="lockId">锁ID</param>
        /// <param name="newItem">如果为 true，则将会话项标识为新项；如果为 false，则将会话项标识为现有的项</param>
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            var session = this.cachedEx.Get<SessionItem>(id).Value;
            if (session != null)
            {
                session.Locked = false;
                session.Binary = SessionSerializer.Serialize(item.Items as SessionStateItemCollection);
                session.TimeOut = item.Timeout;
                this.cachedEx.Set(id, session, TimeSpan.FromMinutes(session.TimeOut));
            }
        }

        /// <summary>
        /// 如果会话状态存储提供程序支持调用 Session_OnEnd 事件，则为 true；否则为 false
        /// </summary>
        /// <param name="expireCallback">回调</param>
        /// <returns></returns>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        /// <summary>
        /// 清理释放资源
        /// </summary>
        public override void Dispose()
        {
            this.cachedEx.Dispose();
        }
    }
}
