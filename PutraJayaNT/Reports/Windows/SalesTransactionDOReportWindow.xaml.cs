namespace ECRP.Reports.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Reporting.WinForms;
    using Models.Sales;
    using Utilities.ModelHelpers;
    using ViewModels.Item;

    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesTransactionDOReportWindow
    {
        private readonly SalesTransaction _salesTransaction;
        private DataTable DODataTable;
        private DataTable DOLinesDataTable;
        private readonly string reportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesDOReport.rdlc");
        private readonly List<WarehouseVM> _warehouses;

        public SalesTransactionDOReportWindow(SalesTransaction transaction)
        {
            InitializeComponent();
            _salesTransaction = transaction;
            _warehouses = new List<WarehouseVM>();
            Warehouses.ItemsSource = _warehouses;
            LoadWarehouses();
            InitializeDataSources();
            SetReportViewer(_warehouses.First());
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
      
        }

        private void LoadWarehouses()
        {
            foreach (var line in _salesTransaction.SalesTransactionLines)
            {
                var warehouseVM = new WarehouseVM {Model = line.Warehouse};
                if (!_warehouses.Contains(warehouseVM)) 
                    _warehouses.Add(warehouseVM);
            }
        }

        private void InitializeDataSources()
        {
            DODataTable = new DataTable();
            DOLinesDataTable = new DataTable();

            DODataTable.Columns.Add(new DataColumn("ItemID", typeof(string)));
            DODataTable.Columns.Add(new DataColumn("ItemName", typeof(string)));
            DODataTable.Columns.Add(new DataColumn("UnitName", typeof(string)));
            DODataTable.Columns.Add(new DataColumn("QuantityPerUnit", typeof(string)));
            DODataTable.Columns.Add(new DataColumn("Quantity", typeof(string)));
            DODataTable.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            DODataTable.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            DODataTable.Columns.Add(new DataColumn("Total", typeof(decimal)));

            DOLinesDataTable.Columns.Add(new DataColumn("InvoiceGrossTotal", typeof(decimal)));
            DOLinesDataTable.Columns.Add(new DataColumn("InvoiceDiscount", typeof(decimal)));
            DOLinesDataTable.Columns.Add(new DataColumn("InvoiceSalesExpense", typeof(decimal)));
            DOLinesDataTable.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            DOLinesDataTable.Columns.Add(new DataColumn("Customer", typeof(string)));
            DOLinesDataTable.Columns.Add(new DataColumn("Address", typeof(string)));
            DOLinesDataTable.Columns.Add(new DataColumn("InvoiceNumber", typeof(string)));
            DOLinesDataTable.Columns.Add(new DataColumn("Date", typeof(string)));
            DOLinesDataTable.Columns.Add(new DataColumn("DueDate", typeof(string)));
            DOLinesDataTable.Columns.Add(new DataColumn("Notes", typeof(string)));
            DOLinesDataTable.Columns.Add(new DataColumn("Warehouse", typeof(string)));
        }

        private void SetReportViewer(WarehouseVM warehouse)
        {
            DODataTable.Rows.Clear();
            DOLinesDataTable.Rows.Clear();

            foreach (var line in _salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(warehouse.ID)).ToList())
            {
                var dr1 = DODataTable.NewRow();
                dr1["ItemID"] = line.Item.ItemID;
                dr1["ItemName"] = line.Item.Name;
                dr1["UnitName"] = InventoryHelper.GetItemUnitName(line.Item);
                dr1["QuantityPerUnit"] = InventoryHelper.GetItemQuantityPerUnit(line.Item);
                dr1["Quantity"] = InventoryHelper.ConvertItemQuantityTostring(line.Item, line.Quantity);
                DODataTable.Rows.Add(dr1);
            }

            var dr2 = DOLinesDataTable.NewRow();
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
            dr2["Warehouse"] = warehouse.Name;
            

            DOLinesDataTable.Rows.Add(dr2);

            var reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", DODataTable);
            var reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", DOLinesDataTable);

            reportViewer.LocalReport.ReportPath = reportPath;
            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
            reportViewer.ShowExportButton = false;
            reportViewer.ShowPrintButton = false;
        }

        private void Warehouses_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetReportViewer(Warehouses.SelectedItem as WarehouseVM);
        }
    }
}
