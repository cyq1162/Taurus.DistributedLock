using CYQ.Data;
using CYQ.Data.Cache;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Taurus.Plugin.DistributedLock
{

    /// <summary>
    /// 分布式锁
    /// </summary>
    public abstract partial class DistributedLock
    {
        #region 对外实例
        /// <summary>
        /// 分布式锁实例【根据配置顺序取值：DataBase => Redis => MemCache => Local】
        /// </summary>
        public static DistributedLock Instance
        {
            get
            {
                if (!string.IsNullOrEmpty(DistributedLockConfig.Conn))
                {
                    return DataBase;
                }
                if (!string.IsNullOrEmpty(DistributedLockConfig.RedisServers))
                {
                    return Redis;
                }
                if (!string.IsNullOrEmpty(DistributedLockConfig.MemCacheServers))
                {
                    return MemCache;
                }
                return Local;

            }
        }

        /// <summary>
        /// Redis 分布式锁实例
        /// </summary>
        public static DistributedLock Redis
        {
            get
            {
                return RedisLock.Instance;
            }
        }

        /// <summary>
        /// MemCach 分布式锁实例
        /// </summary>
        public static DistributedLock MemCache
        {
            get
            {
                return MemCacheLock.Instance;
            }
        }


        /// <summary>
        /// Local 单机锁 基于 Mutex 锁实例
        /// </summary>
        public static DistributedLock Local
        {
            get
            {
                return LocalLock.Instance;

            }
        }

        /// <summary>
        /// Local 单机内 文件锁【可跨进程或线程释放】
        /// </summary>
        public static DistributedLock File
        {
            get
            {
                return FileLock.Instance;
            }
        }

        /// <summary>
        /// 数据库 分布式锁默认实例
        /// </summary>
        public static DistributedLock DataBase
        {
            get
            {
                return new DataBaseLock(DistributedLockConfig.TableName, DistributedLockConfig.Conn);
            }
        }
        /// <summary>
        /// 自定义数据库锁实例。
        /// </summary>
        /// <param name="tableName">自定义表名</param>
        /// <returns></returns>
        public static DistributedLock GetDataBaseLock(string tableName)
        {
            return new DataBaseLock(tableName, DistributedLockConfig.Conn);
        }
        /// <summary>
        /// 自定义数据库锁实例。
        /// </summary>
        /// <param name="tableName">自定义表名</param>
        /// <param name="conn">自定义数据库链接</param>
        /// <returns></returns>
        public static DistributedLock GetDataBaseLock(string tableName, string conn)
        {
            return new DataBaseLock(tableName, conn);
        }
        #endregion

        private static int _ProcessID;
        /// <summary>
        /// 当前进程ID
        /// </summary>
        protected static int ProcessID
        {
            get
            {
                if (_ProcessID == 0)
                {
                    _ProcessID = Process.GetCurrentProcess().Id;
                }
                return _ProcessID;
            }
        }

        /// <summary>
        /// 锁类型
        /// </summary>
        public abstract LockType LockType { get; }
    }

    /// <summary>
    /// 处理分布式锁
    /// </summary>
    public abstract partial class DistributedLock
    {
        /// <summary>
        /// 重入锁 字典
        /// </summary>
        protected MDictionary<string, int> lockAgainDic = new MDictionary<string, int>();

        /// <summary>
        /// 分布式锁（分布式锁需要启用 Redis [配置 Redis.Servers ] 或 Memcache [配置 MemCache.Servers] ）
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="millisecondsTimeout">尝试获取锁的最大等待时间（ms毫秒），超过这个值，则认为获取锁失败</param>
        /// <returns></returns>
        public bool Lock(string key, int millisecondsTimeout)
        {
            #region 可重入锁

            string flag = ProcessID + "," + Thread.CurrentThread.ManagedThreadId + "," + key;
            //已存在锁，锁重入。
            if (lockAgainDic.ContainsKey(flag))
            {
                lockAgainDic[flag]++;
                //Console.WriteLine("Lock Again:" + flag);
                return true;
            }

            #endregion
            if (OnLock(key, flag, millisecondsTimeout))
            {
                lockAgainDic.Add(flag, 0);
                return true;
            }
            return false;

        }
        protected virtual bool OnLock(string key, string value, int millisecondsTimeout)
        {
            int sleep = 5;
            int count = millisecondsTimeout;
            while (true)
            {
                if (!keyValueDic.ContainsKey(key) && AddAll(key, value, 0.1))//
                {
                    AddToWork(key, value);//循环检测超时时间，执行期间，服务挂了，然后重启了？
                    return true;
                }
                Thread.Sleep(sleep);
                count -= sleep;
                if (sleep < 1000)
                {
                    sleep += 5;
                }
                if (count <= 0)
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 释放（分布式锁）
        /// </summary>
        /// <returns></returns>
        public void UnLock(string key)
        {
            #region 可重入锁检测

            string flag = ProcessID + "," + Thread.CurrentThread.ManagedThreadId + "," + key;
            if (lockAgainDic.ContainsKey(flag))
            {

                if (lockAgainDic[flag] > 0)
                {
                    lockAgainDic[flag]--;
                    //Console.WriteLine("Un Lock Again:" + flag + " - " + lockAgain[flag]);
                    return;
                }
                else
                {
                    lockAgainDic.Remove(flag);
                }
            }
            #endregion

            OnUnLock(key, flag);
        }


        protected virtual void OnUnLock(string key, string flag)
        {
            //Console.WriteLine("Un Lock :" + flag);
            RemoveFromWork(key);

            //--释放机制有些问题，需要调整。
            string value = Get(key);
            //自身加的锁
            if (value == flag)
            {
                lock (key)
                {
                    RemoveAll(key);
                }
            }
        }

        protected virtual string Get(string key) { return null; }

        /// <summary>
        /// 往所有节点写入数据【用于分布式锁的超时机制，追续时间】
        /// </summary>
        protected virtual void SetAll(string key, string value, double cacheMinutes) { }

        protected virtual void RemoveAll(string key) { }

        protected virtual bool AddAll(string key, string value, double cacheMinutes) { return false; }

        #region 内部定时日志工作
        /// <summary>
        /// 线程工作字典：存key,value
        /// </summary>
        protected MDictionary<string, string> keyValueDic = new MDictionary<string, string>();
        bool threadIsWorking = false;
        private void AddToWork(string key, string value)
        {
            keyValueDic.Add(key, value);
            if (!threadIsWorking)
            {
                lock (this)
                {
                    if (!threadIsWorking)
                    {
                        threadIsWorking = true;
                        ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(DoLockWork));
                    }
                }
            }
        }
        //移除定时任务。
        private void RemoveFromWork(string key)
        {
            if (keyValueDic.Remove(key))
            {
                //Console.Write(" - Remove Work.");
            }
        }
        private void DoLockWork(object p)
        {
            while (true)
            {
                // Console.WriteLine("DoWork :--------------------------Count : " + keysDic.Count);
                if (keyValueDic.Count > 0)
                {
                    List<string> list = keyValueDic.GetKeys();
                    foreach (string key in list)
                    {
                        //给 key 设置延时时间
                        if (keyValueDic.ContainsKey(key))
                        {
                            lock (key)
                            {
                                if (keyValueDic.ContainsKey(key))
                                {
                                    SetAll(key, keyValueDic[key], 0.1);//延时锁：6秒
                                }
                            }
                        }
                    }
                    list.Clear();
                    Thread.Sleep(3000);//循环。
                }
                else
                {
                    threadIsWorking = false;
                    break;
                }

            }
        }
        #endregion

    }



}
