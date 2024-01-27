using CYQ.Data.Cache;
using System;
using System.Threading;

namespace Taurus.Plugin.DistributedLock
{
    internal partial class LocalLock : DistributedLock
    {
        private static readonly LocalLock _instance = new LocalLock();
        private LocalLock() { }
        public static LocalLock Instance
        {
            get
            {
                return _instance;
            }
        }
        public override LockType LockType
        {
            get
            {
                return LockType.Local;
            }
        }
    }

    /// <summary>
    /// 处理分布式锁【单机】
    /// </summary>
    internal partial class LocalLock
    {
        protected override bool OnLock(string key, string value, int millisecondsTimeout)
        {
            return MutexWaitOne(key, millisecondsTimeout);
        }
        protected override void OnUnLock(string key, string flag)
        {
            MutexRelease(key);
        }
        private static bool MutexWaitOne(string key, int millisecondsTimeout)
        {
            key = "LocalCache-" + key;
            var mutex = new Mutex(false, key);

            try
            {
                //其它进程直接关闭，未释放即退出时，引发此异常。
                return mutex.WaitOne(millisecondsTimeout);
            }
            catch
            {
                //释放其它进程直接关闭导致的锁。
                mutex.ReleaseMutex();
                try
                {
                    //如果存在重入锁【锁多次进入，未释放】，会引发此异常。
                    return mutex.WaitOne(millisecondsTimeout);
                }
                catch
                {
                    return false;
                }
            }
        }
        private static bool MutexRelease(string key)
        {
            key = "LocalCache-" + key;
            var mutex = new Mutex(false, key);
            try
            {
                mutex.ReleaseMutex();
                return true;
            }
            catch
            {
                return false;
            }

        }
    }

    /// <summary>
    /// 处理分布式锁【幂等性】
    /// </summary>
    internal partial class LocalLock
    {

        public override bool Idempotent(string key)
        {
            return Idempotent(key, 0);
        }

        public override bool Idempotent(string key, double keepMinutes)
        {
            return DistributedLock.File.Idempotent(key, keepMinutes);
        }
    }
}
