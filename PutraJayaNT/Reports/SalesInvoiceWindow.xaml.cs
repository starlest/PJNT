using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PutraJayaNT.Reports
{
    /// <summary>
    /// Interaction logic for SalesInvoiceWindow.xaml
    /// </summary>
    public partial class SalesInvoiceWindow : ModernWindow
    {
        SalesTransactionVM _salesTransaction;

        public SalesInvoiceWindow(SalesTransactionVM vm)
        {
            InitializeComponent();
            _salesTransaction = vm;
        }

        private void reportViewer_RenderingComplete(object sender, Microsoft.Reporting.WinForms.RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("ItemID", typeof(string)));
            dt1.Columns.Add(new DataColumn("ItemName", typeof(string)));
            dt1.Columns.Add(new DataColumn("Unit", typeof(string)));
            dt1.Columns.Add(new DataColumn("Units", typeof(int)));
            dt1.Columns.Add(new DataColumn("Pieces", typeof(int)));
            dt1.Columns.Add(new DataColumn("SalesPrice", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Discount", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("InvoiceTotal", typeof(decimal)));

            foreach (var line in _salesTransaction.SalesTransactionLines)
            {
                DataRow dr = dt1.NewRow();
                dr["ItemID"] = line.Item.ItemID;
                dr["ItemName"] = line.Item.Name;
                dr["Unit"] = line.Item.PiecesPerUnit + "/" + line.Item.UnitName;
                dr["Units"] = line.Units;
                dr["Pieces"] = line.Pieces;
                dr["SalesPrice"] = line.SalesPrice;
                dr["Discount"] = line.Discount;
                dr["Total"] = line.Total;
                dr["InvoiceTotal"] = _salesTransaction.Total;
                dt1.Rows.Add(dr);
            }

            DataRow dr2 = dt2.NewRow();
            dt2.Columns.Add(new DataColumn("InvoiceTotal", typeof(decimal)));
            dt2.Columns.Add(new DataColumn("InvoiceDiscount", typeof(decimal)));
            dr2["InvoiceTotal"] = _salesTransaction.Total;
            dr2["InvoiceDiscount"] = 0;
            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("SalesInvoiceLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("SalesInvoiceDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\SalesInvoiceReport.rdlc"); // Path of the rdlc file
       
            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);

            reportViewer.RefreshReport();
        }
    }
}
