using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PutraJayaNT.Views.Menu
{
    /// <summary>
    /// Interaction logic for BackupRestoreView.xaml
    /// </summary>
    public partial class BackupRestoreView : UserControl
    {
        bool isRunning = false;
        bool isDone = false;
        string filename = null;

        public BackupRestoreView()
        {
            InitializeComponent();
        }

        private void Export()
        {
            string constring = "server=localhost;port=3306;database=putrajayant;uid=root;password=root";

            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(filename);
                        mb.ExportInfo.RecordDumpTime = true;
                        conn.Close();
                    }
                    MessageBox.Show("Backup is successful", "Success", MessageBoxButton.OK);
                }
                isDone = true;
            }
        }

        private void Browse(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".sql";
            dlg.Filter = "sql Files (*.sql)|*.sql";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                filename = dlg.FileName;
                BackupPath.Text = filename;
            }
        }

        private void Backup(object sender, RoutedEventArgs e)
        {
            if (filename == null)
            {
                MessageBox.Show("Please select a path to backup.", "Empty Backup Path", MessageBoxButton.OK);
                return;
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            if (!isRunning)
            {
                isRunning = true;
                isDone = false;
                pbStatus.IsIndeterminate = true;
                worker.RunWorkerAsync();
                pbStatus.IsIndeterminate = false;
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Export();
            for (int i = 0; i <= 50; i++)
            {
                if (isDone)
                {
                    (sender as BackgroundWorker).ReportProgress(100);
                    isRunning = false;
                    return;
                }
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep(100);
            }
            // Wait for backup to be done if it isn't yet
            while (!isDone) { }
            isRunning = false;
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage * 2;
        }
    }
}
