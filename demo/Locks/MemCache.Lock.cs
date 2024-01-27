using CYQ.Data;
using CYQ.Data.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Taurus.Plugin.DistributedLock;

namespace DistributedLockTest
{
    class MemCacheLockDemo
    {
        static bool hasShowInfo = false;
        static private DistributedLock dsLock;
        public static void Start()
        {
            DistributedLockConfig.MemCacheServers = "192.168.100.111:11211";
            dsLock = DistributedLock.MemCache;
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
                    Console.Write(ok + " - MemCache OK - " + Thread.CurrentThread.ManagedThreadId);
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
                if (ok % 1000 == 0 && !hasShowInfo)
                {
                    hasShowInfo = true;
                    Console.WriteLine(DistributedCache.MemCache.WorkInfo);
                }
            }
        }
    }
}
