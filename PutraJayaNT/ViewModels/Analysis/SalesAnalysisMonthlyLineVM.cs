namespace ECERP.ViewModels.Analysis
{
    using Item;

    internal class SalesAnalysisMonthlyLineVM
    {
        public ItemVM Item { get; set; }

        public string Jan { get; set; }

        public string Feb { get; set; }

        public string Mar { get; set; }

        public string Apr { get; set; }

        public string May { get; set; }

        public string Jun { get; set; }

        public string Jul { get; set; }

        public string Aug { get; set; }

        public string Sep { get; set; }

        public string Oct { get; set; }

        public string Nov { get; set; }

        public string Dec { get; set; }

        public bool IsJanSalesDown { get; set; }

        public bool IsFebSalesDown { get; set; }

        public bool IsMarSalesDown { get; set; }

        public bool IsAprSalesDown { get; set; }

        public bool IsMaySalesDown { get; set; }

        public bool IsJunSalesDown { get; set; }

        public bool IsJulSalesDown { get; set; }

        public bool IsAugSalesDown { get; set; }

        public bool IsSepSalesDown { get; set; }

        public bool IsOctSalesDown { get; set; }

        public bool IsNovSalesDown { get; set; }

        public bool IsDecSalesDown { get; set; }
    }
}
