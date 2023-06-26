using Aspose.Cells;
using TelegramBot.AsposeExtensions;

namespace TelegramBot.Entities.Report;

public class EconomicReport
{
    public Worksheet DataSheet { get; }
    public Worksheet RankSheet { get; }

    public EconomicReport(Workbook workbook)
    {
        DataSheet = workbook.GetSheet("Данные Магазина");
        RankSheet = workbook.GetSheet("Рейтинг Магазина");
        FixReport(workbook);
    }
    public int GetStoreRank(string nameStore)
    {
        var row = RankSheet.Cells.Find(nameStore, RankSheet.Cells.FirstCell).Row;
        var column = RankSheet.FindColumnNames("общий", "ранг");
        return int.Parse(RankSheet.Cells[row, column].Value.ToString());
    }
    private void FixReport(Workbook workbook)
    {
        var corrections = new List<(string, string)>
        {
            ("АП", "Автоплатеж"),
            ("Спутниковое оборудование", "СТВ"),
            ("МТСД дебетовые", "Карты дебетовые")
        };
        foreach (var corretion in corrections)
        {
            var column = DataSheet.FindColumnNames(corretion.Item1, "Факт");
            DataSheet.Cells[0, column].PutValue("Факт " + corretion.Item2);
        }
    }
    public int GetUpdateDate(string storeName)
    {
        var row = DataSheet.Cells.Find(storeName, DataSheet.Cells.FirstCell).Row;
        var column = DataSheet.Cells.Find("Количество ЛП", DataSheet.Cells.FirstCell).Column;
        return DataSheet.Cells[row, column].IntValue;
    }

    public List<string?> GetStoreNames()
    {
        var result = new List<string?>();
        var column = DataSheet.Cells.Find("Магазин", DataSheet.Cells.FirstCell).Column;
        for (var i = 1; i <= DataSheet.Cells.MaxDataRow; i++) result.Add(DataSheet.Cells[i, column].Value.ToString());
        return result;
    }
}