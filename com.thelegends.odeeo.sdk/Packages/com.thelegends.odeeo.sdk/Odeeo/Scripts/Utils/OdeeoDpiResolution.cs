using UnityEngine;

namespace Odeeo.Utils
{
    internal sealed class OdeeoDpiResolution
    {
        private static IOdeeoSdkPlatform _platform;

        internal OdeeoDpiResolution(IOdeeoSdkPlatform platform)
        {
            _platform = platform;
            
            if (Application.isEditor)
                SetOptimalDPI();
        }

        internal void SetUnityEditorDPI(int dpi)
        {
            _platform.SetUnityEditorDPI(dpi);
        }
        
        internal static void SetTargetDPI(int dpi)
        {
            _platform.SetTargetDPI(dpi);
        }
        
        internal static void SetOptimalDPI()
        {
            _platform.SetOptimalDPI();
        }
        
        internal static float GetScreenDPI()
        {
            return _platform.GetScreenDPI();
        }

        internal static float GetDpiMultiplier()
        {
            return _platform.GetDpiMultiplier();
        }

        internal static float GetDeviceScale()
        {
            return _platform.GetDeviceScale();
        }
    }
}