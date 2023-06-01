#if  (!(UNITY_IOS) || UNITY_EDITOR) && (!(UNITY_ANDROID) || UNITY_EDITOR)
using System;
using System.Collections.Generic;
using GravityEngine.Utils;
using GravitySDK.PC.Main;
using GravitySDK.PC.Utils;
using GravitySDK.PC.DataModel;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;

namespace GravityEngine.Wrapper
{
    public partial class GravityEngineWrapper: IDynamicSuperProperties_PC, IAutoTrackEventCallback_PC
    {
        static IAutoTrackEventCallback mEventCallback;
        public Dictionary<string, object> GetDynamicSuperProperties_PC()
        {
            if (mDynamicSuperProperties != null)
            {
                return mDynamicSuperProperties.GetDynamicSuperProperties();
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }

        public Dictionary<string, object> AutoTrackEventCallback_PC(int type, Dictionary<string, object>properties)
        {
            if (mEventCallback != null)
            {
                return mEventCallback.AutoTrackEventCallback(type, properties);
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }

        private static void init(GravityEngineAPI.Token token)
        {
            GravitySDKConfig config = GravitySDKConfig.GetInstance(token.appid, GravitySDKConstant.SERVER_URL, token.GetInstanceName());
            if (!string.IsNullOrEmpty(token.getTimeZoneId()))
            {
                try
                {
                    config.SetTimeZone(TimeZoneInfo.FindSystemTimeZoneById(token.getTimeZoneId()));
                }
                catch (Exception e)
                {
                    //GravitySDKLogger.Print("TimeZoneInfo set failed : " + e.Message);
                }
            }
            if (token.mode == GravityEngineAPI.SDKRunMode.DEBUG)
            {
                config.SetMode(Mode.DEBUG);
            }
            GravityPCSDK.Init(token.appid, GravitySDKConstant.SERVER_URL, token.GetInstanceName(), config, sMono);
        }

        private static void identify(string uniqueId, string appId)
        {
            GravityPCSDK.Identifiy(uniqueId, appId);
        }

        private static string getDistinctId(string appId)
        {
            return GravityPCSDK.DistinctId(appId);
        }

        private static void login(string accountId, string appId)
        {
            GravityPCSDK.Login(accountId, appId);
        }

        private static void logout(string appId)
        {
            GravityPCSDK.Logout(appId);
        }

        private static void flush(string appId)
        {
           GravityPCSDK.Flush(appId);
        }

        private static void setVersionInfo(string lib_name, string lib_version) {
            GravityPCSDK.SetLibName(lib_name);
            GravityPCSDK.SetLibVersion(lib_version);
        }
        
        private static void track(string eventName, string properties, string appId)
        {  
            Dictionary<string,object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.Track(eventName,propertiesDic,appId);
        }

        private static void track(string eventName, string properties, DateTime dateTime, string appId)
        {
            Dictionary<string,object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.Track(eventName,propertiesDic,dateTime,appId);
        }

        private static void track(string eventName, string properties, DateTime dateTime, TimeZoneInfo timeZone, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.Track(eventName, propertiesDic, dateTime, timeZone, appId);
        }

        private static void trackForAll(string eventName, string properties, DateTime dateTime, TimeZoneInfo timeZone)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.TrackForAll(eventName, propertiesDic, dateTime, timeZone);
        }

        private static void setSuperProperties(string superProperties, string appId)
        {
            Dictionary<string, object> superPropertiesDic = GE_MiniJson.Deserialize(superProperties);
            GravityPCSDK.SetSuperProperties(superPropertiesDic,appId);
        }

        private static void unsetSuperProperty(string superPropertyName, string appId)
        {
            GravityPCSDK.UnsetSuperProperty(superPropertyName,appId);
        }

        private static void clearSuperProperty(string appId)
        {
            GravityPCSDK.ClearSuperProperties(appId);
        }

        private static Dictionary<string, object> getSuperProperties(string appId)
        {
            return GravityPCSDK.SuperProperties(appId);
        }

        private static Dictionary<string, object> getPresetProperties(string appId)
        {
            return GravityPCSDK.PresetProperties(appId);
        }
        private static void timeEvent(string eventName, string appId)
        {
            GravityPCSDK.TimeEvent(eventName,appId);
        }
        private static void timeEventForAll(string eventName)
        {
            GravityPCSDK.TimeEventForAll(eventName);
        }

        private static void userSet(string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSet(propertiesDic,appId);
        }

        private static void userSet(string properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSet(propertiesDic,dateTime,appId);
        }

        private static void userUnset(List<string> properties, string appId)
        {
            GravityPCSDK.UserUnset(properties,appId);
        }

        private static void userUnset(List<string> properties, DateTime dateTime, string appId)
        {
            GravityPCSDK.UserUnset(properties,dateTime,appId);
        }

        private static void userSetOnce(string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSetOnce(propertiesDic, appId);
        }

        private static void userSetOnce(string properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSetOnce(propertiesDic,dateTime,appId);
        }

        private static void userAdd(string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAdd(propertiesDic,appId);
        }

        private static void userAdd(string properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAdd(propertiesDic,dateTime,appId);
        }
        
        private static void userNumberMin(string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMin(propertiesDic,appId);
        }

        private static void userNumberMin(string properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMin(propertiesDic,dateTime,appId);
        }
        
        private static void userNumberMax(string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMax(propertiesDic,appId);
        }

        private static void userNumberMax(string properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMax(propertiesDic,dateTime,appId);
        }

        private static void userDelete(string appId)
        {
            GravityPCSDK.UserDelete(appId);
        }

        private static void userDelete(DateTime dateTime, string appId)
        {
            GravityPCSDK.UserDelete(dateTime,appId);
        }

        private static void userAppend(string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAppend(propertiesDic,appId);
        }

        private static void userAppend(string properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAppend(propertiesDic,dateTime,appId);
        }

        private static void userUniqAppend(string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserUniqAppend(propertiesDic,appId);
        }

        private static void userUniqAppend(string properties, DateTime dateTime, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserUniqAppend(propertiesDic,dateTime,appId);
        }

        private static void setNetworkType(GravityEngineAPI.NetworkType networkType)
        {
            
        }

        private static string getDeviceId() 
        {
            return GravityPCSDK.GetDeviceId();
        }

        private static void setDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties, string appId)
        {
            GravityPCSDK.SetDynamicSuperProperties(new GravityEngineWrapper());
        }

        private static void setTrackStatus(GE_TRACK_STATUS status, string appId)
        {
            GravityPCSDK.SetTrackStatus((GE_TRACK_STATUS)status, appId);
        }

        private static string createLightInstance()
        {
            return GravityPCSDK.CreateLightInstance();
        }

        private static string getTimeString(DateTime dateTime)
        {
            return GravityPCSDK.TimeString(dateTime);
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            AUTO_TRACK_EVENTS pcAutoTrackEvents = AUTO_TRACK_EVENTS.NONE;
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_INSTALL;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_START;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_END;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_CRASH;
            }
#if GRAVITY_WECHAT_GAME_MODE
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHOW) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHOW;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_HIDE) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_HIDE;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHARE) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHARE;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES;
            }
#endif
            
            GravityPCSDK.EnableAutoTrack(pcAutoTrackEvents, propertiesDic, appId);
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, IAutoTrackEventCallback eventCallback, string appId)
        {
            AUTO_TRACK_EVENTS pcAutoTrackEvents = AUTO_TRACK_EVENTS.NONE;
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_INSTALL;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_START;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_END;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.APP_CRASH;
            }
#if GRAVITY_WECHAT_GAME_MODE
           if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHOW) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHOW;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_HIDE) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_HIDE;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_SHARE) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_SHARE;
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES) != 0)
            {
                pcAutoTrackEvents = pcAutoTrackEvents | AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES;
            } 
#endif
            mEventCallback = eventCallback;
            GravityPCSDK.EnableAutoTrack(pcAutoTrackEvents, new GravityEngineWrapper());
        }

        private static void setAutoTrackProperties(AUTO_TRACK_EVENTS autoTrackEvents, string properties, string appId)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_INSTALL, propertiesDic, appId);
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_START, propertiesDic, appId);
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_END, propertiesDic, appId);
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_CRASH, propertiesDic, appId);
            }
        }

        private static void enableLog(bool enable)
        {
            GravityPCSDK.EnableLog(enable);
        }
        private static void calibrateTime(long timestamp)
        {
            GravityPCSDK.CalibrateTime(timestamp);
        }

        private static void calibrateTimeWithNtp(string ntpServer)
        {
            GravityPCSDK.CalibrateTimeWithNtp(ntpServer);
        }

        private static void enableThirdPartySharing(TAThirdPartyShareType shareType, string properties, string appId)
        {
            GravitySDKLogger.Print("Third Party Sharing is not support on PC: " + shareType + ", " + properties + ", "+ appId);
        }
    }
}
#endif