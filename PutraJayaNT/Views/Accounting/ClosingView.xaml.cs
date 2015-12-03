using PutraJayaNT.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Accounting
{
    /// <summary>
    /// Interaction logic for ClosingView.xaml
    /// </summary>
    public partial class ClosingView : UserControl
    {
        ClosingVM vm;

        public ClosingView()
        {
            InitializeComponent();
            vm = new ClosingVM();
            DataContext = vm;
        }

        bool isRunning = false;

        private void OnClick(object sender, EventArgs e)
        {
            if (DateTime.Now.AddDays(1).Day != 1 && DateTime.Now.Hour < 22)
            {
                MessageBox.Show("Unable to close books at this time.", "Failed to Close Books", MessageBoxButton.OK);
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
