namespace ECRP.Reports.Windows
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Windows;
    using FirstFloor.ModernUI.Windows.Controls;
    using Microsoft.Reporting.WinForms;
    using Models.Sales;

    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesTransactionsReportWindow : ModernWindow
    {
        ObservableCollection<SalesTransaction>  _salesTransactions;

        public SalesTransactionsReportWindow(ObservableCollection<SalesTransaction> salesTransactions)
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
            dt.Columns.Add(new DataColumn("ID", typeof(string)));
            dt.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));

            foreach (var t in _salesTransactions)
            {
                DataRow dr = dt.NewRow();
                dr["Date"] = t.Date.ToShortDateString();
                dr["ID"] = t.SalesTransactionID;
                dr["Customer"] = t.Customer.Name;
                dr["InvoiceNetTotal"] = t.NetTotal;
                dt.Rows.Add(dr);
            }

            ReportDataSource reportDataSource = new ReportDataSource("InvoiceDataSet", dt);
 
            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesTransactionsReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
