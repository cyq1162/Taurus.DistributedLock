using CYQ.Data.Cache;

namespace Taurus.Plugin.DistributedLock
{
    internal class MemCacheLock : DLock
    {
        private static readonly MemCacheLock _instance = new MemCacheLock();
        private MemCacheLock() { }
        public static MemCacheLock Instance
        {
            get
            {
                return _instance;
            }
        }
        public override DLockType LockType
        {
            get
            {
                return DLockType.MemCache;
            }
        }


        protected override bool AddAll(string key, string value, double cacheMinutes)
        {
            return DistributedCache.MemCache.SetNXAll(key, value, cacheMinutes);
        }

        protected override string Get(string key)
        {
            return DistributedCache.MemCache.Get<string>(key);
        }

        protected override void RemoveAll(string key)
        {
            DistributedCache.MemCache.RemoveAll(key);
        }

        protected override void SetAll(string key, string value, double cacheMinutes)
        {
            DistributedCache.MemCache.SetAll(key, value, cacheMinutes);
        }
    }
}
