using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.States;

namespace TelegramBot.Services;

public static class TelegramService
{
    public static ITelegramBotClient? Bot { get; set; }
    private static readonly string token = ConfigurationManager.ConnectionStrings["debugToken"].ConnectionString;
    private static long ChatId { get; set; }
    public static string ConnectionString { get; } = ConfigurationManager.ConnectionStrings["debugDB"].ConnectionString;
    private static MenuState? _menuState;

    public static void Start()
    {
        Bot = new TelegramBotClient(token);
        Bot.StartReceiving(UpdateHandler, ErrorHandler);
    }

    private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        throw new NotImplementedException();
    }

    private static async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        if (update.Message != null)
        {
            ChatId = update.Message.Chat.Id;
            var user = await GetUser(update);

            _menuState = user.State switch
            {
                UserState.MainMenu => new MenuState(new MainMenu()),
                UserState.ChooseStore => new MenuState(new ChooseStoreMenu()),
                UserState.DownloadFile => new MenuState(new DownloadFileMenu()),
                _ => _menuState
            };
            _menuState.NextState(update, user);
        }
    }

    private static async Task<User> GetUser(Update update)
    {
        User result = null;
        var id = update.Message.From.Id;
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"select * from users where UserId = {id};", connection);
        var reader = await command.ExecuteReaderAsync();
        if (reader == null)
        {
            Console.WriteLine("reader is null");
        }
        else
        {
            Console.WriteLine("reader isn`t null");
        }
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

    public static async Task SendMessage(string text, IReplyMarkup markup = null)
    {
        await Bot.SendTextMessageAsync(ChatId, text, replyMarkup: markup);
    }

}
public class User
{
    public long Id { get; set; }
    public UserState State { get; set; }

    public User(long id, int state)
    {
        Id = id;
        State = (UserState)state;
    }
}

public enum UserState
{
    MainMenu,
    ChooseStore,
    DownloadFile
}
