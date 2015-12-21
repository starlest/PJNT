using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports
{
    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesInvoiceWindow : ModernWindow
    {
        SalesTransactionVM _salesTransaction;

        public SalesInvoiceWindow(SalesTransactionVM vm)
        {
            InitializeComponent();
            _salesTransaction = vm;
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
            foreach (var line in _salesTransaction.SalesTransactionLines)
            {
                DataRow dr = dt1.NewRow();
                dr["LineNumber"] = count++;
                dr["ItemID"] = line.Item.ItemID;
                dr["ItemName"] = line.Item.Name;
                dr["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr["Units"] = line.Units;
                dr["Pieces"] = line.Pieces;
                dr["SalesPrice"] = line.SalesPrice;
                dr["Discount"] = line.Discount;
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
            dr2["InvoiceGrossTotal"] = _salesTransaction.NewTransactionGrossTotal;
            dr2["InvoiceDiscount"] = _salesTransaction.NewTransactionDiscount == null ? 0 : (decimal)_salesTransaction.NewTransactionDiscount;
            dr2["InvoiceSalesExpense"] = _salesTransaction.NewTransactionSalesExpense == null ? 0 : (decimal) _salesTransaction.NewTransactionSalesExpense;
            dr2["InvoiceNetTotal"] = _salesTransaction.NetTotal;
            dr2["Customer"] = _salesTransaction.NewTransactionCustomer.Name;
            dr2["Address"] = _salesTransaction.NewTransactionCustomer.City;
            dr2["InvoiceNumber"] = _salesTransaction.NewTransactionID;
            dr2["Date"] = _salesTransaction.Model.InvoiceIssued == null ? null : ((DateTime)_salesTransaction.Model.InvoiceIssued).ToShortDateString();
            dr2["DueDate"] = _salesTransaction.Model.DueDate == null ? null : ((DateTime)_salesTransaction.Model.DueDate).ToShortDateString();
            dr2["Notes"] = _salesTransaction.Model.Notes;

            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\SalesInvoiceReport.rdlc"); // Path of the rdlc file
       
            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
