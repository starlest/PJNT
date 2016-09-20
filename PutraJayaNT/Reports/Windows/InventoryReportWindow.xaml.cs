namespace ECRP.Reports.Windows
{
    using System;
    using System.Data;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using ViewModels.Reports;

    /// <summary>
    /// Interaction logic for InventoryReportWindow.xaml
    /// </summary>
    public partial class InventoryReportWindow
    {
        private readonly InventoryReportVM _vm;
        private readonly DataTable _inventoryReportDataTable;
        private readonly DataTable _inventoryReportLinesDataTable;

        public InventoryReportWindow(InventoryReportVM vm)
        {
            InitializeComponent();
            _vm = vm;
            _inventoryReportDataTable = new DataTable();
            _inventoryReportLinesDataTable = new DataTable();
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetUpInventoryReportDataTable();
            LoadInventoryReportDataTableRows();
            SetUpInventoryReportLinesDataTable();
            LoadInventoryReportlinesDataTableRows();
            SetupReportViewer();
            reportViewer.RefreshReport();
        }

        private void SetUpInventoryReportDataTable()
        {
            _inventoryReportDataTable.Columns.Add(new DataColumn("Warehouse", typeof(string)));
            _inventoryReportDataTable.Columns.Add(new DataColumn("Category", typeof(string)));
            _inventoryReportDataTable.Columns.Add(new DataColumn("Supplier", typeof(string)));
            _inventoryReportDataTable.Columns.Add(new DataColumn("Total", typeof(decimal)));
        }

        private void LoadInventoryReportDataTableRows()
        {
            var row = _inventoryReportDataTable.NewRow();
            row["Warehouse"] = _vm.SelectedWarehouse.Name;
            row["Category"] = _vm.SelectedCategory != null ? _vm.SelectedCategory.Name : "";
            row["Supplier"] = _vm.SelectedSupplier != null ? _vm.SelectedSupplier.Name : "";
            row["Total"] = _vm.Total;
            _inventoryReportDataTable.Rows.Add(row);
        }

        private void SetUpInventoryReportLinesDataTable()
        {
            _inventoryReportLinesDataTable.Columns.Add(new DataColumn("ItemID", typeof(string)));
            _inventoryReportLinesDataTable.Columns.Add(new DataColumn("Item", typeof(string)));
            _inventoryReportLinesDataTable.Columns.Add(new DataColumn("Unit", typeof(string)));
            _inventoryReportLinesDataTable.Columns.Add(new DataColumn("Quantity", typeof(string)));
            _inventoryReportLinesDataTable.Columns.Add(new DataColumn("PurchasePrice", typeof(decimal)));
            _inventoryReportLinesDataTable.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            _inventoryReportLinesDataTable.Columns.Add(new DataColumn("Value", typeof(decimal)));
        }

        private void LoadInventoryReportlinesDataTableRows()
        {
            foreach (var line in _vm.DisplayedLines)
            {
                var row = _inventoryReportLinesDataTable.NewRow();
                row["ItemID"] = line.Item.ItemID;
                row["Item"] = line.Item.Name;
                row["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                row["Quantity"] = line.Units + "/" + line.Pieces;
                row["PurchasePrice"] = line.PurchasePrice;
                row["SalesPrice"] = line.SalesPrice;
                row["Value"] = line.InventoryValue;
                _inventoryReportLinesDataTable.Rows.Add(row);
            }
        }

        private void SetupReportViewer()
        {
            LoadReportViewerDataSources();
            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\InventoryReport.rdlc"); // Path of the rdlc file
            reportViewer.PageCountMode = PageCountMode.Actual;
        }

        private void LoadReportViewerDataSources()
        {
            var inventoryReportDataSource = new ReportDataSource("InventoryReportDataset", _inventoryReportDataTable);
            var inventoryReportLinesDataSource = new ReportDataSource("InventoryReportLineDataset", _inventoryReportLinesDataTable);
            reportViewer.LocalReport.DataSources.Add(inventoryReportDataSource);
            reportViewer.LocalReport.DataSources.Add(inventoryReportLinesDataSource);
        }
    }
}
