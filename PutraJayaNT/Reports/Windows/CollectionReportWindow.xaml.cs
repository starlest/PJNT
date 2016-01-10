using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.Models.Sales;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class CollectionReportWindow : ModernWindow
    {
        ObservableCollection<SalesTransaction>  _salesTransactions;

        public CollectionReportWindow(ObservableCollection<SalesTransaction> salesTransactions)
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

            dt.Columns.Add(new DataColumn("ID", typeof(string)));
            dt.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            dt.Columns.Add(new DataColumn("InvoicePaid", typeof(decimal)));
            dt.Columns.Add(new DataColumn("InvoiceRemaining", typeof(decimal)));
            dt.Columns.Add(new DataColumn("DueDate", typeof(string)));

            foreach (var t in _salesTransactions)
            {
                DataRow dr = dt.NewRow();
                dr["ID"] = t.SalesTransactionID;
                dr["Customer"] = t.Customer.Name;
                dr["InvoiceNetTotal"] = t.Total;
                dr["InvoicePaid"] = t.Paid;
                dr["InvoiceRemaining"] = t.Total - t.Paid;
                dr["DueDate"] = t.DueDate.ToShortDateString();
                dt.Rows.Add(dr);
            }

            ReportDataSource reportDataSource = new ReportDataSource("InvoiceDataSet", dt);
 
            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\CollectionReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
