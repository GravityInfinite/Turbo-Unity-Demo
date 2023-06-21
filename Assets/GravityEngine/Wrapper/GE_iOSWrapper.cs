﻿#if UNITY_IOS && !(UNITY_EDITOR)

// #if true
using System;
using System.Collections.Generic;
using GravityEngine.Utils;
using GravitySDK.PC.Constant;
using GravitySDK.PC.GravityTurbo;
using UnityEngine;
using UnityEngine.Networking;

namespace GravityEngine.Wrapper
{
    public partial class GravityEngineWrapper
    {
        // private static readonly string JSON_CLASS = "org.json.JSONObject";
        // private static readonly AndroidJavaClass sdkClass = new AndroidJavaClass("cn.gravity.android.GravityEngineSDK");
        //
        // private static readonly AndroidJavaObject unityAPIInstance = new AndroidJavaObject("cn.gravity.engine.GravityEngineUnityAPI");

        private static GravityEngineAPI.Token mToken;

        /// <summary>
        /// Convert Dictionary object to JSONObject in Java.
        /// </summary>
        /// <returns>The JSONObject instance.</returns>
        /// <param name="data">The Dictionary containing some data </param>
        // private static AndroidJavaObject getJSONObject(string dataString)
        // {
        //     if (dataString.Equals("null"))
        //     {
        //         return null;
        //     }
        //
        //     try
        //     {
        //         return new AndroidJavaObject(JSON_CLASS, dataString);
        //     }
        //     catch (Exception e)
        //     {
        //         GE_Log.w("GravityEngine: unexpected exception: " + e);
        //     }
        //
        //     return null;
        // }

        private static string getTimeString(DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            //
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            // long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            //
            // AndroidJavaObject date = new AndroidJavaObject("java.util.Date", currentMillis);
            return dtFrom.ToString();
        }

        // private static AndroidJavaObject getInstance()
        // {
        //     AndroidJavaObject context =
        //         new AndroidJavaClass("com.unity3d.player.UnityPlayer")
        //             .GetStatic<AndroidJavaObject>("currentActivity"); //获得Context
        //     AndroidJavaObject currentInstance;
        //
        //     currentInstance = sdkClass.CallStatic<AndroidJavaObject>("sharedInstance", context, mToken.accessToken);
        //     return currentInstance;
        // }


        private static void enableLog(bool enable)
        {
            // sdkClass.CallStatic("enableTrackLog", enable);
        }

        private static void setVersionInfo(string libName, string version)
        {
            // sdkClass.CallStatic("setCustomerLibInfo", libName, version);
        }

        private static void init(GravityEngineAPI.Token token)
        {
            // mToken = token;
            // AndroidJavaObject context =
            //     new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            //         .GetStatic<AndroidJavaObject>("currentActivity"); //获得Context
            //
            // Dictionary<string, object> configDic = new Dictionary<string, object>();
            // configDic["accessToken"] = token.accessToken;
            // configDic["mode"] = (int) token.mode;
            // configDic["aesKey"] = token.aesKey;
            //
            // string timeZoneId = token.getTimeZoneId();
            // if (null != timeZoneId && timeZoneId.Length > 0)
            // {
            //     configDic["timeZone"] = timeZoneId;
            // }
            //
            // if (token.enableEncrypt)
            // {
            //     configDic["enableEncrypt"] = true;
            //     configDic["secretKey"] = new Dictionary<string, object>() {
            //         {"publicKey", token.encryptPublicKey},
            //         {"version", token.encryptVersion},
            //         {"symmetricEncryption", "AES"},
            //         {"asymmetricEncryption", "RSA"},
            //     };
            // }
            //
            // unityAPIInstance.Call("sharedInstance", context, GE_MiniJson.Serialize(configDic));
        }

        private static void flush()
        {
            // getInstance().Call("flush");
        }

        // private static AndroidJavaObject getDate(DateTime dateTime)
        // {
        //     long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
        //
        //     DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //     long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
        //     return new AndroidJavaObject("java.util.Date", currentMillis);
        // }

        private static void track(string eventName, string properties, DateTime dateTime)
        {
            // AndroidJavaObject date = getDate(dateTime);
            // AndroidJavaObject tz = null;
            // getInstance().Call("track", eventName, getJSONObject(properties), date, tz);
        }

        private static void track(string eventName, string properties, DateTime dateTime, TimeZoneInfo timeZone)
        {
            // AndroidJavaObject date = getDate(dateTime);
            // AndroidJavaObject tz = null;
            // if (null != timeZone && null != timeZone.Id && timeZone.Id.Length > 0)
            // {
            //     tz = new AndroidJavaClass("java.util.TimeZone").CallStatic<AndroidJavaObject>("getTimeZone",
            //         timeZone.Id);
            // }
            //
            // getInstance().Call("track", eventName, getJSONObject(properties), date, tz);
        }

        private static void track(string eventName, string properties)
        {
            // getInstance().Call("track", eventName, getJSONObject(properties));
        }

        private static void setSuperProperties(string superProperties)
        {
            // getInstance().Call("setSuperProperties", getJSONObject(superProperties));
        }

        private static void unsetSuperProperty(string superPropertyName)
        {
            // getInstance().Call("unsetSuperProperty", superPropertyName);
        }

        private static void clearSuperProperty()
        {
            // getInstance().Call("clearSuperProperties");
        }

        private static Dictionary<string, object> getSuperProperties()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            // AndroidJavaObject superPropertyObject = getInstance().Call<AndroidJavaObject>("getSuperProperties");
            // if (null != superPropertyObject)
            // {
            //     string superPropertiesString = superPropertyObject.Call<string>("toString");
            //     result = GE_MiniJson.Deserialize(superPropertiesString);
            // }

            return result;
        }

        private static void timeEvent(string eventName)
        {
            // getInstance().Call("timeEvent", eventName);
        }

        private static void identify(string uniqueId)
        {
            // getInstance().Call("identify", uniqueId);
        }

        private static string getDistinctId()
        {
            // return getInstance().Call<string>("getDistinctId");
            return "";
        }

        private static void login(string uniqueId)
        {
            // getInstance().Call("login", uniqueId);
        }

        private static void userSetOnce(string properties)
        {
            // getInstance().Call("user_setOnce", getJSONObject(properties));
        }

        private static void userSetOnce(string properties, DateTime dateTime)
        {
            // getInstance().Call("user_setOnce", getJSONObject(properties), getDate(dateTime));
        }

        private static void userSet(string properties)
        {
            // getInstance().Call("user_set", getJSONObject(properties));
        }

        private static void userSet(string properties, DateTime dateTime)
        {
            // getInstance().Call("user_set", getJSONObject(properties), getDate(dateTime));
        }

        private static void userUnset(List<string> properties)
        {
            // userUnset(properties, DateTime.Now);
        }

        private static void userUnset(List<string> properties, DateTime dateTime)
        {
            // Dictionary<string, object> finalProperties = new Dictionary<string, object>();
            // foreach (string s in properties)
            // {
            //     finalProperties.Add(s, 0);
            // }
            //
            // getInstance().Call("user_unset", getJSONObject(GE_MiniJson.Serialize(finalProperties)), getDate(dateTime));
        }

        private static void userAdd(string properties)
        {
            // getInstance().Call("user_increment", getJSONObject(properties));
        }

        private static void userAdd(string properties, DateTime dateTime)
        {
            // getInstance().Call("user_increment", getJSONObject(properties), getDate(dateTime));
        }

        private static void userNumberMax(string properties)
        {
            // getInstance().Call("user_max", getJSONObject(properties));
        }

        private static void userNumberMax(string properties, DateTime dateTime)
        {
            // getInstance().Call("user_max", getJSONObject(properties), getDate(dateTime));
        }

        private static void userNumberMin(string properties)
        {
            // getInstance().Call("user_min", getJSONObject(properties));
        }

        private static void userNumberMin(string properties, DateTime dateTime)
        {
            // getInstance().Call("user_min", getJSONObject(properties), getDate(dateTime));
        }

        private static void userAppend(string properties)
        {
            // getInstance().Call("user_append", getJSONObject(properties));
        }

        private static void userAppend(string properties, DateTime dateTime)
        {
            // getInstance().Call("user_append", getJSONObject(properties), getDate(dateTime));
        }

        private static void userUniqAppend(string properties)
        {
            // getInstance().Call("user_uniqAppend", getJSONObject(properties));
        }

        private static void userUniqAppend(string properties, DateTime dateTime)
        {
            // getInstance().Call("user_uniqAppend", getJSONObject(properties), getDate(dateTime));
        }

        private static void userDelete()
        {
            // getInstance().Call("user_delete");
        }

        private static void userDelete(DateTime dateTime)
        {
            // getInstance().Call("user_delete", getDate(dateTime));
        }

        private static void logout()
        {
            // getInstance().Call("logout");
        }

        private static string getDeviceId()
        {
            // return getInstance().Call<string>("getDeviceId");
            return "";
        }

        private static void setDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            // DynamicListenerAdapter listenerAdapter = new DynamicListenerAdapter();
            // unityAPIInstance.Call("setDynamicSuperPropertiesTrackerListener", listenerAdapter);
        }

        private static void setNetworkType(GravityEngineAPI.NetworkType networkType)
        {
            // Dictionary<string, object> properties = new Dictionary<string, object>() { };
            // switch (networkType)
            // {
            //     case GravityEngineAPI.NetworkType.DEFAULT:
            //         properties["network_type"] = 0;
            //         break;
            //     case GravityEngineAPI.NetworkType.WIFI:
            //         properties["network_type"] = 1;
            //         break;
            //     case GravityEngineAPI.NetworkType.ALL:
            //         properties["network_type"] = 2;
            //         break;
            // }
            // unityAPIInstance.Call("setNetworkType", GE_MiniJson.Serialize(properties));
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS events, string properties)
        {
            // Dictionary<string, object> propertiesNew = new Dictionary<string, object>()
            // {
            //     {"autoTrackType", (int) events}
            // };
            //
            // unityAPIInstance.Call("enableAutoTrack", GE_MiniJson.Serialize(propertiesNew));
            // propertiesNew["properties"] = GE_MiniJson.Deserialize(properties);
            // unityAPIInstance.Call("setAutoTrackProperties", GE_MiniJson.Serialize(propertiesNew));
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS events, IAutoTrackEventCallback eventCallback)
        {
            // AutoTrackListenerAdapter listenerAdapter = new AutoTrackListenerAdapter();
            // Dictionary<string, object> properties = new Dictionary<string, object>()
            // {
            //     {"autoTrackType", (int) events}
            // };
            // unityAPIInstance.Call("enableAutoTrack", GE_MiniJson.Serialize(properties), listenerAdapter);
        }

        private static void setAutoTrackProperties(AUTO_TRACK_EVENTS events, string properties)
        {
            // Dictionary<string, object> propertiesNew = new Dictionary<string, object>()
            // {
            //     {"autoTrackType", (int) events}
            // };
            // propertiesNew["properties"] = GE_MiniJson.Deserialize(properties);
            //
            // unityAPIInstance.Call("setAutoTrackProperties", GE_MiniJson.Serialize(propertiesNew));
        }

        private static void setTrackStatus(GE_TRACK_STATUS status)
        {
            // AndroidJavaClass javaClass = new AndroidJavaClass("cn.gravity.android.GravityEngineSDK$GETrackStatus");
            // AndroidJavaObject trackStatus;
            // switch (status)
            // {
            //     case GE_TRACK_STATUS.PAUSE:
            //         trackStatus = javaClass.GetStatic<AndroidJavaObject>("PAUSE");
            //         break;
            //     case GE_TRACK_STATUS.STOP:
            //         trackStatus = javaClass.GetStatic<AndroidJavaObject>("STOP");
            //         break;
            //     case GE_TRACK_STATUS.SAVE_ONLY:
            //         trackStatus = javaClass.GetStatic<AndroidJavaObject>("SAVE_ONLY");
            //         break;
            //     case GE_TRACK_STATUS.NORMAL:
            //     default:
            //         trackStatus = javaClass.GetStatic<AndroidJavaObject>("NORMAL");
            //         break;
            // }

            // getInstance().Call("setTrackStatus", trackStatus);
        }

        private static void optOutTracking()
        {
            // getInstance().Call("optOutTracking");
        }

        private static void optOutTrackingAndDeleteUser()
        {
            // getInstance().Call("optOutTrackingAndDeleteUser");
        }

        private static void optInTracking()
        {
            // getInstance().Call("optInTracking");
        }

        private static void enableTracking(bool enabled)
        {
            // getInstance().Call("enableTracking", enabled);
        }

        private static void calibrateTime(long timestamp)
        {
            // sdkClass.CallStatic("calibrateTime", timestamp);
        }

        private static void calibrateTimeWithNtp(string ntpServer)
        {
            // sdkClass.CallStatic("calibrateTimeWithNtp", ntpServer);
        }

        private static void register(string name, int version, string wxOpenId, IRegisterCallback registerCallback)
        {
            // RegisterListenerAdapter listenerAdapter = new RegisterListenerAdapter();
            // getInstance().Call("register", Turbo.GetAccessToken(), Turbo.GetClientId(), name, Turbo.GetChannel(),
            //     listenerAdapter);
        }

        private static void reportBytedanceAdToGravity(string wxOpenId, string adUnitId)
        {
            // GE_Log.d("android not support");
        }
    }
}
#endif