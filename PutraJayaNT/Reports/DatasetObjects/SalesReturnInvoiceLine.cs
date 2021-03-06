﻿namespace ECERP.Reports.DatasetObjects
{
    public class SalesReturnInvoiceLine
    {
        public int LineNumber { get; set; }

        public string ItemID { get; set; }

        public string ItemName { get; set; }

        public string Unit { get; set; }

        public string UnitName { get; set; }

        public  string QuantityPerUnit { get; set; }

        public string Quantity { get; set; }

        public int Units { get; set; }

        public int Pieces { get; set; }

        public decimal ReturnPrice { get; set; }

        public decimal Total { get; set; }
    }
}
