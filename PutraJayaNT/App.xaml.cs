using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace PutraJayaNT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            var MyComboBox = sender as ComboBox;

            if (MyComboBox != null && MyComboBox.Text.Length > 0)
            {
                if (e.Key == Key.Enter)
                {
                    BindingExpression binding = MyComboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
                    if (binding != null) binding.UpdateSource();
                }
            }
        }
    }
}
