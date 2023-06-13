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
                new KeyboardButton("Выбрать магазин")
            },
            new List<KeyboardButton>(){
                new KeyboardButton("Инструкция")
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
        switch (update.Message.Text)
        {
            case "Выбрать магазин":
                menuState.State = new ChooseStoreMenu();
                break;
            case "Загрузить отчет":
                menuState.State = new DownloadFileMenu();
                break;
            case "Печатать отчет":
                await PrintReport();
                break;
            case "Инструкция":
                await PrintInstrucion();
                break;
            default:
                await TelegramService.SendMessage("не понял тебя, нажми еще раз");
                menuState.State = new MainMenu();
                break;
        }
    }

    private static async Task PrintInstrucion()
    {
        const string instruction = $"""
            Привет, здесь инструкция по использованию
            Бот может печатать отчет по магазину, основывается он только на тех данных, которые мы туда загрузили.
            Сейчас бот воспринимает только одного пользователя, в ближайшее время отладим поддержку нескольких пользователей.
            Если у тебя он печатает отчет не по твоему магазину - выбери заного свой магазин и повтори
          
            Немного о кнопочках
            => Печать отчета: Печатает отчет по выбранному магазину;
            => Выбрать магазин: После загрузки файла берет актуальный список магазинов и выбирает магазин, который можешь выбрать;
            => Загрузить отчет: Меню куда мы скидываем детализацию. !Важно! Если ты отправишь что-то кроме детализации продаж - то скорей всего бот сломается, я хз я не проверял);

            Хорошего пользования!
            """;
        await TelegramService.SendMessage(instruction);
    }

    private static async Task PrintReport()
    {
        if (string.IsNullOrEmpty(TelegramService.CurrentStoreCode))
        {
            await TelegramService.SendMessage("Магазин не выбран, выбери магазин");
        }
        else
        {
            var report = await DatabaseReportService.GetReportStore(TelegramService.CurrentStoreCode);
            await TelegramService.SendMessage(report);
        }

    }


}