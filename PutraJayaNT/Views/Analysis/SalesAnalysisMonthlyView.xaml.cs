namespace PutraJayaNT.Views.Analysis
{
    using ViewModels.Analysis;

    /// <summary>
    /// Interaction logic for SalesAnalysisMonthlyView.xaml
    /// </summary>
    public partial class SalesAnalysisMonthlyView
    {
        public SalesAnalysisMonthlyView()
        {
            InitializeComponent();
            var vm = new SalesAnalysisMonthlyVM();
            DataContext = vm;
        }
    }
}
