using UnityEngine;

namespace Odeeo.Logging
{
    internal sealed class OdeeoLogging : IOdeeoLogging
    {
        private const string ODEEO_LOG_FORMAT = "OdeeoSdk: {0}";
        
        public OdeeoSdk.LogLevel LogLevel { get; set; } = OdeeoSdk.LogLevel.Debug;

        public void Error(string message)
        {
            if (LogLevel == OdeeoSdk.LogLevel.None)
                return;
            
            Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, ODEEO_LOG_FORMAT, message);
        }

        public void Warning(string message)
        {
            if (LogLevel < OdeeoSdk.LogLevel.Debug)
                return;
            
            Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, ODEEO_LOG_FORMAT, message);
        }

        public void Info(string message)
        {
            if (LogLevel < OdeeoSdk.LogLevel.Debug)
                return;
            
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, ODEEO_LOG_FORMAT, message);
        }
    }
}