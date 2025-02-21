using System;
using Odeeo.Utils;
using UnityEngine;

namespace Odeeo.Platforms.Listeners
{
    internal sealed class OdeeoAndroidListener : AndroidJavaProxy, IOdeeoInitializationListener
    {
        public Action OnInitializationSuccess { get; set; }
        public Action<int, string> OnInitializationFailed { get; set; }

        public OdeeoAndroidListener() 
            : base("io.odeeo.sdk.common.SdkInitializationListener")
        {
            
        }
        
        //  These must follow SdkInitializationListener naming. Don't change the name.

        void onInitializationSucceed()
        {
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => OnInitializationSuccess?.Invoke() ) ;
        }

        void onInitializationFailed(int errorParam, string error)
        {
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => OnInitializationFailed?.Invoke(errorParam, error) ) ;
        }
    }
}