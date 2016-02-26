using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Accounting;
using System;
using System.ComponentModel;
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
            if (UtilityMethods.GetCurrentDate().AddDays(1).Day != 1 && DateTime.Now.Hour < 5)
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
                // Verification
                if (!UtilityMethods.GetVerification()) return;

                isRunning = true;
                worker.RunWorkerAsync();
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            vm.Close((BackgroundWorker)sender);
            isRunning = false;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }
    }
}
