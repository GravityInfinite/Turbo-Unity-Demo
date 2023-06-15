// #if  ((!(UNITY_IOS) || UNITY_EDITOR) && (!(UNITY_ANDROID) || UNITY_EDITOR)) || GRAVITY_IOS_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE
#if false
using System;
using System.Collections.Generic;
using GravityEngine.Utils;
using GravitySDK.PC.Main;
using GravitySDK.PC.Utils;
using GravitySDK.PC.DataModel;
using GravitySDK.PC.Config;
using GravitySDK.PC.Constant;
using UnityEngine.Networking;

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

        private static void identify(string uniqueId)
        {
            GravityPCSDK.Identifiy(uniqueId);
        }

        private static string getDistinctId()
        {
            return GravityPCSDK.DistinctId();
        }

        private static void login(string accountId)
        {
            GravityPCSDK.Login(accountId);
        }

        private static void logout()
        {
            GravityPCSDK.Logout();
        }

        private static void flush()
        {
           GravityPCSDK.Flush();
        }

        private static void setVersionInfo(string lib_name, string lib_version) {
            GravityPCSDK.SetLibName(lib_name);
            GravityPCSDK.SetLibVersion(lib_version);
        }
        
        private static void track(string eventName, string properties)
        {  
            Dictionary<string,object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.Track(eventName,propertiesDic);
        }

        private static void track(string eventName, string properties, DateTime dateTime)
        {
            Dictionary<string,object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.Track(eventName,propertiesDic,dateTime);
        }

        private static void track(string eventName, string properties, DateTime dateTime, TimeZoneInfo timeZone)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.Track(eventName, propertiesDic, dateTime, timeZone);
        }

        private static void setSuperProperties(string superProperties)
        {
            Dictionary<string, object> superPropertiesDic = GE_MiniJson.Deserialize(superProperties);
            GravityPCSDK.SetSuperProperties(superPropertiesDic);
        }

        private static void unsetSuperProperty(string superPropertyName)
        {
            GravityPCSDK.UnsetSuperProperty(superPropertyName);
        }

        private static void clearSuperProperty()
        {
            GravityPCSDK.ClearSuperProperties();
        }

        private static Dictionary<string, object> getSuperProperties()
        {
            return GravityPCSDK.SuperProperties();
        }

        private static void timeEvent(string eventName)
        {
            GravityPCSDK.TimeEvent(eventName);
        }

        private static void userSet(string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSet(propertiesDic);
        }

        private static void userSet(string properties, DateTime dateTime)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSet(propertiesDic,dateTime);
        }

        private static void userUnset(List<string> properties)
        {
            GravityPCSDK.UserUnset(properties);
        }

        private static void userUnset(List<string> properties, DateTime dateTime)
        {
            GravityPCSDK.UserUnset(properties,dateTime);
        }

        private static void userSetOnce(string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSetOnce(propertiesDic);
        }

        private static void userSetOnce(string properties, DateTime dateTime)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserSetOnce(propertiesDic,dateTime);
        }

        private static void userAdd(string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAdd(propertiesDic);
        }

        private static void userAdd(string properties, DateTime dateTime)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAdd(propertiesDic,dateTime);
        }
        
        private static void userNumberMin(string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMin(propertiesDic);
        }

        private static void userNumberMin(string properties, DateTime dateTime)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMin(propertiesDic,dateTime);
        }
        
        private static void userNumberMax(string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMax(propertiesDic);
        }

        private static void userNumberMax(string properties, DateTime dateTime)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserNumberMax(propertiesDic,dateTime);
        }

        private static void userDelete()
        {
            GravityPCSDK.UserDelete();
        }

        private static void userDelete(DateTime dateTime)
        {
            GravityPCSDK.UserDelete(dateTime);
        }

        private static void userAppend(string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAppend(propertiesDic);
        }

        private static void userAppend(string properties, DateTime dateTime)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserAppend(propertiesDic,dateTime);
        }

        private static void userUniqAppend(string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserUniqAppend(propertiesDic);
        }

        private static void userUniqAppend(string properties, DateTime dateTime)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            GravityPCSDK.UserUniqAppend(propertiesDic,dateTime);
        }

        private static void setNetworkType(GravityEngineAPI.NetworkType networkType)
        {
            
        }

        private static string getDeviceId() 
        {
            return GravityPCSDK.GetDeviceId();
        }

        private static void setDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            GravityPCSDK.SetDynamicSuperProperties(new GravityEngineWrapper());
        }

        private static void setTrackStatus(GE_TRACK_STATUS status)
        {
            GravityPCSDK.SetTrackStatus((GE_TRACK_STATUS)status);
        }

        private static string getTimeString(DateTime dateTime)
        {
            return GravityPCSDK.TimeString(dateTime);
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, string properties)
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
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE
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
            
            GravityPCSDK.EnableAutoTrack(pcAutoTrackEvents, propertiesDic);
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, IAutoTrackEventCallback eventCallback)
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
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE
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

        private static void setAutoTrackProperties(AUTO_TRACK_EVENTS autoTrackEvents, string properties)
        {
            Dictionary<string, object> propertiesDic = GE_MiniJson.Deserialize(properties);
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_INSTALL) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_INSTALL, propertiesDic);
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_START) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_START, propertiesDic);
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_END) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_END, propertiesDic);
            }
            if ((autoTrackEvents & AUTO_TRACK_EVENTS.APP_CRASH) != 0)
            {
                GravityPCSDK.SetAutoTrackProperties(AUTO_TRACK_EVENTS.APP_CRASH, propertiesDic);
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

        private static void enableThirdPartySharing(GEThirdPartyShareType shareType, string properties)
        {
            GravitySDKLogger.Print("Third Party Sharing is not support on PC: " + shareType + ", " + properties);
        }

        private static void register(string name, int version, string wxOpenId, string wxUnionId, IRegisterCallback registerCallback)
        {
            
            GravityPCSDK.Register(name, version, wxOpenId, wxUnionId, registerCallback);
        }
        
        private static void getBytedanceEcpmRecords(string wxOpenId, string mpId)
        {
            GravityPCSDK.GetBytedanceEcpmRecords(wxOpenId, mpId);
        }
    }
}
#endif