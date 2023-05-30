using TelegramBot.Services;

namespace TelegramBot;

public class Program
{
    private static void Main()
    {

        TelegramService.Start();
        Console.WriteLine("Finished");
        Console.In.ReadLine();
    }
}