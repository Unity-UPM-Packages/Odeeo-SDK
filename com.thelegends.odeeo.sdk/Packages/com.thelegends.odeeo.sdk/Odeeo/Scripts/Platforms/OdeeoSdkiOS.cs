#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Odeeo.Platforms.Listeners;
using UnityEngine;

// ReSharper disable IdentifierTypo

namespace Odeeo.Platforms
{
    internal sealed class OdeeoSdkiOS : IOdeeoSdkPlatform
    {
        public Action<bool> OnApplicationPause { get; }
        public bool IsInitializationInProgress { get; private set; }
        
        private readonly IOdeeoInitializationListener _listener;
        private readonly IOdeeoLogging _logging;
        
        private int _targetDPI;

        public OdeeoSdkiOS(IOdeeoInitializationListener listener, IOdeeoLogging logging)
        {
            _listener = listener;
            _logging = logging;
            
            OnApplicationPause += TriggerPauseEvents;
        }

        void IOdeeoSdkPlatform.Initialize(string appKey, string unityVersion, string sdkVersion)
        {
            IsInitializationInProgress = true;
            
            _listener.OnInitializationSuccess += CompleteInitialization;
            _listener.OnInitializationFailed += FailInitialization;
            
            _odeeoSdkSetEngineInfo($"unity_{unityVersion}", sdkVersion);
            _odeeoSdkInitialize(appKey);
        }

        private void CompleteInitialization()
        {
            IsInitializationInProgress = false;
            
            _listener.OnInitializationSuccess -= CompleteInitialization;
            _listener.OnInitializationFailed -= FailInitialization;
            
            _logging.Info("Initialized");
        }

        private void FailInitialization(int errorParam, string error)
        {
            IsInitializationInProgress = false;
            
            _listener.OnInitializationSuccess -= CompleteInitialization;
            _listener.OnInitializationFailed -= FailInitialization;
            
            _logging.Error($"Initialization Failed with param: {errorParam.ToString()} and error: {error}");
        }

        bool IOdeeoSdkPlatform.IsInitialized()
        {
            return _odeeoSdkIsInitialized();
        }

        #region Regulation

        void IOdeeoSdkPlatform.SetIsChildDirected(bool flag)
        {
            _odeeoSdkSetIsChildDirected(flag);
        }

        void IOdeeoSdkPlatform.RequestTrackingAuthorization()
        {
            _odeeoSdkRequestTrackingAuthorization();
        }
        
#region RegulationType

        void IOdeeoSdkPlatform.ForceRegulationType(OdeeoSdk.ConsentType type)
        {
            _odeeoSdkForceRegulationType((int)type);
        }

        void IOdeeoSdkPlatform.ClearForceRegulationType()
        {
            _odeeoSdkClearForceRegulationType();
        }

        OdeeoSdk.ConsentType IOdeeoSdkPlatform.GetRegulationType()
        {
            return (OdeeoSdk.ConsentType)_odeeoSdkGetRegulationType();
        }
        
        #endregion
        
        #region DoNotSell
        
        void IOdeeoSdkPlatform.SetDoNotSellPrivacyString(string privacyString)
        {
            _odeeoSdkSetDoNotSellPrivacyString(privacyString);
        }
        
        void IOdeeoSdkPlatform.SetDoNotSell(bool isApplied)
        {
            _odeeoSdkSetDoNotSell(isApplied);
        }

        void IOdeeoSdkPlatform.SetDoNotSell(bool isApplied, string privacyString)
        {
            _odeeoSdkSetDoNotSellWithString(isApplied, privacyString);
        }
        
        #endregion

        #endregion
        
        /// <summary>
        /// Returns current device volume in Percentages from 0 to 100
        /// </summary>
        float IOdeeoSdkPlatform.GetDeviceVolumeLevel()
        {
            return _odeeoSdkGetDeviceVolumeLevel();
        }

        void IOdeeoSdkPlatform.AddCustomAttribute(string key, string value)
        {
            _odeeoSdkAddCustomAttribute(key, value);
        }

        void IOdeeoSdkPlatform.ClearCustomAttributes()
        {
            _odeeoSdkClearCustomAttributes();
        }

        void IOdeeoSdkPlatform.RemoveCustomAttribute(string key)
        {
            _odeeoSdkRemoveCustomAttribute(key);
        }

        List<KeyValuePair<string, string>> IOdeeoSdkPlatform.GetCustomAttributes()
        {
            return ParseCustomAttributesStringIOS(_odeeoSdkGetCustomAttributes());
        }

        List<KeyValuePair<string, string>> IOdeeoSdkPlatform.GetCustomAttributes(string key)
        {
            return ParseCustomAttributesStringIOS(_odeeoSdkGetCustomAttributesWithKey(key));
        }

        private static List<KeyValuePair<string, string>> ParseCustomAttributesStringIOS(string str)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            
            if (string.IsNullOrEmpty(str) || str.Length == 2)
                return list;
            
            str = str.Substring(1, str.Length - 2);
            string[] elements = str.Split(new [] { "," }, StringSplitOptions.None);
            
            for (int i = 0; i < elements.Length; i++)
            {
                string element = elements[i];
                element = element.Substring(1, element.Length - 2);
                
                string[] keyValue = element.Split(new [] { ": " }, StringSplitOptions.None);
                list.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
            }
            
            return list;
        }

        void IOdeeoSdkPlatform.SetPublisherUserID(string id)
        {
            _odeeoSdkSetPublisherUserID(id);
        }

        string IOdeeoSdkPlatform.GetPublisherUserID()
        {
            return _odeeoSdkGetPublisherUserID();
        }
        
        void IOdeeoSdkPlatform.SetExtendedUserId(string partner, string id)
        {
            _odeeoSdkSetExtendedUserID(partner, id);
        }

#region Pause

        private void TriggerPauseEvents(bool isPaused)
        {
            if (isPaused) OnPause();
            else OnResume();
        }

        private static void OnPause()
        {
            _odeeoSdkPause();
        }

        private static void OnResume()
        {
            _odeeoSdkResume();
        }
        
#endregion

#region ScreenSettings
        
        void IOdeeoSdkPlatform.SetOptimalDPI()
        {
            _logging.Warning("This operation is only supported in Unity Editor");
        }

        void IOdeeoSdkPlatform.SetUnityEditorDPI(int dpi)
        {
            _logging.Warning("This operation is only supported in Unity Editor");
        }

        void IOdeeoSdkPlatform.SetTargetDPI(int dpi)
        {
            _targetDPI = dpi;
        }
        
        float IOdeeoSdkPlatform.GetScreenDPI()
        {
            return Screen.dpi;
        }

        float IOdeeoSdkPlatform.GetDpiMultiplier()
        {
            if (_targetDPI <= 0)
                return 1f;

            float currentDPI = Mathf.Min(Screen.dpi, _targetDPI);
            return Screen.dpi / currentDPI;
        }

        float IOdeeoSdkPlatform.GetDeviceScale()
        {
            float scale = _odeeoSdkGetDeviceScale();
            return Mathf.Max(1f, scale);
        }

        #endregion

        void IOdeeoSdkPlatform.SetLogLevel(OdeeoSdk.LogLevel level)
        {
            _odeeoSdkSetLogLevel((int)level);
            _logging.LogLevel = level;
        }
        
        [DllImport("__Internal")]
        private static extern void _odeeoSdkInitialize(string apiKey);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetEngineInfo(string engineName, string engineVersion);
        [DllImport("__Internal")]
        private static extern bool _odeeoSdkIsInitialized();

        // Regulation
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetIsChildDirected(bool flag);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkRequestTrackingAuthorization();
        
        // Regulation Type
        [DllImport("__Internal")]
        private static extern void _odeeoSdkForceRegulationType(int type);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkClearForceRegulationType();
        [DllImport("__Internal")]
        private static extern int _odeeoSdkGetRegulationType();
        //DoNotSell
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetDoNotSellPrivacyString(string privacyString);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetDoNotSell(bool isApplied);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetDoNotSellWithString(bool isApplied, string privacyString);
        
        //End Regulation
        
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetLogLevel(int level);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkAddCustomAttribute(string key, string value);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkClearCustomAttributes();
        [DllImport("__Internal")]
        private static extern void _odeeoSdkRemoveCustomAttribute(string key);
        [DllImport("__Internal")]
        private static extern String _odeeoSdkGetCustomAttributes();
        [DllImport("__Internal")]
        private static extern String _odeeoSdkGetCustomAttributesWithKey(string key);
        [DllImport("__Internal")]
        private static extern String _odeeoSdkGetPublisherUserID();
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetPublisherUserID(string value);
        [DllImport("__Internal")]
        private static extern void _odeeoSdkSetExtendedUserID(string partner, string value);
        [DllImport("__Internal")]
        private static extern float _odeeoSdkGetDeviceVolumeLevel();
        [DllImport("__Internal")]
        private static extern float _odeeoSdkGetDeviceScale();
        [DllImport("__Internal")]
        private static extern void _odeeoSdkPause();
        [DllImport("__Internal")]
        private static extern void _odeeoSdkResume();
    }
}
#endif