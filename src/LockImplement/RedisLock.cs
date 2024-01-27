
using CYQ.Data.Cache;

namespace Taurus.Plugin.DistributedLock
{
    internal class RedisLock : DistributedLock
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
        public override LockType LockType
        {
            get
            {
                return LockType.Redis;
            }
        }


        public override bool Idempotent(string key)
        {
            return Idempotent(key, 0);
        }

        public override bool Idempotent(string key, double keepMinutes)
        {
            return AddAll(key, "1", keepMinutes);
        }

        protected override bool AddAll(string key, string value, double cacheMinutes)
        {
            key = "I_" + key;
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
