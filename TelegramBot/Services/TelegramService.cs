using System.Configuration;
using System.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Entities.User;
using TelegramBot.States;
using User = TelegramBot.Entities.User.User;

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
            var user = await DatabaseService.GetUser(update);

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

    

    public static async Task SendMessage(string text, IReplyMarkup markup = null)
    {
        await Bot.SendTextMessageAsync(ChatId, text, replyMarkup: markup);
    }

}