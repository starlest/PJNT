using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.ViewModels.Accounting;
using System;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for DailyCashFlowReportWindow.xaml
    /// </summary>
    public partial class DailyCashFlowReportWindow : ModernWindow
    {
        DailyCashFlowVM _vm;

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

            var endingBalance = _vm.BeginningBalance;
            foreach (var line in _vm.Lines)
            {
                var dr1 = dt1.NewRow();
                dr1["Account"] = line.LedgerAccount.Name;
                dr1["Documentation"] = line.Documentation;
                dr1["Description"] = line.Description;
                dr1["Debit"] = line.Amount > 0 ? string.Format("{0:N2}", line.Amount) : "";
                dr1["Credit"] = line.Amount < 0 ? string.Format("{0:N2}", -line.Amount) : "";
                dt1.Rows.Add(dr1);
            }

            var dt2 = new DataTable();
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dt2.Columns.Add(new DataColumn("BeginningBalance", typeof(string)));
            dt2.Columns.Add(new DataColumn("EndingBalance", typeof(string)));
            var dr2 = dt2.NewRow();
            dr2["Date"] = _vm.Date.ToString("dd-MM-yyyy");
            dr2["BeginningBalance"] = string.Format("{0:N2}",  _vm.BeginningBalance);
            dr2["EndingBalance"] = string.Format("{0:N2}", _vm.EndingBalance);
            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("DailyCashFlowReportLineDataset", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("DailyCashFlowReportDataset", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\DailyCashFlowReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
