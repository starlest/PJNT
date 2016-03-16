namespace PutraJayaNT.Utilities.PrintHelpers
{
    using System;
    using System.Data;
    using System.Linq;
    using Microsoft.Reporting.WinForms;
    using Models.Inventory;
    using Models.Sales;

    public static class SalesTransactionPrintHelper
    {
        public static LocalReport CreateDOLocalReport(SalesTransaction salesTransaction, Warehouse warehouse)
        {
            var salesDODataTable = new DataTable();
            var salesDOLinesDataTable = new DataTable();
            SetupSalesDODataTable(salesDODataTable);
            SetupSalesDOLinesDataTable(salesDOLinesDataTable);
            LoadSalesDODatable(salesTransaction, warehouse, salesDODataTable);
            LoadSalesDOLinesDataTable(salesTransaction, warehouse, salesDOLinesDataTable);

            var localReport = new LocalReport
            {
                ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesDOReport.rdlc")
            };

            var salesDODataSource = new ReportDataSource("SalesInvoiceDataSet", salesDODataTable);
            var salesDOLineDataSetDataSource = new ReportDataSource("SalesInvoiceLineDataSet", salesDOLinesDataTable);
            localReport.DataSources.Add(salesDODataSource);
            localReport.DataSources.Add(salesDOLineDataSetDataSource);

            return localReport;
        }

        private static void LoadSalesDOLinesDataTable(SalesTransaction salesTransaction, Warehouse warehouse, DataTable salesTransactionLinesDataTable)
        {
            var count = 1;
            foreach (
                var line in
                    salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(warehouse.ID)).ToList())
            {
                var salesTransactionLinesDataTableRow = salesTransactionLinesDataTable.NewRow();
                salesTransactionLinesDataTableRow["LineNumber"] = count++;
                salesTransactionLinesDataTableRow["ItemID"] = line.Item.ItemID;
                salesTransactionLinesDataTableRow["ItemName"] = line.Item.Name;
                salesTransactionLinesDataTableRow["Unit"] = line.Item.UnitName + "/" + line.Item.PiecesPerUnit;
                salesTransactionLinesDataTableRow["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                salesTransactionLinesDataTableRow["Pieces"] = line.Quantity % line.Item.PiecesPerUnit;
                salesTransactionLinesDataTable.Rows.Add(salesTransactionLinesDataTableRow);
            }
        }

        private static void LoadSalesDODatable(SalesTransaction salesTransaction, Warehouse warehouse, DataTable salesTransactionDataTable)
        {
            var salesTranscationDataTableRow = salesTransactionDataTable.NewRow();
            salesTranscationDataTableRow["InvoiceGrossTotal"] = salesTransaction.GrossTotal;
            salesTranscationDataTableRow["InvoiceDiscount"] = salesTransaction.Discount;
            salesTranscationDataTableRow["InvoiceSalesExpense"] = salesTransaction.SalesExpense;
            salesTranscationDataTableRow["InvoiceNetTotal"] = salesTransaction.NetTotal;
            salesTranscationDataTableRow["Customer"] = salesTransaction.Customer.Name;
            salesTranscationDataTableRow["Address"] = salesTransaction.Customer.City;
            salesTranscationDataTableRow["InvoiceNumber"] = salesTransaction.SalesTransactionID;
            salesTranscationDataTableRow["Date"] = salesTransaction.Date.ToString("dd-MM-yyyy");
            salesTranscationDataTableRow["DueDate"] = salesTransaction.DueDate.ToString("dd-MM-yyyy");
            salesTranscationDataTableRow["Notes"] = salesTransaction.Notes;
            salesTranscationDataTableRow["Warehouse"] = warehouse.Name;
            salesTransactionDataTable.Rows.Add(salesTranscationDataTableRow);
        }

        private static void SetupSalesDODataTable(DataTable salesTransactionDataTable)
        {
            salesTransactionDataTable.Columns.Add(new DataColumn("InvoiceGrossTotal", typeof(decimal)));
            salesTransactionDataTable.Columns.Add(new DataColumn("InvoiceDiscount", typeof(decimal)));
            salesTransactionDataTable.Columns.Add(new DataColumn("InvoiceSalesExpense", typeof(decimal)));
            salesTransactionDataTable.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            salesTransactionDataTable.Columns.Add(new DataColumn("Customer", typeof(string)));
            salesTransactionDataTable.Columns.Add(new DataColumn("Address", typeof(string)));
            salesTransactionDataTable.Columns.Add(new DataColumn("InvoiceNumber", typeof(string)));
            salesTransactionDataTable.Columns.Add(new DataColumn("Date", typeof(string)));
            salesTransactionDataTable.Columns.Add(new DataColumn("DueDate", typeof(string)));
            salesTransactionDataTable.Columns.Add(new DataColumn("Notes", typeof(string)));
            salesTransactionDataTable.Columns.Add(new DataColumn("Warehouse", typeof(string)));
        }

        private static void SetupSalesDOLinesDataTable(DataTable salesTransactionLinesDataTable)
        {
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("LineNumber", typeof(int)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("ItemID", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("ItemName", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Unit", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Units", typeof(int)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Pieces", typeof(int)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Total", typeof(decimal)));
        }
    }
}
