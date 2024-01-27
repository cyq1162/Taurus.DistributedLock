
using CYQ.Data.Cache;

namespace Taurus.Plugin.DistributedLock
{
    internal class RedisLock : DLock
    {
        private static readonly RedisLock _instance = new RedisLock();
        private RedisLock() { }
        public static RedisLock Instance
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
                return DLockType.Redis;
            }
        }

        protected override bool AddAll(string key, string value, double cacheMinutes)
        {
            return DistributedCache.Redis.SetNXAll(key, value, cacheMinutes);

        }

        protected override string Get(string key)
        {
            return DistributedCache.Redis.Get<string>(key);
        }

        protected override void RemoveAll(string key)
        {
            DistributedCache.Redis.RemoveAll(key);
        }

        protected override void SetAll(string key, string value, double cacheMinutes)
        {
            DistributedCache.Redis.SetAll(key, value, cacheMinutes);
        }
    }
}
