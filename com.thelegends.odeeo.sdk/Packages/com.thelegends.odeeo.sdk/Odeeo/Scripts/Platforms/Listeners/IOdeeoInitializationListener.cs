using System;

namespace Odeeo.Platforms.Listeners
{
    internal interface IOdeeoInitializationListener
    {
        Action OnInitializationSuccess { get; set; }
        Action<int, string> OnInitializationFailed { get; set; }
    }
}