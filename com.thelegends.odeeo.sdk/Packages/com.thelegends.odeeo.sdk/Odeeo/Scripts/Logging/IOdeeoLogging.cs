namespace Odeeo
{
    internal interface IOdeeoLogging
    {
        OdeeoSdk.LogLevel LogLevel { get; set; }
        
        void Error(string message);
        void Warning(string message);
        void Info(string message);
    }
}