using System.Data.SqlClient;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;
using User = TelegramBot.Services.User;

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
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(string.Empty, connection);
        switch (update.Message.Text)
        {
            case "Выбрать магазин":
                command.CommandText = $"update users set UserState = {(int)UserState.ChooseStore} where UserId = {user.Id};";
                await command.ExecuteNonQueryAsync();
                menuState.State = new ChooseStoreMenu();
                await menuState.State.PrintStateMessage();
                break;
            case "Загрузить отчет":
                command.CommandText = $"update users set UserState = {(int)UserState.DownloadFile} where UserId = {user.Id};";
                await command.ExecuteNonQueryAsync();
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
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand($"select UserStoreCode from users where UserId = {user.Id};", connection);
        var reader = await command.ExecuteReaderAsync();
        if (reader.HasRows)
        {
            var code = string.Empty;
            while (await reader.ReadAsync())
            {
                code = reader.GetString(0);
            }
            await reader.CloseAsync();
            if (string.IsNullOrEmpty(code))
            {
                await TelegramService.SendMessage("Магазин не выбран, выбери магазин");
            }
            else
            {
                var report = await DatabaseReportService.GetReportStore(code);
                await TelegramService.SendMessage(report);
            }
        }
    }


}