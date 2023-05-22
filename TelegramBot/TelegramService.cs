using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot;
using TelegramBot.States;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public static class TelegramService
    {
        public static ITelegramBotClient? Bot { get; set; }
        public static string User;
        private const string Token = "6181768808:AAGqAC9vKIa6YUPRPbJMZGvUcMUaWLrmVbE";
        private static long ChatId { get; set; }
        private static MenuState? _menuState;

        public static void Start()
        {
            Bot = new TelegramBotClient(Token);
            Bot.StartReceiving(UpdateHandler, ErrorHandler);
        }
        private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private static Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            User = update.Message.Chat.Username;
            if (update.Message != null)
                ChatId = update.Message.Chat.Id;
            _menuState ??= new MenuState(new MainMenu());
            _menuState.NextState(update);
            return Task.CompletedTask;
        }

        public static async Task SendMessage(string text, IReplyMarkup markup = null)
        {
            await Bot.SendTextMessageAsync(ChatId, text, replyMarkup:markup);
        }
    }


}
