namespace PutraJayaNT.Reports.Windows
{
    using FirstFloor.ModernUI.Windows.Controls;
    using Microsoft.Reporting.WinForms;
    using ViewModels.Reports;
    using System;
    using System.Data;
    using System.Windows;

    /// <summary>
    /// Interaction logic for InventoryReportWindow.xaml
    /// </summary>
    public partial class InventoryReportWindow : ModernWindow
    {
        InventoryReportVM _vm;

        public InventoryReportWindow(InventoryReportVM vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var dt1 = new DataTable();
            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("Item", typeof(string)));
            dt1.Columns.Add(new DataColumn("Unit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Quantity", typeof(string)));
            dt1.Columns.Add(new DataColumn("PurchasePrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Value", typeof(decimal)));

            foreach (var line in _vm.Lines)
            {
                var dr1 = dt1.NewRow();
                dr1["ItemID"] = line.Item.ItemID;
                dr1["Item"] = line.Item.Name;
                dr1["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                dr1["Quantity"] = line.Units + "/" + line.Pieces;
                dr1["PurchasePrice"] = line.PurchasePrice;
                dr1["SalesPrice"] = line.SalesPrice;
                dr1["Value"] = line.InventoryValue;
                dt1.Rows.Add(dr1);
            }

            var dt2 = new DataTable();
            dt2.Columns.Add(new DataColumn("Warehouse", typeof(string)));
            dt2.Columns.Add(new DataColumn("Category", typeof(string)));
            dt2.Columns.Add(new DataColumn("Supplier", typeof(string)));
            dt2.Columns.Add(new DataColumn("Total", typeof(decimal)));
            var dr2 = dt2.NewRow();
            dr2["Warehouse"] = _vm.SelectedWarehouse.Name;
            dr2["Category"] = _vm.SelectedCategory != null ? _vm.SelectedCategory.Name : "";
            dr2["Supplier"] = _vm.SelectedSupplier != null ? _vm.SelectedSupplier.Name : "";
            dr2["Total"] = _vm.Total;
            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("InventoryReportLineDataset", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("InventoryReportDataset", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\InventoryReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
