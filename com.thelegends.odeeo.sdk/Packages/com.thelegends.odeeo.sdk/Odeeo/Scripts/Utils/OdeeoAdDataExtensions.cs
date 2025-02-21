using System;
using Odeeo.Data;
using Odeeo.Platforms.Serialization;

namespace Odeeo.Utils
{   
    internal static class OdeeoAdDataExtensions
    {
        public static T FromEditor<T>(this OdeeoSdk.PlacementType type, string customTag)
        {
            Type e = typeof(T);
            switch (e.Name)
            {
                case nameof(OdeeoAdData):
                    return (T)(object)new OdeeoAdData(new OdeeoAdDataDto
                    {
                        sessionID = "test_editor", 
                        placementType = (int) type, 
                        placementID = "test_editor",
                        country = "test_editor",
                        eCPM = 0d,
                        transactionID = "test_editor",
                        customTag = customTag   
                    });
                case nameof(OdeeoImpressionData):
                    return (T)(object)new OdeeoImpressionData(new OdeeoImpressionDataDto
                    {
                        sessionID = "test_editor", 
                        placementType = (int) type, 
                        placementID = "test_editor",
                        country = "test_editor",
                        payableAmount = 0,
                        transactionID = "test_editor",
                        customTag = customTag   
                    });
                default:
                    throw new ArgumentException($"Type '{e.Name}' is not supported.");
            }
        }
    }
}