using GravityTurbo;
using UnityEngine;
using WeChatWASM;

public class TurboScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WX.InitSDK((code) => { Debug.Log("wx init end"); });
    }

    public void InitSDK()
    {
        Debug.Log("init sdk clicked");
        Turbo.InitSDK("h8djf2K9adp3FHQESLbsjqmXk7pgsaAm", "123456789003");
    }

    public void Register()
    {
        Debug.Log("register clicked");
        // 获取启动参数
        var option = WX.GetLaunchOptionsSync();
        foreach (var (key, value) in option.query)
        {
            Debug.Log($"key is {key}, value is {value}");
        }

        Turbo.Register("name_123", "test", 1, "your_wx_openid", "your_wx_unionid", option.query,
            request =>
            {
                Debug.Log("register call end");
                Debug.Log(request.downloadHandler.text);
            });
    }

    public void HandleEvent()
    {
        Debug.Log("handle event clicked");
        // 上报注册事件举例
        Turbo.EventRegister(request =>
        {
            Debug.Log("handle event register call end");
            Debug.Log(request.downloadHandler.text);
        });
        // // 上报激活事件举例
        // Turbo.EventActivate(request =>
        // {
        //     Debug.Log("handle event activate call end");
        //     Debug.Log(request.downloadHandler.text);
        // });
        //
        // // 上报次留事件举例
        // Turbo.EventTwice(request =>
        // {
        //     Debug.Log("handle event twice call end");
        //     Debug.Log(request.downloadHandler.text);
        // });
        //
        // // 上报付费事件举例
        // Turbo.EventPay(300, 300, request =>
        // {
        //     Debug.Log("handle event pay call end");
        //     Debug.Log(request.downloadHandler.text);
        // });
        //
        // // 上报关键行为事件举例
        // Turbo.EventKeyActive(request =>
        // {
        //     Debug.Log("handle event key active call end");
        //     Debug.Log(request.downloadHandler.text);
        // });
        // // 上报应用启动事件举例
        // Turbo.EventStartup(request =>
        // {
        //     Debug.Log("handle event startup call end");
        //     Debug.Log(request.downloadHandler.text);
        // });
    }

    public void QueryUser()
    {
        Debug.Log("query user clicked");
        Turbo.QueryUser(request =>
        {
            Debug.Log("query user call end");
            Debug.Log(request.downloadHandler.text);
        });
    }
}