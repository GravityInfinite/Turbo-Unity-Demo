/*
 * 
    Copyright 2019, GravityEngine, Inc
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
 */
using System;
using System.Collections.Generic;
using GravitySDK.PC.GravityTurbo;
using GravityEngine.Utils;
using GravityEngine.Wrapper;
using UnityEngine;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#if GRAVITY_WECHAT_GAME_MODE
using WeChatWASM;
#elif GRAVITY_BYTEDANCE_GAME_MODE
using StarkSDKSpace;
#endif

namespace GravityEngine
{
    [DisallowMultipleComponent]
    public class GravityEngineAPI : MonoBehaviour
    {
        #region settings
        [System.Serializable]
        public struct Token
        {
            public string appid;
            public string accessToken;
            public string channel;
            public string aesKey;
            public string clientId;
            public SDKRunMode mode;
            public SDKTimeZone timeZone;
            public string timeZoneId;
            // 加密 仅支持Android/iOS
            public bool enableEncrypt; // 开启加密传输，默认false
            public int encryptVersion; // 密钥版本号
            public string encryptPublicKey; // 加密公钥
            public SSLPinningMode pinningMode; // SSL证书验证模式，默认NONE
            public bool allowInvalidCertificates; // 是否允许自建证书或者过期SSL证书，默认false
            public bool validatesDomainName; // 是否验证证书域名，默认true
            private string instanceName; // 实例名

            public Token(string appId, string accessToken, string clientId, string channel, string aesKey, SDKRunMode mode = SDKRunMode.NORMAL, SDKTimeZone timeZone = SDKTimeZone.Local, string timeZoneId = null, string instanceName = null)
            {
                this.appid = appId.Replace(" ", "");
                this.accessToken = accessToken.Replace(" ", "");
                this.clientId = clientId.Replace(" ", "");
                this.channel = channel.Replace(" ", "");
                this.aesKey = aesKey.Replace(" ", "");
                this.mode = mode;
                this.timeZone = timeZone;
                this.timeZoneId = timeZoneId;
                this.enableEncrypt = false;
                this.encryptVersion = 0;
                this.encryptPublicKey = null;
                this.pinningMode = SSLPinningMode.NONE;
                this.allowInvalidCertificates = false;
                this.validatesDomainName = true;
                if (!string.IsNullOrEmpty(instanceName))
                {
                    instanceName = instanceName.Replace(" ", "");
                }
                this.instanceName = instanceName;
            }

            public string GetInstanceName()
            {
                return this.instanceName;
            }

            public string getTimeZoneId()
            {
                switch (timeZone)
                {
                    case SDKTimeZone.UTC:
                        return "UTC";
                    case SDKTimeZone.Asia_Shanghai:
                        return "Asia/Shanghai";
                    case SDKTimeZone.Asia_Tokyo:
                        return "Asia/Tokyo";
                    case SDKTimeZone.America_Los_Angeles:
                        return "America/Los_Angeles";
                    case SDKTimeZone.America_New_York:
                        return "America/New_York";
                    case SDKTimeZone.Other:
                        return timeZoneId;
                    default:
                        break;
                }
                return null;
            }
        }

        public enum SDKTimeZone
        {
            Local,
            UTC,
            Asia_Shanghai,
            Asia_Tokyo,
            America_Los_Angeles,
            America_New_York,
            Other = 100
        }

        public enum SDKRunMode
        {
            NORMAL = 0,
            DEBUG = 1
        }

        public enum NetworkType
        {
            DEFAULT = 1,
            WIFI = 2,
            ALL = 3
        }

        [Header("Configuration")]
        [Tooltip("是否手动初始化SDK")]
        public bool startManually = true;

        [Tooltip("设置网络类型")]
        public NetworkType networkType = NetworkType.DEFAULT;

        [Tooltip("项目相关配置, APP ID 会在项目申请时给出")]
        public Token[] tokens = new Token[1];

        #endregion

        /// <summary>
        /// 设置自定义访客 ID，用于替换系统生成的访客 ID
        /// </summary>
        /// <param name="firstId">访客 ID</param>
        /// <param name="appId">项目 ID(可选)</param>
        private static void Identify(string firstId, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Identify(firstId, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { firstId, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 返回当前的访客 ID.
        /// </summary>
        /// <returns>访客 ID</returns>
        /// <param name="appId">项目 ID(可选)</param>
        public static string GetDistinctId(string appId = "")
        {
            if (tracking_enabled)
            {
                return GravityEngineWrapper.GetDistinctId(appId);
            }
            return null;
        }

        /// <summary>
        /// 设置账号 ID. 该方法不会上传用户登录事件，仅仅会保存AccountID做后续的上报.
        /// </summary>
        /// <param name="account">账号 ID</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void Login(string account, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Login(account, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { account, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 清空账号 ID. 该方法不会上传用户登出事件.
        /// </summary>
        /// <param name="appId">项目 ID(可选) </param>
        public static void Logout(string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Logout(appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 开启自动采集功能.
        /// </summary>
        /// <param name="events">自动采集事件</param>
        /// <param name="properties">自动采集事件扩展属性(可选)</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties = null, string appId = "")
        {
            if (tracking_enabled)
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, object>();
                }

                GravityEngineWrapper.EnableAutoTrack(events, properties, appId);
                // C#异常捕获提前，包含所有端
                if ((events & AUTO_TRACK_EVENTS.APP_CRASH) != 0 && !GE_PublicConfig.DisableCSharpException)
                {
                    GravityEngineExceptionHandler.RegisterGEExceptionHandler(properties);
                }
                if ((events & AUTO_TRACK_EVENTS.APP_SCENE_LOAD) != 0)
                {
                    SceneManager.sceneLoaded += GravityEngineAPI.OnSceneLoaded;
                }
                if ((events & AUTO_TRACK_EVENTS.APP_SCENE_UNLOAD) != 0)
                {
                    SceneManager.sceneUnloaded += GravityEngineAPI.OnSceneUnloaded;
                }
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { events, properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }
        
        /// <summary>
        /// 设置自动采集扩展属性.
        /// </summary>
        /// <param name="events">自动采集事件</param>
        /// <param name="properties">自动采集事件扩展属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetAutoTrackProperties(events, properties, appId);
                // C#异常捕获提前，包含所有端
                if ((events & AUTO_TRACK_EVENTS.APP_CRASH) != 0 && !GE_PublicConfig.DisableCSharpException)
                {
                    GravityEngineExceptionHandler.SetAutoTrackProperties(properties);
                }
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { events, properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 上报微信小游戏事件：MPLaunch、MPShow
        /// </summary>
        /// <param name="query"></param>
        /// <param name="scene"></param>
        private static void TrackMPEvent(string eventName, Dictionary<string, string> query, string scene, Dictionary<string, object> properties)
        {
            // join query's key and value with =, and join all query with &, ignore the last &
            string queryStr = "";
            foreach (KeyValuePair<string, string> kv in query)
            {
                queryStr += kv.Key + "=" + kv.Value + "&";
            }

            queryStr = queryStr.Length > 0 ? queryStr.Substring(0, queryStr.Length - 1) : "";
            if (properties == null)
            {
                properties = new Dictionary<string, object>();
            }
            properties.Add("$url_query", queryStr);
            properties.Add("$scene", "" + scene);
            Track(eventName, properties);
        }

        /// <summary>
        /// 上报微信小游戏启动事件 MPLaunch
        /// </summary>
        /// <param name="query"></param>
        /// <param name="scene"></param>
        public static void TrackMPLaunch(Dictionary<string, string> query, string scene)
        {
            TrackMPEvent(GravitySDKConstant.MP_LAUNCH, query, scene, null);
            Flush();
        }

        /// <summary>
        /// 上报微信小游戏显示事件 MPShow
        /// </summary>
        /// <param name="query"></param>
        /// <param name="scene"></param>
        public static void TrackMPShow(Dictionary<string, string> query, string scene, Dictionary<string, object> properties)
        {
            TrackMPEvent(GravitySDKConstant.MP_SHOW, query, scene, properties);
            // $MPHide开始计时
            TimeEvent(GravitySDKConstant.MP_HIDE);
        }

        /// <summary>
        /// 上报微信小游戏退出事件 MPHide
        /// </summary>
        public static void TrackMPHide(Dictionary<string, object> properties)
        {
            Track(GravitySDKConstant.MP_HIDE, properties);
        }
        
        /// <summary>
        /// 上报微信小游戏分享事件 MPShare
        /// </summary>
        public static void TrackMPShare(Dictionary<string, object> properties)
        {
            Track(GravitySDKConstant.MP_SHARE, properties);
        }
        
        /// <summary>
        /// 上报微信小游戏添加收藏事件 MPAddFavorites
        /// </summary>
        public static void TrackMPAddFavorites(Dictionary<string, object> properties)
        {
            Track(GravitySDKConstant.MP_ADD_FAVORITES, properties);
        }
        
        /// <summary>
        /// 上报微信小游戏付费事件 PayEvent
        /// </summary>
        /// <param name="payAmount"></param>            付费金额 单位为分
        /// <param name="payType"></param>              付费类型 按照国际标准组织ISO 4217中规范的3位字母，例如CNY人民币、USD美金等
        /// <param name="orderId"></param>              订单号
        /// <param name="payReason"></param>            付费原因 例如：购买钻石、办理月卡
        /// <param name="payMethod"></param>            付费方式 例如：支付宝、微信、银联等
        public static void TrackPayEvent(int payAmount, string payType, string orderId, string payReason,
            string payMethod)
        {
            Track("$PayEvent", new Dictionary<string, object>()
            {
                {"$pay_amount", payAmount},
                {"$pay_type", payType},
                {"$order_id", orderId},
                {"$pay_reason", payReason},
                {"$pay_method", payMethod}
            });
            Flush();
        }
        
        /// <summary>
        /// 上报微信小游戏广告观看事件 AdShow
        /// </summary>
        /// <param name="adType"></param>               广告类型 取值为：reward、banner、native、interstitial、video_feed、video_begin，分别对应：激励视频广告、Banner广告、原生模板广告、插屏广告、视频广告、视频贴片广告
        /// <param name="adUnitId"></param>             广告位ID
        public static void TrackAdShowEvent(string adType, string adUnitId)
        {
            Track("$AdShow", new Dictionary<string, object>()
            {
                {"$ad_type", adType},
                {"$ad_unit_id", adUnitId},
                {"$adn_type", "wechat"}
            });
            Flush();
        }

        public static void TrackBytedanceAdShowEvent(string currentAccessToken)
        {
            
        }

        /// <summary>
        /// 上报微信小游戏登录事件 $MPLogin
        /// </summary>
        public static void TrackMPLogin()
        {
            Track("$MPLogin");
        }
        
        /// <summary>
        /// 上报微信小游戏退出登录事件 $MPLogout
        /// </summary>
        public static void TrackMPLogout()
        {
            Track("$MPLogout");
        }

        /// <summary>
        /// track 简单事件. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void Track(string eventName, string appId = "")
        {
            Track(eventName, null, appId);
        }

        /// <summary>
        /// track 事件及事件属性. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">Properties</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void Track(string eventName, Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Track(eventName, properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { eventName, properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }
        
        /// <summary>
        /// track 事件及事件属性，并指定 $event_time #zone_offset 属性. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">事件属性</param>
        /// <param name="date">事件时间</param>
        /// <param name="timeZone">事件时区</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void Track(string eventName, Dictionary<string, object> properties, DateTime date, TimeZoneInfo timeZone, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Track(eventName, properties, date, timeZone, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { eventName, properties, date, timeZone, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// track 事件及事件属性，并指定 #event_time #zone_offset 属性. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器. 支持所有实例~
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">事件属性</param>
        /// <param name="date">事件时间</param>
        /// <param name="timeZone">事件时区</param>
        private static void TrackForAll(string eventName, Dictionary<string, object> properties, DateTime date, TimeZoneInfo timeZone)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.TrackForAll(eventName, properties, date, timeZone);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { eventName, properties, date, timeZone };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }
        
        /// <summary>
        /// 主动触发上报缓存事件到服务器. 
        /// </summary>
        /// <param name="appId">项目 ID(可选)</param>
        public static void Flush(string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Flush(appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 注册场景加载监听
        /// </summary>
        /// <param name="scene">场景对象</param>
        /// <param name="mode">场景加载模式</param>
        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>() {
                { "$scene_name", scene.name },
                { "$scene_path", scene.path }
            };
            TrackForAll("$SceneLoaded", properties, DateTime.Now, null);
            TimeEventForAll("$SceneUnloaded");
        }

        /// <summary>
        /// 注册场景卸载监听
        /// </summary>
        /// <param name="scene">场景对象</param>
        public static void OnSceneUnloaded(Scene scene)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>() {
                { "$scene_name", scene.name },
                { "$scene_path", scene.path }
            };
            TrackForAll("$SceneUnloaded", properties, DateTime.Now, null);
        }

        /// <summary>
        /// 设置公共事件属性. 公共事件属性指的就是每个事件都会带有的属性.
        /// </summary>
        /// <param name="superProperties">公共事件属性</param>
        /// <param name="appId">项目 ID（可选）</param>
        public static void SetSuperProperties(Dictionary<string, object> superProperties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetSuperProperties(superProperties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { superProperties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除某个公共事件属性.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UnsetSuperProperty(string property, string appId  = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UnsetSuperProperty(property, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { property, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 返回当前公共事件属性.
        /// </summary>
        /// <returns>公共事件属性</returns>
        /// <param name="appId">项目 ID(可选)</param>
        public static Dictionary<string, object> GetSuperProperties(string appId = "")
        {
            if (tracking_enabled)
            {
                return GravityEngineWrapper.GetSuperProperties(appId);
            }
            return null;
        }

        /// <summary>
        /// 清空公共事件属性.
        /// </summary>
        /// <param name="appId">项目 ID(可选)</param>
        public static void ClearSuperProperties(string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.ClearSuperProperty(appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// Sets the dynamic super properties.
        /// </summary>
        /// <param name="dynamicSuperProperties">Dynamic super properties interface.</param>
        /// <param name="appId">App ID (optional).</param>
        public static void SetDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetDynamicSuperProperties(dynamicSuperProperties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { dynamicSuperProperties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 记录事件时长. 调用 TimeEvent 为某事件开始计时，当 track 传该事件时，SDK 会在在事件属性中加入 #duration 这一属性来表示事件时长，单位为秒.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void TimeEvent(string eventName, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.TimeEvent(eventName, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { eventName, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 记录事件时长. 调用 TimeEvent 为某事件开始计时，当 track 传该事件时，SDK 会在在事件属性中加入 #duration 这一属性来表示事件时长，单位为秒.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        private static void TimeEventForAll(string eventName)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.TimeEventForAll(eventName);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { eventName };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 该接口上传的属性将会覆盖原有的属性值.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserSet(Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSet(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 该接口上传的属性将会覆盖原有的属性值，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">用户属性设置的时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserSet(Dictionary<string, object> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSet(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties , dateTime, appId};
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 重置一个用户属性.
        /// </summary>
        /// <param name="property">用户属性名称</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserUnset(string property, string appId = "")
        {
            List<string> properties = new List<string>();
            properties.Add(property);
            UserUnset(properties, appId);
        }


        /// <summary>
        /// 重置一组用户属性
        /// </summary>
        /// <param name="properties">用户属性列表</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserUnset(List<string> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUnset(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }

        }

        /// <summary>
        /// 重置一组用户属性, 并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性列表</param>
        /// <param name="dateTime">操作时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserUnset(List<string> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUnset(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, dateTime, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 当该属性之前已经有值的时候，将会忽略这条信息.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserSetOnce(Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSetOnce(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }

        }

        /// <summary>
        /// 设置用户属性. 当该属性之前已经有值的时候，将会忽略这条信息，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserSetOnce(Dictionary<string, object> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSetOnce(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, dateTime,appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }

        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果该属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserAdd(string property, object value, string appId = "")
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { property, value }
            };
            UserAdd(properties, appId);
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserAdd(Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAdd(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果属性还未被设置，则会赋值 0 后再进行计算，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserAdd(Dictionary<string, object> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAdd(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, dateTime, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }
        
        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserNumberMin(string property, object value, string appId = "")
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { property, value }
            };
            UserNumberMin(properties, appId);
        }
        
        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserNumberMin(Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMin(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserNumberMin(Dictionary<string, object> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMin(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, dateTime, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }
        
        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserNumberMax(string property, object value, string appId = "")
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { property, value }
            };
            UserNumberMax(properties, appId);
        }
        
        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserNumberMax(Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMax(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserNumberMax(Dictionary<string, object> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMax(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, dateTime, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行追加.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserAppend(Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAppend(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行追加，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserAppend(Dictionary<string, object> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAppend(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, dateTime, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行去重追加.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserUniqAppend(Dictionary<string, object> properties, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUniqAppend(properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行去重追加，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserUniqAppend(Dictionary<string, object> properties, DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUniqAppend(properties, dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { properties, dateTime, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除用户数据. 之后再查询该名用户的用户属性，将为空字典，但该用户产生的事件仍然可以被查询到
        /// </summary>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserDelete(string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserDelete(appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除用户数据并指定操作时间.
        /// </summary>
        /// <param name="appId">项目 ID(可选)</param>
        public static void UserDelete(DateTime dateTime, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserDelete(dateTime, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { dateTime, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置允许上报数据到服务器的网络类型.（只支持Android/iOS）暂时不支持
        /// </summary>
        /// <param name="networkType">网络类型</param>
        /// <param name="appId">项目 ID(可选)</param>
        private static void SetNetworkType(NetworkType networkType, string appId =  "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetNetworkType(networkType);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { networkType, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <returns>The device identifier.</returns>
        public static string GetDeviceId()
        {
            if (tracking_enabled)
            {
                return GravityEngineWrapper.GetDeviceId();
            } 
            return null;
        }

        /// <summary>
        /// 设置数据上报状态
        /// </summary>
        /// <param name="status">上报状态，详见 GE_TRACK_STATUS 定义</param>
        /// <param name="appId">项目ID</param>
        public static void SetTrackStatus(GE_TRACK_STATUS status, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetTrackStatus(status, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { status, appId };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 传入时间戳校准 SDK 时间.
        /// </summary>
        /// <param name="timestamp">当前 Unix timestamp, 单位 毫秒</param>
        public static void CalibrateTime(long timestamp)
        {
            GravityEngineWrapper.CalibrateTime(timestamp);
        }

        /// <summary>
        /// 传入 NTP Server 地址校准 SDK 时间.
        /// 您可以根据您用户所在地传入访问速度较快的 NTP Server 地址, 例如 time.asia.apple.com
        /// SDK 默认情况下会等待 3 秒，去获取时间偏移数据，并用该偏移校准之后的数据.
        /// 如果在 3 秒内未因网络原因未获得正确的时间偏移，本次应用运行期间将不会再校准时间.
        /// </summary>
        /// <param name="timestamp">可用的 NTP 服务器地址</param>
        public static void CalibrateTimeWithNtp(string ntpServer)
        {
            GravityEngineWrapper.CalibrateTimeWithNtp(ntpServer);
        }

        /// <summary>
        /// 三方数据共享
        /// 通过与三方系统共享引力账号体系，打通三方数据 暂时不支持
        /// </summary>
        /// <param name="shareType">三方系统类型</param>
        /// <param name="properties">三方系统自定义属性（部分系统自定义属性的设置是覆盖式更新，所以需要将自定义属性传入引力 SDK，此属性将会与引力账号体系一并传入三方系统）</param>
        /// <param name="appId">项目 ID</param>
        private static void EnableThirdPartySharing(GEThirdPartyShareType shareType, Dictionary<string, object> properties = null, string appId = "")
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.EnableThirdPartySharing(shareType, properties, appId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { shareType };
                eventCaches.Add(new Dictionary<string, object>() {
                    { "method", method},
                    { "parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 获取当前国家/地区代码
        /// 在 ISO 3166 中定义的由两个字母组成的国家/地区代码
        /// </summary>
        /// <returns>国家/地区代码</returns>
        public static string GetLocalRegion()
        {
            return System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName;
        }

        /// <summary>
        /// 手动初始化 Gravity Engine SDK
        /// </summary>
        /// <param name="appId">项目ID</param>
        /// <param name="accessToken">项目token</param>
        /// <param name="clientId">用户唯一ID</param>
        /// <param name="mode">SDK运行模式</param>
        /// <param name="channel">用户渠道（选填）</param>
        /// <param name="aesKey">原生平台AES秘钥，目前只有Android平台需要填写</param>
        public static void StartGravityEngine(string appId, string accessToken, string clientId, SDKRunMode mode, string channel="base_channel", string aesKey="")
        {
            GravityEngineAPI.SDKTimeZone timeZone = GravityEngineAPI.SDKTimeZone.Local;
            GravityEngineAPI.Token token =
                new GravityEngineAPI.Token(appId, accessToken, clientId, channel, aesKey, mode, timeZone);
            GravityEngineAPI.Token[] tokens = new GravityEngineAPI.Token[1];
            tokens[0] = token;
            GravityEngineAPI.StartGravityEngine(tokens);
        }

        /// <summary>
        /// 初始化 Gravity Engine SDK，仅内部调用
        /// </summary>
        /// <param name="token">多项目配置，详情参见 GravityEngineAPI.Token</param>
        private static void StartGravityEngine(Token[] tokens = null)
        {
            tracking_enabled = true;

            if (tracking_enabled)
            {
#if GRAVITY_WECHAT_GAME_MODE
                // 从微信SDK获取属性信息
                var systemInfo = WX.GetSystemInfoSync();
                // 提前设置设备属性信息
                GravitySDKDeviceInfo.SetWechatGameDeviceInfo(new WechatGameDeviceInfo()
                {
                    SDKVersion = systemInfo.SDKVersion, // 微信SDK版本号
                    brand = systemInfo.brand,
                    language = systemInfo.language,
                    model = systemInfo.model,
                    platform = systemInfo.platform,
                    screenHeight = systemInfo.screenHeight,
                    screenWidth = systemInfo.screenWidth,
                    system = systemInfo.system,
                    version = systemInfo.version, // 微信版本号
                });
#elif GRAVITY_BYTEDANCE_GAME_MODE
                // 从抖音SDK获取属性信息
                var systemInfo = StarkSDK.API.GetSystemInfo();
                // 提前设置设备属性信息
                GravitySDKDeviceInfo.SetWechatGameDeviceInfo(new WechatGameDeviceInfo()
                {
                    SDKVersion = systemInfo.sdkVersion, // 抖音SDK版本号
                    brand = systemInfo.brand,
                    language = systemInfo.language,
                    model = systemInfo.model,
                    platform = systemInfo.platform,
                    screenHeight = systemInfo.screenHeight,
                    screenWidth = systemInfo.screenWidth,
                    system = systemInfo.system,
                    version = systemInfo.hostVersion, // 抖音版本号
                });
#endif
                GE_PublicConfig.GetPublicConfig();
                GravityEngineWrapper.SetVersionInfo(GE_PublicConfig.LIB_VERSION);
                if (tokens == null)
                {
                    tokens = _sGravityEngineAPI.tokens;
                }
                try
                {
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        Token token = tokens[i];
                        if (!string.IsNullOrEmpty(token.appid))
                        {
                            token.appid = token.appid.Replace(" ", "");
                            GE_Log.EnableLog(token.mode == SDKRunMode.DEBUG);
                            GravityEngineWrapper.EnableLog(token.mode == SDKRunMode.DEBUG);
                            
                            GE_Log.d("GravityEngine start with APPID 2: " + token.appid + ", SERVER_URL: " + GravitySDKConstant.SERVER_URL + ", MODE: " + token.mode);
                            
                            Turbo.InitSDK(token.accessToken, token.clientId, token.channel);
                            GravityEngineWrapper.ShareInstance(token, _sGravityEngineAPI);
                            GravityEngineWrapper.SetNetworkType(_sGravityEngineAPI.networkType);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
#if GRAVITY_WECHAT_GAME_MODE
                // 记录小程序/小游戏启动事件，在StartEngine并且获取network_type之后调用
                WX.GetNetworkType(new GetNetworkTypeOption()
                {
                    success = (result) => { GravitySDKDeviceInfo.SetNetworkType(result.networkType); },
                    fail = (result) => { GravitySDKDeviceInfo.SetNetworkType("error"); },
                    complete = (result) =>
                    {
                        LaunchOptionsGame launchOptionsSync = WX.GetLaunchOptionsSync();
                        TrackMPLaunch(launchOptionsSync.query, "" + launchOptionsSync.scene);
                    }
                });
#elif GRAVITY_BYTEDANCE_GAME_MODE
                // 抖音小游戏不需要提前获取网络状态，直接使用unity自带的即可
                LaunchOption launchOptionsSync = StarkSDK.API.GetLaunchOptionsSync();
                TrackMPLaunch(launchOptionsSync.Query, launchOptionsSync.Scene);
#endif
            }

            //上报缓存事件
            FlushEventCaches();
        }

        #region turbo

        /// <summary>
        /// 在引力引擎注册，其他方法均需在本方法回调成功之后才可正常使用
        /// </summary>
        /// <param name="name"></param>             用户名
        /// <param name="version"></param>          用户注册的程序版本，比如当前微信小游戏的版本号
        /// <param name="wxOpenId"></param>         微信open id (微信小程序和小游戏必填)
        /// <param name="wxUnionId"></param>        微信union id（微信小程序和小游戏选填）
        /// <param name="actionResult"></param>     网络回调，其他方法均需在回调成功之后才可正常使用
        /// <exception cref="ArgumentException"></exception>
        public static void Register(string name, int version, string wxOpenId, string wxUnionId, IRegisterCallback registerCallback)
        {
            GravityEngineWrapper.Register(name, version, wxOpenId, wxUnionId, registerCallback);
        }

        public static void test()
        {
            // 方便调用
        }

        #endregion

        #region internal
        private static void FlushEventCaches()
        {
            List<Dictionary<string, object>> tmpEventCaches = new List<Dictionary<string, object>>(eventCaches);
            eventCaches.Clear();
            foreach (Dictionary<string, object> eventCache in tmpEventCaches)
            {
                if (eventCache.ContainsKey("method") && eventCache.ContainsKey("parameters"))
                {
                    System.Reflection.MethodBase method = (System.Reflection.MethodBase)eventCache["method"];
                    object[] parameters = (object[])eventCache["parameters"];
                    method.Invoke(null, parameters);
                }
            }
        }

        private void Awake()
        {
            if (_sGravityEngineAPI == null)
            {
                _sGravityEngineAPI = this;
                DontDestroyOnLoad(gameObject);
            } 
            else
            {
                Destroy(gameObject);
                return;
            }

            if (this.startManually == false) 
            {
                GravityEngineAPI.StartGravityEngine();
            }
        }

        private void Start()
        {
        }

        private void OnApplicationQuit()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene != null)
            {
                OnSceneUnloaded(scene);
            }
        }

        private static GravityEngineAPI _sGravityEngineAPI;
        private static bool tracking_enabled = false;
        private static List<Dictionary<string, object>> eventCaches = new List<Dictionary<string, object>>();
        #endregion
    }
}
