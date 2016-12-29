namespace ECERP.Views
{
    using System.Windows.Input;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ConnectIPWindow.xaml
    /// </summary>
    public partial class ConnectIPWindow
    {
        public ConnectIPWindow()
        {
            InitializeComponent();
            var vm = new ConnectIPVM();
            DataContext = vm;
            FocusManager.SetFocusedElement(this, IPAddressBox);
        }
    }
}
