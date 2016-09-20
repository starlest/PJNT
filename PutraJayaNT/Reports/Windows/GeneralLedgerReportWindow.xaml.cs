namespace ECRP.Reports.Windows
{
    using System;
    using System.Data;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using ViewModels.Accounting;

    /// <summary>
    /// Interaction logic for GeneralLedgerReportWindow.xaml
    /// </summary>
    public partial class GeneralLedgerReportWindow
    {
        private readonly GeneralLedgerVM _vm;

        public GeneralLedgerReportWindow(GeneralLedgerVM vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dt1 = new DataTable();
            dt1.Columns.Add(new DataColumn("Date", typeof(string)));
            dt1.Columns.Add(new DataColumn("Account", typeof(string)));
            dt1.Columns.Add(new DataColumn("Documentation", typeof(string)));
            dt1.Columns.Add(new DataColumn("Description", typeof(string)));
            dt1.Columns.Add(new DataColumn("Seq", typeof(string)));
            dt1.Columns.Add(new DataColumn("Amount", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Balance", typeof(decimal)));

            foreach (var line in _vm.DisplayedTransactionLines)
            {
                var dr1 = dt1.NewRow();
                dr1["Date"] = line.Date.ToShortDateString();
                dr1["Account"] = line.LedgerAccount.Name;
                dr1["Documentation"] = line.Documentation;
                dr1["Description"] = line.Description;
                dr1["Seq"] = line.Seq;
                dr1["Amount"] = line.Amount;
                dr1["Balance"] = line.Balance;
                dt1.Rows.Add(dr1);
            }

            var dt2 = new DataTable();
            dt2.Columns.Add(new DataColumn("Account", typeof(string)));
            dt2.Columns.Add(new DataColumn("Year", typeof(int)));
            dt2.Columns.Add(new DataColumn("Month", typeof(int)));
            dt2.Columns.Add(new DataColumn("BeginningBalance", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("EndingBalance", typeof(decimal)));
            var dr2 = dt2.NewRow();
            dr2["Account"] = _vm.SelectedAccount.Name;
            dr2["Year"] = _vm.SelectedYear;
            dr2["Month"] = _vm.SelectedMonth;
            dr2["BeginningBalance"] = _vm.SelectedBeginningBalance;
            dr2["EndingBalance"] = _vm.SelectedEndingBalance;
            dt2.Rows.Add(dr2);

            var reportDataSource1 = new ReportDataSource("LedgerTransactionLineDataset", dt1);
            var reportDataSource2 = new ReportDataSource("GeneralLedgerReportDataset", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\GeneralLedgerReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
