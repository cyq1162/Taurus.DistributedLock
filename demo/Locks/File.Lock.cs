using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Taurus.Plugin.DistributedLock;

namespace DistributedLockTest
{
    class FileLockDemo
    {
        static private DLock dsLock;
        public static void Start()
        {
            dsLock = DLock.File;

            for (int i = 1; i <= 10000; i++)
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
                    Console.Write(ok + " - File OK - " + Thread.CurrentThread.ManagedThreadId);
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
            }
        }
    }
}
