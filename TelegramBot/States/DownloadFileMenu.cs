using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.States;

public class DownloadFileMenu : Menu
{
    private readonly string _title = "Меню загрузки файла, отправь файл сюда";
    private readonly IReplyMarkup _markup = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>()
        {
            new List<KeyboardButton>()
            {
                new KeyboardButton("mainMenuButton")

            },
        }
    );
    public override async Task PrintStateMessage()
    {
        await Program.Bot.SendTextMessageAsync(Program.ChatId, _title, replyMarkup: _markup);
    }

    public override async Task NextMenu(MenuState menuState, Update update)
    {
        if (update.Message.Type == MessageType.Text)
        {
            if (update.Message.Text == "mainMenuButton")
            {
                menuState.State = new MainMenu();
            }
        }
        else if (update.Message.Type == MessageType.Document)
        {
            var fileId = update.Message.Document.FileId;
            const string destinationFilePath = "../economic.xlsx";

            await using Stream fileStream = System.IO.File.Create(destinationFilePath);
            var file = await Program.Bot.GetInfoAndDownloadFileAsync(
                fileId: fileId,
                destination: fileStream);
            //if (file.IsCompleted)
            //{
            Console.WriteLine("file downloaded");
            //await Program.Bot.SendTextMessageAsync(Program.ChatId, "Файл отчета загружен. Возвращаю в главное меню");
            menuState.State = new MainMenu();
            //}           
        }
        else
        {
            await Program.Bot.SendTextMessageAsync(Program.ChatId, "Выполни инструкцию");
        }
    }
}