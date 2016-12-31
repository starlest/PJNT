namespace ECERP.Services
{
    using System;
    using System.Linq;
    using System.Text;
    using Models;
    using Telegram.Bot;
    using Telegram.Bot.Types;
    using Utilities;

    public class TelegramService
    {
        public static void AddTelegramNotification(DateTime when, string message)
        {
            var notification = new TelegramBotNotification { When = when, Message = message, Sent = false };
            using (var context = UtilityMethods.createContext())
            {
                context.TelegramBotNotifications.Add(notification);
                context.SaveChanges();
            }
        }

        public static void CheckUpdates()
        {
            var Bot = new Api(UtilityMethods.GetTelegramKey());
            var offset = 0;
            var updates = Bot.GetUpdates(offset).Result;
            foreach (var update in updates)
            {
                if (update.Message.Type == MessageType.TextMessage)
                {
                    var messageText = update.Message.Text.ToLower();
                    if (messageText.StartsWith("/customer"))
                    {
                        var customerName = messageText.Substring(10);
                        SendCustomerReceivables(customerName);
                    }
                }
                offset = update.Id + 1;
            }
            Bot.GetUpdates(offset);
        }

        private static void SendCustomerReceivables(string customerName)
        {
            using (var context = UtilityMethods.createContext())
            {
                var message = new StringBuilder();
                var customer = context.Customers.FirstOrDefault(c => c.Name.Contains(customerName));
                if (customer == null) message.Append("Customer could not be found.");
                else
                {
                    var customerReceivables =
                        context.SalesTransactions.Where(
                            transaction =>
                                transaction.Customer.ID.Equals(customer.ID) &&
                                transaction.Paid < transaction.NetTotal);
                    message.Append($"{customer.Name}\n");
                    foreach (var receivable in customerReceivables)
                    {
                        var remainingAmount = receivable.NetTotal - receivable.Paid;
                        message.Append($"Date: {receivable.Date:dd/MM/yyyy}, Remaining: {remainingAmount:0#,##.00}\n");
                    }
                }
                AddTelegramNotification(DateTime.Now, message.ToString());
            }
        }

        public static void SendNotifications()
        {
            var Bot = new Api(UtilityMethods.GetTelegramKey());

            using (var context = UtilityMethods.createContext())
            {
                var unsentNotifications =
                    context.TelegramBotNotifications.Where(notification => !notification.Sent).ToList();
                if (unsentNotifications.Count == 0) return;

                foreach (var notification in unsentNotifications)
                {
                    // ReSharper disable once UnusedVariable
                    var result = Bot.SendTextMessage(UtilityMethods.GetTelegramChatID(),
                        $"{notification.When} - {notification.Message}").Result;
                    notification.Sent = true;
                    context.SaveChanges();
                }
            }
        }

        public static void CleanNotifications()
        {
            using (var context = UtilityMethods.createContext())
            {
                var sentNotifications =
                    context.TelegramBotNotifications.Where(notification => notification.Sent);
                context.TelegramBotNotifications.RemoveRange(sentNotifications);
                context.SaveChanges();
            }
        }
    }
}