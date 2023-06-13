using Aspose.Cells;

namespace TelegramBot.AsposeExtensions
{
    public static class WorkbookExtension
    {
        public static Worksheet? GetSheet(this Workbook workbook, string nameSheet)
        {
            return workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == nameSheet);
        }
    }
}
