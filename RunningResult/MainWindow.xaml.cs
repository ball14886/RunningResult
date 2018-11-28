using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        public List<RunnerResult> SearchResult(string eventName, string distance)
        {
            string dateTimeFormat = "HH : mm : ss.fff";
            CultureInfo thaiCulture = new CultureInfo("th-TH");

            List<RunnerResult> result = new List<RunnerResult>();
            using (RunnerDataContext db = new RunnerDataContext())
            {
                var runnerInfos = db.RunnerInfos.Where(x => x.Event == eventName && x.Distance == distance).ToList();
                var runnerScans = db.RunnerScanDateTimes.Where(x => runnerInfos.Select(y => y.RunnerBIB).Contains(x.RunnerIdentification)).GroupBy(x => x.RunnerIdentification).ToList();

                foreach (var group in runnerScans)
                {
                    RunnerResult runnerStatus = new RunnerResult();
                    runnerStatus.RunnerIdentification = group.Key;
                    var runnerInfo = runnerInfos.SingleOrDefault(x => x.RunnerBIB == group.Key);
                    runnerStatus.Name = string.Concat(runnerInfo.FirstName, " ", runnerInfo.LastName);
                    runnerStatus.Distance = runnerInfo.Distance;

                    var runnerMaxScannedTime = group.Max(x => x.ScannedDateTime);
                    var runnerMinScannedTime = group.Min(x => x.ScannedDateTime);

                    runnerStatus.StartTimeString = runnerMinScannedTime.ToString(dateTimeFormat, thaiCulture);

                    if (runnerMaxScannedTime == runnerMinScannedTime)
                    {
                        runnerStatus.EndTimeString = "";
                        runnerStatus.DurationString = "";
                    }
                    else
                    {
                        runnerStatus.EndTimeString = runnerMaxScannedTime.ToString(dateTimeFormat, thaiCulture);
                        var diffTime = (runnerMaxScannedTime - runnerMinScannedTime);
                        var dateTimeTimeDiff = new DateTime(Math.Abs(diffTime.Ticks));
                        runnerStatus.Duration = dateTimeTimeDiff;
                        runnerStatus.DurationString = $"{dateTimeTimeDiff.Hour} ชั่วโมง {dateTimeTimeDiff.Minute} นาที {dateTimeTimeDiff.Second}.{dateTimeTimeDiff.Millisecond} วินาที";
                    }
                    result.Add(runnerStatus);
                }
            }
            return result.OrderByDescending(x => x.Duration).ToList();
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
            GridView.ItemsSource = e.Result as List<RunnerResult>;
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
            GridView.ItemsSource = e.Result as List<RunnerResult>;
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
            GridView.ItemsSource = e.Result as List<RunnerResult>;
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
            GridView.ItemsSource = e.Result as List<RunnerResult>;
            busyIndicator.IsBusy = false;
        }
    }

    public class RunnerResult
    {
        public string RunnerIdentification { get; set; }
        public string Name { get; set; }
        public string Distance { get; set; }
        //public DateTime StartTime { get; set; }
        public string StartTimeString { get; set; }
        //public DateTime EndTime { get; set; }
        public string EndTimeString { get; set; }
        public DateTime Duration { get; set; }
        public string DurationString { get; set; }
    }
}