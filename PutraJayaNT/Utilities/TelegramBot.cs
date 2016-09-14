namespace PutraJayaNT.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Models;
    using Models.Inventory;
    using Telegram.Bot.Types;
    using File = System.IO.File;

    public class TelegramBot
    {
        public static async void testApi()
        {
            List<Stock> stockFromDatabase;
            using (var context = UtilityMethods.createContext())
            {
                stockFromDatabase = context.Stocks.OrderBy(stock => stock.Item.Name).Include("Item").Where(stock => stock.Pieces <= 20).ToList();
            }

            var Bot = new Telegram.Bot.Api("229513906:AAH5-4dU6h_BnI20CpY_X0XAm4xB9xrnvdw");
            //var me = Bot.GetMe();
            //Bot.SendTextMessage(86237212, "test");

            //foreach (var stock in stockFromDatabase)
            //{
            //    Bot.SendTextMessage(86237212, $"{stock.Item.Name} - {stock.Pieces}");
            //}
            var offset = 0;

            while (true)
            {
                var updates = await Bot.GetUpdates(offset);

                foreach (var update in updates)
                {
                    if (update.Message.Type == MessageType.TextMessage)
                    {
                        await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                        await Task.Delay(2000);
                        var t = await Bot.SendTextMessage(update.Message.Chat.Id, update.Message.Text);
                        MessageBox.Show(@"Echo Message: {0}", update.Message.Chat.Id.ToString());
                    }

                    if (update.Message.Type == MessageType.PhotoMessage)
                    {
                        var file = await Bot.GetFile(update.Message.Photo.LastOrDefault()?.FileId);

                        MessageBox.Show(@"Received Photo: {0}", file.FilePath);

                        var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                        using (var profileImageStream = File.Open(filename, FileMode.Create))
                        {
                            await file.FileStream.CopyToAsync(profileImageStream);
                        }
                    }

                    offset = update.Id + 1;
                }

                await Task.Delay(1000);
            }
        }

        public static void AddTelegramNotification(DateTime when, string message)
        {
            var notification = new TelegramBotNotification {When = when, Message = message, Sent = false};
            using (var context = UtilityMethods.createContext())
            {
                context.TelegramBotNotifications.Add(notification);
                context.SaveChanges();
            }
        }
    }
}
