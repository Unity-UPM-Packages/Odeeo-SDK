using Odeeo.Platforms.Serialization;

namespace Odeeo.Data
{
    public sealed class OdeeoAdData
    {
        private readonly string _sessionID;
        private readonly OdeeoSdk.PlacementType _placementType;
        private readonly string _placementID;
        private readonly string _country;
        private readonly double _eCPM;
        private readonly string _transactionID;
        private readonly string _customTag;

        internal OdeeoAdData(OdeeoAdDataDto dto)
        {
            _sessionID = dto.sessionID;
            _placementType = (OdeeoSdk.PlacementType)dto.placementType;
            _placementID = dto.placementID;
            _country = dto.country;
            _eCPM = dto.eCPM;
            _transactionID = dto.transactionID;
            _customTag = dto.customTag;
        }
        
        public string GetSessionID()
        {
            return _sessionID;
        }

        public OdeeoSdk.PlacementType GetPlacementType()
        {
            return _placementType;
        }

        public string GetPlacementID()
        {
            return _placementID;
        }

        public string GetCountry()
        {
            return _country;
        }

        public double GetEcpm()
        {
            return _eCPM;
        }

        public string GetTransactionID()
        {
            return _transactionID;
        }
        
        public string GetCustomTag()
        {
            return _customTag;
        }
    }
}
