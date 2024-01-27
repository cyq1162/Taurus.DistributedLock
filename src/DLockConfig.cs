using CYQ.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Taurus.Plugin.DistributedLock
{
    /// <summary>
    /// 分布式锁配置
    /// </summary>
    public static class DLockConfig
    {
        /// <summary>
        /// 数据库锁：链接字符串
        /// 对应 ConnectionStrings 
        /// 配置：LockConn : server=.;database=mslog;uid=sa;pwd=123456;
        /// </summary>
        public static string Conn
        {
            get
            {
                return AppConfig.GetConn("LockConn");
            }
            set
            {
                AppConfig.SetConn("LockConn", value);
            }
        }

        /// <summary>
        /// 数据库锁：表名
        /// 对应 Appsettings 
        /// 配置：Lock.TableName = Taurus_Lock;
        /// </summary>
        public static string TableName
        {
            get
            {
                return AppConfig.GetApp("Lock.TableName", "Taurus_Lock");
            }
            set
            {
                AppConfig.GetApp("Lock.TableName", value);
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
        /// 配置：Lock.Path = /xxx
        /// </summary>
        public static string Path 
        {
            get
            {
                return AppConfig.GetApp("Lock.Path", System.IO.Path.GetTempPath());
            }
            set
            {
                AppConfig.SetApp("Lock.Path", value);
            }
        }


    }
}
