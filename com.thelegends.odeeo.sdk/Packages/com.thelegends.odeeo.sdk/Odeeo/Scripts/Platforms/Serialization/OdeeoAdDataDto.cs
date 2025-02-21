using System;
using UnityEngine.Serialization;

namespace Odeeo.Platforms.Serialization
{
    [Serializable]
    internal struct OdeeoAdDataDto
    {
        public string sessionID;
        public int placementType;
        public string placementID;
        public string country;
        public double eCPM;
        public string transactionID;
        public string customTag;
    }
}