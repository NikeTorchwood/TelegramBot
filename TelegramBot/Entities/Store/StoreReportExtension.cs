using EconomicDirections;
using TelegramBot.Entities.Report;

namespace TelegramBot.Entities.Store;

public static class StoreReportExtension
{
    public static Store GetStoreFromReport(this Store store, List<(string, bool)> directionList, EconomicReport report)
    {
        store.Rank = report.GetStoreRank(store.Code);
        foreach (var nameDirection in directionList)
        {
            store.StoreEconomicDirections.Add(
                new EconomicDirection(nameDirection).GetDirectionFromReport(store.Code, report));
        }
        store.UpdateDate = report.GetUpdateDate(store.Code);
        return store;
    }
}