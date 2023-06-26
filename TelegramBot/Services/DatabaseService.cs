using Aspose.Cells;
using System.Data.SqlClient;
using System.Text;
using Telegram.Bot.Types;
using TelegramBot.Entities.User;
using User = TelegramBot.Entities.User.User;

namespace TelegramBot.Services;

public static class DatabaseService
{
    public static async Task UpdateReportData(Workbook workbook)
    {
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(GetReportUpdateCmdString(workbook), connection);
        command.ExecuteNonQuery();
        await connection.CloseAsync();
    }

    private static string GetReportUpdateCmdString(Workbook workbook)
    {
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
        return $"""
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
    }

    public static async Task UpdateUserStore(User user, string newStoreCode)
    {
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"update users set UserStoreCode = N'{newStoreCode}' where UserId = {user.Id};", connection);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    public static async Task UpdateUserState(User user, UserState userState)
    {
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"update users set UserState = {(int)userState} where UserId = {user.Id};", connection);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    public static async Task<string> GetUserStore(User user)
    {
        var result = string.Empty;
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"select UserStoreCode from users where UserId = {user.Id};", connection);
        var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) return result;
        while (await reader.ReadAsync())
        {
            result = reader.GetString(0);
        }
        await reader.CloseAsync();
        return result;
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
        var result = new StringBuilder();
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
                    var block = $"""
                        Выбранный магазин {storeName} 
                        Общий ранг магазина {rank}
                        Направление/План/Факт/Рейтинг

                        """;
                    result.Append(block);
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
                    var percent = fact / (double)plan;
                    var sb = new StringBuilder();
                    if (string.IsNullOrEmpty(rank.ToString()))
                    {
                        string block;
                        block = $"""
                        -----------------------------------
                        {name} / План / Факт / % - Рейтинг
                        {plan} / {fact} / {percent:P1} - {rank}

                        """;
                        result.Append(block);
                    }
                    else
                    {
                        string block;
                        block = $"""
                        -----------------------------------
                        {name} / План / Факт / % 
                        {plan} / {fact} / {percent:P1} 

                        """;
                        result.Append(block);
                    }
                }
            }
        }

        return result.ToString();
    }

    public static async Task<User> GetUser(Update update)
    {
        User result = null;
        var id = update.Message.From.Id;
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"select * from users where UserId = {id};", connection);
        var reader = await command.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                var state = reader.GetInt32(1);
                result = new User(id, state);
            }
            await reader.CloseAsync();
        }
        else
        {
            result = new User(id, (int)UserState.MainMenu);
            await reader.CloseAsync();
            command.CommandText = $"insert into users (UserId, UserState) values ({id}, {(int)UserState.MainMenu});";
            await command.ExecuteNonQueryAsync();
        }
        return result;

    }
}