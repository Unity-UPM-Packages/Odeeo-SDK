using System;
using System.Collections.Generic;

namespace Odeeo
{
    internal interface IOdeeoSdkPlatform
    {
        Action<bool> OnApplicationPause { get; }
        bool IsInitializationInProgress { get; }
        
        bool IsInitialized();
        void Initialize(string appKey, string unityVersion, string sdkVersion);
        
        string GetPublisherUserID();
        void SetPublisherUserID(string id);
        void SetExtendedUserId(string partner, string id);

        /// <summary>
        /// Returns current device volume in Percentages from 0 to 100
        /// </summary>
        float GetDeviceVolumeLevel();

        void SetDoNotSell(bool isApplied, string privacyString);
        void SetDoNotSell(bool isApplied);
        void SetDoNotSellPrivacyString(string privacyString);
        
        OdeeoSdk.ConsentType GetRegulationType();
        void ClearForceRegulationType();
        void ForceRegulationType(OdeeoSdk.ConsentType type);
        void RequestTrackingAuthorization();
        void SetIsChildDirected(bool flag);
        
        List<KeyValuePair<string, string>> GetCustomAttributes();
        List<KeyValuePair<string, string>> GetCustomAttributes(string key);
        void RemoveCustomAttribute(string key);
        void ClearCustomAttributes();
        void AddCustomAttribute(string key, string value);
        
        void SetTargetDPI(int dpi);
        float GetScreenDPI();
        float GetDpiMultiplier();
        float GetDeviceScale();
        void SetUnityEditorDPI(int dpi);
        void SetOptimalDPI();
        
        void SetLogLevel(OdeeoSdk.LogLevel level);
    }
}