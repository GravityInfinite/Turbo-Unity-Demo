## turbo-sdk-unity

本文档为**Unity**接入 [turbo 引力引擎](https://gravity-engine.com/)的技术接入方案，具体 Demo
请参考[GitHub](https://github.com/GravityInfinite/Turbo-Unity-Demo)或者[Gitee](https://gitee.com/GravityInfinite/Turbo-Unity-Demo)开源项目（国内用户推荐Gitee），Demo工程中可以参考TurboScript中对方法的调用示例。

#### 引入

去上述开源代码仓库中下载 `turbo.unitypackage`文件并讲`GravityTurbo`文件夹中的代码引入到工程中。

#### init 初始化

```c#
/**
 * 此方法会初始化Turbo需要的基础参数（需要确保每次启动都必须要调用）
 * @param {string} accessToken      项目通行证，在：网站后台-->管理中心-->应用列表中找到Access Token列 复制（首次使用可能需要先新增应用）
 * @param {string} clientId         用户唯一标识，如微信小程序/小游戏的openid、Android ID、iOS的IDFA、或业务侧自行生成的唯一用户ID均可
 */
 
Turbo.InitSDK("your_access_token", "your_client_id");
```

#### register 用户注册

```c#
/**
 * 在用户注册或者可以获取到用户唯一性信息时调用，推荐首次安装启动时调用，其他方法均需在本方法回调成功之后才可正常使用
 * @param {string} name                 用户名（必填）
 * @param {string} channel              用户注册渠道（必填）
 * @param {number} version              用户注册的程序版本（必填）
 * @param {string} wxOpenId             微信open id (微信小程序和小游戏必填)
 * @param {string} wxUnionId            微信union id（微信小程序和小游戏选填）
 * @param {Dictionary} wxLaunchQuery    启动参数字典(微信小程序和小游戏必填)
 * @param {Action} actionResult         网络回调，其他方法均需在回调成功之后才可正常使用
 */
 
var option = WX.GetLaunchOptionsSync();
Turbo.Register("user_name", "user_channel", 1, "user_wx_openid", "user_wx_unionid", option.query,
    request =>
    {
        Debug.Log("register call end");
        Debug.Log(request.downloadHandler.text);
    });
```

#### handleEvent 埋点事件上报

```c#
/**
 * 埋点事件上报（主要分为：注册、激活、次留、付费、关键行为、应用启动六种类型的事件）
 * @param {Action} actionResult         上报回调
 */

// 上报注册事件
Turbo.EventRegister(request =>
{
    Debug.Log("handle event register call end");
    Debug.Log(request.downloadHandler.text);
});
// 上报激活事件举例
Turbo.EventActivate(request =>
{
    Debug.Log("handle event activate call end");
    Debug.Log(request.downloadHandler.text);
});

// 上报次留事件举例
Turbo.EventTwice(request =>
{
    Debug.Log("handle event twice call end");
    Debug.Log(request.downloadHandler.text);
});

// 上报付费事件举例
Turbo.EventPay(300, 300, request =>
{
    Debug.Log("handle event pay call end");
    Debug.Log(request.downloadHandler.text);
});

// 上报关键行为事件举例
Turbo.EventKeyActive(request =>
{
    Debug.Log("handle event key active call end");
    Debug.Log(request.downloadHandler.text);
});
```

#### queryUser 查询用户信息

```c#
/**
 * 查询用户信息，包括
 * 1. client_id             用户ID
 * 2. channel               用户渠道
 * 3. click_company         用户买量来源，枚举值 为：tencent、bytedance、kuaishou  为空则为自然量用户
 * 4. aid                   广告计划ID
 * 5. cid                   广告创意ID
 * 6. advertiser_id         广告账户ID
 *
 * 返回示例如下，具体可以打印返回的data查看
 * "user_list": [
 {
        "create_time": "2022-09-09 14:50:04",
        "client_id": "Bn2RhTcU",
        "advertiser_id": "12948974294275",
        "channel": "wechat_mini_game",
        "click_company": "gdt",
        "aid": "65802182823",
        "cid": "65580218538"
      },
 ]
  * @param {Action} actionResult         上报回调
 */
 
 Turbo.QueryUser(request =>
    {
        Debug.Log("query user call end");
        Debug.Log(request.downloadHandler.text);
    });
```

#### License

Under BSD license，you can check out the license file
