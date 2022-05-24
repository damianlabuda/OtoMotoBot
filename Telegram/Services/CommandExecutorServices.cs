using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Services
{
    public interface ICommandExecutorServices
    {
        Task Execute(Update update);
    }

    public class CommandExecutorServices : ICommandExecutorServices
    {
        private TelegramBotClient _telegramBotClient;

        public CommandExecutorServices(TelegramBot telegramBot)
        {
            _telegramBotClient = telegramBot.GetBot().Result;
        }

        public async Task Execute(Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                switch (update.Message?.Text)
                {
                    case "Hej":
                        Console.WriteLine("Napisal hej");
                        break;
                    case "Narazie":
                        Console.WriteLine("Napisal narazie");
                        break;
                    default:
                        //var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        //{
                        //    new InlineKeyboardButton("Test 1"){CallbackData = "test-1"},
                        //    new InlineKeyboardButton("Test 2"){CallbackData = "test-2"},
                        //    new InlineKeyboardButton("Test 3"){CallbackData = "test-3"}
                        //});
                        string text1 =
                            "Kanał służy do śledzenia nowo dodanych aukcji lub zmian cen już istniejących w serwisie otomoto.pl, Aby otrzymywać takie powiadomienia, dodaj za pomocą przycisku poniżej „Dodaj link” swój link wyszukiwania z interesującymi cię filtrami np. marka, model, przebieg, cena, rocznik itp.\r\n\r\nAby uzyskać swój link wyszukiwania, należy wejść na stronę otomoto.pl, następnie wybrać interesującą nas kategorie/markę/model + zaznaczyć niezbędne filtry, wyszukać aukcje i skopiować wygenerowany link z adresu przeglądarki.\r\n\r\nPoniżej znajduję się przykładowy link wyszukiwania, który sprawdza nowe aukcje/zmianę ceny dla: Volkswagen Passat od 2015 rok, Diesel, Automat. https://www.otomoto.pl/osobowe/volkswagen/passat/od-2015?search%5Bfilter_enum_gearbox%5D=automatic&search%5Bfilter_enum_fuel_type%5D=diesel&search%5Badvanced_search_expanded%5D=true";

                        string text = "kurwa";

                        ReplyKeyboardMarkup inlineKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "Pokaż moje linki", "Dodaj link wyszukiwania" },
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, text,
                            ParseMode.Markdown, replyMarkup: inlineKeyboard);
                        break;

                }
            }
        }
    }
}
