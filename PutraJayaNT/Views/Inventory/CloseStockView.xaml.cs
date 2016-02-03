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

        public CloseStockView()
        {
            InitializeComponent();
            vm = new CloseStockVM();
            DataContext = vm;
        }

        bool isRunning = false;

        private void OnClick(object sender, EventArgs e)
        {
            // Verification
            if (!UtilityMethods.GetVerification()) return;

            if (DateTime.Now.Date.AddDays(1).Day != 1 && DateTime.Now.Hour < 17)
            {
                MessageBox.Show("Unable to close books at this time.", "Failed to Close Stock", MessageBoxButton.OK);
                return;
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            if (!isRunning)
            {
                isRunning = true;
                pbStatus.IsIndeterminate = true;
                worker.RunWorkerAsync();
                pbStatus.IsIndeterminate = false;
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 50; i++)
            {
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep(100);
                if (i == 50) vm.Close();
            }
            isRunning = false;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage * 2;
        }
    }
}
