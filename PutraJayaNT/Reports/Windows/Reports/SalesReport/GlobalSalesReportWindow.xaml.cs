namespace ECERP.Reports.Windows.Reports.SalesReport
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Windows;
    using Microsoft.Reporting.WinForms;
    using ViewModels.Sales;

    /// <summary>
    /// Interaction logic for GlobalSalesReportWindow.xaml
    /// </summary>
    public partial class GlobalSalesReportWindow
    {
        private readonly ObservableCollection<SalesTransactionLineVM>  _salesTransactionLines;

        public GlobalSalesReportWindow(ObservableCollection<SalesTransactionLineVM> salesTransactionLines)
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

            dt.Columns.Add(new DataColumn("ID", typeof(string)));
            dt.Columns.Add(new DataColumn("Product", typeof(string)));
            dt.Columns.Add(new DataColumn("Unit", typeof(string)));
            dt.Columns.Add(new DataColumn("SUnit", typeof(string)));
            dt.Columns.Add(new DataColumn("Units", typeof(int)));
            dt.Columns.Add(new DataColumn("SUnits", typeof(int)));
            dt.Columns.Add(new DataColumn("Pieces", typeof(int)));

            foreach (var line in _salesTransactionLines)
            {
                var dr = dt.NewRow();
                dr["ID"] = line.Item.ItemID;
                dr["Product"] = line.Item.Name;
                dr["Unit"] = line.Item.UnitName;
                dr["SUnit"] = line.Item.SecondaryUnitName;
                dr["Units"] = line.Quantity / line.Item.PiecesPerUnit;
                dr["SUnits"] = line.Item.PiecesPerSecondaryUnit != 0 ? line.Quantity % line.Item.PiecesPerUnit / line.Item.PiecesPerSecondaryUnit : 0;
                dr["Pieces"] = line.Item.PiecesPerSecondaryUnit != 0 ? line.Quantity % line.Item.PiecesPerUnit % line.Item.PiecesPerSecondaryUnit : line.Quantity % line.Item.PiecesPerUnit;
                dt.Rows.Add(dr);
            }

            var reportDataSource = new ReportDataSource("GlobalSalesTransactionLineDataSet", dt);

            reportViewer.LocalReport.ReportPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"Reports\\RDLC\\Reports\\SalesReport\\GlobalSalesReport.rdlc"); // Path of the rdlc file

            reportViewer.LocalReport.DataSources.Add(reportDataSource);
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.RefreshReport();
        }
    }
}
