using System;

namespace GravitySDK.PC.Constant
{
    // 自动采集事件类型
    [Flags]
    public enum AUTO_TRACK_EVENTS
    {
        NONE = 0,
        APP_START = 1 << 0, // 当应用进入前台的时候触发上报，对应 $AppStart
        APP_END = 1 << 1, // 当应用进入后台的时候触发上报，对应 $AppEnd
        APP_CRASH = 1 << 4, // 当出现未捕获异常的时候触发上报，对应 $AppCrash
        APP_INSTALL = 1 << 5, // 应用安装后首次打开的时候触发上报，对应 $AppInstall
        APP_SCENE_LOAD = 1 << 6, // 当应用内加载场景的时候触发上报，对应 $SceneLoaded
        APP_SCENE_UNLOAD = 1 << 7, // 当应用内卸载场景的时候触发上报，对应 $SceneUnloaded
        APP_ALL = APP_START | APP_END | APP_INSTALL | APP_SCENE_LOAD | APP_SCENE_UNLOAD, // 默认不再开启crash信息的收集
#if GRAVITY_WECHAT_GAME_MODE || GRAVITY_BYTEDANCE_GAME_MODE
        // 微信小游戏、抖音小游戏
        MP_SHOW = 1 << 8, // 当小游戏展示的时候触发上报，对应 $MPShow
        MP_HIDE = 1 << 9, // 当小游戏进入后台的时候触发上报，对应 $MPHide
        MP_SHARE = 1 << 10, // 当小游戏分享的时候触发上报，对应 $MPShare
        MP_ADD_TO_FAVORITES = 1 << 11, // 当小游戏添加收藏的时候触发上报，对应 $MPAddFavorites
        WECHAT_GAME_ALL = APP_SCENE_LOAD | APP_SCENE_UNLOAD | MP_SHOW | MP_HIDE | MP_SHARE | MP_ADD_TO_FAVORITES,
        BYTEDANCE_GAME_ALL = APP_SCENE_LOAD | APP_SCENE_UNLOAD | MP_SHOW | MP_HIDE
#endif
    }

    // 数据上报状态
    public enum GE_TRACK_STATUS
    {
        PAUSE = 1, // 暂停数据上报
        STOP = 2, // 停止数据上报，并清除缓存
        SAVE_ONLY = 3, // 数据入库，但不上报
        NORMAL = 4 // 恢复数据上报
    }
}