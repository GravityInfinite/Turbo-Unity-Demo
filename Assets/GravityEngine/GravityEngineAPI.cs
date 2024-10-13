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
using GravitySDK.PC.Storage;
using GravitySDK.PC.Time;
using GravitySDK.PC.Utils;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#if GRAVITY_WECHAT_GAME_MODE
using WeChatWASM;
#elif GRAVITY_BYTEDANCE_GAME_MODE
using StarkSDKSpace;
#elif GRAVITY_KUAISHOU_GAME_MODE
using com.kwai.mini.game;
using com.kwai.mini.game.config;
#elif GRAVITY_OPPO_GAME_MODE
using QGMiniGame;
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
            public string accessToken;
            public string channel;
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

            public Token(string accessToken, string clientId, string channel,
                SDKRunMode mode = SDKRunMode.NORMAL, SDKTimeZone timeZone = SDKTimeZone.Local, string timeZoneId = null,
                string instanceName = null)
            {
                this.accessToken = accessToken.Replace(" ", "");
                this.clientId = clientId.Replace(" ", "");
                this.channel = channel.Replace(" ", "");
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

        [Header("Configuration")] [Tooltip("是否手动初始化SDK")]
        public bool startManually = true;

        [Tooltip("设置网络类型")] public NetworkType networkType = NetworkType.DEFAULT;

        [Tooltip("项目相关配置, APP ID 会在项目申请时给出")] public Token[] tokens = new Token[1];

        #endregion

        /// <summary>
        /// 设置自定义访客 ID，用于替换系统生成的访客 ID
        /// </summary>
        /// <param name="firstId">访客 ID</param>
        private static void Identify(string firstId)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Identify(firstId);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {firstId};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 返回当前的访客 ID.
        /// </summary>
        /// <returns>访客 ID</returns>
        public static string GetDistinctId()
        {
            if (tracking_enabled)
            {
                return GravityEngineWrapper.GetDistinctId();
            }

            return null;
        }

        /// <summary>
        /// 设置账号 ID. 该方法不会上传用户登录事件，仅仅会保存AccountID做后续的上报.
        /// </summary>
        /// <param name="account">账号 ID</param>
        public static void Login(string account)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Login(account);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {account};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 清空账号 ID. 该方法会上传用户登出事件.
        /// </summary>
        public static void Logout(ILogoutCallback logoutCallback)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Logout(logoutCallback);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 开启自动采集功能.
        /// </summary>
        /// <param name="events">自动采集事件</param>
        /// <param name="properties">自动采集事件扩展属性(可选)</param>
        public static void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties = null)
        {
            if (tracking_enabled)
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, object>();
                }

                GravityEngineWrapper.EnableAutoTrack(events, properties);
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
                object[] parameters = new object[] {events, properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置自动采集扩展属性.
        /// </summary>
        /// <param name="events">自动采集事件</param>
        /// <param name="properties">自动采集事件扩展属性</param>
        public static void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetAutoTrackProperties(events, properties);
                // C#异常捕获提前，包含所有端
                if ((events & AUTO_TRACK_EVENTS.APP_CRASH) != 0 && !GE_PublicConfig.DisableCSharpException)
                {
                    GravityEngineExceptionHandler.SetAutoTrackProperties(properties);
                }
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {events, properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 上报微信小游戏事件：MPLaunch、MPShow
        /// </summary>
        /// <param name="query"></param>
        /// <param name="scene"></param>
        private static void TrackMPEvent(string eventName, Dictionary<string, string> query, string scene,
            Dictionary<string, object> properties)
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
            properties.Add(GravitySDKConstant.SCENE, "" + scene);
            Track(eventName, properties);
        }

        /// <summary>
        /// 上报小游戏启动事件 MPLaunch
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
        public static void TrackMPShow(Dictionary<string, string> query, string scene,
            Dictionary<string, object> properties)
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
        /// 绑定数数账号
        /// </summary>
        /// <param name="taAccountId"></param>      当前用户的数数账户 ID (#account_id)
        /// <param name="taDistinctId"></param>     当前用户的数数访客 ID (#distinct_id)
        public static void BindTAThirdPlatform(string taAccountId, string taDistinctId)
        {
            if (GravitySDKUtil.IsEmptyString(taAccountId) && GravitySDKUtil.IsEmptyString(taDistinctId))
            {
                GE_Log.e("taAccountId or taDistinctId must be required");
                return;
            }
            GravityEngineWrapper.BindTAThirdPlatform(taAccountId, taDistinctId);
        }

        /// <summary>
        /// 上报付费事件 PayEvent
        /// </summary>
        /// <param name="payAmount"></param>            付费金额 单位为分
        /// <param name="payType"></param>              付费类型 按照国际标准组织ISO 4217中规范的3位字母，例如CNY人民币、USD美金等
        /// <param name="orderId"></param>              订单号
        /// <param name="payReason"></param>            付费原因 例如：购买钻石、办理月卡
        /// <param name="payMethod"></param>            付费方式 例如：支付宝、微信、银联等
        public static void TrackPayEvent(int payAmount, string payType, string orderId, string payReason,
            string payMethod)
        {
            GravityEngineWrapper.TrackPayEvent(payAmount, payType, orderId, payReason, payMethod);
        }

        /// <summary>
        /// 上报原生App广告观看事件 AdShow
        /// </summary>
        /// <param name="adUnionType"></param>          广告聚合平台类型  （取值为：topon、gromore、admore、self，分别对应Topon、Gromore、Admore、自建聚合）
        /// <param name="adPlacementId"></param>        广告瀑布流ID
        /// <param name="adSourceId"></param>           广告源ID
        /// <param name="adType"></param>               广告类型 （取值为：reward、banner、 native 、interstitial、 splash ，分别对应激励视频广告、横幅广告、信息流广告、插屏广告、开屏广告）
        /// <param name="adnType"></param>              广告平台类型（取值为：csj、gdt、ks、 mint 、baidu，分别对应为穿山甲、优量汇、快手联盟、Mintegral、百度联盟）
        /// <param name="ecpm"></param>                 预估ECPM价格（单位为元）
        public static void TrackNativeAppAdShowEvent(string adUnionType, string adPlacementId, string adSourceId,
            string adType, string adnType, float ecpm)
        {
            GravityEngineWrapper.TrackNativeAppAdShowEvent(adUnionType, adPlacementId, adSourceId, adType, adnType,
                ecpm);
        }

        /// <summary>
        /// 上报微信小游戏广告观看事件 AdShow
        /// </summary>
        /// <param name="adType"></param>               广告类型 取值为：reward、banner、native、interstitial、video_feed、video_begin，分别对应：激励视频广告、Banner广告、原生模板广告、插屏广告、视频广告、视频贴片广告
        /// <param name="adUnitId"></param>             广告位ID
        /// <param name="otherProperties"></param>      其他需要携带的自定义参数
        ///
        public static void TrackWechatAdShowEvent(string adType, string adUnitId,
            Dictionary<string, object> otherProperties = null)
        {
            var properties = new Dictionary<string, object>()
            {
                {"$ad_type", adType},
                {"$ad_unit_id", adUnitId},
                {"$adn_type", "wechat"}
            };
            GE_PropertiesChecker.MergeProperties(otherProperties, properties);
            Track("$AdShow", properties);
            Flush();
        }
        
        /// <summary>
        /// 上报微信小游戏登录事件 $MPLogin
        /// </summary>
        public static void TrackMPLogin()
        {
            Track("$MPLogin");
            Flush();
        }
        
        /// <summary>
        /// 上报微信小游戏注册事件 $MPRegister
        /// </summary>
        public static void TrackMPRegister()
        {
            Track("$MPRegister");
            Flush();
        }
        
        /// <summary>
        /// 上报App注册事件 $AppRegister
        /// </summary>
        public static void TrackAppRegister()
        {
            Track("$AppRegister");
            Flush();
        }

        /// <summary>
        /// 上报微信小游戏退出登录事件 $MPLogout
        /// </summary>
        public static void TrackMPLogout()
        {
            Track("$MPLogout");
            Flush();
        }

        /// <summary>
        /// track 简单事件. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public static void Track(string eventName)
        {
            Track(eventName, null);
        }

        /// <summary>
        /// track 事件及事件属性. 该事件会先缓存在本地，达到触发上报条件或者主动调用 Flush 时会上报到服务器.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="properties">Properties</param>
        public static void Track(string eventName, Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Track(eventName, properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {eventName, properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
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
        public static void Track(string eventName, Dictionary<string, object> properties, DateTime date,
            TimeZoneInfo timeZone)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Track(eventName, properties, date, timeZone);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {eventName, properties, date, timeZone};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 主动触发上报缓存事件到服务器. 
        /// </summary>
        public static void Flush()
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.Flush();
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
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
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {"$scene_name", scene.name},
                {"$scene_path", scene.path}
            };
            Track("$SceneLoaded", properties, DateTime.Now, null);
            TimeEvent("$SceneUnloaded");
        }

        /// <summary>
        /// 注册场景卸载监听
        /// </summary>
        /// <param name="scene">场景对象</param>
        public static void OnSceneUnloaded(Scene scene)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {"$scene_name", scene.name},
                {"$scene_path", scene.path}
            };
            Track("$SceneUnloaded", properties, DateTime.Now, null);
        }

        /// <summary>
        /// 设置公共事件属性. 公共事件属性指的就是每个事件都会带有的属性.
        /// </summary>
        /// <param name="superProperties">公共事件属性</param>
        public static void SetSuperProperties(Dictionary<string, object> superProperties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetSuperProperties(superProperties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {superProperties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除某个公共事件属性.
        /// </summary>
        /// <param name="property">属性名称</param>
        public static void UnsetSuperProperty(string property)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UnsetSuperProperty(property);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {property};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 返回当前公共事件属性.
        /// </summary>
        /// <returns>公共事件属性</returns>
        public static Dictionary<string, object> GetSuperProperties()
        {
            if (tracking_enabled)
            {
                return GravityEngineWrapper.GetSuperProperties();
            }

            return null;
        }
        
        /// <summary>
        /// 返回当前预置事件属性.
        /// </summary>
        /// <returns>预置事件属性</returns>
        public static Dictionary<string, object> GetCurrentPresetProperties()
        {
            if (tracking_enabled)
            {
                return GravityEngineWrapper.GetCurrentPresetProperties();
            }

            return null;
        }

        /// <summary>
        /// 清空公共事件属性.
        /// </summary>
        public static void ClearSuperProperties()
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.ClearSuperProperty();
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// Sets the dynamic super properties.
        /// </summary>
        /// <param name="dynamicSuperProperties">Dynamic super properties interface.</param>
        public static void SetDynamicSuperProperties(IDynamicSuperProperties dynamicSuperProperties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetDynamicSuperProperties(dynamicSuperProperties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {dynamicSuperProperties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 记录事件时长. 调用 TimeEvent 为某事件开始计时，当 track 传该事件时，SDK 会在在事件属性中加入 #duration 这一属性来表示事件时长，单位为秒.
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public static void TimeEvent(string eventName)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.TimeEvent(eventName);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {eventName};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 该接口上传的属性将会覆盖原有的属性值.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserSet(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSet(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 该接口上传的属性将会覆盖原有的属性值，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">用户属性设置的时间</param>
        public static void UserSet(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSet(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 重置一个用户属性.
        /// </summary>
        /// <param name="property">用户属性名称</param>
        public static void UserUnset(string property)
        {
            List<string> properties = new List<string>();
            properties.Add(property);
            UserUnset(properties);
        }


        /// <summary>
        /// 重置一组用户属性
        /// </summary>
        /// <param name="properties">用户属性列表</param>
        public static void UserUnset(List<string> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUnset(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 重置一组用户属性, 并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性列表</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserUnset(List<string> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUnset(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 当该属性之前已经有值的时候，将会忽略这条信息.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserSetOnce(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSetOnce(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置用户属性. 当该属性之前已经有值的时候，将会忽略这条信息，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserSetOnce(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserSetOnce(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果该属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        public static void UserAdd(string property, object value)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {property, value}
            };
            UserAdd(properties);
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserAdd(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAdd(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性进行累加. 如果属性还未被设置，则会赋值 0 后再进行计算，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserAdd(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAdd(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        public static void UserNumberMin(string property, object value)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {property, value}
            };
            UserNumberMin(properties);
        }

        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserNumberMin(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMin(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最小值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserNumberMin(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMin(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="property">属性名称</param>
        /// <param name="value">数值</param>
        public static void UserNumberMax(string property, object value)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {property, value}
            };
            UserNumberMax(properties);
        }

        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserNumberMax(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMax(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对数值类用户属性取最大值. 如果属性还未被设置，则会赋值 0 后再进行计算.
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserNumberMax(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserNumberMax(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行追加.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserAppend(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAppend(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行追加，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserAppend(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行去重追加.
        /// </summary>
        /// <param name="properties">用户属性</param>
        public static void UserUniqAppend(Dictionary<string, object> properties)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUniqAppend(properties);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 对 List 类型的用户属性进行去重追加，并指定操作时间
        /// </summary>
        /// <param name="properties">用户属性</param>
        /// <param name="dateTime">操作时间</param>
        public static void UserUniqAppend(Dictionary<string, object> properties, DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserUniqAppend(properties, dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {properties, dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除用户数据. 之后再查询该名用户的用户属性，将为空字典，但该用户产生的事件仍然可以被查询到
        /// </summary>
        public static void UserDelete()
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserDelete();
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] { };
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 删除用户数据并指定操作时间.
        /// </summary>
        public static void UserDelete(DateTime dateTime)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.UserDelete(dateTime);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {dateTime};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
                });
            }
        }

        /// <summary>
        /// 设置允许上报数据到服务器的网络类型.（只支持Android/iOS）暂时不支持
        /// </summary>
        /// <param name="networkType">网络类型</param>
        private static void SetNetworkType(NetworkType networkType)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetNetworkType(networkType);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {networkType};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
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
        public static void SetTrackStatus(GE_TRACK_STATUS status)
        {
            if (tracking_enabled)
            {
                GravityEngineWrapper.SetTrackStatus(status);
            }
            else
            {
                System.Reflection.MethodBase method = System.Reflection.MethodBase.GetCurrentMethod();
                object[] parameters = new object[] {status};
                eventCaches.Add(new Dictionary<string, object>()
                {
                    {"method", method},
                    {"parameters", parameters}
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
        /// <param name="accessToken">项目token</param>
        /// <param name="clientId">用户唯一ID，iOS产品传固定值default_placeholder，使用SDK提供的默认值即可，SDK会自动采集client ID</param>
        /// <param name="mode">SDK运行模式（默认为正常模式）</param>
        /// <param name="channel">用户渠道（选填）</param>
        public static void StartGravityEngine(string accessToken, string clientId="default_placeholder", SDKRunMode mode = SDKRunMode.NORMAL,
            string channel = "base_channel")
        {
            SDKTimeZone timeZone = SDKTimeZone.Local;
            Token token = new Token(accessToken, clientId, channel, mode, timeZone);
            Token[] tokens = new Token[1];
            tokens[0] = token;
            StartGravityEngine(tokens);
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
#elif GRAVITY_KUAISHOU_GAME_MODE
                // 从快手SDK获取属性信息
                KSSystemInfo ks = KSConfig.kSSystemInfo;
                if (ks!=null)
                {
                    // 提前设置设备属性信息
                    GravitySDKDeviceInfo.SetWechatGameDeviceInfo(new WechatGameDeviceInfo()
                    {
                        version = ks.version, // 快手版本号
                    });    
                }
                else
                {
                    Debug.Log("kuaishou system info is null");
                }
#elif GRAVITY_OPPO_GAME_MODE
                // 从OPPO快游戏SDK获取属性信息
                QG.GetSystemInfo((systemInfo) =>
                {
                    GravitySDKDeviceInfo.SetWechatGameDeviceInfo(new WechatGameDeviceInfo()
                    {
                        SDKVersion = systemInfo.data.COREVersion, // 抖音SDK版本号
                        brand = systemInfo.data.brand,
                        language = systemInfo.data.language,
                        model = systemInfo.data.model,
                        platform = systemInfo.data.platformVersionName,
                        // screenHeight = systemInfo.data.screenHeight,
                        // screenWidth = systemInfo.data.screenWidth,
                        system = systemInfo.data.system,
                    });
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
                        GE_Log.EnableLog(token.mode == SDKRunMode.DEBUG);
                        GravityEngineWrapper.EnableLog(token.mode == SDKRunMode.DEBUG);

                        GE_Log.d("GravityEngine start with SERVER_URL: " + GravitySDKConstant.SERVER_URL +
                                 ", MODE: " + token.mode);

                        GravityHelper.InitSDK(token.accessToken, token.clientId, token.channel);
                        GravityEngineWrapper.ShareInstance(token, _sGravityEngineAPI);
                        GravityEngineWrapper.SetNetworkType(_sGravityEngineAPI.networkType);
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
                        ReportSuperProperties("" + launchOptionsSync.scene);
                    }
                });
#elif GRAVITY_BYTEDANCE_GAME_MODE
                // 抖音小游戏不需要提前获取网络状态，直接使用unity自带的即可
                LaunchOption launchOptionsSync = StarkSDK.API.GetLaunchOptionsSync();
                TrackMPLaunch(launchOptionsSync.Query, launchOptionsSync.Scene);
                ReportSuperProperties(launchOptionsSync.Scene);
#elif GRAVITY_KUAISHOU_GAME_MODE
                // 快手小游戏不需要提前获取网络状态，直接使用unity自带的即可
                KSOutLaunchOption launchOption = KSConfig.kSOutLaunchOption;
                if (launchOption!=null)
                {
                    TrackMPLaunch(launchOption.query, launchOption.from);
                    ReportSuperProperties(launchOption.from);    
                }
                else
                {
                    Debug.Log("kuaishou launch option is null");
                    TrackMPLaunch(new Dictionary<string, string>(), "fake");
                }
#elif GRAVITY_OPPO_GAME_MODE
                var launchStr =  QG.GetEnterOptionsSync();
                Debug.Log("oppo quickgame launch info is " + launchStr);
                if (launchStr!=null)
                {
                    Dictionary<string, object> launchOptionDict = GE_MiniJson.Deserialize(launchStr);
                    if (launchOptionDict.TryGetValue("query", out var queryStr))
                    {
                        Dictionary<string, object> queryDict = (Dictionary<string, object>) queryStr;
                        TrackMPLaunch(ConvertToDictionary(queryDict), "");
                        ReportSuperProperties("");
                        Debug.Log("oppo quickgame query got");
                    }
                    else
                    {
                        queryStr = "{}";
                        Debug.Log("oppo quickgame query is null");
                    }
                }
                else
                {
                    Debug.Log("oppo quickgame launch option is null");
                    TrackMPLaunch(new Dictionary<string, string>(), "fake");
                }
#endif
            }

            //上报缓存事件
            FlushEventCaches();
        }
        
        static Dictionary<string, string> ConvertToDictionary(Dictionary<string, object> sourceDict)
        {
            var targetDict = new Dictionary<string, string>();

            foreach (var kvp in sourceDict)
            {
                // 尝试将值转换为字符串
                string stringValue = kvp.Value?.ToString();
                targetDict[kvp.Key] = stringValue;
            }

            return targetDict;
        }

        // 统计启动场景
        private static void ReportSuperProperties(string scene)
        {
            string latestStartupTimestamp;
            
            if (GravitySDKUtil.IsFirstLaunchToday())
            {
                GravitySDKFile.SaveData(GravitySDKConstant.TODAY_FIRST_SCENE_KEY, scene);
                latestStartupTimestamp = scene;
            }
            else
            {
                latestStartupTimestamp = (string) GravitySDKFile.GetData(GravitySDKConstant.TODAY_FIRST_SCENE_KEY, typeof(string));
            }
            
            Dictionary<string, object> superProperties = new Dictionary<string, object>()
            {
                {GravitySDKConstant.SCENE, scene},
                {GravitySDKConstant.TODAY_FIRST_SCENE, latestStartupTimestamp}
            };
            SetSuperProperties(superProperties);
        }

        #region turbo

        /// <summary>
        /// 在引力引擎初始化，其他方法均需在本方法回调成功之后才可正常使用
        /// </summary>
        /// <param name="clientId"></param>                 用户唯一标识，如产品为小游戏，则必须填用户openid(如传空，则使用调用StartGravityEngine时传入的clientID；如传，则会使用当前传入的clientID)
        /// <param name="nickname"></param>                 用户昵称
        /// <param name="version"></param>                  用户注册的程序版本，比如当前小游戏的版本号
        /// <param name="openId"></param>                   open id (小程序/小游戏必填)
        /// <param name="enableSyncAttribution"></param>    是否开启同步获取归因信息，具体请参考同步归因：https://doc.gravity-engine.com/turbo-integrated/sync_attribution.html
        /// <param name="initializeCallback"></param>       网络回调，其他方法均需在回调成功之后才可正常使用
        /// <exception cref="ArgumentException"></exception>
        public static void Initialize(string clientId, string nickname, int version, string openId,
            bool enableSyncAttribution, IInitializeCallback initializeCallback)
        {
            GravityEngineWrapper.Initialize(clientId, nickname, version, openId, enableSyncAttribution,
                initializeCallback);
        }

        /// <summary>
        /// 在引力引擎注册，其他方法均需在本方法回调成功之后才可正常使用（iOS专用）
        /// </summary>
        /// <param name="enableAsa"></param>                是否开启asa归因
        /// <param name="caid1MD5"></param>                 当前用户中广协 ID 的 md5 hash（20230330 版本）（可为空）
        /// <param name="caid2MD5"></param>                 当前用户中广协 ID 的 md5 hash（20220111 版本）（可为空）
        /// <param name="enableSyncAttribution"></param>    是否开启同步获取归因信息，具体请参考同步归因：https://doc.gravity-engine.com/turbo-integrated/sync_attribution.html
        /// <param name="channel"></param>                  当前包渠道
        /// <param name="initializeCallback"></param>       网络回调，其他方法均需在回调成功之后才可正常使用
        /// <exception cref="ArgumentException"></exception>
        public static void InitializeIOS(bool enableAsa, string caid1MD5, string caid2MD5, bool enableSyncAttribution, string channel, IInitializeCallback initializeCallback)
        {
            GravityEngineWrapper.InitializeIOS(enableAsa, caid1MD5, caid2MD5, enableSyncAttribution, channel, initializeCallback);
        }
        
        public static string GetCurrentClientID()
        {
            return GravityEngineWrapper.GetCurrentClientID();
        }

        public static void ResetClientID(string newClientId, IResetCallback resetClientIdCallback)
        {
            GravityEngineWrapper.ResetClientID(newClientId, resetClientIdCallback);
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
                    System.Reflection.MethodBase method = (System.Reflection.MethodBase) eventCache["method"];
                    object[] parameters = (object[]) eventCache["parameters"];
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