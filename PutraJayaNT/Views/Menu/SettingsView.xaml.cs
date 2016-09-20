namespace ECRP.Views.Menu
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Linq;
    using System.Windows;
    using MySql.Data.MySqlClient;
    using Utilities;

    public partial class SettingsView
    {
        private bool _isRunning;
        private string _filename;

        public SettingsView()
        {
            InitializeComponent();
        }

        private void Export()
        {
            var constring = ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString;

            using (var conn = new MySqlConnection(string.Format(constring, UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress())))
            {
                using (var cmd = new MySqlCommand())
                {
                    using (var mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(_filename);
                        mb.ExportInfo.RecordDumpTime = true;
                        conn.Close();
                    }
                    MessageBox.Show("Backup is successful", "Success", MessageBoxButton.OK);
                }
            }
        }

        private void Browse(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".sql",
                Filter = "sql Files (*.sql)|*.sql"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == false) return;

            // Open document 
            _filename = dlg.FileName;
            backupPath.Text = _filename;
        }

        private void Backup(object sender, RoutedEventArgs e)
        {
            if (_filename == null)
            {
                MessageBox.Show("Please select a path to backup.", "Empty Backup Path", MessageBoxButton.OK);
                return;
            }

            var worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += worker_BackupDatabase;
            worker.ProgressChanged += worker_ProgressChanged;

            if (!_isRunning)
            {
                _isRunning = true;
                pbStatus.IsIndeterminate = true;
                worker.RunWorkerAsync();
                pbStatus.IsIndeterminate = false;
            }
        }

        private void worker_BackupDatabase(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            backgroundWorker?.ReportProgress(0);
            backgroundWorker?.ReportProgress(50);
            Export();
            backgroundWorker?.ReportProgress(100);
            _isRunning = false;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage * 2;
        }

        private void Decrease_Day_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                $"The date now is: {UtilityMethods.GetCurrentDate().ToString("dd-MM-yyyy")} \n Confirm decreasing day?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            if (!UtilityMethods.GetMasterAdminVerification()) return;

            using (var context = UtilityMethods.createContext())
            {
                var currentDate = context.Dates.FirstOrDefault(x => x.Name.Equals("Current"));
                if (currentDate != null) currentDate.DateTime = currentDate.DateTime.AddDays(-1);
                context.SaveChanges();
            }
        }

        private void Increase_Day_Button_Clicked(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                $"The date now is: {UtilityMethods.GetCurrentDate().ToString("dd-MM-yyyy")} \n Confirm increasing day?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            using (var context = UtilityMethods.createContext())
            {
                var currentDate = context.Dates.FirstOrDefault(x => x.Name.Equals("Current"));
                if (currentDate != null)
                {
                    currentDate.DateTime = currentDate.DateTime.AddDays(1);
                }
                context.SaveChanges();
            }
        }
    }
}
