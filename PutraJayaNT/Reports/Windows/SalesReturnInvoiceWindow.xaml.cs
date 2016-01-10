using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesReturnInvoiceWindow : ModernWindow
    {
        SalesReturnTransactionVM _salesReturnTransaction;

        public SalesReturnInvoiceWindow(SalesReturnTransactionVM vm)
        {
            InitializeComponent();
            _salesReturnTransaction = vm;
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
            foreach (var line in _salesReturnTransaction.SalesReturnTransactionLines)
            {
                DataRow dr = dt1.NewRow();
                dr["LineNumber"] = count++;
                dr["ItemID"] = line.Item.ItemID;
                dr["ItemName"] = line.Item.Name;
                dr["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr["Units"] = line.Units;
                dr["Pieces"] = line.Pieces;
                dr["SalesPrice"] = line.SalesPrice;
                dr["Discount"] = _salesReturnTransaction.GetNetDiscount(line);
                line.UpdateTotal();
                dr["Total"] = line.Total;
                dt1.Rows.Add(dr);
            }

            DataRow dr2 = dt2.NewRow();
            dt2.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt2.Columns.Add(new DataColumn("Address", typeof(string)));
            dt2.Columns.Add(new DataColumn("SalesInvoiceNumber", typeof(string)));
            dt2.Columns.Add(new DataColumn("SalesReturnInvoiceNumber", typeof(string)));
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dr2["Customer"] = _salesReturnTransaction.SelectedSalesTransactionCustomer.Name;
            dr2["Address"] = _salesReturnTransaction.SelectedSalesTransactionCustomer.City;
            dr2["SalesInvoiceNumber"] = _salesReturnTransaction.SelectedSalesTransactionID;
            dr2["SalesReturnInvoiceNumber"] = _salesReturnTransaction.SalesReturnTransactionID;
            dr2["Date"] = _salesReturnTransaction.SalesReturnEntryDate.ToShortDateString();

            dt2.Rows.Add(dr2);


            ReportDataSource reportDataSource1 = new ReportDataSource("SalesReturnInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesReturnInvoiceDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesReturnInvoiceReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
            reportViewer.ShowExportButton = false;
        }
    }
}