using CYQ.Data;
using CYQ.Data.Cache;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;



namespace Taurus.Plugin.DistributedLock
{
    internal partial class FileLock : DLock
    {
        private static readonly FileLock _instance = new FileLock();
        string folder = string.Empty;
        private FileLock()
        {
            string path = DLockConfig.Path;
            if (!path.Contains(":") && !path.StartsWith("/tmp"))
            {
                //自定义路径
                path = AppConfig.WebRootPath + path.TrimStart('/', '\\');
            }
            folder = path.TrimEnd('/', '\\') + "/TaurusFileLock/";
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
        }
        public static FileLock Instance
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
                return DLockType.File;
            }
        }

    }

    internal partial class FileLock
    {
        private int removeError, createError, writeError, clearError;
        protected override bool OnLock(string key, string value, int millisecondsTimeout)
        {
            //Console.WriteLine("R：" + removeError + " , C：" + createError + " , W：" + writeError + " , C：" + clearError);

            int sleep = 5;
            int count = millisecondsTimeout;
            while (true)
            {
                if (!workKeysDic.ContainsKey(key) && IsLockOK(key))
                {
                    AddToWork(key, "1");
                    return true;
                }
                CheckTimeout(key);
                Thread.Sleep(sleep);
                count -= sleep;
                if (sleep < 1000)
                {
                    sleep += 5;
                }
                if (count <= 0)
                {
                    return false;
                }

            }
        }

        protected override void OnUnLock(string key, string flag)
        {
            try
            {
                string path = folder + key + ".lock";
                var lockObj = workKeysDic[key];
                if (lockObj == null)
                {
                    return;
                }
                workKeysDic.Remove(key);
                checkKeysDic.Remove(key);
                lock (lockObj)
                {
                    System.IO.File.Delete(path);
                }

            }
            catch (Exception err)
            {

            }
        }

        private bool IsLockOK(string key)
        {
            string path = folder + key + ".lock";
            try
            {
                if (System.IO.File.Exists(path))
                {
                    return false;
                }
                System.IO.File.Create(path).Close();
                return true;
            }
            catch (Exception err)
            {

            }
            return false;
        }

        #region 定时工作 - 续时
        readonly object workLockObj = new object();
        MDictionary<string, string> workKeysDic = new MDictionary<string, string>();
        bool threadIsWorking = false;
        private void AddToWork(string key, string value)
        {
            workKeysDic.Add(key, value);
            if (!threadIsWorking)
            {
                lock (workLockObj)
                {
                    if (!threadIsWorking)
                    {
                        threadIsWorking = true;
                        new Thread(new ThreadStart(DoLockWork), 512).Start();
                    }
                }
            }
        }

        private void DoLockWork()
        {
            try
            {
                while (true)
                {
                    if (workKeysDic.Count > 0)
                    {
                        List<string> list = workKeysDic.GetKeys();
                        foreach (string key in list)
                        {
                            //给 key 设置延时时间
                            if (workKeysDic.ContainsKey(key))
                            {
                                string path = folder + key + ".lock";
                                var lockObj = workKeysDic[key];
                                if (lockObj != null)
                                {
                                    lock (lockObj)
                                    {
                                        if (System.IO.File.Exists(path))
                                        {
                                            System.IO.File.WriteAllText(path, "1"); //延时锁：6秒
                                        }
                                    }
                                }
                            }
                        }
                        list.Clear();
                        Thread.Sleep(5000);//循环。
                    }
                    else
                    {
                        threadIsWorking = false;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                //Interlocked.Increment(ref writeError);
                threadIsWorking = false;
            }
        }



        #endregion

        #region 定时工作 - 过期检测
        readonly object timeoutLockObj = new object();
        bool timeoutThreadIsWorking = false;
        MDictionary<string, string> checkKeysDic = new MDictionary<string, string>();
        private void CheckTimeout(string key)
        {
            checkKeysDic.Add(key, "1");
            if (timeoutThreadIsWorking) { return; }
            lock (timeoutLockObj)
            {
                if (!timeoutThreadIsWorking)
                {
                    timeoutThreadIsWorking = true;
                    new Thread(new ThreadStart(RemoveTimeoutLock), 512).Start();
                }
            }
        }
        /// <summary>
        /// 清除过期锁
        /// </summary>
        private void RemoveTimeoutLock()
        {
            try
            {
                while (true)
                {
                    List<string> keys = checkKeysDic.GetKeys();
                    if (keys.Count > 0)
                    {
                        foreach (var key in keys)
                        {
                            if (workKeysDic.ContainsKey(key))
                            {
                                checkKeysDic.Remove(key);
                                continue;
                            }
                            FileInfo fi = new FileInfo(folder + key + ".lock");
                            if (fi.Exists && fi.LastWriteTime.AddSeconds(6) < DateTime.Now)
                            {
                                checkKeysDic.Remove(key);
                                fi.Delete();
                            }

                        }
                        Thread.Sleep(6000);
                    }
                    else
                    {
                        timeoutThreadIsWorking = false;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                //Interlocked.Increment(ref clearError);
                timeoutThreadIsWorking = false;
            }
        }
        #endregion
    }

    /// <summary>
    /// 处理文件锁【幂等性】
    /// </summary>
    internal partial class FileLock
    {

        private static Mutex GetMutex(string fileName)
        {
            string key = "IO" + fileName.GetHashCode();
            var mutex = new Mutex(false, key);
            try
            {
                mutex.WaitOne();
            }
            catch (AbandonedMutexException ex)
            {
                //其它进程直接关闭，未释放即退出时【锁未对外开放，因此不存在重入锁问题，释放1次即可】。
                mutex.ReleaseMutex();
                mutex.WaitOne();
            }
            return mutex;
        }
    }
}
