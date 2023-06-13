using Aspose.Cells;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using TelegramBot.Entities.Report;

namespace TelegramBot.Services;

public static class DatabaseReportService
{
    public static async Task UpdateDatabaseAsync(Workbook workbook)
    {
        Console.WriteLine("Обновление БД запущено");
        var sw = new Stopwatch();
        sw.Restart();
        var report = new ReportService(workbook);
        var addStoreValues = new StringBuilder();
        var addDirectionValue = new StringBuilder();
        var addToDelete = new StringBuilder();
        for (var i = 0; i < report.StoreList.Count; i++)
        {
            var store = report.StoreList[i];
            addToDelete.Append($"N'{store.Code}'");
            addToDelete.Append(i == report.StoreList.Count - 1 ? ")" : ",");
            addStoreValues.Append($" (N'{store.Code}',{store.Rank}, {store.UpdateDate})");
            addStoreValues.Append(i == report.StoreList.Count - 1 ? ")" : ",");
            for (var j = 0; j < store.StoreEconomicDirections.Count; j++)
            {
                var direction = store.StoreEconomicDirections[j];
                addDirectionValue.Append($" (N'{direction.Name}', N'{store.Code}', {direction.Plan}, {direction.Fact},");
                if (direction.IsRank)
                {
                    addDirectionValue.Append($" {direction.Rank})");
                }
                else
                {
                    addDirectionValue.Append(" Null)");
                }
                if (j == store.StoreEconomicDirections.Count - 1 && i == report.StoreList.Count - 1)
                {
                    addDirectionValue.Append(")");
                }
                else
                {
                    addDirectionValue.Append(",\n");
                }
            }
        }
        var cmdText = $"""
                delete from Stores
                where [StoreCode] not in ({addToDelete};
                merge into Stores as Target using (values
                {addStoreValues}
                as Source ([StoreCode], [Rank], [AmountLP])
                on Target.[StoreCode] = Source.[StoreCode]
                When matched then
                update set Target.[Rank] = Source.[Rank],
                Target.[AmountLP] = Source.[AmountLP]
                when not matched then
                insert([StoreCode], [Rank], [AmountLP]) values(Source.[StoreCode], Source.[Rank], Source.[AmountLP]);
                merge into EconomicDirections as Target using (values
                {addDirectionValue}
                as Source ([Name], [StoreCode], [Plan], [Fact], [Rank])
                on Target.[StoreCode] = Source.[StoreCode]
                and Target.[Name] = Source.[Name]
                When matched then
                update set
                Target.[Name] = Source.[Name],
                Target.[Plan] = Source.[Plan],
                Target.[Fact] = Source.[Fact],                
                Target.[Rank] = Source.[Rank]
                when not matched then
                insert([Name], [StoreCode], [Plan], [Fact], [Rank]) values(Source.[Name], Source.[StoreCode], Source.[Plan], Source.[Fact], Source.[Rank]);
        
                """;
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var sqlCom = new SqlCommand(cmdText, connection);
        sqlCom.ExecuteNonQuery();
        await connection.CloseAsync();
        sw.Stop();
        await TelegramService.SendMessage($"БД обновлена, затраченное время обновления {sw.Elapsed}");
    }



    public static async Task<List<string?>> GetStoreList()
    {
        var result = new List<string?>();
        await using var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand("select (StoreCode) from stores", connection);
        await using var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) return result;
        while (await reader.ReadAsync()) result.Add(reader.GetValue(0).ToString());

        return result;
    }

    public static async Task<string> GetReportStore(string storeName)
    {
        var result = string.Empty;
        await using var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"select [rank] from stores where [StoreCode] = '{storeName}';", connection);
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!reader.HasRows) return "Ошибка выгрузки БД";
            {
                while (await reader.ReadAsync())
                {
                    var rank = reader.GetValue(0).ToString();
                    result += $"""
                        Выбранный магазин {storeName} 
                        Общий ранг магазина {rank}
                        Направление/План/Факт/Рейтинг

                        """;
                }
            }
        }
        command = new SqlCommand(
            $"select [Name], [Plan], [Fact], [Rank] from EconomicDirections where [StoreCode] = '{storeName}';",
            connection);
        await using (var reader = await command.ExecuteReaderAsync())
        {
            if (!reader.HasRows) return null;
            {
                while (await reader.ReadAsync())
                {
                    var name = reader.GetValue(0).ToString();
                    var plan = reader.GetInt32(1);
                    var fact = reader.GetInt32(2);
                    var rank = reader.GetValue(3);
                    var persent = fact / (double)plan;
                    if (string.IsNullOrEmpty(rank.ToString()))
                        result += $"""
                        -----------------------------------
                        {name}/План/Факт/% - Рейтинг
                        {plan}/{fact}/{persent:P1}  - {rank}

                        """;
                    else
                        result += $"""
                        -----------------------------------
                        {name}/План/Факт/% 
                        {plan}/{fact}/{persent:P1} 

                        """;
                }
            }
        }

        return result;
    }
}