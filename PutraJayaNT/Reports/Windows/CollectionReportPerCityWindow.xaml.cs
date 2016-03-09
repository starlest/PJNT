using PutraJayaNT.Utilities.Database.Ledger;

namespace PutraJayaNT.Reports.Windows
{
    using Microsoft.Reporting.WinForms;
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using ViewModels.Sales;

    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class CollectionReportPerCityWindow
    {
        private readonly ObservableCollection<SalesTransactionVM>  _salesTransactions;
        private readonly DateTime _dateSelected;
        private readonly DataTable _reportDataTable;

        public CollectionReportPerCityWindow(ObservableCollection<SalesTransactionVM> salesTransactions, DateTime date)
        {
            InitializeComponent();
            _salesTransactions = salesTransactions;
            _dateSelected = date;
            _reportDataTable = new DataTable();
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetUpReportDataTable();
            LoaroweportDataTableRows();
            SetUpReportViewer();
            reportViewer.RefreshReport();
        }

        private void SetUpReportDataTable()
        {
            _reportDataTable.Columns.Add(new DataColumn("Date", typeof(string)));
            _reportDataTable.Columns.Add(new DataColumn("ID", typeof(string)));
            _reportDataTable.Columns.Add(new DataColumn("City", typeof(string)));
            _reportDataTable.Columns.Add(new DataColumn("Customer", typeof(string)));
            _reportDataTable.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            _reportDataTable.Columns.Add(new DataColumn("InvoicePaid", typeof(decimal)));
            _reportDataTable.Columns.Add(new DataColumn("PaidToday", typeof(decimal)));
            _reportDataTable.Columns.Add(new DataColumn("InvoiceRemaining", typeof(decimal)));
            _reportDataTable.Columns.Add(new DataColumn("DueDate", typeof(string)));
            _reportDataTable.Columns.Add(new DataColumn("CollectionSalesman", typeof(string)));
            _reportDataTable.Columns.Add(new DataColumn("CollectionTotal", typeof(decimal)));
        }

        private void LoaroweportDataTableRows()
        {
            foreach (var salesTransaction in _salesTransactions)
            {
                if (!salesTransaction.IsSelected) continue;
                var row = _reportDataTable.NewRow();
                row["Date"] = salesTransaction.Date.ToShortDateString();
                row["ID"] = salesTransaction.SalesTransactionID;
                row["Customer"] = salesTransaction.Customer.Name;
                row["City"] = salesTransaction.Customer.City;
                row["InvoiceNetTotal"] = salesTransaction.Total;
                row["InvoicePaid"] = salesTransaction.Paid;
                row["PaidToday"] = GetSalesTransactionSelectedDatePaidAmount(salesTransaction);
                row["InvoiceRemaining"] = salesTransaction.Total - salesTransaction.Paid;
                row["DueDate"] = salesTransaction.DueDate.ToString("dd-MM-yyyy");
                row["CollectionSalesman"] = salesTransaction.CollectionSalesman != null ? salesTransaction.CollectionSalesman.Name : "";
                _reportDataTable.Rows.Add(row);
            }
        }

        private void SetUpReportViewer()
        {
            var reportDataSource = new ReportDataSource("InvoiceDataSet", _reportDataTable);
            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\CollectionReportPerCity.rdlc"); // Path of the rdlc file
            reportViewer.PageCountMode = PageCountMode.Actual;
        }

        private decimal GetSalesTransactionSelectedDatePaidAmount(SalesTransactionVM salesTransaction)
        {
            var selectedDateSalesTransactionReceiptsLedgerLinesFromDatabase =
                DatabaseLedgerTransactionLineHelper.Get(
                    line =>
                        line.LedgerTransaction.Description.Equals("Sales Transaction Receipt") &&
                        line.LedgerTransaction.Documentation.Equals(salesTransaction.SalesTransactionID) &&
                        line.LedgerTransaction.Date.Equals(_dateSelected) && line.LedgerAccount.Name.Equals("Cash"));
            return selectedDateSalesTransactionReceiptsLedgerLinesFromDatabase.Sum(line => line.Amount);
        }
    }
}
