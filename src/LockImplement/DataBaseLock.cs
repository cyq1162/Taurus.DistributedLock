using CYQ.Data;
using CYQ.Data.Orm;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Plugin.DistributedLock
{
    internal partial class DataBaseLock : DistributedLock
    {
        string tableName;
        string conn;
        public DataBaseLock(string tableName, string conn)
        {
            this.tableName = tableName;
            this.conn = conn;
        }
        public override LockType LockType
        {
            get
            {
                return LockType.DataBase;
            }
        }
    }
    internal partial class DataBaseLock
    {
        protected override bool AddAll(string key, string value, double cacheMinutes)
        {
            using (SysLock sysLock = new SysLock(tableName, conn))
            {
                sysLock.SetSelectColumns("Expire");
                if (sysLock.Fill(key))
                {
                    if (sysLock.Expire.HasValue && sysLock.Expire.Value < DateTime.Now)
                    {
                        sysLock.Delete(key);
                        //超时,删除，再插入
                        //sysLock.LockValue = value;
                        //sysLock.Expire = DateTime.Now.AddSeconds(6);
                        //return sysLock.Update(key);//这个会产生并发更新成功，需要额外加处理条件。
                    }
                    else
                    {
                        return false;
                    }
                }

                sysLock.LockKey = key;
                sysLock.LockValue = value;
                sysLock.Expire = DateTime.Now.AddMinutes(cacheMinutes);
                return sysLock.Insert(InsertOp.None);//这里会产生冲突日志记录，应此应该关闭日志的输出。

            }
        }
        protected override void RemoveAll(string key)
        {
            using (SysLock sysLock = new SysLock(tableName, conn))
            {
                //sysLock.Expire = DateTime.Now;
                //sysLock.Update(key);
                sysLock.Delete(key);
            }
        }
        protected override string Get(string key)
        {
            using (SysLock sysLock = new SysLock(tableName, conn))
            {
                sysLock.SetSelectColumns("LockValue");
                if (sysLock.Fill(key))
                {
                    return sysLock.LockValue;
                }
                return null;
            }
        }
        protected override void SetAll(string key, string value, double cacheMinutes)
        {
            using (SysLock sysLock = new SysLock(tableName, conn))
            {
                sysLock.Expire = DateTime.Now.AddSeconds(6);
                sysLock.Update(key);
            }
        }
    }
    internal partial class DataBaseLock
    {
        public override bool Idempotent(string key)
        {
            return Idempotent("Idempotent_" + key, 0);
        }

        public override bool Idempotent(string key, double keepMinutes)
        {
            return AddAll(key, "1", keepMinutes);
        }
    }
    internal class SysLock : SimpleOrmBase
    {
        public SysLock(string tableName, string conn)
        {
            base.SetInit(this, tableName, conn, false);
            base.SetAopState(CYQ.Data.Aop.AopOp.CloseAll);//关掉Aop自动缓存。
        }
        [Key(false, true, false)]
        [Length(36)]
        public string LockKey { get; set; }
        [Length(50)]
        public string LockValue { get; set; }
        public DateTime? Expire { get; set; }
    }
}
