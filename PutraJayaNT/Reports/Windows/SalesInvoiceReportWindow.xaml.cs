using Microsoft.Reporting.WinForms;
using PutraJayaNT.Models.Sales;
using System;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    using Utilities.ModelHelpers;

    /// <summary>
    /// Interaction logic for SalesInvoiceReportWindow.xaml
    /// </summary>
    public partial class SalesInvoiceReportWindow
    {
        readonly SalesTransaction _salesTransaction;

        public SalesInvoiceReportWindow(SalesTransaction transaction)
        {
            InitializeComponent();
            _salesTransaction = transaction;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dt1 = new DataTable();
            var dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("LineNumber", typeof(int)));
            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt1.Columns.Add(new DataColumn("UnitName", typeof(string)));
            dt1.Columns.Add(new DataColumn("QuantityPerUnit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Quantity", typeof(string)));
            dt1.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));

            var count = 1;
            foreach (var line in _salesTransaction.SalesTransactionLines)
            {
                var dr = dt1.NewRow();
                dr["LineNumber"] = count++;
                dr["ItemID"] = line.Item.ItemID;
                dr["ItemName"] = line.Item.Name;
                dr["UnitName"] = InventoryHelper.GetItemUnitName(line.Item);
                dr["QuantityPerUnit"] = InventoryHelper.GetItemQuantityPerUnit(line.Item);
                dr["Quantity"] = InventoryHelper.ConvertItemQuantityTostring(line.Item, line.Quantity);
                dr["SalesPrice"] = line.SalesPrice*line.Item.PiecesPerUnit;
                dr["Discount"] = line.Discount*line.Item.PiecesPerUnit;
                dr["Total"] = line.Total;
                dt1.Rows.Add(dr);
            }

            DataRow dr2 = dt2.NewRow();
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
            dt2.Columns.Add(new DataColumn("Copy", typeof(string)));

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
            dr2["Copy"] = _salesTransaction.InvoicePrinted ? "Copy" : "";

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory,
                @"Reports\\RDLC\\SalesInvoiceReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
            reportViewer.ShowExportButton = false;
            reportViewer.ShowPrintButton = false;
        }
    }
}