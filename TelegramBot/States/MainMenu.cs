using System.Threading;
using Aspose.Cells;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;

namespace TelegramBot.States;

public class MainMenu : Menu
{

    private readonly string _title = "Главное меню";
    private readonly IReplyMarkup _markup = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>()
        {
            new List<KeyboardButton>()
            {
                new KeyboardButton("Печатать отчет"),

            },
            new List<KeyboardButton>(){
                new KeyboardButton("Выбрать магазин")

            },
            new List<KeyboardButton>(){
                new KeyboardButton("Загрузить отчет")
            }
        }
    );
    public override async Task PrintStateMessage()
    {
        await TelegramService.SendMessage(_title, _markup);
    }

    public override async Task NextMenu(MenuState menuState, Update update)
    {
        try
        {
            TelegramService.Workbook = new Workbook("../economic.xlsx");
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("e");
        }
        switch (update.Message.Text)
        {
            case "Выбрать магазин":
                menuState.State = new ChooseStoreMenu();
                break;
            case "Загрузить отчет":
                menuState.State = new DownloadFileMenu();
                break;
            case "Печатать отчет":
                await TelegramService.SendMessage("Попал в метод печати отчета");
                await PrintReport();
                break;
            default:
                await TelegramService.SendMessage("не понял тебя, нажми еще раз");
                menuState.State = new MainMenu();
                break;
        }
    }

    private static async Task PrintReport()
    {
        if (TelegramService.CurrentStore == null || TelegramService.Workbook == null)
        {
            await TelegramService.SendMessage("Магазин не выбран, выбери магазин");
        }
        else
        {
            var report = await DatabaseService.GetReportStore(TelegramService.CurrentStore.Name, TelegramService.ConnectionString);
            await TelegramService.SendMessage(report);
        }
    }
}