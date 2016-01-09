using PutraJayaNT.ViewModels.Accounting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PutraJayaNT.Views.Accounting
{
    /// <summary>
    /// Interaction logic for OperationsExpenseView.xaml
    /// </summary>
    public partial class OperatingExpenseView : UserControl
    {
        public OperatingExpenseView()
        {
            InitializeComponent();
            var vm = new OperatingExpenseVM();
            DataContext = vm;
        }

        void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg != null)
            {
                DataGridRow dgr = (DataGridRow)(dg.ItemContainerGenerator.ContainerFromIndex(dg.SelectedIndex));
                if (e.Key == Key.Delete && !dgr.IsEditing)
                {
                    // User is attempting to delete the row
                    var result = MessageBox.Show(
                        "Confirm Deletion?",
                        "Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);
                    e.Handled = (result == MessageBoxResult.No);
                }
            }
        }
    }
}
