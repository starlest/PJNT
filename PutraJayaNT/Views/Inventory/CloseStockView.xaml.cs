namespace ECRP.Views.Inventory
{
    using System;
    using System.ComponentModel;
    using Utilities;
    using ViewModels.Inventory;

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

            if (_isRunning || !UtilityMethods.GetMasterAdminVerification()) return;
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
