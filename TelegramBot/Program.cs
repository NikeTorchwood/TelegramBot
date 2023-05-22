using Aspose.Cells;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.States;

namespace TelegramBot
{
    public class Program
    {
        private static void Main()
        {

            TelegramService.Start();
            Console.WriteLine("Finished");
            Console.ReadKey();
        }

    }

   
}
