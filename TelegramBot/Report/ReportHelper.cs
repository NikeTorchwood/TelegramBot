using Aspose.Cells;

namespace TelegramBot.Report
{
    public static class ReportHelper
    {
        public static void FixReport(Workbook workbook)
        {
            var corrections = new List<(string, string)>()
        {
            ("АП","Автоплатеж"),
            ("Спутниковое оборудование","СТВ"),
            ("МТСД дебетовые", "Карты дебетовые"),

        };
            var dataSheet = GetSheet("Данные Магазина", workbook);
            foreach (var corretion in corrections)
            {
                var column = FindColumnNames(corretion.Item1, "Факт", dataSheet);
                dataSheet.Cells[0, column].PutValue("Факт " + corretion.Item2);
            }
        }
        public static Worksheet? GetSheet(string nameSheet, Workbook workbook)
        {
            return workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == nameSheet);
        }

        public static List<Store> GetStores(List<string> storeList, List<string> directionList, Workbook workbook)
        {
            return storeList.Select(store => new Store(store, directionList, workbook)).ToList();
        }


        public static List<string> GetStoreNames(Workbook workbook)
        {
            var sheet = GetSheet("Данные Магазина", workbook);
            var result = new List<string>();
            var column = sheet.Cells.Find("Магазин", sheet.Cells.FirstCell).Column;
            for (var i = 1; i <= sheet.Cells.MaxDataRow; i++)
            {
                result.Add(sheet.Cells[i, column].Value.ToString());
            }
            return result;
        }

        public static int FindColumnNames(string? columnName, string columnCategory, Worksheet? sheet)
        {
            var firstOption = (columnName + " " + columnCategory.Trim()).ToLower();
            var secondOption = (columnCategory.Trim() + " " + columnName).ToLower();
            for (var i = 0; i <= sheet.Cells.MaxDataColumn; i++)
            {
                if (firstOption == sheet.Cells[0, i].Value.ToString()?.ToLower())
                {
                    return i;
                }
                else if (secondOption == sheet.Cells[0, i].Value.ToString()?.ToLower())
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
