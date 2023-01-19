using System;
namespace GravitySDK.PC.Config
{
    public class GravitySDKPublicConfig
    {
        /*
         * 设置默认是否输出日志
         **/
        bool isPrintLog;
        //库版本号
        string version = "1.0";
        //库名称
        string name = "Unity";
        private  static readonly GravitySDKPublicConfig config = null;

        static GravitySDKPublicConfig()
        {
            config = new GravitySDKPublicConfig();
        }

        private static GravitySDKPublicConfig GetConfig()
        {
            return config;
        }
        public GravitySDKPublicConfig()
        {
            isPrintLog = false;
        }
        public static void SetIsPrintLog(bool isPrint)
        {
            GetConfig().isPrintLog = isPrint;
        }
        public static bool IsPrintLog()
        {
            return GetConfig().isPrintLog;
        }
        public static void SetVersion(string libVersion)
        {
            GetConfig().version = libVersion;
        }
        public static void SetName(string libName)
        {
            GetConfig().name = libName;
        }
        public static string Version()
        {
            return GetConfig().version;
        }
        public static string Name()
        {
            return GetConfig().name;
        }

    }
}
