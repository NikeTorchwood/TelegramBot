using EconomicDirections;

namespace TelegramBot.Entities.Store
{
    public class Store
    {
        public string? Code { get; set; }
        public int Rank { get; set; }
        public List<EconomicDirection> StoreEconomicDirections { get; set; }
        public int UpdateDate { get; set; }

        public Store(string? code)
        {
            Code = code;
            StoreEconomicDirections = new List<EconomicDirection>();
        }

    }
}