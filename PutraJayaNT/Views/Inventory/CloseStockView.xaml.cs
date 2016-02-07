using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Inventory;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Inventory
{
    /// <summary>
    /// Interaction logic for CloseStockView.xaml
    /// </summary>
    public partial class CloseStockView : UserControl
    {
        CloseStockVM vm;
        bool _isRunning = false;

        public CloseStockView()
        {
            InitializeComponent();
            vm = new CloseStockVM();
            DataContext = vm;
        }


        private void OnClick(object sender, EventArgs e)
        {
            //if (DateTime.Now.Date.AddDays(1).Day != 1 && DateTime.Now.Hour < 17)
            //{
            //    MessageBox.Show("Unable to close books at this time.", "Failed to Close Stock", MessageBoxButton.OK);
            //    return;
            //}

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            if (!_isRunning)
            {
                // Verification
                if (!UtilityMethods.GetVerification()) return;

                _isRunning = true;
                worker.RunWorkerAsync();
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            vm.Close((BackgroundWorker)sender);
            _isRunning = false;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }
    }
}
