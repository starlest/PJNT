namespace ECRP.Reports.Windows
{
    using System;
    using System.Data;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using ViewModels.Accounting;

    /// <summary>
    /// Interaction logic for DailyCashFlowReportWindow.xaml
    /// </summary>
    public partial class DailyCashFlowReportWindow
    {
        private readonly DailyCashFlowVM _vm;

        public DailyCashFlowReportWindow(DailyCashFlowVM vm)
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
            dt1.Columns.Add(new DataColumn("Account", typeof(string)));
            dt1.Columns.Add(new DataColumn("Documentation", typeof(string)));
            dt1.Columns.Add(new DataColumn("Description", typeof(string)));
            dt1.Columns.Add(new DataColumn("Debit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Credit", typeof(string)));

            foreach (var line in _vm.DisplayedLines)
            {
                var dr1 = dt1.NewRow();
                dr1["Account"] = line.LedgerAccount.Name;
                dr1["Documentation"] = line.Documentation;
                dr1["Description"] = line.Description;
                dr1["Debit"] = line.Amount > 0 ? $"{line.Amount:N2}" : "";
                dr1["Credit"] = line.Amount < 0 ? $"{-line.Amount:N2}" : "";
                dt1.Rows.Add(dr1);
            }

            var dt2 = new DataTable();
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dt2.Columns.Add(new DataColumn("BeginningBalance", typeof(string)));
            dt2.Columns.Add(new DataColumn("EndingBalance", typeof(string)));
            var dr2 = dt2.NewRow();
            dr2["Date"] = _vm.Date.ToString("dd-MM-yyyy");
            dr2["BeginningBalance"] = $"{_vm.BeginningBalance:N2}";
            dr2["EndingBalance"] = $"{_vm.EndingBalance:N2}";
            dt2.Rows.Add(dr2);

            var reportDataSource1 = new ReportDataSource("DailyCashFlowReportLineDataset", dt1);
            var reportDataSource2 = new ReportDataSource("DailyCashFlowReportDataset", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\DailyCashFlowReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
