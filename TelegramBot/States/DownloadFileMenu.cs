using System.Data.SqlClient;
using System.Diagnostics;
using Aspose.Cells;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;
using File = System.IO.File;
using User = TelegramBot.Services.User;

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

    public override async Task NextMenu(MenuState menuState, Update update, User user)
    {
        var connection = new SqlConnection(TelegramService.ConnectionString);
        await connection.OpenAsync();
        var command = new SqlCommand(string.Empty, connection);
        switch (update.Message.Type)
        {
            case MessageType.Text when update.Message.Text == "Главное Меню":
                command.CommandText = $"update users set UserState = {(int)UserState.MainMenu} where UserId = {user.Id};";
                await command.ExecuteNonQueryAsync();
                menuState.State = new MainMenu();
                await menuState.State.PrintStateMessage();
                break;
            case MessageType.Text:
                await TelegramService.SendMessage("Выполни инструкцию");
                break;
            case MessageType.Document:
                {
                    var sw = new Stopwatch();
                    sw.Restart();
                    await TelegramService.SendMessage("Обновляю данные, дождись скачивания данных...",
                        new ReplyKeyboardRemove());
                    var fileId = update.Message.Document.FileId;
                    var fileInfo = await TelegramService.Bot.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    var destinationFilePath = $"{Environment.CurrentDirectory}/economic.xlsx";
                    var sw1 = new Stopwatch();
                    sw1.Restart();
                    try
                    {
                        var fileStream = new FileStream(destinationFilePath, FileMode.OpenOrCreate);
                        await TelegramService.Bot.DownloadFileAsync(
                            filePath,
                            fileStream);
                        sw1.Stop();
                        Console.WriteLine($"Скачивание файла произошло успешно. Время скачивания {sw1.Elapsed}");
                        var workbook = new Workbook(fileStream);
                        await DatabaseReportService.UpdateDatabaseAsync(workbook);
                        fileStream.Close();
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e);
                    }
                    try
                    {
                        var fi = new FileInfo(destinationFilePath);
                        fi.Delete();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    sw.Stop();
                    Console.WriteLine($"На скачивание и обновление БД понадобилось {sw.Elapsed}");
                    command.CommandText = $"update users set UserState = {(int)UserState.MainMenu} where UserId = {user.Id};";
                    await command.ExecuteNonQueryAsync();
                    menuState.State = new MainMenu();
                    await menuState.State.PrintStateMessage();
                    break;
                }
        }
    }
}