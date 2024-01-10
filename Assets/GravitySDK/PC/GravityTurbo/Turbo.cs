using System;
using System.Collections.Generic;
using GravityEngine;
using GravityEngine.Utils;
using GravitySDK.PC.TaskManager;
using GravitySDK.PC.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace GravitySDK.PC.GravityTurbo
{
    public static class Turbo
    {
        private const string TurboHost = "https://backend.gravity-engine.com";
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
        /// 初始化turbo SDK必须参数（每次启动都需要调用）
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
            GravitySDKLogger.Print("turbo init success");
        }

        public static void Initialize(string name, int version, string openId, Dictionary<string, string> wxLaunchQuery, bool enableSyncAttribution,
            IInitializeCallback initializeCallback, UnityWebRequestMgr.Callback callback)
        {
            // check params
            GlobalCheck();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name must be required");
            }

            var registerRequestDir = new Dictionary<string, object>()
            {
                {"client_id", _clientID},
                {"name", name},
                {"channel", _channel},
                {"version", version},
                {"wx_openid", openId},
                {"wx_unionid", ""},
                {"ad_data", wxLaunchQuery},
                {"need_return_attribution", enableSyncAttribution},
            };

            GravitySDKLogger.Print(registerRequestDir.ToString());

            UnityWebRequestMgr.Instance.Post(
                TurboHost + "/event_center/api/v1/user/initialize/?access_token=" + _accessToken,
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
        
        public static String GetAccessToken()
        {
            return _accessToken;
        }

        public static String GetClientId()
        {
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