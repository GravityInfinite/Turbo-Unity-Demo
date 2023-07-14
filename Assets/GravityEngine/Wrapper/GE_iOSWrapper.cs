#if UNITY_IOS && !(UNITY_EDITOR) && !TE_DISABLE_IOS_OC

// #if true
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GravityEngine;
using GravityEngine.Utils;
using GravitySDK.PC.Constant;
using GravitySDK.PC.GravityTurbo;

namespace GravityEngine.Wrapper
{
    public partial class GravityEngineWrapper
    {
        private static IRegisterCallback _registerCallback;
        private static IRegisterCallback _resetClientIdCallback;
        private static ILogoutCallback _logoutCallback;

        [DllImport("__Internal")]
        private static extern void ge_start(string app_id, string access_token, int mode, string timezone_id,
            bool enable_encrypt,
            int encrypt_version, string encrypt_public_key, int pinning_mode, bool allow_invalid_certificates,
            bool validates_domain_name);

        [DllImport("__Internal")]
        private static extern void ge_identify(string app_id, string unique_id);

        [DllImport("__Internal")]
        private static extern string ge_get_distinct_id(string app_id);

        [DllImport("__Internal")]
        private static extern void ge_login(string app_id, string account_id);

        [DllImport("__Internal")]
        private static extern void ge_logout(string app_id);

        [DllImport("__Internal")]
        private static extern void ge_track(string app_id, string event_name, string properties, long time_stamp_millis,
            string timezone);

        [DllImport("__Internal")]
        private static extern void ge_set_super_properties(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_unset_super_property(string app_id, string property_name);

        [DllImport("__Internal")]
        private static extern void ge_clear_super_properties(string app_id);

        [DllImport("__Internal")]
        private static extern string ge_get_super_properties(string app_id);

        [DllImport("__Internal")]
        private static extern void ge_time_event(string app_id, string event_name);

        [DllImport("__Internal")]
        private static extern void ge_user_set(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_set_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_unset(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_unset_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_set_once(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_set_once_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_increment(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_increment_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_number_max(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_number_max_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_number_min(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_number_min_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_delete(string app_id);

        [DllImport("__Internal")]
        private static extern void ge_user_delete_with_time(string app_id, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_append(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_append_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_user_uniq_append(string app_id, string properties);

        [DllImport("__Internal")]
        private static extern void ge_user_uniq_append_with_time(string app_id, string properties, long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_flush(string app_id);

        [DllImport("__Internal")]
        private static extern void ge_set_network_type(int type);

        [DllImport("__Internal")]
        private static extern void ge_enable_log(bool is_enable);

        [DllImport("__Internal")]
        private static extern string ge_get_device_id();

        [DllImport("__Internal")]
        private static extern void ge_set_dynamic_super_properties(string app_id);

        [DllImport("__Internal")]
        private static extern void ge_set_track_status(string app_id, int status);

        [DllImport("__Internal")]
        private static extern void ge_enable_autoTrack(string app_id, int events, string properties);

        [DllImport("__Internal")]
        private static extern void ge_enable_autoTrack_with_callback(string app_id, int events);

        [DllImport("__Internal")]
        private static extern void ge_set_autoTrack_properties(string app_id, int events, string properties);

        [DllImport("__Internal")]
        private static extern string ge_get_time_string(long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_calibrate_time(long timestamp);

        [DllImport("__Internal")]
        private static extern void ge_calibrate_time_with_ntp(string ntpServer);

        [DllImport("__Internal")]
        private static extern void ge_config_custom_lib_info(string lib_name, string lib_version);

        [DllImport("__Internal")]
        private static extern void ge_track_native_app_ad_show_event(string app_id, string adUnionType,
            string adPlacementId, string adSourceId,
            string adType, string adnType, float ecpm);

        [DllImport("__Internal")]
        private static extern void ge_bind_ta_third_platform(string app_id, string taAccountId, string taDistinctId);

        [DllImport("__Internal")]
        private static extern void ge_track_pay_event(string app_id, int payAmount, string payType, string orderId,
            string payReason,
            string payMethod);

        [DllImport("__Internal")]
        private static extern void ge_register(string app_id, string clientId, string userClientName, bool enableAsa,
            int version, string idfa, string idfv, string caid1_md5, string caid2_md5);

        [DllImport("__Internal")]
        private static extern void ge_resetClientId(string app_id, string newClientId);

        private const string AppID = "gravity_engine_appid";

        private static void init(GravityEngineAPI.Token token)
        {
            registerRecieveGameCallback();
            ge_start(AppID, token.accessToken, (int) token.mode, token.getTimeZoneId(), token.enableEncrypt,
                token.encryptVersion,
                token.encryptPublicKey, (int) token.pinningMode, token.allowInvalidCertificates,
                token.validatesDomainName);
        }

        private static void identify(string uniqueId)
        {
            ge_identify(AppID, uniqueId);
        }

        private static string getDistinctId()
        {
            return ge_get_distinct_id(AppID);
        }

        private static void login(string accountId)
        {
            ge_login(AppID, accountId);
        }

        private static void logout(ILogoutCallback logoutCallback)
        {
            _logoutCallback = logoutCallback;
            ge_logout(AppID);
        }

        private static void flush()
        {
            ge_flush(AppID);
        }

        private static void enableLog(bool enable)
        {
            ge_enable_log(enable);
        }

        private static void setVersionInfo(string lib_name, string lib_version)
        {
            ge_config_custom_lib_info(lib_name, lib_version);
        }

        private static void track(string eventName, string properties)
        {
            ge_track(AppID, eventName, properties, 0, "");
        }

        private static void track(string eventName, string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;

            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            string tz = "";
            ge_track(AppID, eventName, properties, currentMillis, tz);
        }

        private static void track(string eventName, string properties, DateTime dateTime, TimeZoneInfo timeZone)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;

            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            string tz = "";
            if (timeZone != null)
            {
                tz = timeZone.Id;
            }

            ge_track(AppID, eventName, properties, currentMillis, tz);
        }

        private static void setSuperProperties(string superProperties)
        {
            ge_set_super_properties(AppID, superProperties);
        }

        private static void unsetSuperProperty(string superPropertyName)
        {
            ge_unset_super_property(AppID, superPropertyName);
        }

        private static void clearSuperProperty()
        {
            ge_clear_super_properties(AppID);
        }

        private static Dictionary<string, object> getSuperProperties()
        {
            string superPropertiesString = ge_get_super_properties(AppID);
            return GE_MiniJson.Deserialize(superPropertiesString);
        }

        private static void timeEvent(string eventName)
        {
            ge_time_event(AppID, eventName);
        }

        private static void userSet(string properties)
        {
            ge_user_set(AppID, properties);
        }

        private static void userSet(string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_set_with_time(AppID, properties, currentMillis);
        }

        private static void userUnset(List<string> properties)
        {
            foreach (string property in properties)
            {
                ge_user_unset(AppID, property);
            }
        }

        private static void userUnset(List<string> properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            foreach (string property in properties)
            {
                ge_user_unset_with_time(AppID, property, currentMillis);
            }
        }

        private static void userSetOnce(string properties)
        {
            ge_user_set_once(AppID, properties);
        }

        private static void userSetOnce(string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_set_once_with_time(AppID, properties, currentMillis);
        }

        private static void userAdd(string properties)
        {
            ge_user_increment(AppID, properties);
        }

        private static void userAdd(string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_increment_with_time(AppID, properties, currentMillis);
        }

        private static void userNumberMax(string properties)
        {
            ge_user_number_max(AppID, properties);
        }

        private static void userNumberMax(string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_number_max_with_time(AppID, properties, currentMillis);
        }

        private static void userNumberMin(string properties)
        {
            ge_user_number_min(AppID, properties);
        }

        private static void userNumberMin(string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_number_min_with_time(AppID, properties, currentMillis);
        }

        private static void userDelete()
        {
            ge_user_delete(AppID);
        }

        private static void userDelete(DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_delete_with_time(AppID, currentMillis);
        }

        private static void userAppend(string properties)
        {
            ge_user_append(AppID, properties);
        }

        private static void userAppend(string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_append_with_time(AppID, properties, currentMillis);
        }

        private static void userUniqAppend(string properties)
        {
            ge_user_uniq_append(AppID, properties);
        }

        private static void userUniqAppend(string properties, DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            ge_user_uniq_append_with_time(AppID, properties, currentMillis);
        }

        private static void setNetworkType(GravityEngineAPI.NetworkType networkType)
        {
            ge_set_network_type((int) networkType);
        }

        private static string getDeviceId()
        {
            return ge_get_device_id();
        }

        private static void setDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            ge_set_dynamic_super_properties(AppID);
        }

        private static void setTrackStatus(GE_TRACK_STATUS status)
        {
            ge_set_track_status(AppID, (int) status);
        }

        private static string getTimeString(DateTime dateTime)
        {
            long dateTimeTicksUTC = TimeZoneInfo.ConvertTimeToUtc(dateTime).Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long currentMillis = (dateTimeTicksUTC - dtFrom.Ticks) / 10000;
            return ge_get_time_string(currentMillis);
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, string properties)
        {
            ge_enable_autoTrack(AppID, (int) autoTrackEvents, properties);
        }

        private static void enableAutoTrack(AUTO_TRACK_EVENTS autoTrackEvents, IAutoTrackEventCallback eventCallback)
        {
            ge_enable_autoTrack_with_callback(AppID, (int) autoTrackEvents);
        }

        private static void setAutoTrackProperties(AUTO_TRACK_EVENTS autoTrackEvents, string properties)
        {
            ge_set_autoTrack_properties(AppID, (int) autoTrackEvents, properties);
        }

        private static void calibrateTime(long timestamp)
        {
            ge_calibrate_time(timestamp);
        }

        private static void calibrateTimeWithNtp(string ntpServer)
        {
            ge_calibrate_time_with_ntp(ntpServer);
        }

        private static void registerRecieveGameCallback()
        {
            GEResultHandler handler = new GEResultHandler(geResultHandler);
            IntPtr geHandlerPointer = Marshal.GetFunctionPointerForDelegate(handler);
            GERegisterRecieveGameCallback(geHandlerPointer);
        }

        private static void register(string name, int version, string wxOpenId, IRegisterCallback registerCallback)
        {
            GE_Log.d("ios not support register");
        }

        private static void registerIOS(string name, int version, bool enableAsa, string idfa, string idfv,
            string caid1_md5, string caid2_md5, IRegisterCallback registerCallback)
        {
            _registerCallback = registerCallback;
            ge_register(AppID, Turbo.GetClientId(), name, enableAsa, version, idfa, idfv, caid1_md5,
                caid2_md5);
        }

        private static void resetClientId(string newClientId, IRegisterCallback resetClientIdCallback)
        {
            _resetClientIdCallback = resetClientIdCallback;
            ge_resetClientId(AppID, newClientId);
        }

        private static void reportBytedanceAdToGravity(string wxOpenId, string adUnitId)
        {
            GE_Log.d("ios not support reportBytedanceAdToGravity");
        }

        private static void trackPayEvent(int payAmount, string payType, string orderId, string payReason,
            string payMethod)
        {
            ge_track_pay_event(AppID, payAmount, payType, orderId, payReason, payMethod);
        }

        private static void trackNativeAppAdShowEvent(string adUnionType, string adPlacementId, string adSourceId,
            string adType, string adnType, float ecpm)
        {
            ge_track_native_app_ad_show_event(AppID, adUnionType, adPlacementId, adSourceId, adType, adnType, ecpm);
        }

        private static void bindTAThirdPlatform(string taAccountId, string taDistinctId)
        {
            ge_bind_ta_third_platform(AppID, taAccountId, taDistinctId);
        }

        [DllImport("__Internal")]
        public static extern void GERegisterRecieveGameCallback
        (
            IntPtr geHandlerPointer
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string GEResultHandler(string type, string jsonData);

        [AOT.MonoPInvokeCallback(typeof(GEResultHandler))]
        static string geResultHandler(string type, string jsonData)
        {
            if (type == "AutoTrackProperties")
            {
                if (mAutoTrackEventCallback != null)
                {
                    Dictionary<string, object> properties = GE_MiniJson.Deserialize(jsonData);
                    int eventType = Convert.ToInt32(properties["EventType"]);
                    properties.Remove("EventType");
                    Dictionary<string, object> autoTrackProperties =
                        mAutoTrackEventCallback.AutoTrackEventCallback(eventType, properties);
                    return GE_MiniJson.Serialize(autoTrackProperties);
                }
            }
            else if (type == "DynamicSuperProperties")
            {
                if (mDynamicSuperProperties != null)
                {
                    Dictionary<string, object> dynamicSuperProperties =
                        mDynamicSuperProperties.GetDynamicSuperProperties();
                    return GE_MiniJson.Serialize(dynamicSuperProperties);
                }
            }
            else if (type == "LogoutCallback")
            {
                if (_logoutCallback != null)
                {
                    _logoutCallback.onCompleted();
                    _logoutCallback = null;
                }

                GE_Log.d("logout callback");
            }
            else if (type == "RegisterCallbackSuccess")
            {
                if (_registerCallback != null)
                {
                    _registerCallback.onSuccess();
                    _registerCallback = null;
                }

                GE_Log.d("register callback success");
            }
            else if (type == "RegisterCallbackFailed")
            {
                if (_registerCallback != null)
                {
                    _registerCallback.onFailed("register failed, read the logs.");
                    _registerCallback = null;
                }

                GE_Log.d("register callback failed");
            }
            else if (type == "ResetClientIdCallbackSuccess")
            {
                if (_resetClientIdCallback != null)
                {
                    _resetClientIdCallback.onSuccess();
                    _resetClientIdCallback = null;
                }

                GE_Log.d("reset callback success");
            }
            else if (type == "ResetClientIdCallbackFailed")
            {
                if (_resetClientIdCallback != null)
                {
                    _resetClientIdCallback.onFailed("reset failed, read the logs.");
                    _resetClientIdCallback = null;
                }

                GE_Log.d("reset callback failed");
            }

            return "{}";
        }
    }
}
#endif