using Telegram.Bot.Types;
using User = TelegramBot.Services.User;

namespace TelegramBot.States;

public class MenuState
{
    public Menu State { get; set; }

    public MenuState(Menu menu)
    {
        State = menu;
    }

    public void NextState(Update update, User user)
    {
        State.NextMenu(this, update, user);
    }

}