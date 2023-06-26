using Telegram.Bot.Types;
using User = TelegramBot.Entities.User.User;

namespace TelegramBot.States;

public abstract class Menu
{
    protected Menu()
    {
        //PrintStateMessage();
    }
    public abstract Task PrintStateMessage();
    public abstract Task NextMenu(MenuState menuState, Update update, User user);
}