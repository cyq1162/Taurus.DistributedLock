using CYQ.Data;
using CYQ.Data.Cache;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;



namespace Taurus.Plugin.DistributedLock
{
    internal partial class FileLock3 : DistributedLock
    {
        private static readonly FileLock3 _instance = new FileLock3();
        string folder = string.Empty;
        private FileLock3()
        {
            string path = DistributedLockConfig.FilePath;
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
        public static FileLock3 Instance
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
                return LockType.File;
            }
        }

    }
    internal partial class FileLock3
    {
        private int removeError, createError, writeError, clearError;
        protected override bool AddAll(string key, string value, double cacheMinutes)
        {
            string path = folder + key + ".lock";

            FileInfo info = new FileInfo(path);

            try
            {
                if (info.Exists)
                {
                    if (info.LastWriteTime.AddSeconds(6) > DateTime.Now)
                    {
                        //超时
                        return false;
                    }
                    info.Delete();
                }

                using (FileStream fileStream = info.Create())
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(value);
                    fileStream.Write(bytes, 0, bytes.Length);
                }

                return true;
            }
            catch (Exception err)
            {
                Interlocked.Increment(ref createError);
                return false;
            }
        }
        protected override void RemoveAll(string key)
        {
            try
            {
                string path = folder + key + ".lock";
                System.IO.File.Delete(path);
            }
            catch
            {
                Interlocked.Increment(ref removeError);
            }
        }
        protected override string Get(string key)
        {
            try
            {
                string path = folder + key + ".lock";
                return System.IO.File.ReadAllText(path);
            }
            catch (Exception)
            {

                return null;
            }
        }
        protected override void SetAll(string key, string value, double cacheMinutes)
        {
            try
            {
                string path = folder + key + ".lock";
                System.IO.File.WriteAllText(path, value);
            }
            catch (Exception)
            {


            }

        }
    }


    /// <summary>
    /// 处理文件锁【幂等性】
    /// </summary>
    internal partial class FileLock3
    {
        public override bool Idempotent(string key)
        {
            return Idempotent("Idempotent_" + key, 0);
        }
        public override bool Idempotent(string key, double keepMinutes)
        {
            return AddAll(key, "1", keepMinutes);
            //return IsLockOK("Idempotent_" + key);
        }
    }
}
