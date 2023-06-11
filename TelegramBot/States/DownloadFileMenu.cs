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
                new KeyboardButton("Главное Меню")
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
                    await TelegramService.SendMessage("Обновляю данные, дождись скачивания данных...",
                        new ReplyKeyboardRemove());
                    var fileId = update.Message.Document.FileId;
                    var fileInfo = await TelegramService.Bot.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    var destinationFilePath = $"{Environment.CurrentDirectory}/economic.xlsx";
                    Console.WriteLine($"Путь скачивания файла: {destinationFilePath}");
                    try
                    {
                        var fileStream = new FileStream(destinationFilePath, FileMode.OpenOrCreate);
                        await TelegramService.Bot.DownloadFileAsync(
                            filePath,
                            fileStream);
                        var workbook = new Workbook(fileStream);
                        await DatabaseService.UpdateDatabaseAsync(workbook, TelegramService.ConnectionString);
                        fileStream.Close();
                        Console.WriteLine("Скачивание файла произошло успешно");
                    }
                    catch (Exception e)
                    {
                        
                        Console.WriteLine(e);
                    }
                    try
                    {
                        var fi = new FileInfo(destinationFilePath);
                        fi.Delete();
                        Console.WriteLine("Удаление файла произошло успешно");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    menuState.State = new MainMenu();
                    break;
                }
        }
    }
}