namespace ECERP.Reports.Windows
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using ViewModels.Sales;

    /// <summary>
    /// Interaction logic for OverallSalesReportWindow.xaml
    /// </summary>
    public partial class OverallSalesReportWindow
    {
        private readonly ObservableCollection<SalesTransactionVM>  _salesTransactions;

        public OverallSalesReportWindow(ObservableCollection<SalesTransactionVM> salesTransactions)
        {
            InitializeComponent();
            _salesTransactions = salesTransactions;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dt = new DataTable();

            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("DueDate", typeof(string)));
            dt.Columns.Add(new DataColumn("ID", typeof(string)));
            dt.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            dt.Columns.Add(new DataColumn("InvoiceRemaining", typeof(decimal)));

            foreach (var t in _salesTransactions)
            {
                var dr = dt.NewRow();
                dr["Date"] = t.Date.ToShortDateString();
                dr["DueDate"] = t.DueDate.ToShortDateString();
                dr["ID"] = t.SalesTransactionID;
                dr["Customer"] = t.Customer.Name;
                dr["InvoiceNetTotal"] = Math.Round(t.Total, 2);
                dr["InvoiceRemaining"] = Math.Round(t.Total - t.Paid, 2);
                dt.Rows.Add(dr);
            }

            var reportDataSource = new ReportDataSource("InvoiceDataSet", dt);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\OverallSalesReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
