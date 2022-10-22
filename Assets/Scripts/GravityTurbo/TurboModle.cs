using System;

namespace GravityTurbo
{
    [Serializable]
    public class AdData
    {
        // 头条
        public string clue_token;
        public string ad_id;
        public string creative_id;
        public string request_id;
        public string advertiser_id;

        // 快手
        public string callback;
        public string ksChannel;
        public string ksCampaignId;
        public string ksUnitId;
        public string ksCreativeId;
        
        // 腾讯 click_id?
    }

    [Serializable]
    public class RegisterRequestBody
    {
        public string client_id;
        public string name;
        public string channel;
        public int version;
        public string media_type;
        public string wx_openid;
        public string wx_unionid;
        public AdData ad_data;
    }

    [Serializable]
    public class Properties
    {
        public long amount;
        public long real_amount;
    }

    [Serializable]
    public class HandleEventBody
    {
        public string event_type;
        public long timestamp;
        public Properties properties;
    }

    [Serializable]
    public class QueryUserBody
    {
        public string[] user_list;
    }
}