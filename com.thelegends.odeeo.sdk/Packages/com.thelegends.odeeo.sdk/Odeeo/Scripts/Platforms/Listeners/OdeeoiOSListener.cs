#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
using Odeeo.Utils;

// ReSharper disable IdentifierTypo

namespace Odeeo.Platforms.Listeners
{
    internal sealed class OdeeoiOSListener : IOdeeoInitializationListener
    {
        private delegate void OdeeoSdkNoArgsDelegateNative (IntPtr client);
        private delegate void OdeeoSdkInitializationErrorDelegateNative (IntPtr client, int errorCode, string errorMessage);
        
        public Action OnInitializationSuccess { get; set; }
        public Action<int, string> OnInitializationFailed { get; set; }
        
        private IntPtr _odeeoSdkNativeListenerRef;
        
        public OdeeoiOSListener()
        {
            _odeeoSdkNativeListenerRef = _odeeoSdkSetOnInitializationListener((IntPtr)GCHandle.Alloc(this), OnInitializationSuccessNative, OnInitializationFailNative);;
        }

        private static IOdeeoInitializationListener IntPtrToClient(IntPtr cl){
            GCHandle handle = (GCHandle)cl;
            return handle.Target as IOdeeoInitializationListener;
        }

        [MonoPInvokeCallback(typeof(OdeeoSdkNoArgsDelegateNative ))]
        private static void OnInitializationSuccessNative(IntPtr client){
            IOdeeoInitializationListener listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => listener.OnInitializationSuccess?.Invoke() );
        }

        [MonoPInvokeCallback(typeof(OdeeoSdkInitializationErrorDelegateNative ))]
        private static void OnInitializationFailNative(IntPtr client, int errorCode, string errorMessage){
            IOdeeoInitializationListener listener = IntPtrToClient(client);
            OdeeoMainThreadDispatcher.Instance.Enqueue( () => listener.OnInitializationFailed?.Invoke(errorCode, errorMessage) );
        }
        
        [DllImport("__Internal")]
        private static extern IntPtr _odeeoSdkSetOnInitializationListener(IntPtr callbackRef, 
            OdeeoSdkNoArgsDelegateNative onInitializationSuccess, OdeeoSdkInitializationErrorDelegateNative onInitializationFail);
    }
}
#endif