namespace ECERP.Reports.DatasetObjects
{
    public class DetailedSalesTransactionLine
    {
        public string Date { get; set; }

        public int SalesTransactionID { get; set; }

        public string Customer { get; set; }

        public string Address { get; set; }

        public string ItemName { get; set; }

        public string ItemID { get; set; }

        public string Quantity { get; set; }

        public string UnitName { get; set; }

        public string QuantityPerUnit { get; set; }

        public decimal NetTotal { get; set; }

        public decimal Tax { get; set; }
    }
}
