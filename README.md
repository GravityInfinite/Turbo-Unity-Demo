# Unity 接入

本文档为**Unity**接入 [引力引擎](https://gravity-engine.com/)的技术接入方案，具体 Demo
请参考[GitHub](https://github.com/GravityInfinite/Turbo-Unity-Demo)开源项目，Demo 工程中可以参考 `GravityEngineDemo.cs` 脚本中对每一个方法的调用示例。

### 1. SDK基础配置

#### 1.1 SDK 引入

下载最新的 [GravityEngine.unitypackage](https://github.com/GravityInfinite/Turbo-Unity-Demo/releases)
资源文件，并导入资源文件到您的项目中：`Assets > Import Package > Custom Package`，选中您刚刚下载的文件。

> 注意：如果您的项目是微信小游戏，请一定注意要先接入微信团队开源的Unity转微信小游戏的插件，可参考[这里](https://github.com/wechat-miniprogram/minigame-unity-webgl-transform)。

#### 1.2 引力引擎初始化

请直接复制以下代码到您项目中需要进行引力引擎初始化的地方，一般建议在能够获取到用户唯一ID，如微信小游戏的`openId`时，尽早的进行初始化。

```csharp
// 手动初始化（动态挂载 GravityEngineAPI 脚本）
new GameObject("GravityEngine", typeof(GravityEngineAPI));

//设置实例参数并启动引擎，将以下三个参数修改成您应用对应的参数，参数可以在引力后台--管理中心--应用管理中查看
string appId = "your_app_id";
string accessToken = "your_access_token";
string clientId = "your_user_id"; // 通常是某一个用户的唯一标识，比如微信小游戏中的 openId

var systemInfo = WX.GetSystemInfoSync();
// 提前设置设备属性信息
GravitySDKDeviceInfo.SetWechatGameDeviceInfo(new WechatGameDeviceInfo()
{
    SDKVersion = systemInfo.SDKVersion, // 微信SDK版本号
    benchmarkLevel = systemInfo.benchmarkLevel,
    brand = systemInfo.brand,
    deviceOrientation = systemInfo.deviceOrientation,
    language = systemInfo.language,
    model = systemInfo.model,
    platform = systemInfo.platform,
    screenHeight = systemInfo.screenHeight,
    screenWidth = systemInfo.screenWidth,
    system = systemInfo.system,
    version = systemInfo.version, // 微信版本号
});
// 启动引力引擎
GravityEngineAPI.StartGravityEngine(appId, accessToken, clientId, GravityEngineAPI.SDKRunMode.NORMAL);
// 记录小程序启动事件，在StartEngine之后并且获取network_type之后调用
WX.GetNetworkType(new GetNetworkTypeOption()
{
    success = (result) => { GravitySDKDeviceInfo.SetNetworkType(result.networkType); },
    fail = (result) => { GravitySDKDeviceInfo.SetNetworkType("error"); },
    complete = (result) =>
    {
        LaunchOptionsGame launchOptionsSync = WX.GetLaunchOptionsSync();
        GravityEngineAPI.TrackMPLaunch(launchOptionsSync.query, launchOptionsSync.scene);
        GravityEngineAPI.Flush();
    }
});

// 挂载采集器，以开启微信小游戏的自动采集
GameObject mWechatGameAutoTrackObj = new GameObject("WechatGameAutoTrack", typeof(WeChatGameAutoTrack));
WeChatGameAutoTrack mWechatGameAutoTrack = (WeChatGameAutoTrack) mWechatGameAutoTrackObj.GetComponent(typeof(WeChatGameAutoTrack));
mWechatGameAutoTrack.EnableAutoTrack(AUTO_TRACK_EVENTS.WECHAT_GAME);
GravityEngineAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.WECHAT_GAME);
DontDestroyOnLoad(mWechatGameAutoTrackObj);
```

#### 1.3 Register 用户注册

```csharp
/// <summary>
/// 在用户注册或者可以获取到用户唯一性信息时调用，推荐首次安装启动时调用，其他方法均需在本方法回调成功之后才可正常使用
/// </summary>
/// <param name="name"></param>             用户名
/// <param name="channel"></param>          用户注册渠道
/// <param name="version"></param>          用户注册的程序版本，比如当前微信小游戏的版本号
/// <param name="wxOpenId"></param>         微信open id (微信小程序和小游戏必填)
/// <param name="wxUnionId"></param>        微信union id（微信小程序和小游戏选填）
/// <param name="wxLaunchQuery"></param>    启动参数字典(微信小程序和小游戏必填)
/// <param name="actionResult"></param>     网络回调，其他方法均需在回调成功之后才可正常使用
/// <exception cref="ArgumentException"></exception> 当参数校验失败时，会抛出ArgumentException异常

var option = WX.GetLaunchOptionsSync();
GravityEngineAPI.Register("name_123", "test", 1, "your_wx_openid", "your_wx_unionid", option.query,
    request =>
    {
        Debug.Log("register call end");
        Debug.Log(request.downloadHandler.text);
    });
```

#### 1.4 HandleEventUpload 埋点事件上报

```csharp
/// <summary>
/// 埋点事件上报（主要分为：注册、激活、次留、付费、关键行为五种类型的事件）
/// </summary>
/// <param name="eventType"></param>            上报事件类型，取值为：pay、activate、register、key_active、twice，分别对应：付费、激活、注册、关键行为、次留事件
/// <param name="actionResult"></param>         上报回调
/// <param name="amount"></param>               付费金额
/// <param name="realAmount"></param>           付费折后金额（实际付费金额）
/// <param name="isUseClientTime"></param>      是否使用上报的timestamp作为回传时间，默认为false，当为true时，timestamp必填
/// <param name="timestamp"></param>            事件发生时间，用来回传给广告平台，毫秒时间戳(只有在`use_client_time`为`true`时才需要传入)
/// <param name="traceId"></param>              本次事件的唯一id（重复上报会根据该id去重，trace_id的长度不能超过128），可填入订单id，请求id等唯一值。如果为空，引力引擎则会自动生成一个。

// 上报注册事件举例
GravityEngineAPI.HandleEventUpload("register", request =>
{
    Debug.Log("handle event register call end");
    Debug.Log(request.downloadHandler.text);
});

// 上报激活事件举例
GravityEngineAPI.HandleEventUpload("activate", request =>
{
    Debug.Log("handle event activate call end");
    Debug.Log(request.downloadHandler.text);
});

// 上报次留事件举例
GravityEngineAPI.HandleEventUpload("twice", request =>
{
    Debug.Log("handle event twice call end");
    Debug.Log(request.downloadHandler.text);
});

// 上报付费事件举例，一定要传入金额，否则会上报失败影响买量！
GravityEngineAPI.HandleEventUpload("pay", request =>
{
    Debug.Log("handle event pay call end");
    Debug.Log(request.downloadHandler.text);
}, 300, 300);

// 上报关键行为事件举例
GravityEngineAPI.HandleEventUpload("key_active", request =>
{
    Debug.Log("handle event key active call end");
    Debug.Log(request.downloadHandler.text);
});
```

#### 1.5 QueryUser 查询用户信息

```csharp
/// <summary>
/// 查询用户信息，具体信息参考下方文档
/// </summary>
/// <param name="actionResult"></param> 查询回调，返回数据如下：
///  1. client_id       用户ID
//   2. channel         用户渠道
//   3. click_company   用户买量来源，枚举值 为：tencent、bytedance、kuaishou  为空则为自然量用户
//   4. aid             广告计划ID
//   5. cid             广告创意ID
//   6. advertiser_id   广告账户ID
//   7. bytedance_v2    头条体验版数据（用户如果为头条体验版投放获取的，bytedance_v2才有值）
//      1. project_id   项目ID
//      2. promotion_id 广告ID
//      3. mid1         图片ID
//      4. mid2         标题ID
//      5. mid3         视频ID
//      6. mid4         试完ID
//      7. mid5         落地页ID
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

GravityEngineAPI.QueryUser(request =>
    {
        Debug.Log("query user call end");
        Debug.Log(request.downloadHandler.text);
    });
```

### 2. 用户行为数据上报

通过 `GravityEngineAPI.Track()` 可以上报事件及其属性。一般情况下，您可能需要上传十几到上百个不同的事件，如果您是第一次使用引力引擎事件采集系统，我们推荐您先上传几个关键事件。

我们也支持了若干自动采集事件，包括游戏启动、关闭、异常、小游戏添加收藏、Unity场景加载或者卸载等事件，您可以根据业务需求选择是否开启自动采集事件。

#### 2.1 用户自定义事件上报

建议您先在引力后台--管理中心--元事件中添加自定义事件，然后在游戏中指定位置埋点调用 `Track` 方法上报自定义事件。

```csharp
Dictionary<string, object> properties = new Dictionary<string, object>();
properties["channel"] = "base";//字符串，长度不超过2048
properties["age"] = 1;//数字
properties["isVip"] = true;//布尔
properties["birthday"] = DateTime.Now;//时间
properties["movies"] = new List<string>() { "Interstellar", "The Negro Motorist Green Book" };//字符串元素的数组 最大元素个数为 500

GravityEngineAPI.Track("TEST_EVENT_NAME",properties);
```

- 事件名称是 `string` 类型，只能以字母开头，可包含数字，字母和下划线 “_”，长度最大为 50 个字符，对字母大小写不敏感。
- 事件属性是 `Dictionary<string, object>` 类型，其中每个元素代表一个属性；
    - 事件属性 `Key` 为属性名称，为 `string` 类型，规定只能以字母开头，包含数字，字母和下划线 “_”，长度最大为 50 个字符，对字母大小写不敏感；
    - 属性 `Value` 支持`string`、`int`、`float`、`bool`、`DateTime`、`List<string>`；

当您调用 `Track()` 时，SDK 会取系统当前时间作为事件发生的时刻，如果您需要指定事件时间，可以传入 DateTime 类型的参数来设置事件触发时间。

SDK 提供了时间校准接口，允许使用服务器时间对 SDK 时间进行校准，具体请参考Demo中对 `CalibrateTime` 和 `CalibrateTimeWithNtp` 方法的使用。

> 注意：尽管事件可以设置触发时间，但是接收端会做如下的限制：只接收相对服务器时间在前 10 天至后 1 小时的数据，超过时限的数据将会被视为异常数据，整条数据无法入库。

#### 2.2 用户注册事件上报

当用户注册成功时，需要调用 `TrackMPRegister`方法记录用户注册事件

```csharp
// 记录用户注册事件
GravityEngineAPI.TrackMPRegister();
```

#### 2.3 用户登录事件上报

当用户登录成功时，需要调用 `TrackMPLogin`方法记录用户登录事件

```csharp
// 记录用户登录事件
GravityEngineAPI.TrackMPLogin();
```

#### 2.4 用户退出登录事件上报

当用户退出登录成功时，需要调用 `TrackMPLogout`方法记录用户退出登录事件

```csharp
// 记录用户退出登录事件
GravityEngineAPI.TrackMPLogout();
```

#### 2.5 设置事件公共属性

对于一些重要的属性，譬如玩家的区服和渠道等，这些属性需要设置在每个事件中，此时您可以将这些属性设置为公共事件属性。公共事件属性指的就是每个事件都会带有的属性，您可以调用 `SetSuperProperties`
来设置公共事件属性，我们推荐您在发送事件前，先设置公共事件属性。

```csharp
Dictionary<string, object> superProperties = new Dictionary<string, object>()
    {
        {"SERVER", 0},
        {"CHANNEL", "A3"}
    };
GravityEngineAPI.SetSuperProperties(superProperties);
```

公共事件属性将会被保存到缓存中，无需每次启动 APP 时调用。如果调用 `SetSuperProperties` 上传了先前已设置过的公共事件属性，则会覆盖之前的属性。如果公共事件属性和 `Track()` 上传的某个属性的 Key
重复，则该事件的属性会覆盖公共事件属性。

- 如果您需要删除某个公共事件属性，可以调用 `UnsetSuperProperty` 清除其中一个公共事件属性；
- 如果您想要清空所有公共事件属性，则可以调用 `ClearSuperProperties`;
- 如果您想要获取所有公共事件属性，可以调用`GetSuperProperties`;

```csharp
// 清除属性名为 CHANNEL 的公共属性
GravityEngineAPI.UnsetSuperProperty("CHANNEL");

// 清空所有公共属性
GravityEngineAPI.ClearSuperProperties();

// 获取所有公共属性
GravityEngineAPI.GetSuperProperties();
```

> 注意：在上报公共事件属性之前，请确保已经在引力引擎后台配置了该属性，否则会导致数据无法入库。

#### 2.6 记录事件时长

如果您需要记录某个事件持续时长，您可以调用 `TimeEvent()` 来开始计时，配置您想要计时的事件名称，当您上传该事件时，将会自动在您的事件属性中加入 `$event_duration` 这一属性来表示记录的时长，单位为秒。

```csharp
// 调用 TimeEvent 开启对 TIME_EVENT 事件的计时
GravityEngineAPI.TimeEvent("TIME_EVENT");

// do some thing...

// 通过 Track 上传 TIME_EVENT 事件时，会在属性中添加 $event_duration 属性
GravityEngineAPI.Track("TIME_EVENT");
```

#### 2.7 立即上报事件

如果需要立即上报缓存的事件，可以调用 `Flush()` 来上报所有缓存的事件。

```csharp
// 调用 Flush() 上报缓存事件
GravityEngineAPI.Flush();
```

### 3. 用户属性上报

目前支持的用户属性设置接口为 `UserSet`、`UserSetOnce`、`UserAdd`、`UserUnset`、`UserDelete`、`UserAppend`.

#### 3.1 UserSet 设置用户属性

对于一般的用户属性，您可以调用 UserSet 来进行设置，使用该接口上传的属性将会覆盖原有的属性值，如果之前不存在该用户属性，则会新建该用户属性。

```csharp
GravityEngineAPI.UserSet(new Dictionary<string, object>()
    {
        {"USER_PROP_NUM", 0},
        {"USER_PROP_STRING", "A3"}
    });
```

#### 3.2 UserSetOnce 设置用户属性

如果您要上传的用户属性只要设置一次，则可以调用 UserSetOnce 来进行设置，当该属性之前已经有值的时候，将会忽略这条信息：

```csharp
GravityEngineAPI.UserSetOnce(new Dictionary<string, object>()
    {
        {"USER_PROP_NUM", -50},
        {"USER_PROP_STRING", "A3"}
    });
```

#### 3.3 UserAdd 用户属性累加

当您要上传数值型的属性时，您可以调用 UserAdd 来对该属性进行累加操作，如果该属性还未被设置，则会赋值 0 后再进行计算，可传入负值，等同于相减操作。

```csharp
GravityEngineAPI.UserAdd(new Dictionary<string, object>()
    {
        {"USER_PROP_NUM", -100.9},
        {"USER_PROP_NUM2", 10.0}
    });
```

> 设置的属性key为字符串，Value 只允许为数值。

#### 3.4 UserUnset 重置用户属性

如果您需要重置用户的某个属性，可以调用 `UserUnset` 将该用户指定用户属性的值重置，此接口支持传入字符串、列表、布尔类型的参数:

```csharp
// 删除单个用户属性
GravityEngineAPI.UserUnset("userPropertyName");

// 删除多个用户属性
GravityEngineAPI.UserUnset(new List<string>() {"age", "$name", "$first_visit_time", "movies"});
```

> UserUnset: 的传入值为被重置属性的 Key 值。

#### 3.5 UserDelete 删除用户

如果您要删除某个用户，可以调用 `UserDelete` 将这名用户删除，您将无法再查询该名用户的用户属性，但该用户产生的事件仍然可以被查询到。

```csharp
GravityEngineAPI.UserDelete();
```

#### 3.6 UserAppend 用户属性追加

可以调用 UserAppend 为 List 类型的用户属性追加元素:

```csharp
List<string> propList = new List<string>();
propList.Add("Interstellar");
propList.Add("The Negro Motorist Green Book");

// 为属性名为 movies 的用户属性追加 2 个元素
GravityEngineAPI.UserAppend(new Dictionary<string, object>()
{
    {"movies", propList}
});
```

#### 3.7 UserUniqAppend 用户属性去重追加

可以调用 UserUniqAppend 为 List 类型的用户属性进行去重追加元素:

```csharp
List<string> propList = new List<string>();
propList.Add("Interstellar");
propList.Add("The Shawshank Redemption");

// 为属性名为 movies 的用户属性去重追加 2 个元素
GravityEngineAPI.UserUniqAppend(new Dictionary<string, object>()
{
    {"movies", propList}
});
```

### 4. 其他配置

#### 4.1 打印数据Log

您可以调用 `EnableLog` 来关闭日志输出（默认是开启的）。

```csharp
// 关闭打印数据Log
GravityEngineAPI.EnableLog(false);

// 开启打印数据Log
GravityEngineAPI.EnableLog(true);
```

#### 4.2 SDK 运行模式

SDK 支持在两种模式下运行：

- NORMAL: 普通模式，数据会存入缓存，并依据一定的缓存策略上报
- DEBUG: 测试模式，数据逐条上报。当出现问题时会以日志和异常的方式提示用户，也可以去`引力网站后台--管理中心--元数据--事件流`中查看实时上报的数据。

您在调用 `StartGravityEngine` 初始化引擎时，可以传入 `SDKRunMode` 参数来指定 SDK 运行模式。

> 注意: DEBUG 模式仅仅用于集成阶段数据校验，不要在生产模式下使用！

#### 4.3 校准时间

SDK 默认会使用本机时间作为事件发生时间上报，如果用户手动修改设备时间会影响到您的业务分析，您可以使用从服务端获取的当前时间戳对 SDK
的时间进行校准。此后，所有未指定时间的调用，包括事件数据和用户属性设置操作，都会使用校准后的时间作为发生时间。

```csharp
//时间戳,单位毫秒 对应时间为1668982523000 2022-11-21 06:15:23
GravityEngineAPI.CalibrateTime(1668982523000);
```

我们也提供了从 NTP 获取时间对 SDK 校准的功能。您需要传入您的用户可以访问的 NTP 服务器地址。之后 SDK 会尝试从传入的 NTP 服务地址中获取当前时间，并对 SDK 时间进行校准。如果在默认的超时时间（3
秒）之内，未获取正确的返回结果，后续将使用本地时间上报数据。

```csharp
//NTP 时间服务器校准，如：time.apple.com
GravityEngineAPI.CalibrateTimeWithNtp("time.apple.com");
```

> **注意：**
>  - 您需要谨慎地选择您的 NTP 服务器地址，以保证网络状况良好的情况下，用户设备可以很快的获取到服务器时间。
>  - 使用 NTP 服务进行时间校准存在一定的不确定性，建议您优先考虑用时间戳校准的方式。

除了以上校准时间接口外，SDK还提供了所有用户属性接口的时间函数重载，您可以在调用用户属性相关接口时，传入 `DateTime` 对象，则系统会使用传入的 `DateTime` 对象来设定数据的 `time` 字段。

#### License

Under BSD license，you can check out the license file
