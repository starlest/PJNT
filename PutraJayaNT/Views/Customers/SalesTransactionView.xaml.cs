using Microsoft.Reporting.WinForms;
using PutraJayaNT.ViewModels.Customers;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PutraJayaNT.Views.Customers
{
    /// <summary>
    /// Interaction logic for SalesTransactionView.xaml
    /// </summary>
    public partial class SalesTransactionView : UserControl
    {
        public SalesTransactionView()
        {
            InitializeComponent();
            var vm = new SalesTransactionVM();
            DataContext = vm;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = SalesIDTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }

        private void DiscountPercentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = DiscountPercentTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }

        private void OverallDiscountPercentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = OverallDiscountPercentTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }


        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
