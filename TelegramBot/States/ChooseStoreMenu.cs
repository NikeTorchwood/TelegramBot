using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Report;
using TelegramBot.Services;
using TelegramBot.States;

namespace TelegramBot;

public class ChooseStoreMenu : Menu
{
    private readonly string _title = "Выбери магазин";
    private IReplyMarkup _markup;
    private static List<string> _storeList = new();

    private static async Task<IReplyMarkup> CreateMarkup()
    {
        if (TelegramService.Workbook == null)
        {
            await TelegramService.SendMessage("Файл пустой нужно загрузить файл");
            return new ReplyKeyboardMarkup(new List<KeyboardButton>
            {
                new("Загрузить отчет"),
                new("Главное Меню")
            });
        }
        else
        {

            _storeList = ReportHelper.GetStoreNames(TelegramService.Workbook);

            var keyboard = new List<List<KeyboardButton>>();
            var rowButtons = new List<KeyboardButton>();
            for (var i = 0; i < _storeList.Count; i++)
                if (i % 4 == 0)
                {
                    rowButtons = new List<KeyboardButton> { new(_storeList[i]) };
                    keyboard.Add(rowButtons);
                }
                else
                {
                    rowButtons.Add(new KeyboardButton(_storeList[i]));
                }

            keyboard.Add(new List<KeyboardButton>
        {
            new("Главное Меню")
        });

            return new ReplyKeyboardMarkup(keyboard);
        }
    }

    public override async Task PrintStateMessage()
    {
        _markup = await CreateMarkup();
        await TelegramService.SendMessage(_title, _markup);
    }

    public override async Task NextMenu(MenuState menuState, Update update)
    {
        if (_storeList != null)
        {
            foreach (var store in _storeList.Where(store => update.Message.Text == store))
            {
                TelegramService.CurrentStore =
                    new Store(store, TelegramService.directionList, TelegramService.Workbook);
                await TelegramService.SendMessage($"Был выбран магазин {store}");
            }

            menuState.State = new MainMenu();
        }
        else
        {
            menuState.State = update.Message.Text switch
            {
                "Главное Меню" => new MainMenu(),
                "Загрузить отчет" => new DownloadFileMenu(),
                _ => menuState.State
            };
        }
    }
}