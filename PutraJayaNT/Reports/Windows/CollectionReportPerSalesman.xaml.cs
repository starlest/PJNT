using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using PutraJayaNT.ViewModels.Sales;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for CollectionReportPerSalesmanWindow.xaml
    /// </summary>
    public partial class CollectionReportPerSalesmanWindow : ModernWindow
    {
        ObservableCollection<ViewModels.Sales.SalesTransactionVM>  _salesTransactions;
        DateTime _dateSelected;

        public CollectionReportPerSalesmanWindow(ObservableCollection<ViewModels.Sales.SalesTransactionVM> salesTransactions, DateTime date)
        {
            InitializeComponent();
            _salesTransactions = salesTransactions;
            _dateSelected = date;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("ID", typeof(string)));
            dt.Columns.Add(new DataColumn("City", typeof(string)));
            dt.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            dt.Columns.Add(new DataColumn("InvoicePaid", typeof(decimal)));
            dt.Columns.Add(new DataColumn("PaidToday", typeof(decimal)));
            dt.Columns.Add(new DataColumn("InvoiceRemaining", typeof(decimal)));
            dt.Columns.Add(new DataColumn("DueDate", typeof(string)));
            dt.Columns.Add(new DataColumn("CollectionSalesman", typeof(string)));
            dt.Columns.Add(new DataColumn("CollectionTotal", typeof(decimal)));

            foreach (var t in _salesTransactions)
            {
                if (t.IsSelected)
                {
                    DataRow dr = dt.NewRow();
                    dr["Date"] = t.Date.ToShortDateString();
                    dr["ID"] = t.SalesTransactionID;
                    dr["Customer"] = t.Customer.Name;
                    dr["City"] = t.Customer.City;
                    dr["InvoiceNetTotal"] = t.Total;
                    dr["InvoicePaid"] = t.Paid;
                    dr["PaidToday"] = GetPaidToday(t);
                    dr["InvoiceRemaining"] = t.Total - t.Paid;
                    dr["DueDate"] = t.DueDate.ToString("dd-MM-yyyy");
                    dr["CollectionSalesman"] = t.CollectionSalesman != null ? t.CollectionSalesman.Name : "";
                    dt.Rows.Add(dr);
                }
            }

            ReportDataSource reportDataSource = new ReportDataSource("InvoiceDataSet", dt);
 
            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\CollectionReportPerSalesman.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }

        private decimal GetPaidToday(ViewModels.Sales.SalesTransactionVM t)
        {
            using (var context = new ERPContext())
            {
                var receipts = context.Ledger_Transaction_Lines.Where(e => e.LedgerTransaction.Description.Equals("Sales Transaction Receipt") && e.LedgerTransaction.Documentation.Equals(t.SalesTransactionID) && e.LedgerTransaction.Date.Equals(_dateSelected) && e.LedgerAccount.Name.Equals("Cash")).ToList();
                var balance = 0m;
                foreach (var r in receipts)
                    balance += r.Amount;
                return balance;
            }
        }
    }
}
