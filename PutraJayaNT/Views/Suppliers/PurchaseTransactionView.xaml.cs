﻿using PutraJayaNT.ViewModels;
using PutraJayaNT.ViewModels.Suppliers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PutraJayaNT.Views.Suppliers
{
    /// <summary>
    /// Interaction logic for PurchaseTransactionView.xaml
    /// </summary>
    public partial class PurchaseTransactionView : UserControl
    {
        public PurchaseTransactionView()
        {
            InitializeComponent();
            var vm = new PurchaseTransactionVM();
            DataContext = vm;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = PurchaseIDTextBox.GetBindingExpression(TextBox.TextProperty);
                if (exp != null) exp.UpdateSource();
            }
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            var MyComboBox = sender as ComboBox;

            if (MyComboBox != null && MyComboBox.Text.Length > 0)
            {
                if (e.Key == Key.Enter)
                {
                    BindingExpression binding = MyComboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
                    binding.UpdateSource();
                }
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
