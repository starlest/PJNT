namespace PutraJayaNT.Reports
{
    class SalesInvoice
    {
        public string ID { get; set; }

        public string Customer { get; set; }

        public string Warehouse { get; set; }

        public string Address { get; set; }

        public string CollectionSalesman { get; set; }

        public decimal InvoiceNumber { get; set; }

        public string Date { get; set; }

        public string DueDate { get; set; }

        public decimal InvoiceGrossTotal { get; set; }

        public decimal InvoiceDiscount { get; set; }

        public decimal InvoiceSalesExpense { get; set; }

        public string InvoiceNetTotal { get; set; }

        public string InvoicePaid { get; set; }

        public decimal InvoiceRemaining { get; set; }

        public string Notes { get; set; }

        public decimal CollectionTotal { get; set; }
    }
}
