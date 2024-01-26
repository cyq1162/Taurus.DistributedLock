using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DistributedLockTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------------");
            Console.WriteLine("1、LocalLock - Demo。");
            Console.WriteLine("2、FileLock - Demo。");
            Console.WriteLine("3、DataBaseLock - Demo。");
            Console.WriteLine("4、RedisLock - Demo。");
            Console.WriteLine("5、MemCacheLock - Demo。");
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Input num to choose : ");
            string key = Console.ReadLine();
            while (true)
            {
                switch (key)
                {
                    case "2":
                        FileLockDemo.Start();
                        break;
                    case "3":
                        DataBaseLockDemo.Start();
                        break;
                    case "4":
                        RedisLockDemo.Start();
                        break;
                    case "5":
                        MemCacheLockDemo.Start();
                        break;
                    default:
                        LocalLockDemo.Start();
                        break;
                }
                key = Console.ReadLine();
            }

        }
    }
}
