namespace PutraJayaNT.Reports.DatasetObjects
{
    internal class GeneralLedgerReport
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public string Account { get; set; }

        public decimal BeginningBalance { get; set; }

        public decimal EndingBalance { get; set; }
    }
}
