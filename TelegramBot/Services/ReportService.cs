using Aspose.Cells;
using TelegramBot.Entities.Report;
using TelegramBot.Entities.Store;

namespace TelegramBot.Services
{
    public class ReportService
    {
        private readonly List<(string, bool)> _directionList = new()
        {
            ("SIM Продажи", false),
            ("SIM", true),
            ("Автоплатеж", false),
            ("Sim АП", false),
            ("ФС", false),
            ("СТВ", false),
            ("Цифровая Экосистема МТС", true),
            ("Телефоны", true),
            ("Прочие товары", true),
            ("Дополнительные услуги", true),
            ("Банковские карты", true)
        };
        public List<Store> StoreList { get; }

        public ReportService(Workbook workbook)
        {
            StoreList = GetListStoresFromReport(workbook);
        }
        public List<Store> GetListStoresFromReport(Workbook workbook)
        {
            var report = new EconomicReport(workbook);
            var nameList = report.GetStoreNames();
            return nameList.Select(storeName => new Store(storeName).GetStoreFromReport(_directionList, report)).ToList();
        }
    }
}