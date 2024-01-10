using UnityEngine;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Storage;

namespace GravitySDK.PC.Utils
{
    public class GravitySDKUtil
    {
        public GravitySDKUtil()
        {
        }

        public static List<string> DisPresetProperties = GravitySDKUtil.GetDisPresetProperties();

        /*
         *判断是否为有效URL
         */
        public static bool IsValiadURL(string url)
        {
            return !(url == null || url.Length == 0 || !url.Contains("http") || !url.Contains("https"));
        }

        /*
         * 判断字符串是否为空
         */
        public static bool IsEmptyString(string str)
        {
            return (str == null || str.Length == 0);
        }

        public static Dictionary<string, object> DeviceInfo()
        {
            Dictionary<string, object> deviceInfo = new Dictionary<string, object>();
            deviceInfo[GravitySDKConstant.DEVICE_ID] = GravitySDKDeviceInfo.DeviceID();
            deviceInfo[GravitySDKConstant.LIB_VERSION] = GravitySDKAppInfo.LibVersion();
            deviceInfo[GravitySDKConstant.LIB] = GravitySDKAppInfo.LibName();
            deviceInfo[GravitySDKConstant.OS] = GravitySDKDeviceInfo.OS();
            deviceInfo[GravitySDKConstant.SCREEN_HEIGHT] = GravitySDKDeviceInfo.ScreenHeight();
            deviceInfo[GravitySDKConstant.SCREEN_WIDTH] = GravitySDKDeviceInfo.ScreenWidth();
            deviceInfo[GravitySDKConstant.MANUFACTURE] = GravitySDKDeviceInfo.Manufacture();
            deviceInfo[GravitySDKConstant.DEVICE_BRAND] = GravitySDKDeviceInfo.Manufacture().ToUpper();
            deviceInfo[GravitySDKConstant.DEVICE_MODEL] = GravitySDKDeviceInfo.DeviceModel();
            deviceInfo[GravitySDKConstant.SYSTEM_LANGUAGE] = GravitySDKDeviceInfo.MachineLanguage();
            deviceInfo[GravitySDKConstant.OS_VERSION] = GravitySDKDeviceInfo.OSVersion();
            deviceInfo[GravitySDKConstant.APP_VERSION] = GravitySDKAppInfo.AppVersion();
            deviceInfo[GravitySDKConstant.NETWORK_TYPE] = GravitySDKDeviceInfo.NetworkType();
            deviceInfo[GravitySDKConstant.APP_BUNDLEID] = GravitySDKAppInfo.AppIdentifier();
            return deviceInfo;
        }

        // 禁用的预置属性
        private static List<string> GetDisPresetProperties()
        {
            List<string> properties = new List<string>();

            TextAsset textAsset = Resources.Load<TextAsset>("ge_public_config");
            if (textAsset != null && textAsset.text != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                // xmlDoc.Load(srcPath);
                xmlDoc.LoadXml(textAsset.text);
                XmlNode root = xmlDoc.SelectSingleNode("resources");
                //遍历节点
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    XmlNode x1 = root.ChildNodes[i];
                    if (x1.NodeType == XmlNodeType.Element)
                    {
                        XmlElement e1 = x1 as XmlElement;
                        if (e1.HasAttributes)
                        {
                            string name = e1.GetAttribute("name");
                            if (name == "GEDisPresetProperties" && e1.HasChildNodes)
                            {
                                for (int j = 0; j < e1.ChildNodes.Count; j++)
                                {
                                    XmlNode x2 = e1.ChildNodes[j];
                                    if (x2.NodeType == XmlNodeType.Element)
                                    {
                                        properties.Add(x2.InnerText);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return properties;
        }

        //随机数持久化,作为访客ID的备选
        public static string RandomID(bool persistent = true)
        {
            string randomID = null;
            if (persistent)
            {
                randomID = (string) GravitySDKFile.GetData(GravitySDKConstant.RANDOM_ID, typeof(string));
            }

            if (string.IsNullOrEmpty(randomID))
            {
                randomID = System.Guid.NewGuid().ToString("N");
                if (persistent)
                {
                    GravitySDKFile.SaveData(GravitySDKConstant.RANDOM_ID, randomID);
                }
            }

            return randomID;
        }

        //获取时区偏移
        public static double ZoneOffset(DateTime dateTime, TimeZoneInfo timeZone)
        {
            bool success = true;
            TimeSpan timeSpan = new TimeSpan();
            try
            {
                timeSpan = timeZone.BaseUtcOffset;
            }
            catch (Exception e)
            {
                success = false;
                //GravitySDKLogger.Print("ZoneOffset: TimeSpan get failed : " + e.Message);
            }

            try
            {
                if (timeZone.IsDaylightSavingTime(dateTime))
                {
                    TimeSpan timeSpan1 = TimeSpan.FromHours(1);
                    timeSpan = timeSpan.Add(timeSpan1);
                }
            }
            catch (Exception e)
            {
                success = false;
                //GravitySDKLogger.Print("ZoneOffset: IsDaylightSavingTime get failed : " + e.Message);
            }

            if (success == false)
            {
                timeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            }

            return timeSpan.TotalHours;
        }

        //时间格式化，转换成字符串的时候会考虑时区
        public static string FormatDateTime(DateTime dateTime, TimeZoneInfo timeZone)
        {
            return GetDateTime(dateTime, timeZone).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string FormatDateTimeWithFormat(DateTime dateTime, TimeZoneInfo timeZone, string format)
        {
            return GetDateTime(dateTime, timeZone).ToString(format, CultureInfo.InvariantCulture);
        }

        // 不考虑时区，只获取utc时间戳
        public static long FormatDateTimeToUtcTimestamp(DateTime dateTime)
        {
            // Debug.Log("lpf_test0 " + dateTime);
            // DateTime currentDateTime = GetDateTime(dateTime, timeZone); // 为什么这里会被加8？因为考虑了时区
            //
            // DateTime d1 =
            //     DateTime.ParseExact(currentDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            //         "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            //
            // // 做了一次转换之后，看看dateTime是否有变化？
            // Debug.Log("lpf_test1 " + currentDateTime + " " + d1 + " equals " + currentDateTime.Equals(d1));
            //
            // long milliseconds = new DateTimeOffset(d1).ToUnixTimeMilliseconds(); // right
            //
            // long m1 = new DateTimeOffset(currentDateTime).ToUnixTimeMilliseconds();
            // long m2 = new DateTimeOffset(d1).ToUnixTimeMilliseconds(); // right
            // long m3 = new DateTimeOffset(dateTime).ToUnixTimeMilliseconds(); // right
            //
            // // 看看最后输出的时间戳是否有变化
            // Debug.Log("lpf_test2 " + " error " + ((DateTimeOffset) currentDateTime).ToUnixTimeMilliseconds() +
            //           " error2 " + ((DateTimeOffset) d1).ToUnixTimeMilliseconds() + // right
            //           " right " + milliseconds + " m1 " + m1 + " m2 " + m2 + " m3 " + m3);
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
            ;
        }

        private static DateTime GetDateTime(DateTime dateTime, TimeZoneInfo timeZone)
        {
            bool success = true;
            DateTime univDateTime = dateTime.ToUniversalTime();
            TimeSpan timeSpan = new TimeSpan();
            try
            {
                timeSpan = timeZone.BaseUtcOffset;
            }
            catch (Exception e)
            {
                success = false;
                //GravitySDKLogger.Print("FormatDate - TimeSpan get failed : " + e.Message);
            }

            try
            {
                if (timeZone.IsDaylightSavingTime(dateTime))
                {
                    TimeSpan timeSpan1 = TimeSpan.FromHours(1);
                    timeSpan = timeSpan.Add(timeSpan1);
                }
            }
            catch (Exception e)
            {
                success = false;
                //GravitySDKLogger.Print("FormatDate: IsDaylightSavingTime get failed : " + e.Message);
            }

            if (success == false)
            {
                timeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            }

            DateTime dateNew = univDateTime + timeSpan;
            return dateNew;
        }

        //向Dictionary添加Dictionary
        public static void AddDictionary(Dictionary<string, object> originalDic, Dictionary<string, object> subDic)
        {
            foreach (KeyValuePair<string, object> kv in subDic)
            {
                originalDic[kv.Key] = kv.Value;
            }
        }

        //获取时间戳
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        private const string LastLaunchDateKey = "GravityLastLaunchDate";
        private static bool _FirstCheckCalled = false;
        private static bool _IsFirstLaunchToday = false;

        public static bool IsFirstLaunchToday()
        {
            if (_FirstCheckCalled)
            {
                return _IsFirstLaunchToday;
            }

            _FirstCheckCalled = true;

            // 获取上次启动的日期
            string lastLaunchDate = (string) GravitySDKFile.GetData(LastLaunchDateKey, typeof(string));

            // 获取当前日期
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            // 如果上次启动的日期为空或不是当天的日期，则返回 true 表示当日首次启动
            if (string.IsNullOrEmpty(lastLaunchDate) || !string.Equals(lastLaunchDate, currentDate))
            {
                // 将当前日期保存为上次启动的日期
                GravitySDKFile.SaveData(LastLaunchDateKey, currentDate);
                _IsFirstLaunchToday = true;
                return true;
            }

            _IsFirstLaunchToday = false;
            return false;
        }
    }
}