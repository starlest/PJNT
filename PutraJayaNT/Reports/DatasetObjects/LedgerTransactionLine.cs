namespace ECRP.Reports.DatasetObjects
{
    internal class LedgerTransactionLine
    {
        public string Account { get; set; }

        public string Date { get; set; }

        public string Documentation { get; set; }

        public string Description { get; set; }

        public string Seq { get; set; }

        public decimal Amount { get; set; }

        public decimal Balance { get; set; }
    }
}
