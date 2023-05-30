using Telegram.Bot.Types;

namespace TelegramBot.States;

public class MenuState
{
    public Menu State { get; set; }

    public MenuState(Menu menu)
    {
        State = menu;
    }

    public void NextState(Update update)
    {
        State.NextMenu(this, update);
    }

}