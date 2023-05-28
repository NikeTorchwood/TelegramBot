using Aspose.Cells;
using Telegram.Bot;
using TelegramBot.States;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Report;

namespace TelegramBot.Services
{
    public static class TelegramService
    {
        public static ITelegramBotClient? Bot { get; set; }
        private const string Token = "6181768808:AAGqAC9vKIa6YUPRPbJMZGvUcMUaWLrmVbE";
        private static long ChatId { get; set; }
        public static Workbook Workbook { get; set; }

        public static readonly string ConnectionString =
            "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ntwdc\\source\\repos\\TelegramBot\\TelegramBot\\TelegramBotDB.mdf;Integrated Security=True";

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
        public static Store CurrentStore = null;
        public static void Start()
        {
            Bot = new TelegramBotClient(Token);
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
            _menuState ??= new MenuState(new MainMenu());
            _menuState.NextState(update);
        }

        public static async Task SendMessage(string text, IReplyMarkup markup = null)
        {
            await Bot.SendTextMessageAsync(ChatId, text, replyMarkup: markup);
        }
    }


}
