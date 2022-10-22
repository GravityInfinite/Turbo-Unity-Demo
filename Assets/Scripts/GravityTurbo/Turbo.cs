using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GravityTurbo
{
    public static class Turbo
    {
        private const string TurboHost = "https://turbo.api.plutus-cat.com/event_center/api/v1";
        private static string _accessToken;
        private static string _clientID;

        private static (string platform, AdData adData) GetAdData(
            IReadOnlyDictionary<string, string> wxQueryPathDictionary)
        {
            AdData adData = null;
            var platform = "tencent";

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
                    advertiser_id = wxQueryPathDictionary.GetValueOrDefault("advertiser_id", "")
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

        /// <summary>
        /// 在引力引擎注册，其他方法均需在本方法回调成功之后才可正常使用
        /// </summary>
        /// <param name="name"></param>             用户名
        /// <param name="channel"></param>          用户注册渠道
        /// <param name="version"></param>          用户注册的程序版本
        /// <param name="wxOpenId"></param>         微信open id (微信小程序和小游戏必填)
        /// <param name="wxUnionId"></param>        微信union id（微信小程序和小游戏选填）
        /// <param name="wxLaunchQuery"></param>    启动参数字典(微信小程序和小游戏必填)
        /// <param name="actionResult"></param>     网络回调，其他方法均需在回调成功之后才可正常使用
        /// <exception cref="ArgumentException"></exception>
        public static void Register(string name, string channel, int version, string wxOpenId, string wxUnionId,
            Dictionary<string, string> wxLaunchQuery, Action<UnityWebRequest> actionResult)
        {
            // check params
            GlobalCheck();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name must be required");
            }

            if (string.IsNullOrEmpty(channel))
            {
                throw new ArgumentException("channel must be required");
            }

            var (platform, adData) = GetAdData(wxLaunchQuery);
            var registerRequestBody = new RegisterRequestBody
            {
                client_id = _clientID,
                name = name,
                version = version,
                media_type = platform,
                wx_openid = wxOpenId,
                wx_unionid = wxUnionId,
                ad_data = adData
            };

            UnityWebRequestMgr.Instance.Post(TurboHost + "/user/register/?access_token=" + _accessToken,
                registerRequestBody, actionResult);
        }

        /// <summary>
        /// 启动埋点上报
        /// </summary>
        /// <param name="actionResult"></param> 上报回调
        public static void EventStartup(Action<UnityWebRequest> actionResult)
        {
            HandleEvent("start", actionResult);
        }

        /// <summary>
        /// 激活埋点上报
        /// </summary>
        /// <param name="actionResult"></param> 上报回调
        public static void EventActivate(Action<UnityWebRequest> actionResult)
        {
            HandleEvent("activate", actionResult);
        }

        /// <summary>
        /// 注册埋点上报
        /// </summary>
        /// <param name="actionResult"></param> 上报回调
        public static void EventRegister(Action<UnityWebRequest> actionResult)
        {
            HandleEvent("register", actionResult);
        }

        /// <summary>
        /// 次留埋点上报
        /// </summary>
        /// <param name="actionResult"></param> 上报回调
        public static void EventTwice(Action<UnityWebRequest> actionResult)
        {
            HandleEvent("twice", actionResult);
        }

        /// <summary>
        /// 关键行为埋点上报
        /// </summary>
        /// <param name="actionResult"></param> 上报回调
        public static void EventKeyActive(Action<UnityWebRequest> actionResult)
        {
            HandleEvent("key_active", actionResult);
        }

        /// <summary>
        /// 付费埋点上报
        /// </summary>
        /// <param name="amount"></param>       付费金额
        /// <param name="realAmount"></param>   付费折后金额（实际付费金额）
        /// <param name="actionResult"></param> 上报回调
        public static void EventPay(long amount, long realAmount, Action<UnityWebRequest> actionResult)
        {
            HandleEvent("pay", actionResult, amount, realAmount);
        }

        private static void HandleEvent(string eventType, Action<UnityWebRequest> actionResult, long amount = 0,
            long realAmount = 0
        )
        {
            // check params
            GlobalCheck();

            // prepare data body
            var handleEventBody = new HandleEventBody
            {
                event_type = eventType,
                timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            if (eventType == "pay")
            {
                var properties = new Properties
                {
                    amount = amount,
                    real_amount = realAmount
                };
                handleEventBody.properties = properties;
            }

            UnityWebRequestMgr.Instance.Post(
                TurboHost + "/event/handle_event/?access_token=" + _accessToken + "&client_id=" + _clientID,
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
        //     "cid": "65580218538"
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
                TurboHost + "/user/get/?access_token=" + _accessToken, queryUserBody, actionResult);
        }
    }
}