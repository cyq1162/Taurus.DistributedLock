using CYQ.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Taurus.Plugin.DistributedLock
{
    /// <summary>
    /// 相关配置项
    /// </summary>
    public static class DistributedLockConfig
    {
        private static string _LockConn;
        /// <summary>
        /// 数据库锁：链接字符串
        /// 对应 ConnectionStrings 
        /// 配置：LockConn : server=.;database=mslog;uid=sa;pwd=123456;
        /// </summary>
        public static string LockConn
        {
            get
            {
                if (string.IsNullOrEmpty(_LockConn))
                {
                    string conn = AppConfig.GetConn("LockConn");
                    if (string.IsNullOrEmpty(conn))
                    {
                        conn = AppConfig.DB.DefaultConn;
                    }
                    _LockConn = conn;
                }
                return _LockConn;
            }
            set
            {
                _LockConn = value;
            }
        }


        private static string _LockTable;
        /// <summary>
        /// 数据库锁：表名
        /// 对应 Appsettings 
        /// 配置：Lock.Table = Taurus_Lock;
        /// </summary>
        public static string LockTable
        {
            get
            {
                if (string.IsNullOrEmpty(_LockTable))
                {
                    _LockTable = AppConfig.GetApp("Lock.Table", "Taurus_Lock");
                }
                return _LockTable;
            }
            set
            {
                _LockTable = value;
            }
        }

        /// <summary>
        /// Redis分布式缓存的服务器配置，多个用逗号（,）分隔
        /// 格式：ip:port - password
        /// 配置：Redis.Servers = 192.168.1.9:6379 - 888888
        /// </summary>
        public static string RedisServers
        {
            get
            {
                return AppConfig.Redis.Servers;
            }
            set
            {
                AppConfig.Redis.Servers = value;
            }
        }

        /// <summary>
        /// MemCache分布式缓存的服务器配置，多个用逗号（,）分隔
        /// 格式：ip:port
        /// 配置：MemCache.Servers = 192.168.1.9:12121
        /// </summary>
        public static string MemCacheServers
        {
            get
            {
                return AppConfig.MemCache.Servers;
            }
            set
            {
                AppConfig.MemCache.Servers = value;
            }
        }

        /// <summary>
        /// 文件锁：存锁文件目录，默认临时目录
        /// 配置：File.Path = /xxx
        /// </summary>
        public static string FilePath
        {
            get
            {
                return AppConfig.GetApp("File.Path", Path.GetTempPath());
            }
            set
            {
                AppConfig.SetApp("File.Path", value);
            }
        }


    }
}
