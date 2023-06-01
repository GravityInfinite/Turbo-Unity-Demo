using System;
using System.Collections.Generic;
using GravitySDK.PC.TaskManager;
using UnityEngine;
using UnityEngine.Networking;

namespace GravitySDK.PC.GravityTurbo
{
    public static class Turbo
    {
        private const string TurboHost = "https://backend.gravity-engine.com";
        private static string _accessToken;
        private static string _clientID;

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
        /// <param name="accessToken"></param> 项目通行证，在：网站后台-->管理中心-->应用列表中找到Access Token列 复制（首次使用可能需要先新增应用）
        /// <param name="clientId"></param> 用户唯一标识，如微信小程序/小游戏的openid、Android ID、iOS的IDFA、或业务侧自行生成的唯一用户ID均可
        public static void InitSDK(string accessToken, string clientId)
        {
            _accessToken = accessToken;
            _clientID = clientId;
            GlobalCheck();
            Debug.Log("turbo init success");
        }

        public static void Register(string name, int version, string wxOpenId, string wxUnionId,
            Dictionary<string, string> wxLaunchQuery, Action<UnityWebRequest> actionResult,
            UnityWebRequestMgr.Callback callback)
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
                {"channel", "base_channel"},
                {"version", version},
                {"wx_openid", wxOpenId},
                {"wx_unionid", wxUnionId},
                {"ad_data", wxLaunchQuery},
            };
            
            if (wxLaunchQuery.ContainsKey("turbo_promoted_object_id"))
            {
                string value;
                if (wxLaunchQuery.TryGetValue("turbo_promoted_object_id", out value))
                {
                    Debug.Log("get promoted object id");
                }
                else
                {
                    Debug.Log("no promoted object id");
                }
                registerRequestDir["promoted_object_id"] = value ?? "";
            }
            
            Debug.Log(registerRequestDir.ToString());

            UnityWebRequestMgr.Instance.Post(
                TurboHost + "/event_center/api/v1/user/register/?access_token=" + _accessToken,
                registerRequestDir, actionResult, callback);
        }

        public static String GetAccessToken()
        {
            return _accessToken;
        }
        
        public static String GetClientId()
        {
            return _clientID;
        }
    }
}