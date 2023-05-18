using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.States;

namespace TelegramBot;

public class ChooseStoreMenu : Menu
{

    private readonly string _title = "Выбери магазин";
    private readonly IReplyMarkup _markup = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>()
        {
            new List<KeyboardButton>()
            {
                new KeyboardButton("mainMenuButton")

            },
            new List<KeyboardButton>(){
                new KeyboardButton("ChooseStore2")

            },
            new List<KeyboardButton>(){
                new KeyboardButton("DownloadFiles3")
            }
        }
    );

    public override async Task PrintStateMessage()
    {
        await Program.Bot.SendTextMessageAsync(Program.ChatId, _title, replyMarkup: _markup);
    }

    public override Task NextMenu(MenuState menuState, Update update)
    {
        if (update.Message.Text == "mainMenuButton")
        {
            menuState.State = new MainMenu();
        }

        return Task.CompletedTask;
    }
}