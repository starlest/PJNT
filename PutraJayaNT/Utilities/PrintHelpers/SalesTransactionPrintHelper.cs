namespace ECERP.Utilities.PrintHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using ModelHelpers;
    using Models.Inventory;
    using Models.Sales;

    public static class SalesTransactionPrintHelper
    {
        public static void PrintSalesTransactionDOFromDatabase(SalesTransaction salesTransaction)
        {
            var salesTransactionFromDatabase = GetSalesTransactionFromDatabase(salesTransaction);
            var localReports = CreateSalesInvoiceDOLocalReports(salesTransactionFromDatabase);
            PrintDOLocalReports(localReports);
            SetSalesTransactionDOPrintedStatusToTrue(salesTransaction);
        }

        public static void PrintSalesTransactionFromDatabaseInvoice(SalesTransaction salesTransaction)
        {
            var salesTransactionFromDatabase = GetSalesTransactionFromDatabase(salesTransaction);
            var localReport = CreateSalesTransactionInvoiceLocalReport(salesTransactionFromDatabase);
            PrintInvoiceLocalReport(localReport);
            SetSalesTransactionInvoicePrintedStatusToTrue(salesTransaction);
        }

        private static SalesTransaction GetSalesTransactionFromDatabase(SalesTransaction salesTransaction)
        {
            using (var context = UtilityMethods.createContext())
            {
                return context.SalesTransactions
                    .Include("Customer")
                    .Include("CollectionSalesman")
                    .Include("SalesReturnTransactions")
                    .Include("SalesTransactionLines")
                    .Include("SalesTransactionLines.Salesman")
                    .Include("SalesTransactionLines.Item")
                    .Include("SalesTransactionLines.Warehouse")
                    .SingleOrDefault(e => e.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));
            }
        }

        #region Print DO Local Report Helper Methods
        private static void SetSalesTransactionDOPrintedStatusToTrue(SalesTransaction salesTransaction)
        {
            using (var context = UtilityMethods.createContext())
            {
                var salesTransactionFromDatabase = context.SalesTransactions.Single(
                    transaction => transaction.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));

                if (salesTransactionFromDatabase.DOPrinted) return;
                salesTransactionFromDatabase.DOPrinted = true;
                context.SaveChanges();
            }
        }

        private static IEnumerable<LocalReport> CreateSalesInvoiceDOLocalReports(SalesTransaction salesTransaction)
        {
            var reports = new List<LocalReport>();
            using (var context = UtilityMethods.createContext())
            {
                var warehouses = context.Warehouses;
                foreach (var warehouse in warehouses)
                {
                    if (IsThereSalesTransactionItemFromThisWarehouse(salesTransaction, warehouse))
                        reports.Add(CreateSalesInvoiceDOLocalReport(salesTransaction, warehouse));
                }
            }
            return reports;
        }

        private static bool IsThereSalesTransactionItemFromThisWarehouse(SalesTransaction salesTransaction, Warehouse warehouse)
        {
            return salesTransaction.SalesTransactionLines.ToList().Any(line => line.Warehouse.ID.Equals(warehouse.ID));
        }

        public static LocalReport CreateSalesInvoiceDOLocalReport(SalesTransaction salesTransaction, Warehouse warehouse)
        {
            var salesDODataTable = new DataTable();
            var salesDOLinesDataTable = new DataTable();
            SetupSalesDODataTable(salesDODataTable);
            SetupSalesDOLinesDataTable(salesDOLinesDataTable);
            LoadSalesDODatable(salesTransaction, warehouse, salesDODataTable);
            LoadSalesDOLinesDataTable(salesTransaction, warehouse, salesDOLinesDataTable);

            var salesInvoiceDOLocalReport = new LocalReport
            {
                ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesDOReport.rdlc")
            };

            var salesDODataSource = new ReportDataSource("SalesInvoiceDataSet", salesDODataTable);
            var salesDOLineDataSetDataSource = new ReportDataSource("SalesInvoiceLineDataSet", salesDOLinesDataTable);
            salesInvoiceDOLocalReport.DataSources.Add(salesDODataSource);
            salesInvoiceDOLocalReport.DataSources.Add(salesDOLineDataSetDataSource);

            return salesInvoiceDOLocalReport;
        }

        private static void LoadSalesDOLinesDataTable(SalesTransaction salesTransaction, Warehouse warehouse, DataTable salesTransactionLinesDataTable)
        {
            foreach (
                var line in
                    salesTransaction.SalesTransactionLines.Where(e => e.Warehouse.ID.Equals(warehouse.ID)).ToList())
            {
                var salesTransactionLinesDataTableRow = salesTransactionLinesDataTable.NewRow();
                salesTransactionLinesDataTableRow["ItemID"] = line.Item.ItemID;
                salesTransactionLinesDataTableRow["ItemName"] = line.Item.Name;
                salesTransactionLinesDataTableRow["UnitName"] = InventoryHelper.GetItemUnitName(line.Item);
                salesTransactionLinesDataTableRow["QuantityPerUnit"] = InventoryHelper.GetItemQuantityPerUnit(line.Item);
                salesTransactionLinesDataTableRow["Quantity"] = InventoryHelper.ConvertItemQuantityTostring(line.Item, line.Quantity);
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
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("ItemID", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("ItemName", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("UnitName", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("QuantityPerUnit", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Quantity", typeof(string)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            salesTransactionLinesDataTable.Columns.Add(new DataColumn("Total", typeof(decimal)));
        }

        public static void PrintDOLocalReports(IEnumerable<LocalReport> localReports)
        {
            var printHelper = new PrintHelper();
            try
            {
                foreach (var report in localReports)
                {
                    printHelper.Run(report);
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK);
            }

            finally
            {
                printHelper.Dispose();
            }
        }
        #endregion

        #region Print Invoice Helper Methods
        private static void SetSalesTransactionInvoicePrintedStatusToTrue(SalesTransaction salesTransaction)
        {
            using (var context = UtilityMethods.createContext())
            {
                var salesTransactionFromDatabase = context.SalesTransactions.Single(
                    transaction => transaction.SalesTransactionID.Equals(salesTransaction.SalesTransactionID));

                if (salesTransactionFromDatabase.InvoicePrinted) return;
                salesTransactionFromDatabase.InvoicePrinted = true;
                context.SaveChanges();
            }
        }

        public static LocalReport CreateSalesTransactionInvoiceLocalReport(SalesTransaction salesTransaction)
        {
            var salesInvoiceDataTable = new DataTable();
            var salesInvoiceLinesDataTable = new DataTable();
            SetupSalesInvoiceDataTable(salesInvoiceDataTable);
            SetupSalesInvoiceLinesDataTable(salesInvoiceLinesDataTable);
            LoadSalesInvoiceDataTable(salesInvoiceDataTable, salesTransaction);
            LoadSalesInvoiceLinesDataTable(salesInvoiceLinesDataTable, salesTransaction);

            var salesTransactionInvoiceLocalReport = new LocalReport
            {
                ReportPath =
                    System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\SalesInvoiceReport.rdlc")
            };

            var salesInvoiceDataSource = new ReportDataSource("SalesInvoiceDataSet", salesInvoiceDataTable);
            var salesInvoiceLinesDataSource = new ReportDataSource("SalesInvoiceLineDataSet", salesInvoiceLinesDataTable);
            salesTransactionInvoiceLocalReport.DataSources.Add(salesInvoiceLinesDataSource);
            salesTransactionInvoiceLocalReport.DataSources.Add(salesInvoiceDataSource);

            return salesTransactionInvoiceLocalReport;
        }

        private static void LoadSalesInvoiceDataTable(DataTable salesInvoiceDataTable, SalesTransaction salesTransaction)
        {

            var dr2 = salesInvoiceDataTable.NewRow();

            dr2["InvoiceGrossTotal"] = salesTransaction.GrossTotal;
            dr2["InvoiceDiscount"] = salesTransaction.Discount;
            dr2["InvoiceSalesExpense"] = salesTransaction.SalesExpense;
            dr2["InvoiceNetTotal"] = salesTransaction.NetTotal;
            dr2["Customer"] = salesTransaction.Customer.Name;
            dr2["Address"] = salesTransaction.Customer.City;
            dr2["InvoiceNumber"] = salesTransaction.SalesTransactionID;
            dr2["Date"] = salesTransaction.Date.ToString("dd-MM-yyyy");
            dr2["DueDate"] = salesTransaction.DueDate.ToString("dd-MM-yyyy");
            dr2["Notes"] = salesTransaction.Notes;
            dr2["Copy"] = salesTransaction.InvoicePrinted ? "Copy" : "";

            salesInvoiceDataTable.Rows.Add(dr2);
        }

        private static void SetupSalesInvoiceLinesDataTable(DataTable salesInvoiceLinesDataTable)
        {
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("ItemID", typeof(string)));
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("ItemName", typeof(string)));
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("UnitName", typeof(string)));
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("QuantityPerUnit", typeof(string)));
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("Quantity", typeof(string)));
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            salesInvoiceLinesDataTable.Columns.Add(new DataColumn("Total", typeof(decimal)));
        }

        private static void SetupSalesInvoiceDataTable(DataTable salesInvoiceDataTable)
        {
            salesInvoiceDataTable.Columns.Add(new DataColumn("InvoiceGrossTotal", typeof(decimal)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("InvoiceDiscount", typeof(decimal)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("InvoiceSalesExpense", typeof(decimal)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("InvoiceNetTotal", typeof(decimal)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("Customer", typeof(string)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("Address", typeof(string)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("InvoiceNumber", typeof(string)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("Date", typeof(string)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("DueDate", typeof(string)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("Notes", typeof(string)));
            salesInvoiceDataTable.Columns.Add(new DataColumn("Copy", typeof(string)));
        }

        private static void LoadSalesInvoiceLinesDataTable(DataTable salesInvoiceLinesDataTable, SalesTransaction salesTransaction)
        {
            foreach (var line in salesTransaction.SalesTransactionLines)
            {
                var row = salesInvoiceLinesDataTable.NewRow();
                row["ItemID"] = line.Item.ItemID;
                row["ItemName"] = line.Item.Name;
                row["UnitName"] = InventoryHelper.GetItemUnitName(line.Item);
                row["QuantityPerUnit"] = InventoryHelper.GetItemQuantityPerUnit(line.Item);
                row["Quantity"] = InventoryHelper.ConvertItemQuantityTostring(line.Item, line.Quantity);
                row["SalesPrice"] = line.SalesPrice * line.Item.PiecesPerUnit;
                row["Discount"] = line.Discount * line.Item.PiecesPerUnit;
                row["Total"] = line.Total;
                salesInvoiceLinesDataTable.Rows.Add(row);
            }
        }

        private static void PrintInvoiceLocalReport(LocalReport localReport)
        {
            var printHelper = new PrintHelper();
            try
            {
                printHelper.Run(localReport);
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK);
            }

            finally
            {
                printHelper.Dispose();
            }
        }
        #endregion
    }
}
