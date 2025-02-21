using System;
using System.Collections.Generic;
using Odeeo.Logging;
using Odeeo.Platforms;
using Odeeo.Platforms.Listeners;
using Odeeo.Utils;
using UnityEngine;

namespace Odeeo
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class OdeeoSdk
    {
        public const string SDK_VERSION = OdeeoBuildConfig.SDK_VERSION;

        public enum LogLevel
        {
            None,
            Info,
            Debug
        }

        public enum IconPosition
        {
            TopLeft = 0,
            TopCenter = 1,
            TopRight = 2,
            CenterLeft = 3,
            Centered = 4,
            CenterRight = 5,
            BottomLeft = 6,
            BottomCenter = 7,
            BottomRight = 8
        }

        public enum BannerPosition
        {
            TopCenter = 1,
            BottomCenter = 7
        }

        public enum PlacementType
        {
            AudioBannerAd,
            RewardedAudioBannerAd,
            AudioIconAd,
            RewardedAudioIconAd
        }

        public enum ConsentType
        {
            Undefined,
            None,
            Gdpr,
            Ccpa
        }

        public enum AdSizingMethod
        {
            Flexible,
            Strict
        }
        
        #region Initialization
        
        public static event Action OnInitializationSuccess;
        public static event Action<int, string> OnInitializationFailed;
        
        public static Action<bool> onApplicationPause = paused => {};

        private static readonly IOdeeoInitializationListener _listener;
        private static readonly IOdeeoSdkPlatform _platform;
        private static readonly IOdeeoLogging _logging;
        private static readonly OdeeoDpiResolution _dpiResolution;

        static OdeeoSdk()
        {
            _logging = new OdeeoLogging();

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (Application.platform)
            {
                case RuntimePlatform.Android when !Application.isEditor:
#if UNITY_ANDROID
                    _listener = new OdeeoAndroidListener();
                    _platform = new OdeeoSdkAndroid(_listener, _logging);
#endif
                    break;
                case RuntimePlatform.IPhonePlayer when !Application.isEditor:
#if UNITY_IOS
                    _listener = new OdeeoiOSListener();
                    _platform = new OdeeoSdkiOS(_listener, _logging);
#endif
                    break;
                default:
                {
                    if (Application.isEditor)
                    {
#if UNITY_EDITOR
                        _listener = new OdeeoEditorListener();
                        _platform = new OdeeoSdkUnityEditor(_listener, _logging);
#endif
                    }
                    else
                    {
                        _logging.Error("Unsupported platform. Only iOS and Android are supported at the moment.");
                    }

                    break;
                }
            }

            _dpiResolution = new OdeeoDpiResolution(_platform);
        }
        
        public static void Initialize(string appKey)
        {
            if (!Application.isEditor && Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
            {
                _logging.Error("Unsupported platform. Only iOS and Android are supported at the moment.");
                return;
            }
            
            if (_platform.IsInitializationInProgress)
            {
                _logging.Warning("OdeeoSDK Initialization in progress");
                return;
            }
            
            if (_platform.IsInitialized())
            {
                _logging.Info("OdeeoSDK already initialized");
                return;
            }
            
            OdeeoMainThreadDispatcher.Instance.Enqueue(() => _logging.Info("Sync created"));
            
            _listener.OnInitializationSuccess += OnInitializationSuccess;
            _listener.OnInitializationFailed += OnInitializationFailed;
            _listener.OnInitializationFailed += ResetListeners;

            onApplicationPause += _platform.OnApplicationPause;
            
            _platform.Initialize(appKey, Application.unityVersion, SDK_VERSION);

            void ResetListeners(int x, string y)
            {   
                _logging.Info("OdeeoSDK failed to initialize... Cleaning internal subscriptions");
                
                _listener.OnInitializationSuccess = null;
                _listener.OnInitializationFailed = null;
                onApplicationPause = null;
            }
        }

        public static bool IsInitialized()
        {
            return _platform.IsInitialized();
        }
        
#endregion

#region Regulation

        public static void SetIsChildDirected(bool flag)
        {
            _platform.SetIsChildDirected(flag);
        }

        public static void RequestTrackingAuthorization()
        {
            _platform.RequestTrackingAuthorization();
        }
        
#region RegulationType

        public static void ForceRegulationType(ConsentType type)
        {
            _platform.ForceRegulationType(type);
        }

        public static void ClearForceRegulationType()
        {
            _platform.ClearForceRegulationType();
        }

        public static ConsentType GetRegulationType()
        {
            return _platform.GetRegulationType();
        }
        
#endregion

#region Gdpr
        
        [Obsolete("SetGdprConsentString is deprecated, SDK fetches values from CMP")]
        public static void SetGdprConsentString(string consentString) { }

        [Obsolete("SetGdprConsent is deprecated, SDK fetches values from CMP")]
        public static void SetGdprConsent(bool consent) { }
        
        [Obsolete("SetGdprConsent is deprecated, SDK fetches values from CMP")]
        public static void SetGdprConsent(bool consent, string consentString) { }
        
        #endregion
        
        #region DoNotSell
        
        public static void SetDoNotSellPrivacyString(string privacyString)
        {
            _platform.SetDoNotSellPrivacyString(privacyString);
        }
        
        public static void SetDoNotSell(bool isApplied)
        {
            _platform.SetDoNotSell(isApplied);
        }

        public static void SetDoNotSell(bool isApplied, string privacyString)
        {
            _platform.SetDoNotSell(isApplied, privacyString);
        }
        
#endregion

#endregion
        
        /// <summary>
        /// Returns current device volume in Percentages from 0 to 100
        /// </summary>
        public static float GetDeviceVolumeLevel()
        {
            return _platform.GetDeviceVolumeLevel();
        }

        public static void AddCustomAttribute(string key, string value)
        {
            _platform.AddCustomAttribute(key, value);
        }

        public static void ClearCustomAttributes()
        {
            _platform.ClearCustomAttributes();
        }

        public static void RemoveCustomAttribute(string key)
        {
            _platform.RemoveCustomAttribute(key);
        }

        public static List<KeyValuePair<string, string>> GetCustomAttributes()
        {
            return _platform.GetCustomAttributes();
        }

        public static List<KeyValuePair<string, string>> GetCustomAttributes(string key)
        {
            return _platform.GetCustomAttributes(key);
        }

        public static void SetPublisherUserID(string id)
        {
            _platform.SetPublisherUserID(id);
        }
        
        public static void SetExtendedUserId(string partner, string id)
        {
            _platform.SetExtendedUserId(partner, id);
        }

        public static string GetPublisherUserID()
        {
            return _platform.GetPublisherUserID();
        }

        public static void SetUnityEditorDPI(int dpi)
        {
            _dpiResolution.SetUnityEditorDPI(dpi);
        }

        public static void SetLogLevel(LogLevel level)
        {
            _platform.SetLogLevel(level);
        }
    }
}