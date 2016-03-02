using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for OverallSalesReportWindow.xaml
    /// </summary>
    public partial class OverallSalesReportWindow : ModernWindow
    {
        ObservableCollection<SalesTransactionMultiPurposeVM>  _salesTransactions;

        public OverallSalesReportWindow(ObservableCollection<SalesTransactionMultiPurposeVM> salesTransactions)
        {
            InitializeComponent();
            _salesTransactions = salesTransactions;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("DueDate", typeof(string)));
            dt.Columns.Add(new DataColumn("ID", typeof(string)));
            dt.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            dt.Columns.Add(new DataColumn("InvoiceRemaining", typeof(decimal)));

            foreach (var t in _salesTransactions)
            {
                DataRow dr = dt.NewRow();
                dr["Date"] = t.When.ToShortDateString();
                dr["DueDate"] = t.DueDate.ToShortDateString();
                dr["ID"] = t.SalesTransactionID;
                dr["Customer"] = t.Customer.Name;
                dr["InvoiceNetTotal"] = t.Total;
                dr["InvoiceRemaining"] = t.Total - t.Paid;
                dt.Rows.Add(dr);
            }

            ReportDataSource reportDataSource = new ReportDataSource("InvoiceDataSet", dt);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\OverallSalesReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
