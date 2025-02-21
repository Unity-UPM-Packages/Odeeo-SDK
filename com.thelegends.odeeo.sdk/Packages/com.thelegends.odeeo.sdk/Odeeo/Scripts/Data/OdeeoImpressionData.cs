using Odeeo.Platforms.Serialization;

namespace Odeeo.Data
{
    public sealed class OdeeoImpressionData
    {
        private readonly string _sessionID;
        private readonly OdeeoSdk.PlacementType _placementType;
        private readonly string _placementID;
        private readonly string _country;
        private readonly double _payableAmount;
        private readonly string _transactionID;
        private readonly string _customTag;

        internal OdeeoImpressionData(OdeeoImpressionDataDto dto)
        {
            _sessionID = dto.sessionID;
            _placementType = (OdeeoSdk.PlacementType)dto.placementType;
            _placementID = dto.placementID;
            _country = dto.country;
            _payableAmount = dto.payableAmount;
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

        public double GetPayableAmount()
        {
            return _payableAmount;
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