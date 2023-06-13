using Aspose.Cells;

namespace TelegramBot.AsposeExtensions
{
    public static class WorksheetExtension
    {
        public static int FindColumnNames(this Worksheet sheet, string columnName, string columnCategory)
        {
            var firstOption = (columnName + " " + columnCategory.Trim()).ToLower();
            var secondOption = (columnCategory.Trim() + " " + columnName).ToLower();
            for (var i = 0; i <= sheet.Cells.MaxDataColumn; i++)
                if (firstOption == sheet.Cells[0, i].Value.ToString()?.ToLower())
                    return i;
                else if (secondOption == sheet.Cells[0, i].Value.ToString()?.ToLower()) return i;
            return 0;
        }

    }
}
