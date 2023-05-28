using Aspose.Cells;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;
using File = System.IO.File;

namespace TelegramBot.States;

public class DownloadFileMenu : Menu
{
    private readonly string _title = "Меню загрузки файла, отправь файл сюда";

    private readonly IReplyMarkup _markup = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>
        {
            new()
            {
                new("Главное Меню")
            }
        }
    );

    public override async Task PrintStateMessage()
    {
        await TelegramService.SendMessage(_title, _markup);
    }

    public override async Task NextMenu(MenuState menuState, Update update)
    {
        switch (update.Message.Type)
        {
            case MessageType.Text when update.Message.Text == "Главное Меню":
                menuState.State = new MainMenu();
                break;
            case MessageType.Text:
                await TelegramService.SendMessage("Выполни инструкцию");
                break;
            case MessageType.Document:
                {
                    var fileId = update.Message.Document.FileId;
                    const string destinationFilePath = "../economic.xlsx";
                    await using (Stream fileStream = File.Create(destinationFilePath))
                    {

                        await TelegramService.SendMessage("Я запустил загрузку файла, дождись чтобы он скачался",
                            new ReplyKeyboardRemove());
                        var tasks = async () =>
                        {
                            await TelegramService.Bot.GetInfoAndDownloadFileAsync(fileId, fileStream);
                        };
                        await tasks.Invoke().ContinueWith((asd) =>
                        {
                            TelegramService.Workbook = new Workbook("../economic.xlsx");
                        });
                    }
                    await TelegramService.SendMessage($"Файл скачан");
                    await DatabaseService.UpdateDatabaseAsync(TelegramService.Workbook, TelegramService.ConnectionString);
                    menuState.State = new MainMenu();
                    break;
                }
        }

        ReportService.Update();
    }
}

