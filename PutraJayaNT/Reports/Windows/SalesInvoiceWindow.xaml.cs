using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.Models.Sales;
using System;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesInvoiceWindow : ModernWindow
    {
        SalesTransaction _salesTransaction;

        public SalesInvoiceWindow(SalesTransaction transaction)
        {
            InitializeComponent();
            _salesTransaction = transaction;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("LineNumber", typeof(int)));
            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt1.Columns.Add(new DataColumn("Unit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Units", typeof(int)));
            dt1.Columns.Add(new DataColumn("Pieces", typeof(int)));
            dt1.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));

            int count = 1;
            foreach (var line in _salesTransaction.TransactionLines)
            {
                DataRow dr = dt1.NewRow();
                dr["LineNumber"] = count++;
                dr["ItemID"] = line.Item.ItemID;
                dr["ItemName"] = line.Item.Name;
                dr["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                dr["SalesPrice"] = line.SalesPrice * line.Item.PiecesPerUnit;
                dr["Discount"] = line.Discount * line.Item.PiecesPerUnit;
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
            dr2["InvoiceGrossTotal"] = _salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.Total;
            dr2["Customer"] = _salesTransaction.Customer.Name;
            dr2["Address"] = _salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = _salesTransaction.SalesTransactionID;
            dr2["Date"] = _salesTransaction.When.ToShortDateString();
            dr2["DueDate"] = _salesTransaction.DueDate.ToShortDateString();
            dr2["Notes"] = _salesTransaction.Notes;

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesInvoiceReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
            reportViewer.ShowExportButton = false;
            reportViewer.ShowPrintButton = false;
        }
    }
}