#if UNITY_EDITOR
using System;

namespace Odeeo.Platforms.Listeners
{
    internal sealed class OdeeoEditorListener : IOdeeoInitializationListener
    {
        public Action OnInitializationSuccess { get; set; }
        public Action<int, string> OnInitializationFailed { get; set; }
    }
}
#endif