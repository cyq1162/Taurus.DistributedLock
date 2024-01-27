using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Taurus.Plugin.DistributedLock;

namespace DistributedLockTest
{
    class RedisLockDemo
    {
        static bool hasShowInfo = false;
        static private DLock dsLock;
        public static void Start()
        {
            DLockConfig.RedisServers = "127.0.0.1:6379";
            dsLock = DLock.Redis;
            hasShowInfo = false;
            for (int i = 1; i <= 1000; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(LockThread), i);
            }

        }
        static int ok = 0;
        static int fail = 0;
        static void LockThread(object i)
        {
            string key = "myLock" + ((int)i) % 10;
            bool isOK = false;
            try
            {

                isOK = dsLock.Lock(key, 30000);
                if (isOK)
                {
                    //isOK = dsLock.Lock(key, 30000);
                    //dsLock.UnLock(key);
                    Interlocked.Increment(ref ok);
                    Console.Write(ok + " - Redis OK - " + Thread.CurrentThread.ManagedThreadId);
                    //Console.WriteLine("数字：" + i + " -- 线程ID：" + Thread.CurrentThread.ManagedThreadId + " 获得锁成功。");
                }
                else
                {
                    Interlocked.Increment(ref fail);
                    Console.WriteLine(fail + "Fail ----------------------------");
                    //Console.WriteLine("数字：" + i + " -- 线程ID：" + Thread.CurrentThread.ManagedThreadId + " 获得锁失败！");
                }
            }
            finally
            {
                if (isOK)
                {
                    Console.WriteLine(" - UnLock.");
                    dsLock.UnLock(key);
                }
                //if (ok % 1000 == 0 && !hasShowInfo)
                //{
                //    hasShowInfo = true;
                //    Console.WriteLine(DistributedCache.Redis.WorkInfo);
                //}
            }
        }
    }
}
