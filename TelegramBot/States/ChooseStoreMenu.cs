using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.States;

namespace TelegramBot;

public class ChooseStoreMenu : Menu
{
    private readonly string _title = "Выбери магазин";
    private IReplyMarkup _markup;

    private static async Task<IReplyMarkup> CreateMarkup()
    {
        if (ReportService.StoreList == null)
        {
            await TelegramService.SendMessage("Файл пустой нужно загрузить файл");
            return new ReplyKeyboardMarkup(new List<KeyboardButton>
            {
                new("DownloadFileMenu"),
                new("mainMenuButton")
            });
        }

        var keyboard = new List<List<KeyboardButton>>();
        var rowButtons = new List<KeyboardButton>();
        for (var i = 0; i < ReportService.StoreList.Count; i++)
            if (i % 4 == 0)
            {
                rowButtons = new List<KeyboardButton> { new(ReportService.StoreList[i].Code) };
                keyboard.Add(rowButtons);
            }
            else
            {
                rowButtons.Add(new KeyboardButton(ReportService.StoreList[i].Code));
            }

        keyboard.Add(new List<KeyboardButton>
        {
            new("mainMenuButton")
        });

        return new ReplyKeyboardMarkup(keyboard);
    }

    public override async Task PrintStateMessage()
    {
        _markup = await CreateMarkup();
        await TelegramService.SendMessage(_title, _markup);
    }

    public override async Task NextMenu(MenuState menuState, Update update)
    {
        if (ReportService.StoreList != null)
        {
            foreach (var store in ReportService.StoreList.Where(store => store.Code == update.Message.Text))
            {
                ReportService.CurrentStore = store;
                Console.WriteLine(ReportService.CurrentStore);
                await TelegramService.SendMessage($"Был выбран магазин{ReportService.CurrentStore.Code}");
                menuState.State = new MainMenu();
            }

        }
        menuState.State = update.Message.Text switch
        {
            "mainMenuButton" => new MainMenu(),
            "DownloadFileMenu" => new DownloadFileMenu(),
            _ => menuState.State
        };
    }
}