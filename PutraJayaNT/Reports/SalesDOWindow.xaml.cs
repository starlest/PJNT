using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Data;
using System.Windows;
using System.Linq;
using PutraJayaNT.Utilities;

namespace PutraJayaNT.Reports
{
    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesDOWindow : ModernWindow
    {
        SalesTransactionVM _salesTransaction;
        DataTable dt1;
        DataTable dt2;

        public SalesDOWindow(SalesTransactionVM vm)
        {
            InitializeComponent();
            _salesTransaction = vm;
        }

        private void InitializeDataSources()
        {
            dt1 = new DataTable();
            dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("LineNumber", typeof(int)));
            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt1.Columns.Add(new DataColumn("Unit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Units", typeof(int)));
            dt1.Columns.Add(new DataColumn("Pieces", typeof(int)));
            dt1.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));

            dt2.Columns.Add(new DataColumn("InvoiceGrossTotal", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceDiscount", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceSalesExpense", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt2.Columns.Add(new DataColumn("Address", typeof(string)));
            dt2.Columns.Add(new DataColumn("InvoiceNumber", typeof(string)));
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dt2.Columns.Add(new DataColumn("DueDate", typeof(string)));
            dt2.Columns.Add(new DataColumn("Notes", typeof(string)));
            dt2.Columns.Add(new DataColumn("Warehouse", typeof(string)));
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadReportViewer1(); // Gudang ABC
            LoadReportViewer2(); // Gudang Bangunan
            LoadReportViewer3(); // Gudang 781
            LoadReportViewer4(); // Gudang Kantor
        }

        private void LoadReportViewer1()
        {
            InitializeDataSources();
            int count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(2)).ToList())
            {
                DataRow dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Units;
                dr1["Pieces"] = line.Pieces;
                dt1.Rows.Add(dr1);
            }

            DataRow dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.NewTransactionGrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.NewTransactionDiscount == null ? 0 : (decimal)_salesTransaction.NewTransactionDiscount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.NewTransactionSalesExpense == null ? 0 : (decimal)_salesTransaction.NewTransactionSalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.NewTransactionCustomer.Name;
            dr2["Address"] = _salesTransaction.NewTransactionCustomer.City;
            dr2["InvoiceNumber"] = _salesTransaction.NewTransactionID;
            dr2["Date"] = _salesTransaction.Model.InvoiceIssued == null ? null : ((DateTime)_salesTransaction.Model.InvoiceIssued).ToShortDateString();
            dr2["DueDate"] = _salesTransaction.Model.DueDate == null ? null : ((DateTime)_salesTransaction.Model.DueDate).ToShortDateString();
            dr2["Notes"] = _salesTransaction.Model.Notes;
            using (var context = new ERPContext())
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(2)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer1.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\SalesDOReport.rdlc"); // Path of the rdlc file
            reportViewer1.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer1.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer1.PageCountMode = PageCountMode.Actual;
            reportViewer1.RefreshReport();
        }

        private void LoadReportViewer2()
        {
            InitializeDataSources();
            int count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(1)).ToList())
            {
                DataRow dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Units;
                dr1["Pieces"] = line.Pieces;
                dt1.Rows.Add(dr1);
            }

            DataRow dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.NewTransactionGrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.NewTransactionDiscount == null ? 0 : (decimal)_salesTransaction.NewTransactionDiscount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.NewTransactionSalesExpense == null ? 0 : (decimal)_salesTransaction.NewTransactionSalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.NewTransactionCustomer.Name;
            dr2["Address"] = _salesTransaction.NewTransactionCustomer.City;
            dr2["InvoiceNumber"] = _salesTransaction.NewTransactionID;
            dr2["Date"] = _salesTransaction.Model.InvoiceIssued == null ? null : ((DateTime)_salesTransaction.Model.InvoiceIssued).ToShortDateString();
            dr2["DueDate"] = _salesTransaction.Model.DueDate == null ? null : ((DateTime)_salesTransaction.Model.DueDate).ToShortDateString();
            dr2["Notes"] = _salesTransaction.Model.Notes;
            using (var context = new ERPContext())
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(1)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer2.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\SalesDOReport.rdlc"); // Path of the rdlc file
            reportViewer2.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer2.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer2.PageCountMode = PageCountMode.Actual;
            reportViewer2.RefreshReport();
        }

        private void LoadReportViewer3()
        {
            InitializeDataSources();
            int count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(3)).ToList())
            {
                DataRow dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Units;
                dr1["Pieces"] = line.Pieces;
                dt1.Rows.Add(dr1);
            }

            DataRow dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.NewTransactionGrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.NewTransactionDiscount == null ? 0 : (decimal)_salesTransaction.NewTransactionDiscount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.NewTransactionSalesExpense == null ? 0 : (decimal)_salesTransaction.NewTransactionSalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.NewTransactionCustomer.Name;
            dr2["Address"] = _salesTransaction.NewTransactionCustomer.City;
            dr2["InvoiceNumber"] = _salesTransaction.NewTransactionID;
            dr2["Date"] = _salesTransaction.Model.InvoiceIssued == null ? null : ((DateTime)_salesTransaction.Model.InvoiceIssued).ToShortDateString();
            dr2["DueDate"] = _salesTransaction.Model.DueDate == null ? null : ((DateTime)_salesTransaction.Model.DueDate).ToShortDateString();
            dr2["Notes"] = _salesTransaction.Model.Notes;
            using (var context = new ERPContext())
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(3)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer3.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\SalesDOReport.rdlc"); // Path of the rdlc file
            reportViewer3.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer3.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer3.PageCountMode = PageCountMode.Actual;
            reportViewer3.RefreshReport();
        }

        private void LoadReportViewer4()
        {
            InitializeDataSources();
            int count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(4)).ToList())
            {
                DataRow dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Units;
                dr1["Pieces"] = line.Pieces;
                dt1.Rows.Add(dr1);
            }

            DataRow dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.NewTransactionGrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.NewTransactionDiscount == null ? 0 : (decimal)_salesTransaction.NewTransactionDiscount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.NewTransactionSalesExpense == null ? 0 : (decimal)_salesTransaction.NewTransactionSalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.NewTransactionCustomer.Name;
            dr2["Address"] = _salesTransaction.NewTransactionCustomer.City;
            dr2["InvoiceNumber"] = _salesTransaction.NewTransactionID;
            dr2["Date"] = _salesTransaction.Model.InvoiceIssued == null ? null : ((DateTime)_salesTransaction.Model.InvoiceIssued).ToShortDateString();
            dr2["DueDate"] = _salesTransaction.Model.DueDate == null ? null : ((DateTime)_salesTransaction.Model.DueDate).ToShortDateString();
            dr2["Notes"] = _salesTransaction.Model.Notes;
            using (var context = new ERPContext())
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(4)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer4.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\SalesDOReport.rdlc"); // Path of the rdlc file
            reportViewer4.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer4.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer4.PageCountMode = PageCountMode.Actual;
            reportViewer4.RefreshReport();
        }
    }
}
