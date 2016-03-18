using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.ViewModels.Inventory;
using System;
using System.Data;
using System.Drawing.Printing;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for StockCardMovementWindow.xaml
    /// </summary>
    public partial class StockMovementReportWindow : ModernWindow
    {
        StockMovementVM _transaction;

        public StockMovementReportWindow(StockMovementVM transaction)
        {
            InitializeComponent();
            _transaction = transaction;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("Index", typeof(int)));
            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt1.Columns.Add(new DataColumn("Quantity", typeof(string)));

            dt2.Columns.Add(new DataColumn("ID", typeof(string)));
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dt2.Columns.Add(new DataColumn("FromWarehouse", typeof(string)));
            dt2.Columns.Add(new DataColumn("ToWarehouse", typeof(string)));

            var index = 1;
            foreach (var line in _transaction.StockMovementTransactionLines)
            {
                DataRow dr1 = dt1.NewRow();
                dr1["Index"] = index;
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["Quantity"] = line.Units + "/" + line.Pieces;
                dt1.Rows.Add(dr1);

                index++;
            }

            DataRow dr2 = dt2.NewRow();
            dr2["ID"] = _transaction.TransactionID;
            dr2["Date"] = _transaction.TransactionDate.ToString("dd-MM-yyyy");
            dr2["FromWarehouse"] = _transaction.TransactionFromWarehouse.Name;
            dr2["ToWarehouse"] = _transaction.TransactionToWarehouse.Name;
            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("StockMovementLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("StockMovementDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\StockMovementReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;

            var pg = new PageSettings();
            pg.Margins.Top = 0;
            pg.Margins.Bottom = 0;
            pg.Margins.Left = 0;
            pg.Margins.Right = 0;
            var size = new PaperSize("Custom", 827, 550);
            pg.PaperSize = size;
            pg.Landscape = false;
            reportViewer.SetPageSettings(pg);
            reportViewer.RefreshReport();
        }
    }
}
