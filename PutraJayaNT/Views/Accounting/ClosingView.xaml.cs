using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Accounting;
using System;
using System.ComponentModel;
using System.Windows;

namespace PutraJayaNT.Views.Accounting
{
    /// <summary>
    /// Interaction logic for ClosingView.xaml
    /// </summary>
    public partial class ClosingView
    {
        private readonly ClosingVM vm;

        public ClosingView()
        {
            InitializeComponent();
            vm = new ClosingVM();
            DataContext = vm;
        }

        private bool isRunning;

        private void OnClick(object sender, EventArgs e)
        {
            if (UtilityMethods.GetCurrentDate().AddDays(1).Day != 1 && DateTime.Now.Hour < 5)
            {
                MessageBox.Show("Unable to close books at this time.", "Failed to Close Books", MessageBoxButton.OK);
                return;
            }

            var worker = new BackgroundWorker {WorkerReportsProgress = true};
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            if (isRunning) return;
            if (!UtilityMethods.GetMasterAdminVerification()) return;
            isRunning = true;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            vm.Close((BackgroundWorker)sender);
            isRunning = false;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }
    }
}
