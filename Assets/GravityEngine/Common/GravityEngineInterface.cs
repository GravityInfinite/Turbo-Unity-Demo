using System.Collections.Generic;

namespace GravityEngine
{
    /// <summary>
    /// Dynamic super properties interfaces.
    /// </summary>
    public interface IDynamicSuperProperties
    {
        Dictionary<string, object> GetDynamicSuperProperties();
    }

    /// <summary>
    /// Auto track event callback interfaces.
    /// </summary>
    public interface IAutoTrackEventCallback
    {
        Dictionary<string, object> AutoTrackEventCallback(int type, Dictionary<string, object> properties);
    }

    public interface IInitializeCallback
    {
        void onFailed(string errorMsg);

        void onSuccess(Dictionary<string, object> responseJson);
    }
    
    public interface IResetCallback
    {
        void onFailed(string errorMsg);

        void onSuccess();
    }

    public interface ILogoutCallback
    {
        void onCompleted();
    }
}