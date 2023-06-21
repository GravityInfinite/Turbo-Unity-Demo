using UnityEngine;
using UnityEngine.SceneManagement;
using GravityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using GravityEngine.Utils;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Utils;

#if GRAVITY_WECHAT_GAME_MODE
using WeChatWASM;
#elif GRAVITY_BYTEDANCE_GAME_MODE
using StarkSDKSpace;
#endif

public class RegisterCallbackImpl : IRegisterCallback
{
    
    public void onFailed(string errorMsg)
    {
        Debug.Log("register failed  with message " + errorMsg);
    }

    public void onSuccess()
    {
        Debug.Log("register success");
        Debug.Log("register call end");
        // 建议在此执行一次Flush
        GravityEngineAPI.Flush();
    }
}

public class GravityEngineDemo : MonoBehaviour, IDynamicSuperProperties
{
    public GUISkin skin;
    private const int Margin = 20;

    private const int Height = 60;

    private Vector2 scrollPosition;

    // 动态公共属性接口
    public Dictionary<string, object> GetDynamicSuperProperties()
    {
        return new Dictionary<string, object>()
        {
            {"DynamicProperty", DateTime.Now}
        };
    }

    private void Start()
    {
#if GRAVITY_WECHAT_GAME_MODE
        WXBase.InitSDK((code) => { Debug.Log("wx init end"); });
#elif GRAVITY_BYTEDANCE_GAME_MODE
        StarkSDK.Init();
        StarkSDK.API.SetContainerInitCallback((env =>
        {
            Debug.Log("stark init end");
            foreach (var item in env.GetLaunchOptionsSync().Query)
            {
                Debug.Log("key is " + item.Key + " value is " + item.Value);
            }

            StarkUIManager.ShowToastLong("stark init end " + env + " ::: " + env.GetLaunchOptionsSync().Query);
        }));
#endif
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Margin, Margin, Screen.width - 2 * Margin, Screen.height));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(Screen.width - 2 * Margin),
            GUILayout.Height(Screen.height - 100));

        GUIStyle style = GUI.skin.label;
        style.fontSize = 25;
        GUILayout.Label("StartEngine / EventUpload", style);

        GUIStyle buttonStyle = GUI.skin.button;
        buttonStyle.fontSize = 20;
        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(Height));
        if (GUILayout.Button("StartEngine", GUILayout.Height(Height)))
        {
            // 手动初始化（动态挂载 GravityEngineAPI 脚本）
            new GameObject("GravityEngine", typeof(GravityEngineAPI));

#if GRAVITY_WECHAT_GAME_MODE
            // 微信小游戏示例
            //设置实例参数并启动引擎，将以下三个参数修改成您应用对应的参数，参数可以在引力后台--管理中心--应用管理中查看
            string accessToken = "gZGljPsq7I4wc3BMvkAUsevQznx1jahi";
            string clientId = "1234567890067";
            
            // 启动引力引擎
            GravityEngineAPI.StartGravityEngine(accessToken, clientId, GravityEngineAPI.SDKRunMode.DEBUG);
            // 微信小游戏开启自动采集，并设置自定属性
            GravityEngineAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.WECHAT_GAME_ALL, new Dictionary<string, object>()
            {
                {"auto_track_key", "auto_track_value"} // 静态属性
            });
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && !GRAVITY_BYTEDANCE_GAME_MODE
            // Android、iOS原生应用示例
            //设置实例参数并启动引擎，将以下三个参数修改成您应用对应的参数，参数可以在引力后台--管理中心--应用管理中查看
            string accessToken = "x5emsWAxqnlwqpDH1j4bbicR8igmhruT";
            string clientId = "1234567890067";
            string aesKey = "k7ZjSgc1Z8j551UJUNLlWA==";

            // 启动引力引擎
            GravityEngineAPI.StartGravityEngine(accessToken, clientId, GravityEngineAPI.SDKRunMode.DEBUG, "xiaomi", aesKey);
            // 原生app开启自动采集，并设置自定属性
            GravityEngineAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.APP_ALL, new Dictionary<string, object>()
            {
                {"auto_track_key", "auto_track_value"} // 静态属性
            });
#elif GRAVITY_BYTEDANCE_GAME_MODE
            // 抖音小游戏示例
            //设置实例参数并启动引擎，将以下三个参数修改成您应用对应的参数，参数可以在引力后台--管理中心--应用管理中查看
            string accessToken = "z4gcI6n1O52DRibPXZfjvn8w3YVtLUqp";
            string clientId = "1234567";

            // 启动引力引擎
            GravityEngineAPI.StartGravityEngine(accessToken, clientId, GravityEngineAPI.SDKRunMode.DEBUG);
            // 微信小游戏开启自动采集，并设置自定属性
            GravityEngineAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.BYTEDANCE_GAME_ALL, new Dictionary<string, object>()
            {
                {"auto_track_key", "auto_track_value"} // 静态属性
            });
#elif UNITY_EDITOR
            // Unity Editor
            //设置实例参数并启动引擎，将以下三个参数修改成您应用对应的参数，参数可以在引力后台--管理中心--应用管理中查看
            string accessToken = "x5emsWAxqnlwqpDH1j4bbicR8igmhruT";
            string clientId = "1234567890067";
            string aesKey = "k7ZjSgc1Z8j551UJUNLlWA==";

            // 启动引力引擎
            GravityEngineAPI.StartGravityEngine(accessToken, clientId, GravityEngineAPI.SDKRunMode.DEBUG, "xiaomi", aesKey);
#endif
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Register", GUILayout.Height(Height)))
        {
            Debug.Log("register clicked");
            GravityEngineAPI.Register("name_123", 1, "your_openid_111", new RegisterCallbackImpl());
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.Label("EventTrack", GUI.skin.label);
        GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Height(Height));
        if (GUILayout.Button("TrackEvent", GUILayout.Height(Height)))
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties["channel"] = "base"; //字符串，长度不超过2048
            properties["age"] = 1; //数字
            properties["isVip"] = true; //布尔
            properties["birthday"] = DateTime.Now; //时间
            properties["movies"] = new List<string>()
                {"Interstellar", "The Negro Motorist Green Book"}; //字符串元素的数组 最大元素个数为 500

            GravityEngineAPI.Track("GE_000", properties);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("TrackBytedanceAdShowEvent", GUILayout.Height(Height)))
        {
            // 记录用户头条广告观看事件
            GravityEngineAPI.TrackBytedanceAdShowEvent("your_open_id", "your_unit_id");
        }
        
        GUILayout.Space(20);
        if (GUILayout.Button("TrackAdShowEvent", GUILayout.Height(Height)))
        {
            // 记录用户广告观看事件
            GravityEngineAPI.TrackAdShowEvent("reward", "your_ad_unit_id");
        }

        GUILayout.Space(20);
        if (GUILayout.Button("TrackPayEvent", GUILayout.Height(Height)))
        {
            // 记录用户付费事件
            GravityEngineAPI.TrackPayEvent(300, "CNY", "your_order_id", "月卡", "支付宝");
        }

        GUILayout.Space(20);
        if (GUILayout.Button("TrackMPLogin", GUILayout.Height(Height)))
        {
            // 记录用户登录事件
            GravityEngineAPI.TrackMPLogin();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("TrackMPLogout", GUILayout.Height(Height)))
        {
            // 记录用户退出登录事件
            GravityEngineAPI.TrackMPLogout();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Height(Height));
        if (GUILayout.Button("TrackEventWithTimeTravel", GUILayout.Height(Height)))
        {
            GravityEngineAPI.TimeEvent("TestTimeEvent");
#if !(UNITY_WEBGL)
            Thread.Sleep(1000);
#endif
            GravityEngineAPI.Track("TestTimeEvent");
        }

        GUILayout.Space(20);
        if (GUILayout.Button("TrackEventWithDate", GUILayout.Height(Height)))
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties["status"] = 4;
            GravityEngineAPI.Track("GE_001", properties, DateTime.Now, TimeZoneInfo.Utc);
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.Label("UserProfile", GUI.skin.label);
        GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Height(Height));
        if (GUILayout.Button("UserSet", GUILayout.Height(Height)))
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            userProperties["$name"] = "your_name2";
            GravityEngineAPI.UserSet(userProperties);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserSetOnce", GUILayout.Height(Height)))
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            userProperties["$first_visit_time"] = DateTime.Now;
            GravityEngineAPI.UserSetOnce(userProperties);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserAdd", GUILayout.Height(Height)))
        {
            GravityEngineAPI.UserAdd("age", 1);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserNumberMin", GUILayout.Height(Height)))
        {
            GravityEngineAPI.UserNumberMin("age", 0);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserNumberMax", GUILayout.Height(Height)))
        {
            GravityEngineAPI.UserNumberMax("age", 10);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserUnset", GUILayout.Height(Height)))
        {
            GravityEngineAPI.UserUnset(new List<string>() {"age", "$name", "$first_visit_time", "movies"});
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserDelete", GUILayout.Height(Height)))
        {
            GravityEngineAPI.UserDelete();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserAppend", GUILayout.Height(Height)))
        {
            List<string> propList = new List<string>();
            propList.Add("Interstellar");
            propList.Add("The Negro Motorist Green Book");

            // 为属性名为 movies 的用户属性追加 2 个元素
            GravityEngineAPI.UserAppend(new Dictionary<string, object>()
            {
                {"movies", propList}
            });
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UserUniqAppend", GUILayout.Height(Height)))
        {
            List<string> propList = new List<string>();
            propList.Add("Interstellar");
            propList.Add("The Shawshank Redemption");

            // 为属性名为 movies 的用户属性去重追加 2 个元素
            GravityEngineAPI.UserUniqAppend(new Dictionary<string, object>()
            {
                {"movies", propList}
            });
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("SuperProperties", GUI.skin.label);
        GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Height(Height));
        if (GUILayout.Button("SetSuperProperties", GUILayout.Height(Height)))
        {
            Dictionary<string, object> superProperties = new Dictionary<string, object>();
            superProperties["vipLevel"] = 1;
            GravityEngineAPI.SetSuperProperties(superProperties);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("UpdateSuperProperties", GUILayout.Height(Height)))
        {
            Dictionary<string, object> superProperties = new Dictionary<string, object>();
            superProperties["vipLevel"] = 2;
            GravityEngineAPI.SetSuperProperties(superProperties);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("ClearSuperProperties", GUILayout.Height(Height)))
        {
            GravityEngineAPI.UnsetSuperProperty("vipLevel");
        }

        GUILayout.Space(20);
        if (GUILayout.Button("ClearAllSuperProperties", GUILayout.Height(Height)))
        {
            GravityEngineAPI.ClearSuperProperties();
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Height(Height));
        if (GUILayout.Button("SetDynamicSuperProperties", GUILayout.Height(Height)))
        {
            GravityEngineAPI.SetDynamicSuperProperties(this);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("LoadScene", GUILayout.Height(Height)))
        {
            SceneManager.LoadScene("NewScene", LoadSceneMode.Single);
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.Label("Others", GUI.skin.label);
        GUILayout.BeginHorizontal(GUI.skin.textArea, GUILayout.Height(Height));
        if (GUILayout.Button("Flush", GUILayout.Height(Height)))
        {
            GravityEngineAPI.Flush();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("GetDeviceID", GUILayout.Height(Height)))
        {
            Debug.Log("DeviceID: " + GravityEngineAPI.GetDeviceId());
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Pause", GUILayout.Height(Height)))
        {
            GravityEngineAPI.SetTrackStatus(GE_TRACK_STATUS.PAUSE);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Stop", GUILayout.Height(Height)))
        {
            GravityEngineAPI.SetTrackStatus(GE_TRACK_STATUS.STOP);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("SaveOnly", GUILayout.Height(Height)))
        {
            GravityEngineAPI.SetTrackStatus(GE_TRACK_STATUS.SAVE_ONLY);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Normal", GUILayout.Height(Height)))
        {
            GravityEngineAPI.SetTrackStatus(GE_TRACK_STATUS.NORMAL);
        }

        GUILayout.Space(20);
        if (GUILayout.Button("CalibrateTime", GUILayout.Height(Height)))
        {
            //时间戳,单位毫秒 对应时间为1668982523000 2022-11-21 06:15:23
            GravityEngineAPI.CalibrateTime(1668982523000);

            //NTP 时间服务器校准，如：time.apple.com
            //GravityEngineAPI.CalibrateTimeWithNtp("time.apple.com");
        }

        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}