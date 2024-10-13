using System;
using System.Collections.Generic;
using GravityEngine;
using GravityEngine.Utils;
using GravitySDK.PC.TaskManager;
using GravitySDK.PC.Utils;
#if GRAVITY_OPPO_GAME_MODE
using QGMiniGame;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace GravitySDK.PC.GravityTurbo
{
    public static class GravityHelper
    {
        private const string GravityHost = "https://backend.gravity-engine.com";
        private static string _accessToken;
        private static string _clientID;
        private static string _channel;

        private static void GlobalCheck()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                throw new ArgumentException("accessToken must be required");
            }

            if (string.IsNullOrEmpty(_clientID))
            {
                throw new ArgumentException("clientId must be required");
            }
        }

        /// <summary>
        /// 初始化GravityHelper SDK必须参数（每次启动都需要调用）
        /// </summary>
        /// <param name="accessToken">项目通行证，在：网站后台-->管理中心-->应用列表中找到Access Token列 复制（首次使用可能需要先新增应用）</param> 
        /// <param name="clientId">用户唯一标识，如微信小程序/小游戏的openid、Android ID、iOS的IDFA、或业务侧自行生成的唯一用户ID均可</param>
        /// <param name="channel">用户渠道</param>
        public static void InitSDK(string accessToken, string clientId, string channel)
        {
            _accessToken = accessToken;
            _clientID = clientId;
            _channel = channel;
            GlobalCheck();
            GravitySDKLogger.Print("GravityHelper init success");
        }

        public static void Initialize(string clientId, string name, int version, string openId, Dictionary<string, string> wxLaunchQuery, bool enableSyncAttribution,
            IInitializeCallback initializeCallback, UnityWebRequestMgr.Callback callback)
        {
            // check params
            GlobalCheck();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name must be required");
            }

            var currentClientId = SetOrGetClientId(clientId);
            var registerRequestDir = new Dictionary<string, object>()
            {
                {"client_id", currentClientId},
                {"name", name},
                {"channel", _channel},
                {"version", version},
                {"wx_openid", openId},
                {"wx_unionid", ""},
                {"ad_data", wxLaunchQuery},
                {"need_return_attribution", enableSyncAttribution},
            };
#if GRAVITY_OPPO_GAME_MODE
            string deviceId = "";
            QG.GetDeviceId(
                (success) =>
                {
                    deviceId = success.data.deviceId; //设备唯一标识
                },
                (fail) =>
                {
                    Debug.Log("QG.GetDeviceId fail = " + JsonUtility.ToJson(fail));
                },
                (complete) =>
                {
                    Debug.Log("gravity-engine current deviceId is " + deviceId);
                    QG.GetSystemInfo((msg) =>
                        {
                            string brand = msg.data.brand; // 手机品牌
                            string language = msg.data.language; // 系统语言
                            string model = msg.data.model; // 手机型号
                            string platformVersionName = msg.data.platformVersionName; // 客户端平台
                            string platformVersionCode = msg.data.platformVersionCode; // Version
                            string screenHeight = msg.data.screenHeight; // 屏幕高度
                            string screenWidth = msg.data.screenWidth; // 屏幕宽度
                            string system = msg.data.system; // 系统版本
                            string COREVersion = msg.data.COREVersion; // 版本号
                            
                            var deviceInfo = new Dictionary<string, string>()
                            {
                                {"os_name", "android"},
                                {"android_id", deviceId},
                                {"imei", deviceId},
                                {"oaid", deviceId},
                                {"rom", platformVersionName},
                                {"rom_version", platformVersionCode},
                                {"brand", brand},
                                {"model", model},
                                {"android_version", COREVersion}
                            };
                            registerRequestDir["device_info"] = deviceInfo;
                            RequestInitialize(initializeCallback, callback, currentClientId, registerRequestDir);
                        },
                        (err) =>
                        {
                            Debug.Log("QG.GetSystemInfo fail = " + JsonUtility.ToJson(err));
                            var deviceInfo = new Dictionary<string, string>()
                            {
                                {"os_name", "android"},
                                {"android_id", deviceId},
                                {"imei", deviceId},
                                {"oaid", deviceId},
                            };
                            registerRequestDir["device_info"] = deviceInfo;
                            RequestInitialize(initializeCallback, callback, currentClientId, registerRequestDir);
                        });
                });
#else
            RequestInitialize(initializeCallback, callback, currentClientId, registerRequestDir);
#endif
        }

        private static void RequestInitialize(IInitializeCallback initializeCallback, UnityWebRequestMgr.Callback callback, string currentClientId,
            Dictionary<string, object> registerRequestDir)
        {
            GravitySDKLogger.Print(registerRequestDir.ToString());
            UnityWebRequestMgr.Instance.Post(
                GravityHost + "/event_center/api/v1/user/initialize/?access_token=" + _accessToken + "&client_id=" + currentClientId,
                registerRequestDir, (request =>
                {
                    string responseText = request.downloadHandler.text;
                    Dictionary<string, object> res = GE_MiniJson.Deserialize(responseText);
                    GravitySDKLogger.Print("response is " + responseText);
                    if (res != null)
                    {
                        if (res.TryGetValue("code", out var re))
                        {
                            int code = Convert.ToInt32(re);
                            if (code == 0)
                            {
                                if (res.TryGetValue("data", out object dataObj))
                                {
                                    initializeCallback?.onSuccess((Dictionary<string, object>) dataObj);
                                }
                                else
                                {
                                    initializeCallback?.onSuccess(new Dictionary<string, object>());
                                }
                                return;
                            }
                        }
                    }

                    initializeCallback?.onFailed("code is not 0, failed with msg " + res?["msg"]);
                }), callback);
        }
        
        
        public static void GetOpenId(string code, string accessToken, IGetOpenIdCallback getOpenIdCallback)
        {
#if GRAVITY_WECHAT_GAME_MODE
            string platformStr = "wx"
#elif GRAVITY_BYTEDANCE_GAME_MODE
            string platformStr = "dy"
#elif GRAVITY_KUAISHOU_GAME_MODE
            string platformStr = "ks"
#else
            string platformStr = null;
#endif
            if (getOpenIdCallback == null)
            {
                GravitySDKLogger.Print("Callback error");
                return;
            }
            if (platformStr == null)
            {
                getOpenIdCallback.onFailed("平台类型错误，请检查是否正确添加全局宏参数：GRAVITY_WECHAT_GAME_MODE、GRAVITY_BYTEDANCE_GAME_MODE或者GRAVITY_KUAISHOU_GAME_MODE");
            }
            else
            {
                var requestDir = new Dictionary<string, object>()
                {
                    {"code", code},
                };
                UnityWebRequestMgr.Instance.Post(
                    GravityHost + "/event_center/api/v1/base/"+ platformStr + "/code2Session/?access_token=" + accessToken, requestDir,
                (request =>
                {
                    string responseText = request.downloadHandler.text;
                    // string responseText = "{\n  \"data\": {\n    \"resp\": {\n      \"session_key\": \"wHVdBZh5CmqKTl7zTnloSg==\",\n      \"openid\": \"oeleK67L8sN8MIqAkZa2fkPftyvs\",\n      \"unionid\": \"oaNSjv-v854AXW_1W41MZv3sSwKU\"\n    }\n  },\n  \"extra\": {},\n  \"code\": 0,\n  \"msg\": \"成功\"\n}";
                    
                    Dictionary<string, object> res = GE_MiniJson.Deserialize(responseText);
                    Debug.Log("response is " + responseText);
                    if (res != null)
                    {
                        if (res.TryGetValue("code", out var re))
                        {
                            int code = Convert.ToInt32(re);
                            if (code == 0)
                            {
                                if (res.TryGetValue("data", out object dataObj))
                                {
                                    Dictionary<string, object> dataDict = (Dictionary<string, object>) dataObj;
                                    // foreach (var kvp in dataDict)
                                    // {
                                    //     Debug.Log("key " + kvp.Key + " : " + kvp.Value?.ToString());
                                    // }
                                    if (dataDict.TryGetValue("resp", out object respObj))
                                    {
                                        // Dictionary<string, object> respDict = (Dictionary<string, object>) respObj;
                                        // foreach (var kvp in respDict)
                                        // {
                                        //     Debug.Log("key " + kvp.Key + " : " + kvp.Value?.ToString());
                                        // }
                                        getOpenIdCallback?.onSuccess((Dictionary<string, object>) respObj);
                                    }
                                    else
                                    {
                                        getOpenIdCallback?.onSuccess(new Dictionary<string, object>());
                                    }
                                }
                                else
                                {
                                    getOpenIdCallback?.onSuccess(new Dictionary<string, object>());
                                }
                                return;
                            }
                        }
                    }

                    getOpenIdCallback?.onFailed("code is not 0, failed with msg " + res?["msg"]);
                }));
            }
        }

        public static String GetAccessToken()
        {
            return _accessToken;
        }

        public static String GetClientId()
        {
            return _clientID;
        }
        
        /// <summary>
        /// 如果传入的currentClientId不为空，则set到_clientID，否则直接返回当前的_clientId
        /// </summary>
        /// <param name="currentClientId"></param>
        /// <returns></returns>
        public static String SetOrGetClientId(string currentClientId)
        {
            if (!GravitySDKUtil.IsEmptyString(currentClientId))
            {
                _clientID = currentClientId;
            }
            return _clientID;
        }

        public static void SetClientId(string clientId)
        {
            _clientID = clientId;
        }
        
        public static String GetChannel()
        {
            return _channel;
        }
    }
}