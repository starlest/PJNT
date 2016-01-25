namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update5 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("SalesReturnTransactionLines");
            DropPrimaryKey("SalesTransactionLines");
            DropPrimaryKey("PurchaseReturnTransactionLines");
            DropPrimaryKey("PurchaseTransactionLines");
            AlterColumn("Inventory", "PurchasePrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Inventory", "SalesPrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Inventory", "SalesExpense", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesReturnTransactionLines", "SalesPrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesReturnTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesReturnTransactionLines", "ReturnPrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesReturnTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesReturnTransactionLines", "CostOfGoodsSold", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesReturnTransactions", "NetTotal", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactions", "Discount", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactions", "SalesExpense", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactions", "Total", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactions", "Paid", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactionLines", "SalesPrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Customers", "SalesReturnCredits", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Suppliers", "PurchaseReturnCredits", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "BeginningBalance", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance1", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance2", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance3", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance4", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance5", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance6", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance7", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance8", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance9", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance10", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance11", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Account_Balances", "Balance12", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_General", "Debit", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_General", "Credit", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("Ledger_Transaction_Lines", "Amount", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseReturnTransactionLines", "PurchasePrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseReturnTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseReturnTransactionLines", "ReturnPrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseReturnTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseReturnTransactions", "NetTotal", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactions", "Discount", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactions", "Tax", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactions", "Total", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactions", "Paid", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactionLines", "PurchasePrice", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("PurchaseTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AlterColumn("SalesCommissions", "Percentage", c => c.Decimal(nullable: false, precision: 50, scale: 30));
            AddPrimaryKey("SalesReturnTransactionLines", new[] { "SalesReturnTransactionID", "ItemID", "WarehouseID", "SalesPrice", "Discount", "ReturnPrice" });
            AddPrimaryKey("SalesTransactionLines", new[] { "SalesTransactionID", "ItemID", "WarehouseID", "SalesPrice", "Discount" });
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID", "ReturnWarehouseID", "PurchasePrice", "Discount", "ReturnPrice" });
            AddPrimaryKey("PurchaseTransactionLines", new[] { "PurchaseTransactionID", "ItemID", "WarehouseID", "PurchasePrice", "Discount" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("PurchaseTransactionLines");
            DropPrimaryKey("PurchaseReturnTransactionLines");
            DropPrimaryKey("SalesTransactionLines");
            DropPrimaryKey("SalesReturnTransactionLines");
            AlterColumn("SalesCommissions", "Percentage", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactionLines", "PurchasePrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactions", "Paid", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactions", "Total", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactions", "Tax", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactions", "Discount", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseReturnTransactions", "NetTotal", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseReturnTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseReturnTransactionLines", "ReturnPrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseReturnTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("PurchaseReturnTransactionLines", "PurchasePrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Transaction_Lines", "Amount", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_General", "Credit", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_General", "Debit", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance12", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance11", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance10", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance9", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance8", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance7", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance6", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance5", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance4", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance3", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance2", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "Balance1", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Ledger_Account_Balances", "BeginningBalance", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Suppliers", "PurchaseReturnCredits", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Customers", "SalesReturnCredits", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactionLines", "SalesPrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactions", "Paid", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactions", "Total", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactions", "SalesExpense", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactions", "Discount", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesReturnTransactions", "NetTotal", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesReturnTransactionLines", "CostOfGoodsSold", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesReturnTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesReturnTransactionLines", "ReturnPrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesReturnTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("SalesReturnTransactionLines", "SalesPrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Inventory", "SalesExpense", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Inventory", "SalesPrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AlterColumn("Inventory", "PurchasePrice", c => c.Decimal(nullable: false, precision: 38, scale: 18));
            AddPrimaryKey("PurchaseTransactionLines", new[] { "PurchaseTransactionID", "ItemID", "WarehouseID", "PurchasePrice", "Discount" });
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID", "ReturnWarehouseID", "PurchasePrice", "Discount", "ReturnPrice" });
            AddPrimaryKey("SalesTransactionLines", new[] { "SalesTransactionID", "ItemID", "WarehouseID", "SalesPrice", "Discount" });
            AddPrimaryKey("SalesReturnTransactionLines", new[] { "SalesReturnTransactionID", "ItemID", "WarehouseID", "SalesPrice", "Discount", "ReturnPrice" });
        }
    }
}
