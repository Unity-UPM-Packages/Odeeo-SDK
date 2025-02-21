#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Odeeo.Platforms.Listeners;
using Odeeo.Utils;
using UnityEngine;

namespace Odeeo.Platforms
{
    internal sealed  class OdeeoSdkUnityEditor : IOdeeoSdkPlatform
    {
        public Action<bool> OnApplicationPause { get; }
        public bool IsInitializationInProgress { get; private set; }
        
        private readonly IOdeeoInitializationListener _listener;
        private readonly IOdeeoLogging _logging;

        private static bool _isSetByUserDPI;
        
        private readonly List<KeyValuePair<string, string>> _customAttributes = new List<KeyValuePair<string, string>>();
        private bool _isInitialized;
        private int _targetDPI;
        private int _editorDpi;
        private string _publisherId;
        private OdeeoSdk.ConsentType _regulation = OdeeoSdk.ConsentType.None;

        public OdeeoSdkUnityEditor(IOdeeoInitializationListener listener, IOdeeoLogging logging)
        {
            _listener = listener;
            _logging = logging;

            OnApplicationPause += TriggerPauseEvents;
        }

        void IOdeeoSdkPlatform.Initialize(string appKey, string unityVersion, string sdkVersion)
        {
            IsInitializationInProgress = true;
            
            _logging.Info("Unity Editor Dummy Initialization");

            if (string.IsNullOrEmpty(appKey))
                FailInitialization(-1, "App key is empty during Odeeo Initialize");
            else
                CompleteInitialization();
        }

        private void CompleteInitialization()
        {
            _isInitialized = true;
            IsInitializationInProgress = false;
            
            _logging.Info("Initialized");

            _listener.OnInitializationSuccess?.Invoke();
        }

        private void FailInitialization(int errorParam, string error)
        {
            _isInitialized = false;
            IsInitializationInProgress = false;
            
            _logging.Error($"Initialization Failed with param: {errorParam.ToString()} and error: {error}");

            _listener.OnInitializationFailed?.Invoke(errorParam, error);
        }

        bool IOdeeoSdkPlatform.IsInitialized()
        {
            return _isInitialized;
        }

#region Regulation

        void IOdeeoSdkPlatform.SetIsChildDirected(bool flag)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.SetIsChildDirected)} received {flag}");
        }

        void IOdeeoSdkPlatform.RequestTrackingAuthorization()
        {
            _logging.Warning("RequestTrackingAuthorization() ignored. Requesting tracking authorization is made only for iOS platform.");
        }
        
#region RegulationType

        void IOdeeoSdkPlatform.ForceRegulationType(OdeeoSdk.ConsentType type)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.ForceRegulationType)} received {type}");
            _regulation = type;
        }

        void IOdeeoSdkPlatform.ClearForceRegulationType()
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.ClearForceRegulationType)}");
            _regulation = OdeeoSdk.ConsentType.None;
        }

        OdeeoSdk.ConsentType IOdeeoSdkPlatform.GetRegulationType()
        {
            return _regulation;
        }
        
#endregion
        
#region DoNotSell
        
        void IOdeeoSdkPlatform.SetDoNotSellPrivacyString(string privacyString)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.SetDoNotSellPrivacyString)} received {privacyString}");
        }
        
        void IOdeeoSdkPlatform.SetDoNotSell(bool isApplied)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.SetDoNotSell)} received {isApplied}");
        }

        void IOdeeoSdkPlatform.SetDoNotSell(bool isApplied, string privacyString)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.SetDoNotSell)} received {isApplied} {privacyString}");
        }
        
#endregion

#endregion

/// <summary>
        /// Returns current device volume in Percentages from 0 to 100
        /// </summary>
        float IOdeeoSdkPlatform.GetDeviceVolumeLevel()
        {
            _logging.Warning("Editor mode is not supported. Returned value always 100");
            return 100.0f;
        }

        void IOdeeoSdkPlatform.AddCustomAttribute(string key, string value)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.AddCustomAttribute)} received {key}:{value}");
            
            _customAttributes.Add(new KeyValuePair<string, string>(key, value));
        }

        void IOdeeoSdkPlatform.ClearCustomAttributes()
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.ClearCustomAttributes)}");
            _customAttributes.Clear();
        }

        void IOdeeoSdkPlatform.RemoveCustomAttribute(string key)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.RemoveCustomAttribute)}");
            
            for (var i = 0; i < _customAttributes.Count; i++)
            {
                if (_customAttributes[i].Key == key)
                    _customAttributes.Remove(_customAttributes[i]);
            }
        }

        List<KeyValuePair<string, string>> IOdeeoSdkPlatform.GetCustomAttributes()
        {
            return _customAttributes;
        }

        List<KeyValuePair<string, string>> IOdeeoSdkPlatform.GetCustomAttributes(string key)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var pair in _customAttributes)
            {
                if (pair.Key == key)
                    list.Add(pair);
            }
            
            return list;
        }

        void IOdeeoSdkPlatform.SetPublisherUserID(string id)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.SetPublisherUserID)} received {id}");
            _publisherId = id;
        }

        string IOdeeoSdkPlatform.GetPublisherUserID()
        {
            return _publisherId;
        }
        
        void IOdeeoSdkPlatform.SetExtendedUserId(string partner, string id)
        {
            _logging.Info($"{nameof(IOdeeoSdkPlatform.SetExtendedUserId)} received {partner} {id}");
        }
        
        private void TriggerPauseEvents(bool isPaused)
        {
            if (isPaused) OnPause();
            else OnResume();
        }
        
        private void OnPause()
        {
            _logging.Info($"{nameof(OnPause)} would be called on the native side");
        }

        private void OnResume()
        {
            _logging.Info($"{nameof(OnResume)} would be called on the native side");
        }
        
        #region ScreenSettings
        
        void IOdeeoSdkPlatform.SetUnityEditorDPI(int dpi)
        {
            _editorDpi = dpi;
            _isSetByUserDPI = true;
        }
        
        void IOdeeoSdkPlatform.SetOptimalDPI()
        {
            int dpi = GetOptimalDPI();

            if(!_isSetByUserDPI)
                _editorDpi = dpi;
        }

        void IOdeeoSdkPlatform.SetTargetDPI(int dpi)
        {
            _targetDPI = dpi;
        }
        
        float IOdeeoSdkPlatform.GetScreenDPI()
        {
            return _editorDpi;
        }

        float IOdeeoSdkPlatform.GetDpiMultiplier()
        {
            if (_targetDPI <= 0)
                return 1f;

            float currentDPI = Mathf.Min(_editorDpi, _targetDPI);
            return _editorDpi / currentDPI;
        }

        float IOdeeoSdkPlatform.GetDeviceScale()
        {
            float scale = 1f;
            
#if UNITY_ANDROID
            scale = _editorDpi / 160f;
#elif UNITY_IOS
            scale =  Mathf.Round(_editorDpi / 160f);
#endif
            return Mathf.Max(1f, scale);
        }
        
        private static int GetOptimalDPI()
        {
            int dpi = OdeeoEditorHelper.GetScreenDPI(Screen.width, Screen.height);
            if (dpi > 0)
                return dpi;

            float shortSide = Screen.width < Screen.height ? Screen.width : Screen.height;
            if (shortSide >= 1440)
                dpi = 440;
            else if (shortSide >= 1080)
                dpi = 323;
            else if (shortSide >= 720)
                dpi = 252;
            else if (shortSide >= 480)
                dpi = 170;

            return dpi;
        }

        #endregion

        void IOdeeoSdkPlatform.SetLogLevel(OdeeoSdk.LogLevel level)
        {
            _logging.LogLevel = level;
        }
    }
}
#endif