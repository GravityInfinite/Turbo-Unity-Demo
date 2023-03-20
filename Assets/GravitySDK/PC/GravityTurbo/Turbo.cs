using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GravitySDK.PC.GravityTurbo
{
    public static class Turbo
    {
        private const string TurboHost = "https://backend.gravity-engine.com";
        private static string _accessToken;
        private static string _clientID;

        private static (string platform, AdData adData) GetAdData(
            IReadOnlyDictionary<string, string> wxQueryPathDictionary)
        {
            AdData adData = null;
            var platform = "tencent";
            if (wxQueryPathDictionary == null)
            {
                return (platform, adData);
            }

            // 判断是否为头条巨量
            if (wxQueryPathDictionary.ContainsKey("clue_token") ||
                wxQueryPathDictionary.ContainsKey("ad_id") ||
                wxQueryPathDictionary.ContainsKey("creative_id") ||
                wxQueryPathDictionary.ContainsKey("request_id") ||
                wxQueryPathDictionary.ContainsKey("advertiser_id"))
            {
                adData = new AdData()
                {
                    clue_token = wxQueryPathDictionary.GetValueOrDefault("clue_token", ""),
                    ad_id = wxQueryPathDictionary.GetValueOrDefault("ad_id", ""),
                    creative_id = wxQueryPathDictionary.GetValueOrDefault("creative_id", ""),
                    request_id = wxQueryPathDictionary.GetValueOrDefault("request_id", ""),
                    req_id = wxQueryPathDictionary.GetValueOrDefault("req_id", ""),
                    advertiser_id = wxQueryPathDictionary.GetValueOrDefault("advertiser_id", ""),
                    
                    // 头条V2.0
                    project_id = wxQueryPathDictionary.GetValueOrDefault("project_id", ""),
                    promotion_id = wxQueryPathDictionary.GetValueOrDefault("promotion_id", ""),
                    mid1 = wxQueryPathDictionary.GetValueOrDefault("mid1", ""),
                    mid2 = wxQueryPathDictionary.GetValueOrDefault("mid2", ""),
                    mid3 = wxQueryPathDictionary.GetValueOrDefault("mid3", ""),
                    mid4 = wxQueryPathDictionary.GetValueOrDefault("mid4", ""),
                    mid5 = wxQueryPathDictionary.GetValueOrDefault("mid5", ""),

                };
                platform = "bytedance";
            }

            // 判断是否为快手磁力
            if (wxQueryPathDictionary.ContainsKey("ksUnitId") || wxQueryPathDictionary.ContainsKey("ksCampaignId") ||
                wxQueryPathDictionary.ContainsKey("ksChannel"))
            {
                adData = new AdData()
                {
                    callback = wxQueryPathDictionary.GetValueOrDefault("callback", ""),
                    ksChannel = wxQueryPathDictionary.GetValueOrDefault("ksChannel", ""),
                    ksCampaignId = wxQueryPathDictionary.GetValueOrDefault("ksCampaignId", ""),
                    ksUnitId = wxQueryPathDictionary.GetValueOrDefault("ksUnitId", ""),
                    ksCreativeId = wxQueryPathDictionary.GetValueOrDefault("ksCreativeId", "")
                };
                platform = "kuaishou";
            }

            if (wxQueryPathDictionary.ContainsKey("gdt_vid"))
            {
                adData = new AdData()
                {
                    gdt_vid = wxQueryPathDictionary.GetValueOrDefault("gdt_vid", "")
                };
                platform = "tencent";
            }
            
            if (wxQueryPathDictionary.ContainsKey("bd_vid"))
            {
                adData = new AdData()
                {
                    bd_vid = wxQueryPathDictionary.GetValueOrDefault("bd_vid", "")
                };
                platform = "baidu";
            }

            if (wxQueryPathDictionary.ContainsKey("turbo_vid"))
            {
                adData = new AdData()
                {
                    turbo_vid = wxQueryPathDictionary.GetValueOrDefault("turbo_vid", "")
                };
                platform = "gravity";
            }
            
            // 默认走到腾讯
            return (platform, adData);
        }

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
            Dictionary<string, string> wxLaunchQuery, Action<UnityWebRequest> actionResult)
        {
            // check params
            GlobalCheck();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name must be required");
            }

            var (platform, adData) = GetAdData(wxLaunchQuery);
            var registerRequestBody = new RegisterRequestBody
            {
                client_id = _clientID,
                name = name,
                channel = "base_channel",
                version = version,
                media_type = platform,
                wx_openid = wxOpenId,
                wx_unionid = wxUnionId,
                ad_data = adData,
                query_object = wxLaunchQuery
            };

            if (wxLaunchQuery.ContainsKey("turbo_promoted_object_id"))
            {
                registerRequestBody.promoted_object_id = wxLaunchQuery.GetValueOrDefault("turbo_promoted_object_id", "");
            }

            UnityWebRequestMgr.Instance.Post(TurboHost + "/event_center/api/v1/user/register/?access_token=" + _accessToken,
                registerRequestBody, actionResult);
        }

        public static void HandleEvent(string eventType, Action<UnityWebRequest> actionResult, bool isUseClientTime,
            long timestamp, string traceId, long amount, long realAmount
        )
        {
            // check params
            GlobalCheck();

            if (isUseClientTime && timestamp == 0)
            {
                throw new ArgumentException("timestamp must be required when isUseClientTime is true");
            }
            if (!string.IsNullOrEmpty(traceId) && traceId.Length > 128) {
                throw new ArgumentException("traceId is too long, max length is 128!");
            }

            // prepare data body
            var handleEventBody = new HandleEventBody
            {
                event_type = eventType,
                use_client_time = isUseClientTime,
                timestamp = timestamp,
                trace_id = traceId,
            };
            if (eventType == "pay")
            {
                if (amount == 0 || realAmount == 0)
                {
                    throw new ArgumentException("amount and realAmount must be required when eventType is pay");
                }
                var properties = new Properties
                {
                    amount = amount,
                    real_amount = realAmount
                };
                handleEventBody.properties = properties;
            }

            UnityWebRequestMgr.Instance.Post(
                TurboHost + "/event_center/api/v1/event/handle_event/?access_token=" + _accessToken + "&client_id=" + _clientID,
                handleEventBody, actionResult);
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="actionResult"></param> 查询回调，返回数据如下：
        // "user_list": [
        // {
        //     "create_time": "2022-09-09 14:50:04",
        //     "client_id": "Bn2RhTcU",
        //     "advertiser_id": "12948974294275",
        //     "channel": "wechat_mini_game",
        //     "click_company": "gdt",
        //     "aid": "65802182823",
        //     "cid": "65580218538",
        //     "bytedance_v2": {
        //         "project_id":"924563792",
        //         "promotion_id":"93795753",
        //         "mid1":"3256634642",
        //         "mid2":"2353252367",
        //         "mid3":"3245235236",
        //         "mid4":"6346347623",
        //         "mid5":"7345232424"
        //     }
        // },
        // ]
        public static void QueryUser(Action<UnityWebRequest> actionResult)
        {
            // check params
            GlobalCheck();
            // prepare data body
            var userList = new[] {_clientID};

            var queryUserBody = new QueryUserBody
            {
                user_list = userList
            };
            UnityWebRequestMgr.Instance.Post(
                TurboHost + "/event_center/api/v1/user/get/?access_token=" + _accessToken, queryUserBody, actionResult);
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