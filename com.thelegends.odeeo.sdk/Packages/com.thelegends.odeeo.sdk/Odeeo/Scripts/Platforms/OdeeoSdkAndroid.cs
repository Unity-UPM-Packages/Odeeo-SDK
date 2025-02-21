#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using Odeeo.Platforms.Listeners;
using Odeeo.Utils;
using UnityEngine;

namespace Odeeo.Platforms
{
    internal sealed class OdeeoSdkAndroid : IOdeeoSdkPlatform
    {
        private const string AndroidBridgeClassName = "io.odeeo.sdk.OdeeoSDK";
        
        private static AndroidJavaObject _bridge;

        private static AndroidJavaObject Bridge
        {
            get
            {
                if (_bridge != null)
                    return _bridge;

                using (AndroidJavaClass pluginClass = new AndroidJavaClass(AndroidBridgeClassName))
                    _bridge = pluginClass.GetStatic<AndroidJavaObject>("INSTANCE");

                return _bridge;
            }
        }

        public Action<bool> OnApplicationPause { get; }
        public bool IsInitializationInProgress { get; private set; }
        
        private readonly IOdeeoInitializationListener _listener;
        private readonly IOdeeoLogging _logging;
        
        private int _targetDPI;
        private AndroidJavaObject _activity;

        public OdeeoSdkAndroid(IOdeeoInitializationListener listener, IOdeeoLogging logging)
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
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() =>
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    
                    try
                    {
                        var runnable = new AndroidJavaRunnable(() =>
                        {
                            Bridge.CallStatic("setEngineInformation", $"unity_{unityVersion}", sdkVersion);
                            Bridge.CallStatic("setOnInitializationListener", _listener);
                            Bridge.CallStatic("initialize", _activity, appKey);
                        });
                        
                        _activity.Call("runOnUiThread", runnable);
                    }
                    catch (Exception e)
                    {
                        _logging.Error($"Unity initialization exception {e.Message}");
                    }
                }
            });
        }

        private void CompleteInitialization()
        {
            IsInitializationInProgress = false;
            
            _listener.OnInitializationSuccess -= CompleteInitialization;
            _listener.OnInitializationFailed -= FailInitialization;
            
            _logging.Info("Initialized");
            
            _activity?.Dispose();
        }

        private void FailInitialization(int errorParam, string error)
        {
            IsInitializationInProgress = false;
            
            _listener.OnInitializationSuccess -= CompleteInitialization;
            _listener.OnInitializationFailed -= FailInitialization;
            
            _logging.Error($"Initialization Failed with param: {errorParam.ToString()} and error: {error}");
            
            _activity?.Dispose();
        }

        bool IOdeeoSdkPlatform.IsInitialized()
        {
            return Bridge.CallStatic<bool>("isInitialized");
        }

#region Regulation

        void IOdeeoSdkPlatform.SetIsChildDirected(bool flag)
        {
            Bridge.CallStatic("setIsChildDirected", flag);
        }

        void IOdeeoSdkPlatform.RequestTrackingAuthorization()
        {
            _logging.Warning("RequestTrackingAuthorization() ignored. Requesting tracking authorization is made only for iOS platform.");
        }
        
#region RegulationType

        void IOdeeoSdkPlatform.ForceRegulationType(OdeeoSdk.ConsentType type)
        {
            AndroidJavaClass consentEnum = new AndroidJavaClass ("io.odeeo.sdk.consent.ConsentType");
            AndroidJavaObject curType = consentEnum.CallStatic<AndroidJavaObject> ("valueOf", type.ToString ());
            Bridge.CallStatic("forceRegulationType", curType);
        }

        void IOdeeoSdkPlatform.ClearForceRegulationType()
        {
            Bridge.CallStatic("clearForceRegulationType");
        }

        OdeeoSdk.ConsentType IOdeeoSdkPlatform.GetRegulationType()
        {
            AndroidJavaObject consentEnum = Bridge.CallStatic<AndroidJavaObject>("getRegulationType");
            int typeIndex = consentEnum.Call<int> ("ordinal");
            return (OdeeoSdk.ConsentType)typeIndex;
        }
        
        #endregion
        
#region DoNotSell
        
        void IOdeeoSdkPlatform.SetDoNotSellPrivacyString(string privacyString)
        {
            Bridge.CallStatic("setDoNotSellPrivacyString", privacyString);
        }
        
        void IOdeeoSdkPlatform.SetDoNotSell(bool isApplied)
        {
            Bridge.CallStatic("setDoNotSell", isApplied);
        }

        void IOdeeoSdkPlatform.SetDoNotSell(bool isApplied, string privacyString)
        {
            Bridge.CallStatic("setDoNotSell", isApplied, privacyString);
        }
        
#endregion

#endregion

        /// <summary>
        /// Returns current device volume in Percentages from 0 to 100
        /// </summary>
        float IOdeeoSdkPlatform.GetDeviceVolumeLevel()
        {
            return Bridge.CallStatic<float>("getDeviceVolumeLevel");
        }

        void IOdeeoSdkPlatform.AddCustomAttribute(string key, string value)
        {
            Bridge.CallStatic("addCustomAttribute", key, value);
        }

        void IOdeeoSdkPlatform.ClearCustomAttributes()
        {
            Bridge.CallStatic("clearCustomAttributes");
        }

        void IOdeeoSdkPlatform.RemoveCustomAttribute(string key)
        {
            Bridge.CallStatic("removeCustomAttribute", key);
        }

        List<KeyValuePair<string, string>> IOdeeoSdkPlatform.GetCustomAttributes()
        {
            AndroidJavaObject obj = Bridge.CallStatic<AndroidJavaObject>("getCustomAttributes");
            string str = obj.Call<string>("toString");
            return ParseCustomAttributesStringAndroid(str);
        }

        List<KeyValuePair<String, String>> IOdeeoSdkPlatform.GetCustomAttributes(string key)
        {
            AndroidJavaObject obj = Bridge.CallStatic<AndroidJavaObject>("getCustomAttributes", key);
            string str = obj.Call<string>("toString");
            return ParseCustomAttributesStringAndroid(str);
        }

        private static List<KeyValuePair<string, string>> ParseCustomAttributesStringAndroid(string str)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            
            if (string.IsNullOrEmpty(str) || str.Length == 2)
                return list;

            str = str.Substring(1, str.Length - 2);
            string[] elements = str.Split(new [] { ", " }, StringSplitOptions.None);
            
            for (int i = 0; i < elements.Length; i++)
            {
                string[] keyValue = elements[i].Split('=');
                list.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
            }

            return list;
        }
        
        void IOdeeoSdkPlatform.SetExtendedUserId(string partner, string id)
        {
            Bridge.CallStatic("setExtendedUserId", partner, id);
        }

        void IOdeeoSdkPlatform.SetPublisherUserID(string id)
        {
            Bridge.CallStatic("setPublisherUserID", id);
        }

        string IOdeeoSdkPlatform.GetPublisherUserID()
        {
            return Bridge.CallStatic<string>("getPublisherUserID");
        }
        
        #region Pause
        
        private static void TriggerPauseEvents(bool isPaused)
        {
            if (isPaused) OnPause();
            else OnResume();
        }
        
        private static void OnPause()
        {
            Bridge.CallStatic("onPause");
        }

        private static void OnResume()
        {
            Bridge.CallStatic("onResume");
        }
        
        #endregion
        
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
            return Screen.dpi / 160f;
        }

        void IOdeeoSdkPlatform.SetLogLevel(OdeeoSdk.LogLevel level)
        {
            AndroidJavaClass typeEnum = new AndroidJavaClass ("io.odeeo.sdk.common.LogLevel");
            AndroidJavaObject curType = typeEnum.CallStatic<AndroidJavaObject> ("valueOf", level.ToString ());
            Bridge.CallStatic("setLogLevel", curType);
            _logging.LogLevel = level;
        }
    }
}
#endif