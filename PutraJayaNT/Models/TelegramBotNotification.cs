namespace PutraJayaNT.Models
{
    using System;

    public class TelegramBotNotification
    {
        public int ID { get; set; }

        public DateTime When { get; set; }

        public string Message { get; set; }

        public bool Sent { get; set; }
    }
}
