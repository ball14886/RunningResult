using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RunningResult
{
    public partial class MainWindow : Window
    {
        List<RunnerResult> _List = new List<RunnerResult>();
        string _eventName = "";
        string _distance = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Start

        #region Menus
        private void StartMenu_Click(object sender, RoutedEventArgs e)
        {
            StartGrid.Visibility = Visibility.Visible;
            SummaryGrid.Visibility = Visibility.Collapsed;
        }

        private void Summary_Click(object sender, RoutedEventArgs e)
        {
            SummaryGrid.Visibility = Visibility.Visible;
            StartGrid.Visibility = Visibility.Collapsed;
        }

        private void Danger_Click(object sender, RoutedEventArgs e)
        {
            if (ClearDBRecords.Visibility == Visibility.Visible)
            {
                ClearDBRecords.Visibility = Visibility.Collapsed;
                ClearSingleRun5K.Visibility = Visibility.Collapsed;
                ClearSingleRun10K.Visibility = Visibility.Collapsed;
                ClearSingleRun20K.Visibility = Visibility.Collapsed;
            }
            else
            {
                ClearDBRecords.Visibility = Visibility.Visible;
                ClearSingleRun5K.Visibility = Visibility.Visible;
                ClearSingleRun10K.Visibility = Visibility.Visible;
                ClearSingleRun20K.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Functions
        public void StartRunning(string eventName, string distance)
        {
            try
            {
                var now = DateTime.Now;
                List<RunnerInfo> runners = new List<RunnerInfo>();
                using (RunnerDataContext db = new RunnerDataContext())
                {
                    runners = db.RunnerInfos.Where(x => x.Event == eventName && x.Distance == distance).ToList();

                    List<RunnerScanDateTime> insertingList = new List<RunnerScanDateTime>();
                    foreach (var runner in runners)
                    {
                        RunnerScanDateTime record = new RunnerScanDateTime()
                        {
                            RunnerIdentification = runner.RunnerBIB,
                            ScannedDateTime = now
                        };
                        insertingList.Add(record);
                    }

                    db.RunnerScanDateTimes.InsertAllOnSubmit(insertingList);
                    db.SubmitChanges();
                }
                MessageBox.Show($"Start {eventName} {distance} successfully");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ClearRunningRecord(string eventName, string distance)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"Confirm clearing {eventName} {distance} runner record?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    using (RunnerDataContext db = new RunnerDataContext())
                    {
                        var runnerInfos = db.RunnerInfos.Where(x => x.Event == eventName && x.Distance == distance);
                        var runnerScans = db.RunnerScanDateTimes.Where(x => runnerInfos.Select(y => y.RunnerBIB).Contains(x.RunnerIdentification));

                        if (runnerScans.Any())
                        {
                            db.RunnerScanDateTimes.DeleteAllOnSubmit(runnerScans);
                            db.SubmitChanges();

                            MessageBox.Show($"Remove {eventName} {distance} successfully");
                        }
                        else
                        {
                            MessageBox.Show($"{eventName} {distance} found no record");
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
        #endregion

        #region Button Click
        private void ClearDBRecords_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Confirm clearing WHOLE runner record?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    using (RunnerDataContext db = new RunnerDataContext())
                    {
                        if (db.RunnerScanDateTimes.Any())
                        {
                            db.RunnerScanDateTimes.DeleteAllOnSubmit(db.RunnerScanDateTimes);
                            db.SubmitChanges();
                        }
                    }
                    MessageBox.Show("Clear Whole record successfully");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void StartSingleRun5K_Click(object sender, RoutedEventArgs e)
        {
            StartRunning("Single Running", "5 K");
        }

        private void ClearSingleRun5K_Click(object sender, RoutedEventArgs e)
        {
            ClearRunningRecord("Single Running", "5 K");
        }

        private void StartSingleRun10K_Click(object sender, RoutedEventArgs e)
        {
            StartRunning("Single Running", "10 K");
        }

        private void ClearSingleRun10K_Click(object sender, RoutedEventArgs e)
        {
            ClearRunningRecord("Single Running", "10 K");
        }

        private void StartSingleRun20K_Click(object sender, RoutedEventArgs e)
        {
            StartRunning("Single Running", "20 K");
        }

        private void ClearSingleRun20K_Click(object sender, RoutedEventArgs e)
        {
            ClearRunningRecord("Single Running", "20 K");
        }
        #endregion

        #endregion

        #region Result
        public List<RunnerResult> SearchResult(string eventName, string distance)
        {
            _eventName = eventName;
            _distance = distance;

            string dateTimeFormat = "HH : mm : ss.fff";
            CultureInfo thaiCulture = new CultureInfo("th-TH");

            List<RunnerResult> result = new List<RunnerResult>();
            using (RunnerDataContext db = new RunnerDataContext())
            {
                var runnerInfos = db.RunnerInfos.Where(x => x.Event == eventName && x.Distance == distance).ToList();
                var runnerScans = db.RunnerScanDateTimes.Where(x => runnerInfos.Select(y => y.RunnerBIB).Contains(x.RunnerIdentification)).GroupBy(x => x.RunnerIdentification).ToList();

                foreach (var group in runnerScans)
                {
                    var runnerMaxScannedTime = group.Max(x => x.ScannedDateTime);
                    var runnerMinScannedTime = group.Min(x => x.ScannedDateTime);

                    if (runnerMaxScannedTime > runnerMinScannedTime)
                    {
                        RunnerResult runnerStatus = new RunnerResult();
                        runnerStatus.RunnerIdentification = group.Key;
                        var runnerInfo = runnerInfos.SingleOrDefault(x => x.RunnerBIB == group.Key);
                        runnerStatus.Name = string.Concat(runnerInfo.FirstName, " ", runnerInfo.LastName);
                        runnerStatus.Distance = runnerInfo.Distance;

                        runnerStatus.StartTimeString = $"ที่เวลา {runnerMinScannedTime.ToString(dateTimeFormat, thaiCulture)}";
                        runnerStatus.EndTimeString = $"ที่เวลา {runnerMaxScannedTime.ToString(dateTimeFormat, thaiCulture)}";

                        var diffTime = (runnerMaxScannedTime - runnerMinScannedTime);
                        var dateTimeTimeDiff = new DateTime(Math.Abs(diffTime.Ticks));
                        runnerStatus.Duration = dateTimeTimeDiff;
                        runnerStatus.DurationString = $"{dateTimeTimeDiff.Hour} ชั่วโมง {dateTimeTimeDiff.Minute} นาที {dateTimeTimeDiff.Second}.{dateTimeTimeDiff.Millisecond} วินาที";

                        result.Add(runnerStatus);
                    }
                }
            }
            return result.OrderBy(x => x.Duration).ToList();
        }

        private void ThreeKiloButton_Click(object sender, RoutedEventArgs e)
        {
            busyIndicator.IsBusy = true;

            EventTextBox.Text = "Ozone";
            DistanceTextBox.Text = "3Km";

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += ThreeKiloButton_DoWork;
            worker.RunWorkerCompleted += ThreeKiloButton_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void ThreeKiloButton_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = SearchResult("Ozone", "3 Km");
        }

        private void ThreeKiloButton_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _List = e.Result as List<RunnerResult>;
            GridView.ItemsSource = _List;
            busyIndicator.IsBusy = false;
        }

        private void FiveKiloButton_Click(object sender, RoutedEventArgs e)
        {
            busyIndicator.IsBusy = true;

            EventTextBox.Text = "Single Running";
            DistanceTextBox.Text = "5 Km";

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += FiveKiloButton_DoWork;
            worker.RunWorkerCompleted += FiveKiloButton_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void FiveKiloButton_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = SearchResult("Single Running", "5 Km");
        }

        private void FiveKiloButton_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _List = e.Result as List<RunnerResult>;
            GridView.ItemsSource = _List;
            busyIndicator.IsBusy = false;
        }

        private void TenKiloButton_Click(object sender, RoutedEventArgs e)
        {
            busyIndicator.IsBusy = true;

            bool isSingleRunning = (bool)SingleRunningRadio.IsChecked;
            string eventName = isSingleRunning ? "Single Running" : "Ozone";

            EventTextBox.Text = eventName;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += TenKiloButton_DoWork;
            worker.RunWorkerCompleted += TenKiloButton_RunWorkerCompleted;
            worker.RunWorkerAsync(eventName);
        }

        private void TenKiloButton_DoWork(object sender, DoWorkEventArgs e)
        {
            string eventName = e.Argument as string;
            e.Result = SearchResult(eventName, eventName == "Single Running" ? "10 K" : "10 Km");
        }

        private void TenKiloButton_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _List = e.Result as List<RunnerResult>;
            GridView.ItemsSource = _List;
            busyIndicator.IsBusy = false;
        }

        private void TwentyKiloButton_Click(object sender, RoutedEventArgs e)
        {
            busyIndicator.IsBusy = true;

            EventTextBox.Text = "Single Running";
            DistanceTextBox.Text = "20 K";

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += TwentyKiloButton_DoWork;
            worker.RunWorkerCompleted += TwentyKiloButton_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void TwentyKiloButton_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = SearchResult("Single Running", "20 K");
        }

        private void TwentyKiloButton_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _List = e.Result as List<RunnerResult>;
            GridView.ItemsSource = _List;
            busyIndicator.IsBusy = false;
        }

        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "csv files (*.csv)|*.csv";
            saveFileDialog1.FileName = "logs";
            saveFileDialog1.Title = "Export to Excel";
            StringBuilder sb = new StringBuilder();

            int objectCount = _List.Count;
            for (int i = 0; i < objectCount; i++)
            {
                var row = _List[i];
                sb.Append(row.RunnerIdentification.ToString() + ",");
                sb.Append(row.Name.ToString() + ",");
                sb.Append(row.Distance.ToString() + ",");
                sb.Append(row.StartTimeString.ToString() + ",");
                sb.Append(row.EndTimeString.ToString() + ",");
                sb.Append(row.DurationString.ToString());

                sb.AppendLine();
            }

            try
            {
                StreamWriter sw = new StreamWriter($"D:/{_eventName}_{_distance}_{DateTime.Now.ToString("hh.MM.ss")}.csv", true, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
            }
            catch
            {
                StreamWriter sw = new StreamWriter($"E:/{_eventName}_{_distance}_{DateTime.Now.ToString("hh.MM.ss")}.csv", true, Encoding.UTF8);
                sw.Write(sb.ToString());
                sw.Close();
            }
        }
        #endregion
    }

    public class RunnerResult
    {
        public string RunnerIdentification { get; set; }
        public string Name { get; set; }
        public string Distance { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        public DateTime Duration { get; set; }
        public string DurationString { get; set; }
    }
}