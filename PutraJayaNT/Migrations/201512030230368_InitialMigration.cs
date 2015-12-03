namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Inventory",
                c => new
                    {
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Name = c.String(unicode: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Stock = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        Category_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ItemID)
                .ForeignKey("dbo.Categories", t => t.Category_ID)
                .Index(t => t.Category_ID);
            
            CreateTable(
                "dbo.Suppliers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100, storeType: "nvarchar"),
                        Address = c.String(nullable: false, unicode: false),
                        GSTID = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.SalesTransactionLines",
                c => new
                    {
                        SalesTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Quantity = c.Int(nullable: false),
                        SalesPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.SalesTransactionID, t.ItemID })
                .ForeignKey("dbo.Inventory", t => t.ItemID, cascadeDelete: true)
                .ForeignKey("dbo.SalesTransactions", t => t.SalesTransactionID, cascadeDelete: true)
                .Index(t => t.SalesTransactionID)
                .Index(t => t.ItemID);
            
            CreateTable(
                "dbo.SalesTransactions",
                c => new
                    {
                        SalesTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        CashierName = c.String(maxLength: 128, storeType: "nvarchar"),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        When = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.SalesTransactionID)
                .ForeignKey("dbo.Users", t => t.CashierName)
                .Index(t => t.CashierName)
                .Index(t => t.When);
            
            CreateTable(
                "dbo.SalesReturnTransactions",
                c => new
                    {
                        SalesReturnTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Date = c.DateTime(nullable: false, precision: 0),
                        SalesTransaction_SalesTransactionID = c.String(maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.SalesReturnTransactionID)
                .ForeignKey("dbo.SalesTransactions", t => t.SalesTransaction_SalesTransactionID)
                .Index(t => t.SalesTransaction_SalesTransactionID);
            
            CreateTable(
                "dbo.SalesReturnTransactionLines",
                c => new
                    {
                        SalesReturnTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Quantity = c.Int(nullable: false),
                        SalesPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CostOfGoodsSold = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.SalesReturnTransactionID, t.ItemID })
                .ForeignKey("dbo.Inventory", t => t.ItemID, cascadeDelete: true)
                .ForeignKey("dbo.SalesReturnTransactions", t => t.SalesReturnTransactionID, cascadeDelete: true)
                .Index(t => t.SalesReturnTransactionID)
                .Index(t => t.ItemID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Password = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Username);
            
            CreateTable(
                "dbo.Ledger_Account_Balances",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        PeriodYear = c.Int(nullable: false),
                        BeginningBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance1 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance2 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance3 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance4 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance5 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance6 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance7 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance8 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance9 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance10 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance11 = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Balance12 = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Ledger_Accounts", t => t.ID)
                .Index(t => t.ID);
            
            CreateTable(
                "dbo.Ledger_Accounts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100, storeType: "nvarchar"),
                        Notes = c.String(unicode: false),
                        Class = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.Ledger_General",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        PeriodYear = c.Int(nullable: false),
                        Period = c.Int(nullable: false),
                        Debit = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Credit = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Ledger_Accounts", t => t.ID)
                .Index(t => t.ID);
            
            CreateTable(
                "dbo.Ledger_Transaction_Lines",
                c => new
                    {
                        LedgerTransactionID = c.Int(nullable: false),
                        LedgerAccountID = c.Int(nullable: false),
                        Seq = c.String(nullable: false, unicode: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.LedgerTransactionID, t.LedgerAccountID })
                .ForeignKey("dbo.Ledger_Accounts", t => t.LedgerAccountID, cascadeDelete: true)
                .ForeignKey("dbo.Ledger_Transactions", t => t.LedgerTransactionID, cascadeDelete: true)
                .Index(t => t.LedgerTransactionID)
                .Index(t => t.LedgerAccountID);
            
            CreateTable(
                "dbo.Ledger_Transactions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false, precision: 0),
                        Documentation = c.String(unicode: false),
                        Description = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.Date);
            
            CreateTable(
                "dbo.PurchaseReturnTransactionLines",
                c => new
                    {
                        PurchaseReturnTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Quantity = c.Int(nullable: false),
                        PurchasePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.PurchaseReturnTransactionID, t.ItemID })
                .ForeignKey("dbo.Inventory", t => t.ItemID, cascadeDelete: true)
                .ForeignKey("dbo.PurchaseReturnTransactions", t => t.PurchaseReturnTransactionID, cascadeDelete: true)
                .Index(t => t.PurchaseReturnTransactionID)
                .Index(t => t.ItemID);
            
            CreateTable(
                "dbo.PurchaseReturnTransactions",
                c => new
                    {
                        PurchaseReturnTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Date = c.DateTime(nullable: false, precision: 0),
                        PurchaseTransaction_PurchaseID = c.String(maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.PurchaseReturnTransactionID)
                .ForeignKey("dbo.PurchaseTransactions", t => t.PurchaseTransaction_PurchaseID)
                .Index(t => t.PurchaseTransaction_PurchaseID);
            
            CreateTable(
                "dbo.PurchaseTransactions",
                c => new
                    {
                        PurchaseID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Date = c.DateTime(nullable: false, precision: 0),
                        DueDate = c.DateTime(nullable: false, precision: 0),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Paid = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Supplier_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PurchaseID)
                .ForeignKey("dbo.Suppliers", t => t.Supplier_ID, cascadeDelete: true)
                .Index(t => t.Date)
                .Index(t => t.DueDate)
                .Index(t => t.Supplier_ID);
            
            CreateTable(
                "dbo.PurchaseTransactionLines",
                c => new
                    {
                        PurchaseID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        PurchasePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Quantity = c.Int(nullable: false),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => new { t.PurchaseID, t.ItemID })
                .ForeignKey("dbo.Inventory", t => t.ItemID, cascadeDelete: true)
                .ForeignKey("dbo.PurchaseTransactions", t => t.PurchaseID, cascadeDelete: true)
                .Index(t => t.PurchaseID)
                .Index(t => t.ItemID);
            
            CreateTable(
                "dbo.SupplierItems",
                c => new
                    {
                        Supplier_ID = c.Int(nullable: false),
                        Item_ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.Supplier_ID, t.Item_ItemID })
                .ForeignKey("dbo.Suppliers", t => t.Supplier_ID, cascadeDelete: true)
                .ForeignKey("dbo.Inventory", t => t.Item_ItemID, cascadeDelete: true)
                .Index(t => t.Supplier_ID)
                .Index(t => t.Item_ItemID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PurchaseReturnTransactionLines", "PurchaseReturnTransactionID", "dbo.PurchaseReturnTransactions");
            DropForeignKey("dbo.PurchaseTransactions", "Supplier_ID", "dbo.Suppliers");
            DropForeignKey("dbo.PurchaseTransactionLines", "PurchaseID", "dbo.PurchaseTransactions");
            DropForeignKey("dbo.PurchaseTransactionLines", "ItemID", "dbo.Inventory");
            DropForeignKey("dbo.PurchaseReturnTransactions", "PurchaseTransaction_PurchaseID", "dbo.PurchaseTransactions");
            DropForeignKey("dbo.PurchaseReturnTransactionLines", "ItemID", "dbo.Inventory");
            DropForeignKey("dbo.Ledger_Account_Balances", "ID", "dbo.Ledger_Accounts");
            DropForeignKey("dbo.Ledger_Transaction_Lines", "LedgerTransactionID", "dbo.Ledger_Transactions");
            DropForeignKey("dbo.Ledger_Transaction_Lines", "LedgerAccountID", "dbo.Ledger_Accounts");
            DropForeignKey("dbo.Ledger_General", "ID", "dbo.Ledger_Accounts");
            DropForeignKey("dbo.SalesTransactions", "CashierName", "dbo.Users");
            DropForeignKey("dbo.SalesTransactionLines", "SalesTransactionID", "dbo.SalesTransactions");
            DropForeignKey("dbo.SalesReturnTransactionLines", "SalesReturnTransactionID", "dbo.SalesReturnTransactions");
            DropForeignKey("dbo.SalesReturnTransactionLines", "ItemID", "dbo.Inventory");
            DropForeignKey("dbo.SalesReturnTransactions", "SalesTransaction_SalesTransactionID", "dbo.SalesTransactions");
            DropForeignKey("dbo.SalesTransactionLines", "ItemID", "dbo.Inventory");
            DropForeignKey("dbo.SupplierItems", "Item_ItemID", "dbo.Inventory");
            DropForeignKey("dbo.SupplierItems", "Supplier_ID", "dbo.Suppliers");
            DropForeignKey("dbo.Inventory", "Category_ID", "dbo.Categories");
            DropIndex("dbo.SupplierItems", new[] { "Item_ItemID" });
            DropIndex("dbo.SupplierItems", new[] { "Supplier_ID" });
            DropIndex("dbo.PurchaseTransactionLines", new[] { "ItemID" });
            DropIndex("dbo.PurchaseTransactionLines", new[] { "PurchaseID" });
            DropIndex("dbo.PurchaseTransactions", new[] { "Supplier_ID" });
            DropIndex("dbo.PurchaseTransactions", new[] { "DueDate" });
            DropIndex("dbo.PurchaseTransactions", new[] { "Date" });
            DropIndex("dbo.PurchaseReturnTransactions", new[] { "PurchaseTransaction_PurchaseID" });
            DropIndex("dbo.PurchaseReturnTransactionLines", new[] { "ItemID" });
            DropIndex("dbo.PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID" });
            DropIndex("dbo.Ledger_Transactions", new[] { "Date" });
            DropIndex("dbo.Ledger_Transaction_Lines", new[] { "LedgerAccountID" });
            DropIndex("dbo.Ledger_Transaction_Lines", new[] { "LedgerTransactionID" });
            DropIndex("dbo.Ledger_General", new[] { "ID" });
            DropIndex("dbo.Ledger_Accounts", new[] { "Name" });
            DropIndex("dbo.Ledger_Account_Balances", new[] { "ID" });
            DropIndex("dbo.SalesReturnTransactionLines", new[] { "ItemID" });
            DropIndex("dbo.SalesReturnTransactionLines", new[] { "SalesReturnTransactionID" });
            DropIndex("dbo.SalesReturnTransactions", new[] { "SalesTransaction_SalesTransactionID" });
            DropIndex("dbo.SalesTransactions", new[] { "When" });
            DropIndex("dbo.SalesTransactions", new[] { "CashierName" });
            DropIndex("dbo.SalesTransactionLines", new[] { "ItemID" });
            DropIndex("dbo.SalesTransactionLines", new[] { "SalesTransactionID" });
            DropIndex("dbo.Suppliers", new[] { "Name" });
            DropIndex("dbo.Inventory", new[] { "Category_ID" });
            DropTable("dbo.SupplierItems");
            DropTable("dbo.PurchaseTransactionLines");
            DropTable("dbo.PurchaseTransactions");
            DropTable("dbo.PurchaseReturnTransactions");
            DropTable("dbo.PurchaseReturnTransactionLines");
            DropTable("dbo.Ledger_Transactions");
            DropTable("dbo.Ledger_Transaction_Lines");
            DropTable("dbo.Ledger_General");
            DropTable("dbo.Ledger_Accounts");
            DropTable("dbo.Ledger_Account_Balances");
            DropTable("dbo.Users");
            DropTable("dbo.SalesReturnTransactionLines");
            DropTable("dbo.SalesReturnTransactions");
            DropTable("dbo.SalesTransactions");
            DropTable("dbo.SalesTransactionLines");
            DropTable("dbo.Suppliers");
            DropTable("dbo.Inventory");
            DropTable("dbo.Categories");
        }
    }
}
