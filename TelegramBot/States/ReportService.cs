using System.Collections;
using System.Text;
using System.Xml.Linq;
using Aspose.Cells;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot;

public class ReportService
{
    public static Store CurrentStore;
    private static Workbook _workbook;
    public static List<Store> StoreList;

    public static void Update()
    {
        UpdateReport();
    }


    private static async Task UpdateReport()
    {
        try
        {
            _workbook = new Workbook("..//economic.xlsx");
        }
        catch (Exception e)
        {
            await TelegramService.SendMessage(e.Message);
        }
        StoreList = ReportHelper.FindAllStores(_workbook);
    }



    //public string CreateEconomicReport()
    //{
    //    var sb = new StringBuilder();
    //    sb.Append($"Магазин: {CurrentStore}");
    //    foreach (var economicDirection in directionList)
    //    {
    //        var direction = new Direction(economicDirection);
    //        sb.Append(
    //            $"{direction.Name} План: {_datasheet.Cells[CurrentStore.Row, direction.PlanColumn].Value} | Факт {_datasheet.Cells[CurrentStore.Row, direction.PlanColumn].Value}");
    //    }
    //    return sb.ToString();
    //}
}

public class EconomicDirection
{
}

public static class ReportHelper
{
    public static Worksheet? FindSheet(Workbook workbook, string name)
    {
        return workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == name);
    }

    public static List<Store> FindAllStores(Workbook workbook)
    {
        var result = new List<Store>();
        var sheet = FindSheet(workbook, "Данные ОП");
        var column = 0;
        for (var i = 0; i <= sheet.Cells.MaxDataColumn; i++)
        {
            if ("магазин" == sheet.Cells[0, i].Value.ToString().ToLower())
            {
                column = i;
                break;
            }
        }

        for (var i = 1; i <= sheet.Cells.MaxDataRow; i++)
        {
            result.Add(new Store(sheet.Cells[i, column].Value.ToString()));
        }
        return result;
    }
}

public class Store
{
    public string Code { get; set; }
    public static List<EconomicDirection> directions;

    public Store(string code) { Code = code; }
}


