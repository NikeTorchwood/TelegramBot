namespace TelegramBot.Entities.User;

public class User
{
    public long Id { get; set; }
    public UserState State { get; set; }

    public User(long id, int state)
    {
        Id = id;
        State = (UserState)state;
    }

}