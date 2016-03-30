namespace PutraJayaNT.Reports.Windows
{
    using Microsoft.Reporting.WinForms;
    using Utilities;
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using ViewModels.Sales;

    /// <summary>
    /// Interaction logic for CollectionReportPerSalesmanWindow.xaml
    /// </summary>
    public partial class CollectionReportPerSalesmanWindow
    {
        private readonly ObservableCollection<SalesTransactionVM>  _salesTransactions;
        private readonly DateTime _dateSelected;
        private readonly DataTable _reportDataTable;

        public CollectionReportPerSalesmanWindow(ObservableCollection<SalesTransactionVM> salesTransactions, DateTime date)
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
            SetupReportDataTable();
            LoadReportDataTableRows();
            SetupReportViewer();
            reportViewer.RefreshReport();
        }

        private void SetupReportDataTable()
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

        private void LoadReportDataTableRows()
        {
            foreach (var salesTransaction in _salesTransactions)
            {
                if (!salesTransaction.IsSelected) continue;
                var dr = _reportDataTable.NewRow();
                dr["Date"] = salesTransaction.Date.ToShortDateString();
                dr["ID"] = salesTransaction.SalesTransactionID;
                dr["Customer"] = salesTransaction.Customer.Name;
                dr["City"] = salesTransaction.Customer.City;
                dr["InvoiceNetTotal"] = salesTransaction.Total;
                dr["InvoicePaid"] = salesTransaction.Paid;
                dr["PaidToday"] = GetSelectedDateSalesTransactionPaidAmount(salesTransaction);
                dr["InvoiceRemaining"] = salesTransaction.Total - salesTransaction.Paid;
                dr["DueDate"] = salesTransaction.DueDate.ToString("dd-MM-yyyy");
                dr["CollectionSalesman"] = salesTransaction.CollectionSalesman != null ? salesTransaction.CollectionSalesman.Name : "";
                _reportDataTable.Rows.Add(dr);
            }
        }

        private decimal GetSelectedDateSalesTransactionPaidAmount(SalesTransactionVM salesTransaction)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var receipts = context.Ledger_Transaction_Lines
                    .Where(line => line.LedgerTransaction.Description.Equals("Sales Transaction Receipt") && 
                    line.LedgerTransaction.Documentation.Equals(salesTransaction.SalesTransactionID) && line.LedgerTransaction.Date.Equals(_dateSelected) &&
                    line.LedgerAccount.Name.Equals("Cash")).ToList();
                return receipts.Sum(r => r.Amount);
            }
        }

        private void SetupReportViewer()
        {
            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\CollectionReportPerSalesman.rdlc"); // Path of the rdlc file
            var reportDataSource = new ReportDataSource("InvoiceDataSet", _reportDataTable);
            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
        }
    }
}
