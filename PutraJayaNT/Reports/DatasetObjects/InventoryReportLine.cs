namespace ECERP.Reports.DatasetObjects
{
    internal class InventoryReportLine
    {
        public string ItemID { get; set; }

        public string Item { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal SalesPrice { get; set; }

        public string Unit { get; set; }

        public string Quantity { get; set; }

        public decimal Value { get; set; }
    }
}
