using Aspose.Cells;

namespace TelegramBot.Report;

public class EconomicDirection
{
    public string? Name { get; set; }
    public int Plan { get; set; }
    public int Fact { get; set; }
    public int Rank { get; set; }

    public EconomicDirection(string storeName, string name, Workbook workbook)
    {
        Name = name;
        var dataSheet = ReportHelper.GetSheet("Данные Магазина", workbook);
        var row = dataSheet.Cells.Find(storeName, dataSheet.Cells.FirstCell).Row;
        var planColumn = ReportHelper.FindColumnNames(name, "План", dataSheet);
        var factColumn = ReportHelper.FindColumnNames(name, "Факт", dataSheet);
        Plan = dataSheet.Cells[row, planColumn].Type != CellValueType.IsString
            ? dataSheet.Cells[row, planColumn].IntValue
            : 0;
        Fact = dataSheet.Cells[row, factColumn].Type != CellValueType.IsString
            ? dataSheet.Cells[row, factColumn].IntValue
            : 0;
        var rankSheet = ReportHelper.GetSheet("Рейтинг Магазинаa", workbook);
        var rankColumn = ReportHelper.FindColumnNames(name, "Ранг", rankSheet);
        Rank = rankColumn == 0 ? 0 : rankSheet.Cells[row, rankColumn].IntValue;
    }
}