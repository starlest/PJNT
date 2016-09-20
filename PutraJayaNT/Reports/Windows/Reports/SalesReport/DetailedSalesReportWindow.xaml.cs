namespace ECRP.Reports.Windows.Reports.SalesReport
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using Utilities.ModelHelpers;
    using ViewModels.Sales;

    /// <summary>
    /// Interaction logic for DetailedSalesReportWindow.xaml
    /// </summary>
    public partial class DetailedSalesReportWindow
    {
        private readonly ObservableCollection<SalesTransactionLineVM>  _salesTransactionLines;

        public DetailedSalesReportWindow(ObservableCollection<SalesTransactionLineVM> salesTransactionLines)
        {
            InitializeComponent();
            _salesTransactionLines = salesTransactionLines;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dt = new DataTable();

            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("SalesTransactionID", typeof(string)));
            dt.Columns.Add(new DataColumn("Customer", typeof(string)));
            dt.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt.Columns.Add(new DataColumn("UnitName", typeof(string)));
            dt.Columns.Add(new DataColumn("QuantityPerUnit", typeof(string)));
            dt.Columns.Add(new DataColumn("Quantity", typeof(string)));

            foreach (var line in _salesTransactionLines)
            {
                var dr = dt.NewRow();
                dr["Date"] = line.SalesTransaction.Date.ToString("dd-MM-yyyy");
                dr["SalesTransactionID"] = line.SalesTransaction.SalesTransactionID;
                dr["Customer"] = line.SalesTransaction.Customer.Name;
                dr["ItemID"] = line.Item.ItemID;
                dr["ItemName"] = line.Item.Name;
                dr["UnitName"] = InventoryHelper.GetItemUnitName(line.Item);
                dr["QuantityPerUnit"] = InventoryHelper.GetItemQuantityPerUnit(line.Item);
                dr["Quantity"] = InventoryHelper.ConvertItemQuantityTostring(line.Item, line.Quantity);
                dt.Rows.Add(dr);
            }

            var reportDataSource = new ReportDataSource("DetailedSalesTransactionLineDataSet", dt);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\Reports\\SalesReport\\DetailedSalesReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
