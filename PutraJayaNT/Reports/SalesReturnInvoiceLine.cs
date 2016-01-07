namespace PutraJayaNT.Reports
{
    public class SalesReturnInvoiceLine
    {
        public int LineNumber { get; set; }

        public string ItemID { get; set; }

        public string ItemName { get; set; }

        public string Unit { get; set; }

        public int Units { get; set; }

        public int Pieces { get; set; }

        public decimal SalesPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }
    }
}
