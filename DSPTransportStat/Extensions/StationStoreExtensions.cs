using DSPTransportStat.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPTransportStat.Extensions
{
    static class StationStoreExtensions
    {
        static public string GetCountAsString (this StationStore stationStore)
        {
            return $"{stationStore.count}";
        }

        static public string GetTotalOrderAsString (this StationStore stationStore)
        {
            return $"{stationStore.totalOrdered}";
        }

        static public string GetMaxAsString (this StationStore stationStore)
        {
            return $"{stationStore.max}";
        }

        static public string GetLocalLogicAsString (this StationStore stationStore)
        {
            switch (stationStore.localLogic)
            {
                case ELogisticStorage.None: return Strings.Common.StationStoreLogic.InPlanetStorage;
                case ELogisticStorage.Supply: return Strings.Common.StationStoreLogic.InPlanetSupply;
                case ELogisticStorage.Demand: return Strings.Common.StationStoreLogic.InPlanetDemand;
                default: return "Undefined Local Logic (" + stationStore.localLogic.ToString() + ")";
            }
        }

        static public string GetRemoteLogicAsString (this StationStore stationStore)
        {
            switch (stationStore.remoteLogic)
            {
                case ELogisticStorage.None: return Strings.Common.StationStoreLogic.InterstellarStorage;
                case ELogisticStorage.Supply: return Strings.Common.StationStoreLogic.InterstellarSupply;
                case ELogisticStorage.Demand: return Strings.Common.StationStoreLogic.InterstellarDemand;
                default: return "Undefined Remote Logic (" + stationStore.remoteLogic.ToString() + ")";
            }
        }
    }
}
