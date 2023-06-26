using System.Data.SqlClient;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Entities.User;
using TelegramBot.Services;
using User = TelegramBot.Entities.User.User;

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

    public override async Task NextMenu(MenuState menuState, Update update, User user)
    {
        switch (update.Message.Text)
        {
            case "Выбрать магазин":
                await DatabaseService.UpdateUserState(user, UserState.ChooseStore);
                menuState.State = new ChooseStoreMenu();
                await menuState.State.PrintStateMessage();
                break;
            case "Загрузить отчет":
                await DatabaseService.UpdateUserState(user, UserState.DownloadFile);
                menuState.State = new DownloadFileMenu();
                await menuState.State.PrintStateMessage();
                break;
            case "Печатать отчет":
                await PrintReport(user);
                break;
            case "Инструкция":
                await PrintInstrucion();
                break;
            default:
                await TelegramService.SendMessage("не понял тебя, нажми еще раз");
                await PrintStateMessage();
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

    private static async Task PrintReport(User user)
    {
        var code = await DatabaseService.GetUserStore(user);
        if (string.IsNullOrEmpty(code))
        {
            await TelegramService.SendMessage("Магазин не выбран, выбери магазин");
        }
        else
        {
            var report = await DatabaseService.GetReportStore(code);
            await TelegramService.SendMessage(report);
        }
    }


}