using Aspose.Cells;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.States;

namespace TelegramBot
{
    public class Program
    {
        public static ITelegramBotClient? Bot { get; set; }
        private const string Token = "6181768808:AAGqAC9vKIa6YUPRPbJMZGvUcMUaWLrmVbE";
        public static long ChatId { get; set; }
        private static MenuState? _menuState;
        private static void Main()
        {
            Bot = new TelegramBotClient(Token);
            Bot.StartReceiving(UpdateHandler, ErrorHandler);
            Console.WriteLine("Finished");
            Console.ReadKey();
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
    }
}
