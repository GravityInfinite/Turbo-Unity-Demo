using System;
using System.Collections;
using System.Collections.Generic;
using GravityEngine;
using GravityEngine.Wrapper;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;
using GravitySDK.PC.DataModel;
using GravitySDK.PC.GravityTurbo;
using GravitySDK.PC.Storage;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine;
using UnityEngine.Networking;

#if GRAVITY_WECHAT_GAME_MODE
using WeChatWASM;

#elif GRAVITY_BYTEDANCE_GAME_MODE
using StarkSDKSpace;
#endif

namespace GravitySDK.PC.Main
{
    public class GravityPCSDK
    {
        private GravityPCSDK()
        {
        }

        private static GravitySDKInstance _instance;
        private static readonly object Locker = new object();

        private static GravitySDKInstance GetInstance()
        {
            return _instance;
        }

        // 要早于GetInstance调用
        public static GravitySDKInstance Init(string server, string instanceName,
            GravitySDKConfig config = null, MonoBehaviour mono = null)
        {
            lock (Locker)
            {
                if (_instance == null)
                {
                    _instance = new GravitySDKInstance(server, instanceName, config, mono);
                }
            }

            return _instance;
        }

        /// <summary>
        /// 设置访客ID
        /// </summary>
        /// <param name="distinctID"></param>
        public static void Identifiy(string distinctID)
        {
            GetInstance().Identifiy(distinctID);
        }

        public static GravitySDKTimeInter GetTime(DateTime dateTime)
        {
            return GetInstance().GetTime(dateTime);
        }

        /// <summary>
        /// 获取访客ID
        /// </summary>
        /// <returns></returns>
        public static string DistinctId()
        {
            return GetInstance().DistinctId();
        }

        /// <summary>
        /// 设置账号ID
        /// </summary>
        /// <param name="accountID"></param>
        public static void Login(string accountID)
        {
            GetInstance().Login(accountID);
        }

        /// <summary>
        /// 获取账号ID
        /// </summary>
        /// <returns></returns>
        public static string AccountID()
        {
            return GetInstance().AccountID();
        }

        /// <summary>
        ///清空账号ID
        /// </summary>
        public static void Logout()
        {
            GetInstance().Logout();
        }

        /// <summary>
        /// 设置自动采集事件
        /// </summary>
        /// <param name="events"></param>
        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            GetInstance().EnableAutoTrack(events, properties);
        }

        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, IAutoTrackEventCallback_PC eventCallback)
        {
            GetInstance().EnableAutoTrack(events, eventCallback);
        }

        public static void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            GetInstance().SetAutoTrackProperties(events, properties);
        }

        public static void Track(string eventName)
        {
            GetInstance().Track(eventName);
        }

        public static void Track(string eventName, Dictionary<string, object> properties)
        {
            GetInstance().Track(eventName, properties);
        }

        public static void Track(string eventName, Dictionary<string, object> properties, DateTime date)
        {
            GetInstance().Track(eventName, properties, date);
        }

        public static void Track(string eventName, Dictionary<string, object> properties, DateTime date,
            TimeZoneInfo timeZone)
        {
            GetInstance().Track(eventName, properties, date, timeZone);
        }

        public static void Track(GravitySDKEventData analyticsEvent)
        {
            GetInstance().Track(analyticsEvent);
        }

        public static void Flush()
        {
            GetInstance().Flush();
        }

        public static void FlushImmediately()
        {
            GetInstance().FlushImmediately();
        }

        public static void SetSuperProperties(Dictionary<string, object> superProperties)
        {
            GetInstance().SetSuperProperties(superProperties);
        }

        public static void UnsetSuperProperty(string propertyKey)
        {
            GetInstance().UnsetSuperProperty(propertyKey);
        }

        public static Dictionary<string, object> SuperProperties()
        {
            return GetInstance().SuperProperties();
        }

        public static void ClearSuperProperties()
        {
            GetInstance().ClearSuperProperties();
        }

        public static void TimeEvent(string eventName)
        {
            GetInstance().TimeEvent(eventName);
        }

        /// <summary>
        /// 暂停事件的计时
        /// </summary>
        /// <param name="status">暂停状态，ture：暂停计时，false：取消暂停计时</param>
        /// <param name="eventName">事件名称，有值：暂停指定事件计时，无值：暂停全部事件计时</param>
        public static void PauseTimeEvent(bool status, string eventName = "")
        {
            GetInstance().PauseTimeEvent(status, eventName);
        }

        public static void UserSet(Dictionary<string, object> properties)
        {
            GetInstance().UserSet(properties);
        }

        public static void UserSet(Dictionary<string, object> properties, DateTime dateTime)
        {
            GetInstance().UserSet(properties, dateTime);
        }

        public static void UserUnset(string propertyKey)
        {
            GetInstance().UserUnset(propertyKey);
        }

        public static void UserUnset(string propertyKey, DateTime dateTime)
        {
            GetInstance().UserUnset(propertyKey, dateTime);
        }

        public static void UserUnset(List<string> propertyKeys)
        {
            GetInstance().UserUnset(propertyKeys);
        }

        public static void UserUnset(List<string> propertyKeys, DateTime dateTime)
        {
            GetInstance().UserUnset(propertyKeys, dateTime);
        }

        public static void UserSetOnce(Dictionary<string, object> properties)
        {
            GetInstance().UserSetOnce(properties);
        }

        public static void UserSetOnce(Dictionary<string, object> properties, DateTime dateTime)
        {
            GetInstance().UserSetOnce(properties, dateTime);
        }

        public static void UserAdd(Dictionary<string, object> properties)
        {
            GetInstance().UserAdd(properties);
        }

        public static void UserAdd(Dictionary<string, object> properties, DateTime dateTime)
        {
            GetInstance().UserAdd(properties, dateTime);
        }

        public static void UserNumberMin(Dictionary<string, object> properties)
        {
            GetInstance().UserNumberMin(properties);
        }

        public static void UserNumberMin(Dictionary<string, object> properties, DateTime dateTime)
        {
            GetInstance().UserNumberMin(properties, dateTime);
        }

        public static void UserNumberMax(Dictionary<string, object> properties)
        {
            GetInstance().UserNumberMax(properties);
        }

        public static void UserNumberMax(Dictionary<string, object> properties, DateTime dateTime)
        {
            GetInstance().UserNumberMax(properties, dateTime);
        }

        public static void UserAppend(Dictionary<string, object> properties)
        {
            GetInstance().UserAppend(properties);
        }

        public static void UserAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GetInstance().UserAppend(properties, dateTime);
        }

        public static void UserUniqAppend(Dictionary<string, object> properties)
        {
            GetInstance().UserUniqAppend(properties);
        }

        public static void UserUniqAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GetInstance().UserUniqAppend(properties, dateTime);
        }

        public static void UserDelete()
        {
            GetInstance().UserDelete();
        }

        public static void UserDelete(DateTime dateTime)
        {
            GetInstance().UserDelete(dateTime);
        }

        public static void SetDynamicSuperProperties(IDynamicSuperProperties_PC dynamicSuperProperties)
        {
            GetInstance().SetDynamicSuperProperties(dynamicSuperProperties);
        }

        public static void SetTrackStatus(GE_TRACK_STATUS status)
        {
            GetInstance().SetTrackStatus(status);
        }

        /*
        停止或开启数据上报,默认是开启状态,设置为停止时还会清空本地的访客ID,账号ID,静态公共属性
        其中true表示可以上报数据,false表示停止数据上报
        **/
        public static void OptTracking(bool optTracking)
        {
            GetInstance().OptTracking(optTracking);
        }

        //是否暂停数据上报,默认是正常上报状态,其中true表示可以上报数据,false表示暂停数据上报
        public static void EnableTracking(bool isEnable)
        {
            GetInstance().EnableTracking(isEnable);
        }

        //停止数据上报
        public static void OptTrackingAndDeleteUser()
        {
            GetInstance().OptTrackingAndDeleteUser();
        }

        /// <summary>
        /// 通过时间戳校准时间
        /// </summary>
        /// <param name="timestamp"></param>
        public static void CalibrateTime(long timestamp)
        {
            GravitySDKTimestampCalibration timestampCalibration = new GravitySDKTimestampCalibration(timestamp);
            GetInstance().SetTimeCalibratieton(timestampCalibration);
        }

        /// <summary>
        /// 通过NTP服务器校准时间
        /// </summary>
        /// <param name="ntpServer"></param>
        public static void CalibrateTimeWithNtp(string ntpServer)
        {
            GravitySDKNTPCalibration ntpCalibration = new GravitySDKNTPCalibration(ntpServer);
            GetInstance().SetTimeCalibratieton(ntpCalibration);
        }

        /// <summary>
        /// 获取设备ID
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceId()
        {
            return GravitySDKDeviceInfo.DeviceID();
        }

        /// <summary>
        ///
        /// 是否打开客户端日志
        /// </summary>
        /// <param name="isEnable"></param>
        public static void EnableLog(bool isEnable)
        {
            GravitySDKPublicConfig.SetIsPrintLog(isEnable);
        }

        public static void SetLibName(string name)
        {
            GravitySDKPublicConfig.SetName(name);
        }

        public static void SetLibVersion(string versionCode)
        {
            GravitySDKPublicConfig.SetVersion(versionCode);
        }

        public static string TimeString(DateTime dateTime)
        {
            return GetInstance().TimeString(dateTime);
        }

        public static void Register(string name, int version, string wxOpenId, IRegisterCallback registerCallback)
        {
#if GRAVITY_WECHAT_GAME_MODE
            var wxLaunchQuery = WX.GetLaunchOptionsSync().query;
#elif GRAVITY_BYTEDANCE_GAME_MODE
            var wxLaunchQuery = StarkSDK.API.GetLaunchOptionsSync().Query;
#else
            Dictionary<string, string> wxLaunchQuery = new Dictionary<string, string>();
#endif
            Turbo.Register(name, version, wxOpenId, wxLaunchQuery, registerCallback, () =>
            {
                // 自动采集注册事件
                Track("$MPRegister");
                UserSetOnce(new Dictionary<string, object>()
                {
                    {GravitySDKConstant.MANUFACTURE, GravitySDKDeviceInfo.Manufacture()},
                    {GravitySDKConstant.DEVICE_MODEL, GravitySDKDeviceInfo.DeviceModel()},
                    {GravitySDKConstant.DEVICE_BRAND, GravitySDKDeviceInfo.Manufacture().ToUpper()},
                    {GravitySDKConstant.OS, GravitySDKDeviceInfo.OS()},
                    {"$first_visit_time", GravityEngineWrapper.GetTimeString(DateTime.Now)}
                });
                Flush();
            });
        }

        public static void GetBytedanceEcpmRecords(string wxOpenId, string mpId)
        {
            string dateHourStr = GetTime(DateTime.MinValue).GetTimeWithFormat(null, "yyyy-MM-dd HH");
            Turbo.GetBytedanceEcpmRecords(wxOpenId, mpId, dateHourStr);
        }
    }
}