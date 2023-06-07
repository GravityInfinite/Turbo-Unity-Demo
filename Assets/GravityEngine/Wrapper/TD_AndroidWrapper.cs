// #if UNITY_ANDROID && !(UNITY_EDITOR)
#if false
using System;
using System.Collections.Generic;
using GravityEngine.Utils;
using GravitySDK.PC.Constant;
using UnityEngine;
using UnityEngine.Networking;

namespace GravityEngine.Wrapper
{
    public partial class GravityEngineWrapper
    {
        private static readonly string JSON_CLASS = "org.json.JSONObject";
        private static readonly AndroidJavaClass sdkClass = new AndroidJavaClass("cn.gravity.android.GravityEngineSDK");
        private static readonly AndroidJavaClass configClass = new AndroidJavaClass("cn.gravity.android.GEConfig");

        private static string default_appId = null;

        /// <summary>
        /// Convert Dictionary object to JSONObject in Java.
        /// </summary>
        /// <returns>The JSONObject instance.</returns>
        /// <param name="data">The Dictionary containing some data </param>
        private static AndroidJavaObject getJSONObject(string dataString)
        {
            if (dataString.Equals("null"))
            {
                return null;
            }

            try
            {
                return new AndroidJavaObject(JSON_CLASS, dataString);
            }
            catch (Exception e)
            {
                GE_Log.w("GravityEngine: unexpected exception: " + e);
            }
            return null;
        }

        private static string getTimeString(DateTime dateTime) {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;

            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;

            AndroidJavaObject date = new AndroidJavaObject("java.util.Date", currentMillis);
            return getInstance(default_appId).Call<string>("getTimeString", date);
        }

        private static AndroidJavaObject getInstance(string appId) {
            AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"); //获得Context
            AndroidJavaObject currentInstance;

            if (string.IsNullOrEmpty(appId))
            {
                appId = default_appId;
            }
            
            currentInstance = sdkClass.CallStatic<AndroidJavaObject>("sharedInstance", context, appId);

            if (currentInstance == null)
            {
                currentInstance = sdkClass.CallStatic<AndroidJavaObject>("sharedInstance", context, default_appId);
            }

            return currentInstance;
        }


        private static void enableLog(bool enable) {
            sdkClass.CallStatic("enableTrackLog", enable);
        }
        private static void setVersionInfo(string libName, string version) {
            sdkClass.CallStatic("setCustomerLibInfo", libName, version);
        }

        private static void init(GravityEngineAPI.Token token)
        {
            if (string.IsNullOrEmpty(default_appId))
            {
                default_appId = token.appid;
            }
            AndroidJavaObject context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"); //获得Context
            AndroidJavaObject config = null;
            if (!string.IsNullOrEmpty(token.GetInstanceName()))
            {
                config = configClass.CallStatic<AndroidJavaObject>("getInstance", context, token.appid, token.GetInstanceName());
            }
            else
            {
                config = configClass.CallStatic<AndroidJavaObject>("getInstance", context, token.appid);
            }
            config.Call("setMode", (int) token.mode);

            string timeZoneId = token.getTimeZoneId();
            if (null != timeZoneId && timeZoneId.Length > 0)
            {
                AndroidJavaObject timeZone = new AndroidJavaClass("java.util.TimeZone").CallStatic<AndroidJavaObject>("getTimeZone", timeZoneId);
                if (null != timeZone)
                {
                    config.Call("setDefaultTimeZone", timeZone);
                }
            }

            if (token.enableEncrypt == true)
            {
                config.Call("enableEncrypt", true);
                AndroidJavaObject secreteKey = new AndroidJavaObject("cn.gravity.android.encrypt.TDSecreteKey", token.encryptPublicKey, token.encryptVersion, "AES", "RSA");
                config.Call("setSecretKey", secreteKey);
            }

            sdkClass.CallStatic<AndroidJavaObject>("sharedInstance", config);
        }

        private static void flush(string appId)
        {
            getInstance(appId).Call("flush");
        }

        private static AndroidJavaObject getDate(DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;

            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            return new AndroidJavaObject("java.util.Date", currentMillis);
        }

        private static void track(string eventName, string properties, DateTime dateTime, string appId)
        {
            AndroidJavaObject date = getDate(dateTime);
            AndroidJavaObject tz = null;
            getInstance(appId).Call("track", eventName, getJSONObject(properties), date, tz);
        }

        private static void track(string eventName, string properties, DateTime dateTime, TimeZoneInfo timeZone, string appId)
        {
            AndroidJavaObject date = getDate(dateTime);
            AndroidJavaObject tz = null;
            if (null != timeZone && null != timeZone.Id && timeZone.Id.Length > 0)
            {
                tz = new AndroidJavaClass("java.util.TimeZone").CallStatic<AndroidJavaObject>("getTimeZone", timeZone.Id);
            }
            getInstance(appId).Call("track", eventName, getJSONObject(properties), date, tz);
        }

        private static void trackForAll(string eventName, string properties, DateTime dateTime, TimeZoneInfo timeZone)
        {
            string appId = "";
            AndroidJavaObject date = getDate(dateTime);
            AndroidJavaObject tz = null;
            if (null != timeZone && null != timeZone.Id && timeZone.Id.Length > 0)
            {
                tz = new AndroidJavaClass("java.util.TimeZone").CallStatic<AndroidJavaObject>("getTimeZone", timeZone.Id);
            }

            getInstance(appId).Call("track", eventName, getJSONObject(properties), date, tz);
        }

        private static void track(string eventName, string properties, string appId)
        {
            getInstance(appId).Call("track", eventName, getJSONObject(properties));
        }

        private static void setSuperProperties(string superProperties, string appId)
        {
            getInstance(appId).Call("setSuperProperties", getJSONObject(superProperties));
        }

        private static void unsetSuperProperty(string superPropertyName, string appId)
        {
            getInstance(appId).Call("unsetSuperProperty", superPropertyName);
        }

        private static void clearSuperProperty(string appId)
        {
            getInstance(appId).Call("clearSuperProperties");
        }

        private static Dictionary<string, object> getSuperProperties(string appId)
        {
            Dictionary<string, object> result = null;
            AndroidJavaObject superPropertyObject = getInstance(appId).Call<AndroidJavaObject>("getSuperProperties");
            if (null != superPropertyObject)
            {
                string superPropertiesString = superPropertyObject.Call<string>("toString");
                result = GE_MiniJson.Deserialize(superPropertiesString);
            }
            return result;
        }
        
        private static void timeEvent(string eventName, string appId)
        {
            getInstance(appId).Call("timeEvent", eventName);
        }

        private static void timeEventForAll(string eventName)
        {
            getInstance("").Call("timeEvent", eventName);
        }

        private static void identify(string uniqueId, string appId)
        {
            getInstance(appId).Call("identify", uniqueId);
        }

        private static string getDistinctId(string appId)
        {
            return getInstance(appId).Call<string>("getDistinctId");
        }

        private static void login(string uniqueId, string appId)
        {
            getInstance(appId).Call("login", uniqueId);
        }

        private static void userSetOnce(string properties, string appId)
        {
            getInstance(appId).Call("user_setOnce", getJSONObject(properties));
        }

        private static void userSetOnce(string properties, DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_setOnce", getJSONObject(properties), getDate(dateTime));
        }

        private static void userSet(string properties, string appId)
        {
            getInstance(appId).Call("user_set", getJSONObject(properties));
        }

        private static void userSet(string properties, DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_set", getJSONObject(properties), getDate(dateTime));
        }

        private static void userUnset(List<string> properties, string appId)
        {
            userUnset(properties, DateTime.Now, appId);
        }

        private static void userUnset(List<string> properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> finalProperties = new Dictionary<string, object>();
            foreach(string s in properties)
            {
                finalProperties.Add(s, 0);
            }

            getInstance(appId).Call("user_unset", getJSONObject(GE_MiniJson.Serialize(finalProperties)), getDate(dateTime));
        }

        private static void userAdd(string properties, string appId)
        {
            getInstance(appId).Call("user_increment", getJSONObject(properties));
        }

        private static void userAdd(string properties, DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_increment", getJSONObject(properties), getDate(dateTime));
        }
        
        private static void userNumberMax(string properties, string appId)
        {
            getInstance(appId).Call("user_max", getJSONObject(properties));
        }

        private static void userNumberMax(string properties, DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_max", getJSONObject(properties), getDate(dateTime));
        }
        
        private static void userNumberMin(string properties, string appId)
        {
            getInstance(appId).Call("user_min", getJSONObject(properties));
        }

        private static void userNumberMin(string properties, DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_min", getJSONObject(properties), getDate(dateTime));
        }

        private static void userAppend(string properties, string appId)
        {
            getInstance(appId).Call("user_append", getJSONObject(properties));
        }

        private static void userAppend(string properties, DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_append", getJSONObject(properties), getDate(dateTime));
        }

        private static void userUniqAppend(string properties, string appId)
        {
            getInstance(appId).Call("user_uniqAppend", getJSONObject(properties));
        }

        private static void userUniqAppend(string properties, DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_uniqAppend", getJSONObject(properties), getDate(dateTime));
        }

        private static void userDelete(string appId)
        {
            getInstance(appId).Call("user_delete");
        }

        private static void userDelete(DateTime dateTime, string appId)
        {
            getInstance(appId).Call("user_delete", getDate(dateTime));
        }

        private static void logout(string appId)
        {
            getInstance(appId).Call("logout");
        }

        private static string getDeviceId()
        {
            return getInstance(default_appId).Call<string>("getDeviceId");
        }

        private static void setDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties, string appId)
        {
            DynamicListenerAdapter listenerAdapter = new DynamicListenerAdapter();
            getInstance(appId).Call("setDynamicSuperPropertiesTrackerListener", listenerAdapter);
        }

        private static void setNetworkType(GravityEngineAPI.NetworkType networkType) {
            switch (networkType)
            {
                case GravityEngineAPI.NetworkType.DEFAULT:
                    getInstance(default_appId).Call("setNetworkType", 0);
                    break;
                case GravityEngineAPI.NetworkType.WIFI:
                    getInstance(default_appId).Call("setNetworkType", 1);
                    break;
                case GravityEngineAPI.NetworkType.ALL:
                    getInstance(default_appId).Call("setNetworkType", 2);
                    break;
            }
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS events, string properties, string appId)
        {
            getInstance(appId).Call("enableAutoTrack", (int) events, getJSONObject(properties));
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS events, IAutoTrackEventCallback eventCallback, string appId)
        {
            AutoTrackListenerAdapter listenerAdapter = new AutoTrackListenerAdapter();
            getInstance(appId).Call("enableAutoTrack", (int) events, listenerAdapter);
        }

        private static void setAutoTrackProperties(AUTO_TRACK_EVENTS events, string properties, string appId)
        {
            getInstance(appId).Call("setAutoTrackProperties", (int) events, getJSONObject(properties));
        }

        private static void setTrackStatus(GE_TRACK_STATUS status, string appId)
        {
            AndroidJavaClass javaClass = new AndroidJavaClass("cn.gravity.android.GravityEngineSDK$GETrackStatus");
            AndroidJavaObject trackStatus;
            switch (status)
            {
                case GE_TRACK_STATUS.PAUSE:
                    trackStatus = javaClass.GetStatic<AndroidJavaObject>("PAUSE");
                    break;
                case GE_TRACK_STATUS.STOP:
                    trackStatus = javaClass.GetStatic<AndroidJavaObject>("STOP");
                    break;
                case GE_TRACK_STATUS.SAVE_ONLY:
                    trackStatus = javaClass.GetStatic<AndroidJavaObject>("SAVE_ONLY");
                    break;
                case GE_TRACK_STATUS.NORMAL:
                default:
                    trackStatus = javaClass.GetStatic<AndroidJavaObject>("NORMAL");
                    break;
            }
            getInstance(appId).Call("setTrackStatus", trackStatus);
        }

        private static void optOutTracking(string appId)
        {
            getInstance(appId).Call("optOutTracking");
        }

        private static void optOutTrackingAndDeleteUser(string appId)
        {
            getInstance(appId).Call("optOutTrackingAndDeleteUser");
        }

        private static void optInTracking(string appId)
        {
            getInstance(appId).Call("optInTracking");
        }

        private static void enableTracking(bool enabled, string appId)
        {
            getInstance(appId).Call("enableTracking", enabled);
        }

        private static void calibrateTime(long timestamp)
        {
            sdkClass.CallStatic("calibrateTime", timestamp);
        }

        private static void calibrateTimeWithNtp(string ntpServer)
        {
            sdkClass.CallStatic("calibrateTimeWithNtp", ntpServer);
        }

        private static void enableThirdPartySharing(TAThirdPartyShareType shareType, string properties, string appId)
        {
            getInstance(appId).Call("enableThirdPartySharing", (int) shareType, getJSONObject(properties));
        }

        //动态公共属性
        public interface IDynamicSuperPropertiesTrackerListener
        {
            string getDynamicSuperPropertiesString();
        }

        private class DynamicListenerAdapter : AndroidJavaProxy {
            public DynamicListenerAdapter() : base("cn.gravity.android.GravityEngineSDK$DynamicSuperPropertiesTrackerListener") {}
            public string getDynamicSuperPropertiesString()
            {
                Dictionary<string, object> ret;
                if (GravityEngineWrapper.mDynamicSuperProperties != null) {
                    ret = GravityEngineWrapper.mDynamicSuperProperties.GetDynamicSuperProperties();
                } 
                else {
                    ret = new Dictionary<string, object>();
                }
                return GE_MiniJson.Serialize(ret);
            }
        }

        //自动采集事件回调
        public interface IAutoTrackEventTrackerListener
        {
            string eventCallback(int type, string properties);
        }

        private class AutoTrackListenerAdapter : AndroidJavaProxy {
            public AutoTrackListenerAdapter() : base("cn.gravity.android.GravityEngineSDK$AutoTrackEventTrackerListener") {}
            string eventCallback(int type, string properties)
            {
                Dictionary<string, object> ret;
                if (GravityEngineWrapper.mAutoTrackEventCallback != null) {
                    Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
                    ret = GravityEngineWrapper.mAutoTrackEventCallback.AutoTrackEventCallback(type, propertiesDic);
                } 
                else {
                    ret = new Dictionary<string, object>();
                }
                return GE_MiniJson.Serialize(ret);
            }
        }
        
        private static void register(string name, int version, string wxOpenId, string wxUnionId, Action<UnityWebRequest> actionResult)
        {
            // TODO 这里的actionResult要修改一下
            sdkClass.CallStatic("register", name, version, wxOpenId, wxUnionId, null);
        }
    }
}
#endif