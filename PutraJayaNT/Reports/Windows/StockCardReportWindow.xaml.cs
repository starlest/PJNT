using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.ViewModels.Customers;
using PutraJayaNT.ViewModels.Reports;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for StockCardReportWindow.xaml
    /// </summary>
    public partial class StockCardReportWindow : ModernWindow
    {
        StockBalancesReportVM _stockReport;

        public StockCardReportWindow(StockBalancesReportVM stockReport)
        {
            InitializeComponent();
            _stockReport = stockReport;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("Date", typeof(string)));
            dt1.Columns.Add(new DataColumn("Documentation", typeof(string)));
            dt1.Columns.Add(new DataColumn("Description", typeof(string)));
            dt1.Columns.Add(new DataColumn("CustomerSupplier", typeof(string)));
            dt1.Columns.Add(new DataColumn("InQuantity", typeof(string)));
            dt1.Columns.Add(new DataColumn("OutQuantity", typeof(string)));
            dt1.Columns.Add(new DataColumn("Balance", typeof(string)));

            dt2.Columns.Add(new DataColumn("Item", typeof(string)));
            dt2.Columns.Add(new DataColumn("BeginningBalance", typeof(string)));
            dt2.Columns.Add(new DataColumn("EndingBalance", typeof(string)));
            dt2.Columns.Add(new DataColumn("TotalIn", typeof(string)));
            dt2.Columns.Add(new DataColumn("TotalOut", typeof(string)));

            string endingBalance = "";

            foreach (var line in _stockReport.Lines)
            {
                DataRow dr1 = dt1.NewRow();
                dr1["Date"] = line.Date.ToString("dd-MM-yyyy");
                dr1["Documentation"] = line.Documentation;
                dr1["Description"] = line.Description;
                dr1["CustomerSupplier"] = line.CustomerSupplier;
                dr1["InQuantity"] = line.InQuantity;
                dr1["OutQuantity"] = line.OutQuantity;
                dr1["Balance"] = line.StringBalance;
                dt1.Rows.Add(dr1);

                endingBalance = line.StringBalance;
            }

            DataRow dr2 = dt2.NewRow();
            dr2["Item"] = _stockReport.SelectedProduct.Name;
            dr2["BeginningBalance"] = _stockReport.BeginningBalanceString;
            dr2["EndingBalance"] = endingBalance;
            dr2["TotalIn"] = _stockReport.TotalInString;
            dr2["TotalOut"] = _stockReport.TotalOutString;
            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("StockCardLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("StockCardDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\StockCardReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
