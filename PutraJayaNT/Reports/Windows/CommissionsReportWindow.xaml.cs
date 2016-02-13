using FirstFloor.ModernUI.Windows.Controls;
using Microsoft.Reporting.WinForms;
using PutraJayaNT.Models.Sales;
using PutraJayaNT.ViewModels.Customers;
using PutraJayaNT.ViewModels.Reports;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace PutraJayaNT.Reports.Windows
{
    /// <summary>
    /// Interaction logic for StockCardReportWindow.xaml
    /// </summary>
    public partial class CommissionsReportWindow : ModernWindow
    {
        CommissionsReportVM _commissionsReport;

        public CommissionsReportWindow(CommissionsReportVM commissionsReport)
        {
            InitializeComponent();
            _commissionsReport = commissionsReport;
        }

        private void reportViewer_RenderingComplete(object sender, RenderingCompleteEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add(new DataColumn("Salesman", typeof(string)));
            dt1.Columns.Add(new DataColumn("Category", typeof(string)));
            dt1.Columns.Add(new DataColumn("Percentage", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Total", typeof(decimal)));
            dt1.Columns.Add(new DataColumn("Commission", typeof(decimal)));

            dt2.Columns.Add(new DataColumn("FromDate", typeof(string)));
            dt2.Columns.Add(new DataColumn("ToDate", typeof(string)));
            dt2.Columns.Add(new DataColumn("Total", typeof(decimal)));

            foreach (var line in _commissionsReport.Lines)
            {
                if (line.Salesman.Name.Equals("Office")) continue;

                DataRow dr1 = dt1.NewRow();
                dr1["Salesman"] = line.Salesman.Name;
                dr1["Category"] = line.Category.Name;
                dr1["Percentage"] = line.Percentage;
                dr1["Total"] = Math.Round(line.Total, 2);
                dr1["Commission"] = Math.Round(line.Commission, 2);
                dt1.Rows.Add(dr1);
            }

            DataRow dr2 = dt2.NewRow();
            dr2["FromDate"] = _commissionsReport.ToDate.ToString("dd-MM-yyyy");
            dr2["ToDate"] = _commissionsReport.FromDate.ToString("dd-MM-yyyy");
            dr2["Total"] = Math.Round(_commissionsReport.Total, 2);
            dt2.Rows.Add(dr2);

            ReportDataSource reportDataSource1 = new ReportDataSource("CommissionsReportLineDataSet", dt1);
            ReportDataSource reportDataSource2 = new ReportDataSource("CommissionsReportDataSet", dt2);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\CommissionsReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
