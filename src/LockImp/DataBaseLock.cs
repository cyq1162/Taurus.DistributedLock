using CYQ.Data;
using CYQ.Data.Orm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Taurus.Plugin.DistributedLock
{
    internal partial class DataBaseLock : DLock
    {
        string tableName;
        string conn;
        public DataBaseLock(string tableName, string conn)
        {
            this.tableName = tableName;
            this.conn = conn;
        }
        public override DLockType LockType
        {
            get
            {
                return DLockType.DataBase;
            }
        }
    }
    internal partial class DataBaseLock
    {
        protected override bool AddAll(string key, string value, double cacheMinutes)
        {
            //bool isTimeOut = false;
            using (SysLock sysLock = new SysLock(tableName, conn))
            {
                //sysLock.SetSelectColumns("Expire");
                if (sysLock.Fill(key))
                {
                    if (sysLock.Expire.HasValue && sysLock.Expire.Value < DateTime.Now)
                    {
                        string where = "LockKey='" + key + "' and LockValue='" + sysLock.LockValue + "'";
                        if (!sysLock.Delete(where))
                        {
                            return false;
                        }
                        else
                        {
                            // Console.WriteLine(where + " - " + sysLock.BaseInfo.RecordsAffected);
                        }
                        //isTimeOut = true;
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
                //if (isTimeOut)
                //{
                //    lock (this)
                //    {
                //        sysLock.Delete(key);
                //        return sysLock.Insert(InsertOp.None);//这里会产生冲突日志记录，应此应该关闭日志的输出。
                //    }

                //}
                //else
                //{
                return sysLock.Insert(InsertOp.None);//这里会产生冲突日志记录，应此应该关闭日志的输出。
                //}
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
