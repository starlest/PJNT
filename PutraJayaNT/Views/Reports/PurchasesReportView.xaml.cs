﻿namespace ECERP.Views.Reports
{
    using System.Windows.Controls;
    using ViewModels.Reports;

    /// <summary>
    /// Interaction logic for PurchasesReportView.xaml
    /// </summary>
    public partial class PurchasesReportView : UserControl
    {
        //PurchasesReportVM vm;

        public PurchasesReportView()
        {
            InitializeComponent();
            var vm = new PurchasesReportVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
