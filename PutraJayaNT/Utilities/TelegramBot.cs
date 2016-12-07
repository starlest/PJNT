namespace ECRP.Utilities
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using Models;
    using Telegram.Bot;
    using Telegram.Bot.Types;

    public class TelegramBot
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
            var Bot = new Api("229513906:AAH5-4dU6h_BnI20CpY_X0XAm4xB9xrnvdw");
            var offset = 0;
            var updates = Bot.GetUpdates(offset).Result;
            var selectedServer = Application.Current.Resources[Constants.SELECTEDSERVER].ToString().ToLower();
            foreach (var update in updates)
            {
                if (update.Message.Type == MessageType.TextMessage)
                {
                    var messageText = update.Message.Text.ToLower();
                    if (messageText.StartsWith("/customer") && messageText.EndsWith(selectedServer))
                    {
                        var customerName = messageText.Substring(10);
                        customerName = customerName.Substring(0, customerName.Length - selectedServer.Length - 1);
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
                    var serverName = Application.Current.Resources[Constants.SELECTEDSERVER] as string;
                    message.Append($"\n{serverName} --- {customer.Name}\n");
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
            var Bot = new Api("229513906:AAH5-4dU6h_BnI20CpY_X0XAm4xB9xrnvdw");

            using (var context = UtilityMethods.createContext())
            {
                var unsentNotifications =
                    context.TelegramBotNotifications.Where(notification => !notification.Sent).ToList();
                if (unsentNotifications.Count == 0) return;

                foreach (var notification in unsentNotifications)
                {
                    // ReSharper disable once UnusedVariable
                    var result = Bot.SendTextMessage(-104676249,
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