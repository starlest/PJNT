using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Windows;
using System.Linq;
using PutraJayaNT.Utilities;
using PutraJayaNT.Models.Sales;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesTransactionDOReportWindow
    {
        readonly SalesTransaction _salesTransaction;
        private DataTable dt1;
        private DataTable dt2;
        private readonly string reportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesDOReport.rdlc");

        public SalesTransactionDOReportWindow(SalesTransaction transaction)
        {
            InitializeComponent();
            _salesTransaction = transaction;
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
            var count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(2)).ToList())
            {
                DataRow dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr1["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                dt1.Rows.Add(dr1);
            }

            DataRow dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.Customer.Name;
            dr2["Address"] = _salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = _salesTransaction.SalesTransactionID;
            dr2["Date"] = _salesTransaction.Date.ToString("dd-MM-yyyy");
            dr2["DueDate"] = _salesTransaction.DueDate.ToString("dd-MM-yyyy");
            dr2["Notes"] = _salesTransaction.Notes;
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(2)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            var reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            var reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer1.LocalReport.ReportPath = reportPath;
            reportViewer1.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer1.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer1.PageCountMode = PageCountMode.Actual;
            reportViewer1.RefreshReport();
            reportViewer1.ShowExportButton = false;
            reportViewer1.ShowPrintButton = false;
        }

        private void LoadReportViewer2()
        {
            InitializeDataSources();
            var count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(1)).ToList())
            {
                var dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr1["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                dt1.Rows.Add(dr1);
            }

            var dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.Customer.Name;
            dr2["Address"] = _salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = _salesTransaction.SalesTransactionID;
            dr2["Date"] = _salesTransaction.Date.ToString("dd-MM-yyyy");
            dr2["DueDate"] = _salesTransaction.DueDate.ToString("dd-MM-yyyy");
            dr2["Notes"] = _salesTransaction.Notes;
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(1)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            var reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            var reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer2.LocalReport.ReportPath = reportPath;
            reportViewer2.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer2.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer2.PageCountMode = PageCountMode.Actual;
            reportViewer2.RefreshReport();
            reportViewer2.ShowExportButton = false;
            reportViewer2.ShowPrintButton = false;
        }

        private void LoadReportViewer3()
        {
            InitializeDataSources();
            var count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(3)).ToList())
            {
                var dr1 = dt1.NewRow();
                dr1["LineNumber"] = count++;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr1["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                dt1.Rows.Add(dr1);
            }

            var dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.Customer.Name;
            dr2["Address"] = _salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = _salesTransaction.SalesTransactionID;
            dr2["Date"] = _salesTransaction.Date.ToString("dd-MM-yyyy");
            dr2["DueDate"] = _salesTransaction.DueDate.ToString("dd-MM-yyyy");
            dr2["Notes"] = _salesTransaction.Notes;
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(3)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            var reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            var reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer3.LocalReport.ReportPath = reportPath;
            reportViewer3.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer3.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer3.PageCountMode = PageCountMode.Actual;
            reportViewer3.RefreshReport();
            reportViewer3.ShowExportButton = false;
            reportViewer3.ShowPrintButton = false;
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
                dr1["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr1["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                dt1.Rows.Add(dr1);
            }

            DataRow dr2 = dt2.NewRow();
            dr2["InvoiceGrossTotal"] = _salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.Customer.Name;
            dr2["Address"] = _salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = _salesTransaction.SalesTransactionID;
            dr2["Date"] = _salesTransaction.Date.ToString("dd-MM-yyyy");
            dr2["DueDate"] = _salesTransaction.DueDate.ToString("dd-MM-yyyy");
            dr2["Notes"] = _salesTransaction.Notes;
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                dr2["Warehouse"] = context.Warehouses.Where(e => e.ID.Equals(4)).FirstOrDefault().Name;
            }

            dt2.Rows.Add(dr2);

            var reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            var reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer4.LocalReport.ReportPath = reportPath; // Path of the rdlc file
            reportViewer4.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer4.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer4.PageCountMode = PageCountMode.Actual;
            reportViewer4.RefreshReport();
            reportViewer4.ShowExportButton = false;
            reportViewer4.ShowPrintButton = false;
        }
    }
}
