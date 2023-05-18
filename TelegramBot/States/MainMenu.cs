﻿using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.States;

public class MainMenu : Menu
{

    private readonly string _title = "Главное меню";
    private readonly IReplyMarkup _markup = new ReplyKeyboardMarkup(new List<List<KeyboardButton>>()
        {
            new List<KeyboardButton>()
            {
                new KeyboardButton("mainMenuButton")

            },
            new List<KeyboardButton>(){
                new KeyboardButton("ChooseStore")

            },
            new List<KeyboardButton>(){
                new KeyboardButton("DownloadFiles")
            }
        }
    );

    public override async Task PrintStateMessage()
    {
        await Program.Bot.SendTextMessageAsync(Program.ChatId, _title, replyMarkup: _markup);
    }

    public override Task NextMenu(MenuState menuState, Update update)
    {
        if (update.Message.Text == "ChooseStore")
        {
            menuState.State = new ChooseStoreMenu();
        }

        if (update.Message.Text == "DownloadFiles")
        {
            menuState.State = new DownloadFileMenu();
        }

        return Task.CompletedTask;
    }
}