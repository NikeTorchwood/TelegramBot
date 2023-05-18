using Telegram.Bot.Types;

namespace TelegramBot.States;

public abstract class Menu
{
    protected Menu()
    {
        PrintStateMessage();
    }
    public abstract Task PrintStateMessage();
    public abstract Task NextMenu(MenuState menuState, Update update);
}