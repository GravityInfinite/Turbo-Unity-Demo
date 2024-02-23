#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE || GRAVITY_KUAISHOU_GAME_MODE
using System.Collections;
using System.Collections.Generic;
using GravityEngine;
using GravitySDK.PC.Constant;
using GravitySDK.PC.Utils;

#if GRAVITY_WECHAT_GAME_MODE
using WeChatWASM;
#elif GRAVITY_BYTEDANCE_GAME_MODE
using StarkSDKSpace;
#endif
using UnityEngine;

/// <summary>
/// 微信小游戏事件自动采集类，要求引用Unity WebGL 微信小游戏适配方案中的Unity插件，参考：https://github.com/wechat-miniprogram/minigame-unity-webgl-transform
/// </summary>
public class WeChatGameAutoTrack : MonoBehaviour
{
    private AUTO_TRACK_EVENTS _mAutoTrackEvents = AUTO_TRACK_EVENTS.NONE;

    private readonly Dictionary<string, Dictionary<string, object>> _mAutoTrackProperties =
        new Dictionary<string, Dictionary<string, object>>();

    private Dictionary<string, object> GetAutoTrackProperties(string eventName)
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();
        if (_mAutoTrackProperties.ContainsKey(eventName))
        {
            GravitySDKUtil.AddDictionary(properties, _mAutoTrackProperties[eventName]);
        }

        return properties;
    }

    public void EnableAutoTrack(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties = null)
    {
        SetAutoTrackProperties(events, properties);
        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHOW))
        {
#if GRAVITY_WECHAT_GAME_MODE
            WX.OnShow((result =>
            {
                GravitySDKLogger.Print("wechat game on show");
                GravityEngineAPI.TrackMPShow(result.query, "" + result.scene, GetAutoTrackProperties(AUTO_TRACK_EVENTS.MP_SHOW.ToString()));
            }));
#elif GRAVITY_BYTEDANCE_GAME_MODE
            StarkSDK.API.GetStarkAppLifeCycle().OnShow += (scene, query, refererInfo) =>
            {
                GravitySDKLogger.Print("bytedance game on show");
                GravityEngineAPI.TrackMPShow(query, "" + scene,
                    GetAutoTrackProperties(AUTO_TRACK_EVENTS.MP_SHOW.ToString()));
            };
#endif
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_HIDE))
        {
#if GRAVITY_WECHAT_GAME_MODE
            WX.OnHide((result =>
            {
                GravitySDKLogger.Print("wechat game on hide");
                GravityEngineAPI.TrackMPHide(GetAutoTrackProperties(AUTO_TRACK_EVENTS.MP_HIDE.ToString()));
            }));
#elif GRAVITY_BYTEDANCE_GAME_MODE
            StarkSDK.API.GetStarkAppLifeCycle().OnHide += () =>
            {
                GravitySDKLogger.Print("bytedance game on hide");
                GravityEngineAPI.TrackMPHide(GetAutoTrackProperties(AUTO_TRACK_EVENTS.MP_HIDE.ToString()));
            };
#endif
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES))
        {
#if GRAVITY_WECHAT_GAME_MODE
            WX.OnAddToFavorites((action =>
            {
                GravitySDKLogger.Print("wechat game on add to favorites");
                GravityEngineAPI.TrackMPAddFavorites(GetAutoTrackProperties(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString()));
            }));
#endif
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHARE))
        {
#if GRAVITY_WECHAT_GAME_MODE
            WXShareAppMessageParam param = new WXShareAppMessageParam();
            WX.OnShareAppMessage(param, action =>
            {
                action.Invoke(param);
                GravityEngineAPI.TrackMPShare(GetAutoTrackProperties(AUTO_TRACK_EVENTS.MP_SHARE.ToString()));
            });
#endif
        }
    }

    public void SetAutoTrackProperties(AUTO_TRACK_EVENTS events, Dictionary<string, object> properties)
    {
        _mAutoTrackEvents = events;
        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHOW))
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_SHOW.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHOW.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHOW.ToString()] = properties;
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_HIDE))
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_HIDE.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_HIDE.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_HIDE.ToString()] = properties;
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_SHARE))
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_SHARE.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHARE.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_SHARE.ToString()] = properties;
        }

        if (_mAutoTrackEvents.HasFlag(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES))
        {
            if (_mAutoTrackProperties.ContainsKey(AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString()))
            {
                GravitySDKUtil.AddDictionary(_mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString()],
                    properties);
            }

            _mAutoTrackProperties[AUTO_TRACK_EVENTS.MP_ADD_TO_FAVORITES.ToString()] = properties;
        }
    }
}
#endif