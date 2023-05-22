using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace TelegramBot.States;

public class DownloadFileMenu : Menu
{
    private readonly string _title = "Меню загрузки файла, отправь файл сюда";

    private readonly IReplyMarkup _markup = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>
        {
            new()
            {
                new("mainMenuButton")
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
            case MessageType.Text when update.Message.Text == "mainMenuButton":
                menuState.State = new MainMenu();
                break;
            case MessageType.Text:
                await TelegramService.SendMessage("Выполни инструкцию");
                break;
            case MessageType.Document:
            {
                var fileId = update.Message.Document.FileId;
                const string destinationFilePath = "../economic.xlsx";
                await using Stream fileStream = File.Create(destinationFilePath);
                await TelegramService.SendMessage("Я запустил загрузку файла, дождись чтобы он скачался",new ReplyKeyboardRemove());
                menuState.State = new MainMenu();
                var tasks = async () =>
                {
                    await TelegramService.Bot.GetInfoAndDownloadFileAsync(
                        fileId,
                        fileStream);
                    await TelegramService.SendMessage("Файл скачался");
                };
                await tasks.Invoke();
                break;
            }
        }

        ReportService.Update();
    }
}