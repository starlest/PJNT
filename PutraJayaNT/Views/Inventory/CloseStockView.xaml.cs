using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Inventory;
using System;
using System.ComponentModel;

namespace PutraJayaNT.Views.Inventory
{
    /// <summary>
    /// Interaction logic for CloseStockView.xaml
    /// </summary>
    public partial class CloseStockView
    {
        private readonly CloseStockVM vm;
        private bool _isRunning;

        public CloseStockView()
        {
            InitializeComponent();
            vm = new CloseStockVM();
            DataContext = vm;
        }


        private void OnClick(object sender, EventArgs e)
        {
            var worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            if (_isRunning || !UtilityMethods.GetVerification()) return;
            _isRunning = true;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            vm.Close((BackgroundWorker)sender);
            _isRunning = false;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }
    }
}
