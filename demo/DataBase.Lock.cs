using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Taurus.Plugin.DistributedLock;

namespace DistributedLockTest
{
    class DataBaseLockDemo
    {
        static private DistributedLock dsLock;
        public static void Start()
        {
            DistributedLockConfig.LockConn = "server=.;database=mslog;uid=sa;pwd=123456";//由数据库链接决定启用什么链接
            DistributedLockConfig.LockTable = "taurus_lock";

            //使用全局默认配置
            dsLock = DistributedLock.DataBase;

            //使用临时配置
            //dsLock = DistributedLock.GetDataBaseLock("myCustomTable");

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
                    isOK = dsLock.Lock(key, 30000);
                    dsLock.UnLock(key);
                    Interlocked.Increment(ref ok);
                    Console.Write(ok + " - DataBase OK - " + Thread.CurrentThread.ManagedThreadId);
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
