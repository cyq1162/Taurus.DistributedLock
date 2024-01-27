using CYQ.Data.Cache;

namespace Taurus.Plugin.DistributedLock
{
    internal class MemCacheLock : DistributedLock
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
        public override LockType LockType
        {
            get
            {
                return LockType.MemCache;
            }
        }



        public override bool Idempotent(string key)
        {
            return Idempotent(key, 0);
        }

        public override bool Idempotent(string key, double keepMinutes)
        {
            key = "I_" + key;
            return AddAll(key, "1", keepMinutes);
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
