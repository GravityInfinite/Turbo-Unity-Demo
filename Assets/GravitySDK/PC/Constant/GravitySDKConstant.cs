using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GravitySDK.PC.Constant
{
    public delegate void ResponseHandle(Dictionary<string, object> result = null);

    public class GravitySDKConstant
    {
        //库运行的平台
        public static readonly string PLATFORM = "PC";

        //时间格式
        public static readonly string TIME_PATTERN = "{0:yyyy-MM-dd HH:mm:ss.fff}";

        /*
         * 关键字声明
         * */
        //事件类型
        public static readonly string TYPE = "type";

        //时间
        public static readonly string TIME = "time";

        //访客ID
        public static readonly string DISTINCT_ID = "$distinct_id";

        //事件名称
        public static readonly string EVENT_NAME = "event";

        //账号ID
        public static readonly string ACCOUNT_ID = "$account_id";

        //属性
        public static readonly string PROPERTIES = "properties";

        //网络类型
        public static readonly string NETWORK_TYPE = "$network_type";

        //库版本
        public static readonly string LIB_VERSION = "$lib_version";

        //库名称
        public static readonly string LIB = "$lib";

        //系统类型
        public static readonly string OS = "$os";

        //设备ID
        public static readonly string DEVICE_ID = "$device_id";

        //设备高度
        public static readonly string SCREEN_HEIGHT = "$screen_height";

        //设备宽度
        public static readonly string SCREEN_WIDTH = "$screen_width";

        //厂商
        public static readonly string MANUFACTURE = "$manufacturer";

        //设备型号
        public static readonly string DEVICE_MODEL = "$model";

        // 设备品牌，一般等同于厂商
        public static readonly string DEVICE_BRAND = "$brand";

        //系统语言
        public static readonly string SYSTEM_LANGUAGE = "$system_language";

        //系统版本号
        public static readonly string OS_VERSION = "$os_version";

        //app版本号
        public static readonly string APP_VERSION = "$app_version";

        //app唯一标识
        public static readonly string APP_BUNDLEID = "$bundle_id";

        //时区偏移
        public static readonly string ZONE_OFFSET = "$zone_offset";

        //appId
        public static readonly string APPID = "$app_id";

        //单条数据唯一标识
        public static readonly string UUID = "$uuid";

        //首次事件ID
        public static readonly string FIRST_CHECK_ID = "$first_check_id";

        //事件唯一ID
        public static readonly string EVENT_ID = "$event_id";

        //随机数
        public static readonly string RANDOM_ID = "RANDDOM_ID";

        //随机设备ID(WebGL)
        public static readonly string RANDOM_DEVICE_ID = "RANDOM_DEVICE_ID";

        //事件持续时长
        public static readonly string DURATION = "$event_duration";

        //静态公共事件属性
        public static readonly string SUPER_PROPERTY = "super_properties";

        //用户属性相关
        public static readonly string USER_ADD = "profile_increment";
        public static readonly string USER_NUMBER_MIN = "profile_number_min";
        public static readonly string USER_NUMBER_MAX = "profile_number_max";
        public static readonly string USER_SET = "profile_set";
        public static readonly string USER_SETONCE = "profile_set_once";
        public static readonly string USER_UNSET = "profile_unset";
        public static readonly string USER_DEL = "profile_delete";
        public static readonly string USER_APPEND = "profile_append";
        public static readonly string USER_UNIQ_APPEND = "profile_uniq_append";

        //是否暂停数据上报
        public static readonly string ENABLE_TRACK = "enable_track";

        //是否停止数据上报
        public static readonly string OPT_TRACK = "opt_track";

        //是否安装
        public static readonly string IS_INSTALL = "is_install";

        //安装事件
        public static readonly string INSTALL_EVENT = "$AppInstall";

        //启动事件
        public static readonly string START_EVENT = "$AppStart";

        //结束事件
        public static readonly string END_EVENT = "$AppEnd";

        //崩溃事件
        public static readonly string CRASH_EVENT = "$AppCrash";

        //小游戏启动事件
        public static readonly string MP_LAUNCH = "$MPLaunch";

        //小游戏展示事件
        public static readonly string MP_SHOW = "$MPShow";

        //小游戏退到后台事件
        public static readonly string MP_HIDE = "$MPHide";

        //小游戏分享事件
        public static readonly string MP_SHARE = "$MPShare";

        //小游戏添加收藏事件
        public static readonly string MP_ADD_FAVORITES = "$MPAddFavorites";

        //崩溃原因
        public static readonly string CRASH_REASON = "$app_crashed_reason";

        // host
        public static readonly string SERVER_URL = "https://backend.gravity-engine.com";
    }
}