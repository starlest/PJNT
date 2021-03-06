﻿namespace ECERP.Views.Accounting
{
    using System.Windows.Controls;
    using ViewModels.Accounting;

    /// <summary>
    /// Interaction logic for GeneralLedgerView.xaml
    /// </summary>
    public partial class GeneralLedgerView : UserControl
    {
        public GeneralLedgerView()
        {
            InitializeComponent();
            var vm = new GeneralLedgerVM();
            DataContext = vm;
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
