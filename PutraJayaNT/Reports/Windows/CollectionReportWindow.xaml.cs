using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.ViewModels.Customers;
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
        ObservableCollection<SalesCollectionListLineVM>  _salesTransactions;

        public CollectionReportWindow(ObservableCollection<SalesCollectionListLineVM> salesTransactions)
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
            dt.Columns.Add(new DataColumn("CollectionSalesman", typeof(string)));
            dt.Columns.Add(new DataColumn("CollectionTotal", typeof(decimal)));

            foreach (var t in _salesTransactions)
            {
                DataRow dr = dt.NewRow();
                dr["ID"] = t.SalesTransactionID;
                dr["Customer"] = t.Customer.Name;
                dr["InvoiceNetTotal"] = t.Total;
                dr["InvoicePaid"] = t.Paid;
                dr["InvoiceRemaining"] = t.Total - t.Paid;
                dr["DueDate"] = t.DueDate.ToShortDateString();
                dr["CollectionSalesman"] = t.CollectionSalesman != null ? t.CollectionSalesman.Name : "";
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
