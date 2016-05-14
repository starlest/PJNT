namespace PutraJayaNT.ViewModels.Analysis
{
    using Item;
    using MVVMFramework;

    internal class SalesForecastMonthlyLineVM : ViewModelBase
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

        public bool IsJanTargetNotMet { get; set; }

        public bool IsFebTargetNotMet { get; set; }

        public bool IsMarTargetNotMet { get; set; }

        public bool IsAprTargetNotMet { get; set; }

        public bool IsMayTargetNotMet { get; set; }

        public bool IsJunTargetNotMet { get; set; }

        public bool IsJulTargetNotMet { get; set; }

        public bool IsAugTargetNotMet { get; set; }

        public bool IsSepTargetNotMet { get; set; }

        public bool IsOctTargetNotMet { get; set; }

        public bool IsNovTargetNotMet { get; set; }

        public bool IsDecTargetNotMet { get; set; }
    }
}
