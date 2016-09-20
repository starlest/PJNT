namespace ECRP.Reports.DatasetObjects
{
    public class DetailedSalesTransactionLine
    {
        public string Date { get; set; }

        public int SalesTransactionID { get; set; }

        public string Customer { get; set; }

        public string ItemName { get; set; }

        public string ItemID { get; set; }

        public string Quantity { get; set; }

        public string UnitName { get; set; }

        public string QuantityPerUnit { get; set; }
    }
}
