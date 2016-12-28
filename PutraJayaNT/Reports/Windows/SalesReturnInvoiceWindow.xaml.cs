namespace ECERP.Reports.Windows
{
    using System;
    using System.Data;
    using System.Drawing.Printing;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using Utilities.ModelHelpers;
    using ViewModels.Customers.SalesReturn;

    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesReturnInvoiceWindow
    {
        readonly SalesReturnVM _salesReturnTransaction;

        public SalesReturnInvoiceWindow(SalesReturnVM vm)
        {
            InitializeComponent();
            _salesReturnTransaction = vm;
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
            dt1.Columns.Add(new DataColumn("ReturnPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));

            var count = 1;
            foreach (var line in _salesReturnTransaction.DisplayedSalesReturnTransactionLines)
            {
                var dr = dt1.NewRow();
                dr["LineNumber"] = count++;
                dr["ItemID"] = line.Item.ItemID;
                dr["ItemName"] = line.Item.Name;
                dr["UnitName"] = InventoryHelper.GetItemUnitName(line.Item);
                dr["QuantityPerUnit"] = InventoryHelper.GetItemQuantityPerUnit(line.Item);
                dr["Quantity"] = InventoryHelper.ConvertItemQuantityTostring(line.Item, line.Quantity);
                dr["ReturnPrice"] = line.ReturnPrice * line.Item.PiecesPerUnit;
                dr["Total"] = line.Total;

                dt1.Rows.Add(dr);
            }

            var dr2 = dt2.NewRow();
            dt2.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt2.Columns.Add(new DataColumn("Address", typeof(string)));
            dt2.Columns.Add(new DataColumn("SalesInvoiceNumber", typeof(string)));
            dt2.Columns.Add(new DataColumn("SalesReturnInvoiceNumber", typeof(string)));
            dt2.Columns.Add(new DataColumn("Date", typeof(string)));
            dr2["Customer"] = _salesReturnTransaction.SelectedSalesTransactionCustomer.Name;
            dr2["Address"] = _salesReturnTransaction.SelectedSalesTransactionCustomer.City;
            dr2["SalesInvoiceNumber"] = _salesReturnTransaction.SelectedSalesTransactionID;
            dr2["SalesReturnInvoiceNumber"] = _salesReturnTransaction.SalesReturnTransactionID;
            dr2["Date"] = _salesReturnTransaction.SalesReturnTransactionDate.ToString("dd-MM-yyyy");

            dt2.Rows.Add(dr2);


            ReportDataSource reportDataSource1 = new ReportDataSource("SalesReturnInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesReturnInvoiceDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesReturnInvoiceReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;

            var pg = new PageSettings
            {
                Margins =
                {
                    Top = 0,
                    Bottom = 0,
                    Left = 0,
                    Right = 0
                }
            };
            var size = new PaperSize("Custom", 827, 550);
            pg.PaperSize = size;
            pg.Landscape = false;
            reportViewer.SetPageSettings(pg);

            reportViewer.RefreshReport();
            reportViewer.ShowExportButton = false;
        }
    }
}