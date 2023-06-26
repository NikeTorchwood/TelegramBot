using System.Data.SqlClient;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Entities.User;
using TelegramBot.Services;
using User = TelegramBot.Entities.User.User;

namespace TelegramBot.States;

public class ChooseStoreMenu : Menu
{
    private readonly string _title = "Выбери магазин";
    private IReplyMarkup _markup;
    private static List<string?> _storeList = new();

    private static async Task<IReplyMarkup> CreateMarkup()
    {
        try
        {
            _storeList = await DatabaseService.GetStoreList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        if (_storeList == null)
        {
            await TelegramService.SendMessage("Файл пустой нужно загрузить файл");
            return new ReplyKeyboardMarkup(new List<KeyboardButton>
            {
                new("Загрузить отчет"),
                new("Главное Меню")
            });
        }

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

    public override async Task PrintStateMessage()
    {
        _markup = await CreateMarkup();
        await TelegramService.SendMessage(_title, _markup);
    }

    public override async Task NextMenu(MenuState menuState, Update update, User user)
    {
        if (_storeList != null)
        {
            foreach (var store in _storeList.Where(store => update.Message.Text == store))
            {
                await TelegramService.SendMessage($"Был выбран магазин {store}");
                await DatabaseService.UpdateUserStore(user, store);
            }

            await DatabaseService.UpdateUserState(user, UserState.MainMenu);
            menuState.State = new MainMenu();
            await menuState.State.PrintStateMessage();
        }
        else
        {
            switch (update.Message.Text)
            {
                case "Главное Меню":
                    await DatabaseService.UpdateUserState(user, UserState.MainMenu);
                    menuState.State = new MainMenu();
                    await menuState.State.PrintStateMessage();
                    break;
                case "Загрузить отчет":
                    await DatabaseService.UpdateUserState(user, UserState.DownloadFile);
                    menuState.State = new DownloadFileMenu();
                    await menuState.State.PrintStateMessage();
                    break;
            }
        }
    }
}