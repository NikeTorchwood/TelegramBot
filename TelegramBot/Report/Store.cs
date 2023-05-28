using Aspose.Cells;

namespace TelegramBot.Report
{
    public class Store
    {
        public Store(string name, List<string> directionNames, Workbook workbook)
        {
            Name = name;
            Directions = new List<EconomicDirection>();
            foreach (var directionName in directionNames)
            {
                Directions.Add(new EconomicDirection(Name, directionName, workbook));
            }
            Rank = GetStoreRank(workbook, name);
        }

        public string? Name { get; set; }
        public int Rank { get; set; }
        public List<EconomicDirection> Directions { get; set; }

        private static int GetStoreRank(Workbook workbook, string storeName)
        {
            var sheet = ReportHelper.GetSheet("Рейтинг Магазина", workbook);
            var row = sheet.Cells.Find(storeName, sheet.Cells.FirstCell).Row;
            var column = ReportHelper.FindColumnNames("общий", "ранг", sheet);
            return int.Parse(sheet.Cells[row, column].Value.ToString());
        }
    }
}
