namespace PutraJayaNT.Reports
{
    class SalesInvoice
    {
        public string Customer { get; set; }

        public string Address { get; set; }

        public decimal InvoiceNumber { get; set; }

        public string Date { get; set; }

        public string DueDate { get; set; }

        public decimal InvoiceGrossTotal { get; set; }

        public decimal InvoiceDiscount { get; set; }

        public decimal InvoiceSalesExpense { get; set; }

        public decimal InvoiceNetTotal { get; set; }

        public string Notes { get; set; }
    }
}
