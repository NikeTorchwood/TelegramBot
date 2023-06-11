using Aspose.Cells;
using System.Data.SqlClient;
using TelegramBot.Report;

namespace TelegramBot.Services;

public static class DatabaseService
{
    public static async Task UpdateDatabaseAsync(Workbook workbook, string connectionString)
    {
        Console.WriteLine("попал в метод обновления бд");
        ReportHelper.FixReport(workbook);
        await RemoveUnnecessaryStores(workbook, connectionString);
        await AddStoreList(TelegramService.directionList, workbook, connectionString);
        await RefreshDatabase(TelegramService.directionList, workbook, connectionString);
        await TelegramService.SendMessage("БД обновлена.");
    }

    private static async Task AddStoreList(List<string> directionNames, Workbook workbook, string connectionString)
    {
        var storeList = ReportHelper.GetStores(ReportHelper.GetStoreNames(workbook), directionNames, workbook);
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var countStore = 0;
        var countDirection = 0;
        var command = new SqlCommand(string.Empty, connection);
        var currentStoreList = await GetStoreList(connectionString);
        var itemsToAdd = storeList.Where(item => !currentStoreList.Contains(item.Name)).ToList();
        foreach (var item in itemsToAdd)
        {
            countStore++;
            command.CommandText = $"insert into stores (name, rank) values ('{item.Name}', {item.Rank});";
            await command.ExecuteNonQueryAsync();
            foreach (var direction in item.Directions)
            {
                countDirection++;
                command.CommandText = $"""
                    insert into EconomicDirections (name, StoreName, StoreId, [Plan], Fact, Rank)
                    values (N'{direction.Name}',
                    '{item.Name}',
                    (select Id from Stores where name = '{item.Name}'),
                    {direction.Plan},
                    {direction.Fact},
                    {direction.Rank});
                    """;
                await command.ExecuteNonQueryAsync();
                Console.WriteLine(
                    $"{direction.Name}|{direction.Plan}|{direction.Fact}|{direction.Rank} - был добавлен");
            }

            Console.WriteLine($"{item} - был добавлен");
        }

        Console.WriteLine($"Было добавлено {countStore} записей магазинов");
        Console.WriteLine($"Было добавлено {countDirection} записей направлений");
    }

    private static async Task RemoveUnnecessaryStores(Workbook workbook, string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var names = ReportHelper.GetStoreNames(workbook);
        var count = 0;
        var command = new SqlCommand(string.Empty, connection);
        var currentStoreList = await GetStoreList(connectionString);
        var itemsToRemove = currentStoreList.Where(item => !names.Contains(item)).ToList();
        foreach (var item in itemsToRemove)
        {
            count++;
            command.CommandText = $"delete stores where name = '{item}'";
            await command.ExecuteNonQueryAsync();
            Console.WriteLine($"было удалено {item}");
        }

        Console.WriteLine($"Было удалено {count} записей магазинов");
    }

    public static async Task<List<string?>> GetStoreList(string connectionString)
    {
        var result = new List<string?>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand("select (name) from stores", connection);
        await using var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) return result;
        while (await reader.ReadAsync()) result.Add(reader.GetValue(0).ToString());

        return result;
    }

    public static async Task RefreshDatabase(List<string> directionNames, Workbook workbook, string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var count = 0;
        var command = new SqlCommand(string.Empty, connection);
        var storeList = ReportHelper.GetStores(ReportHelper.GetStoreNames(workbook), directionNames, workbook);
        foreach (var store in storeList)
        {
            count++;
            command.CommandText = $"""
                update Stores 
                set Rank = {store.Rank}
                from
                (select * from Stores where name like '{store.Name}') as Selected
                where Stores.Id = Selected.Id;
                """;
            await command.ExecuteNonQueryAsync();
            foreach (var direction in store.Directions)
            {
                count++;
                command.CommandText = $"""
                update EconomicDirections 
                set [Plan] = {direction.Plan},
                [Fact] = {direction.Fact},
                [Rank] = {direction.Rank}
                from
                (select * from EconomicDirections where StoreName like '{store.Name}' and Name like N'{direction.Name}') as Selected
                where EconomicDirections.Id = Selected.Id;
                """;
                await command.ExecuteNonQueryAsync();
            }
        }

        Console.WriteLine($"обновлено {count} записей");
    }

    public static async Task<string> GetReportStore(string storeName, string connectionString)
    {
        var result = string.Empty;
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"select [rank] from stores where [Name] = '{storeName}';", connection);
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
            $"select [Name], [Plan], [Fact], [Rank] from EconomicDirections where [StoreName] = '{storeName}';",
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
                    var rank = reader.GetInt32(3);
                    var persent = (double)fact / (double)plan;
                    if (rank != 0)
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