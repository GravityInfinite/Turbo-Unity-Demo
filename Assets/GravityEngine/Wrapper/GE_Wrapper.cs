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
        public static MonoBehaviour sMono;
        private static IDynamicSuperProperties mDynamicSuperProperties;
        private static IAutoTrackEventCallback mAutoTrackEventCallback;
        private static IRegisterCallback mRegisterCallback;

        private static System.Random rnd = new System.Random();

        private static string serilize<T>(Dictionary<string, T> data)
        {
            return GE_MiniJson.Serialize(data, getTimeString);
        }

        public static string GetTimeString(DateTime dateTime)
        {
            return getTimeString(dateTime);
        }

        public static void ShareInstance(GravityEngineAPI.Token token, MonoBehaviour mono, bool initRequired = true)
        {
            sMono = mono;
            if (initRequired) init(token);
        }

        public static void EnableLog(bool enable)
        {
            enableLog(enable);
        }

        public static void SetVersionInfo(string version)
        {
            setVersionInfo("Unity", version);
        }

        public static void Identify(string uniqueId)
        {
            identify(uniqueId);
        }

        public static string GetDistinctId()
        {
            return getDistinctId();
        }

        public static void Login(string accountId)
        {
            login(accountId);
        }

        public static void Logout()
        {
            logout();
        }

        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            enableAutoTrack(events, serilize(properties));
        }

        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, IAutoTrackEventCallback eventCallback)
        {
            mAutoTrackEventCallback = eventCallback;
            enableAutoTrack(events, eventCallback);
        }

        public static void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            setAutoTrackProperties(events, serilize(properties));
        }

        private static string getFinalEventProperties(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);

            if (null != mDynamicSuperProperties)
            {
                Dictionary<string, object> finalProperties = new Dictionary<string, object>();
                GE_PropertiesChecker.MergeProperties(mDynamicSuperProperties.GetDynamicSuperProperties(),
                    finalProperties);
                GE_PropertiesChecker.MergeProperties(properties, finalProperties);
                return serilize(finalProperties);
            }
            else
            {
                return serilize(properties);
            }
        }

        public static void Track(string eventName, Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckString(eventName);
            track(eventName, getFinalEventProperties(properties));
        }

        public static void Track(string eventName, Dictionary<string, object> properties, DateTime datetime)
        {
            GE_PropertiesChecker.CheckString(eventName);
            track(eventName, getFinalEventProperties(properties), datetime);
        }

        public static void Track(string eventName, Dictionary<string, object> properties, DateTime datetime,
            TimeZoneInfo timeZone)
        {
            GE_PropertiesChecker.CheckString(eventName);
            track(eventName, getFinalEventProperties(properties), datetime, timeZone);
        }

        public static void SetSuperProperties(Dictionary<string, object> superProperties)
        {
            GE_PropertiesChecker.CheckProperties(superProperties);
            setSuperProperties(serilize(superProperties));
        }

        public static void UnsetSuperProperty(string superPropertyName)
        {
            GE_PropertiesChecker.CheckString(superPropertyName);
            unsetSuperProperty(superPropertyName);
        }

        public static void ClearSuperProperty()
        {
            clearSuperProperty();
        }


        public static void TimeEvent(string eventName)
        {
            GE_PropertiesChecker.CheckString(eventName);
            timeEvent(eventName);
        }

        public static Dictionary<string, object> GetSuperProperties()
        {
            return getSuperProperties();
        }

        public static void UserSet(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userSet(serilize(properties));
        }

        public static void UserSet(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userSet(serilize(properties), dateTime);
        }

        public static void UserSetOnce(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userSetOnce(serilize(properties));
        }

        public static void UserSetOnce(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userSetOnce(serilize(properties), dateTime);
        }

        public static void UserUnset(List<string> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userUnset(properties);
        }

        public static void UserUnset(List<string> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userUnset(properties, dateTime);
        }

        public static void UserAdd(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userAdd(serilize(properties));
        }

        public static void UserAdd(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userAdd(serilize(properties), dateTime);
        }

        public static void UserNumberMin(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userNumberMin(serilize(properties));
        }

        public static void UserNumberMin(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userNumberMin(serilize(properties), dateTime);
        }

        public static void UserNumberMax(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userNumberMax(serilize(properties));
        }

        public static void UserNumberMax(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userNumberMax(serilize(properties), dateTime);
        }

        public static void UserAppend(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userAppend(serilize(properties));
        }

        public static void UserAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userAppend(serilize(properties), dateTime);
        }

        public static void UserUniqAppend(Dictionary<string, object> properties)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userUniqAppend(serilize(properties));
        }

        public static void UserUniqAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            GE_PropertiesChecker.CheckProperties(properties);
            userUniqAppend(serilize(properties), dateTime);
        }

        public static void UserDelete()
        {
            userDelete();
        }

        public static void UserDelete(DateTime dateTime)
        {
            userDelete(dateTime);
        }

        public static void Flush()
        {
            flush();
        }

        public static void SetNetworkType(GravityEngineAPI.NetworkType networkType)
        {
            setNetworkType(networkType);
        }

        public static string GetDeviceId()
        {
            return getDeviceId();
        }

        public static void SetDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            if (!GE_PropertiesChecker.CheckProperties(dynamicSuperProperties.GetDynamicSuperProperties()))
            {
                GE_Log.d("GE.Wrapper - Cannot set dynamic super properties due to invalid properties.");
            }

            mDynamicSuperProperties = dynamicSuperProperties;
            setDynamicSuperProperties(dynamicSuperProperties);
        }

        public static void SetTrackStatus(GE_TRACK_STATUS status)
        {
            setTrackStatus(status);
        }

        public static void CalibrateTime(long timestamp)
        {
            calibrateTime(timestamp);
        }

        public static void CalibrateTimeWithNtp(string ntpServer)
        {
            calibrateTimeWithNtp(ntpServer);
        }

        public static void Register(string name, int version, string wxOpenId, IRegisterCallback registerCallback)
        {
            mRegisterCallback = registerCallback;
            register(name, version, wxOpenId, registerCallback);
        }

        public static void ReportBytedanceAdToGravity(string wxOpenId, string adUnitId)
        {
            reportBytedanceAdToGravity(wxOpenId, adUnitId);
        }
    }
}