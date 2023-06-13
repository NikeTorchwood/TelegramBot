using Aspose.Cells;
using TelegramBot.AsposeExtensions;
using TelegramBot.Entities.Report;

namespace EconomicDirections;

public static class EconomicDirectionExtension
{
    public static EconomicDirection GetDirectionFromReport(this EconomicDirection direction, string storeName, EconomicReport report)
    {
        var row = report.DataSheet.Cells.Find(storeName, report.DataSheet.Cells.FirstCell).Row;
        var planColumn = report.DataSheet.FindColumnNames(direction.Name, "План");
        var factColumn = report.DataSheet.FindColumnNames(direction.Name, "Факт");
        direction.Plan = report.DataSheet.Cells[row, planColumn].Type != CellValueType.IsString
            ? report.DataSheet.Cells[row, planColumn].IntValue
            : 0;
        direction.Fact = report.DataSheet.Cells[row, factColumn].Type != CellValueType.IsString
            ? report.DataSheet.Cells[row, factColumn].IntValue
            : 0;
        if (!direction.IsRank) return direction;
        var rankColumn = report.RankSheet.FindColumnNames(direction.Name, "Ранг");
        direction.Rank = rankColumn == 0 ? 0 : report.RankSheet.Cells[row, rankColumn].IntValue;
        return direction;
    }

}