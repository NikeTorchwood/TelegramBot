using System.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.States;

namespace TelegramBot.Services;

public static class TelegramService
{
    public static ITelegramBotClient? Bot { get; set; }
    private static readonly string token = ConfigurationManager.ConnectionStrings["token"].ConnectionString;
    private static long ChatId { get; set; }

    public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString;

    public static readonly List<string> directionList = new()
    {
        "SIM Продажи",
        "SIM",
        "Автоплатеж",
        "Sim АП",
        "ФС",
        "СТВ",
        "Цифровая Экосистема МТС",
        "Телефоны",
        "Прочие товары",
        "Дополнительные услуги",
        "Банковские карты"
    };

    private static MenuState? _menuState;
    public static string CurrentStoreCode = string.Empty;

    public static void Start()
    {
        Bot = new TelegramBotClient(token);
        _menuState = new MenuState(new MainMenu());
        Bot.StartReceiving(UpdateHandler, ErrorHandler);
    }

    private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        throw new NotImplementedException();
    }

    private static async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        if (update.Message != null)
            ChatId = update.Message.Chat.Id;
        _menuState.NextState(update);
    }

    public static async Task SendMessage(string text, IReplyMarkup markup = null)
    {
        await Bot.SendTextMessageAsync(ChatId, text, replyMarkup: markup);
    }
}