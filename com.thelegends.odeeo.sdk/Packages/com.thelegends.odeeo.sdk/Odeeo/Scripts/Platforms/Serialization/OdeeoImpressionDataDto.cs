using System;
using UnityEngine.Serialization;

namespace Odeeo.Platforms.Serialization
{
    [Serializable]
    internal struct OdeeoImpressionDataDto
    {
        public string sessionID;
        public int placementType;
        public string placementID;
        public string country;
        public double payableAmount;
        public string transactionID;
        public string customTag;
    }
}