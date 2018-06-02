using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Text;


namespace TimeManagement
{
    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        private int hGlobal, mGlobal, sGlobal;
        private int hLocal, mLocal, sLocal;
        private DispatcherTimer timer;
        private TimeSpan startedTimeSpan;
        private TimeSpan pausedTimeSpan;
        private StopWatchState state;

        static DependencyProperty FormatProperty;
        static DependencyProperty IntervalProperty;


        public enum StopWatchState
        {
            Stopped = 0,
            Started = 1,
            Paused = 2
        }


        int id;
        public ProjectWindow(int _id)
        {
            InitializeComponent();
            state = StopWatchState.Stopped;
            timer = new DispatcherTimer();

            // this.Interval is not available here yet
            //timer.Interval = new TimeSpan(this.Interval * 10000);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            id = _id;
            RetrieveData(id);
        }

        /// <summary>
        /// Static constructor.s
        /// </summary>
        static ProjectWindow()
        {
            PropertyChangedCallback formatChangedCallback = new PropertyChangedCallback(FormatChanged);
            FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(ProjectWindow), new UIPropertyMetadata("HH:mm:ss", formatChangedCallback));
            IntervalProperty = DependencyProperty.Register("Interval", typeof(int), typeof(ProjectWindow), new UIPropertyMetadata(1000));
        }
        
        static void FormatChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ProjectWindow thisStopWatch = (ProjectWindow)sender;
            if (thisStopWatch != null)
            {
                //thisStopWatch.timerLabel.Content = thisStopWatch.Format;
            }
        }

        public string Format
        {
            get
            {
                return (string)base.GetValue(FormatProperty);
            }
            set
            {
                base.SetValue(FormatProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the measure of duration between each tick.
        /// </summary>
        public int Interval
        {
            get
            {
                return (int)base.GetValue(IntervalProperty);
            }
            set
            {
                base.SetValue(IntervalProperty, value);
            }
        }

        /// <summary>
        /// Gets the state of StopWatch. It can be either Stopped, Started or Paused.
        /// </summary>        
        public StopWatchState State
        {
            get { return this.state; }
        }

        /// <summary>
        /// Starts the StopWatch.
        /// </summary>
        /// <exception cref="InvalidOperationException">It is thrown when trying to start the StopWatch while is in Started state.</exception>
        public void Start()
        {
            if (this.state == StopWatchState.Started)
            {
                return;
            }

            // If StopWatch is stopped...
            if (this.state == StopWatchState.Stopped)
            {

                timer.Tick += timer_Tick;
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
            }

            // Or if it is Paused...
            else if (this.state == StopWatchState.Paused)
            {
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks).Subtract(pausedTimeSpan).Add(startedTimeSpan);
                pausedTimeSpan = TimeSpan.Zero;
            }

            // Start StopWatch
            if (timer.Interval.Ticks == 0)
            {
                timer.Interval = new TimeSpan(this.Interval * 10000);
            }

            timer.Start();
            state = StopWatchState.Started;
        }

        /// <summary>
        /// Pauses the StopWatch.
        /// </summary>
        /// <exception cref="InvalidOperationException">It is thrown when trying to pause the StopWatch when it is not in Started state.</exception>
        public void Pause()
        {
            if (this.state != StopWatchState.Started)
            {
                return;
            }

            timer.Stop();
            state = StopWatchState.Paused;
            pausedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
        }

        /// <summary>
        /// Stops the StopWatch.
        /// </summary>
        /// <exception cref="InvalidOperationException">It is thrown when trying to stop the StopWatch when it is not in Started or Paused state.</exception>
        public void Save()
        {
            Database databaseObject = new Database();

            //INSERT INTO DATABASE
            string query = "UPDATE projects SET h= '"+ (hLocal + hGlobal)+ "', min='" + (mLocal + mGlobal) + "', sec='" + (sLocal + sGlobal) + "' WHERE id = " + id;

            SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
            databaseObject.OpenConnection();
            var result = command.ExecuteNonQuery();
            databaseObject.CloseConnection();
            timer.Stop();
            timer.Tick -= timer_Tick;
            hLocal = 0;
            mLocal = 0;
            sLocal = 0;
            timerLabel.Content = hLocal + "h  " + mLocal + "min  " + sLocal + "s";
            if (state == StopWatchState.Started)
            {
                timer.Tick += timer_Tick;
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
                timer.Start();
                state = StopWatchState.Started;
            }
            else
            {
                state = StopWatchState.Stopped;
            }

            RetrieveData(id);
        }

        public void Reset()
        {
            timer.Stop();
            timer.Tick -= timer_Tick;
            hLocal = 0;
            mLocal = 0;
            sLocal = 0;
            timerLabel.Content = hLocal + "h  " + mLocal + "min  " + sLocal + "s";
            state = StopWatchState.Stopped;
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= timer_Tick;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now.Subtract(startedTimeSpan);

            hLocal = Int32.Parse(date.ToString("HH"));
            mLocal = Int32.Parse(date.ToString("mm"));
            sLocal = Int32.Parse(date.ToString("ss"));
            timerLabel.Content = hLocal + "h  " + mLocal + "min  " + sLocal + "s";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            switch (b.Name)
            {
                case "btn_start":
                    Start();
                    break;
                case "btn_pause":
                    Pause();
                    break;
                case "btn_save":
                    Save();
                    break;
                case "btn_clear":
                    Reset();
                    break;
                case "btn_back":
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                    break;
            }
        }

        private void RetrieveData(int id)
        {
            Database databaseObject = new Database();

            //RETRIEVE FROM DATABASE
            string query = "SELECT * FROM projects WHERE id ='" + id + "'";

            SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
            databaseObject.OpenConnection();
            SQLiteDataReader result = command.ExecuteReader();

            if (result.HasRows)
            {
                while (result.Read())
                {
                    hGlobal = Int32.Parse(result["h"].ToString());
                    mGlobal = Int32.Parse(result["min"].ToString());
                    sGlobal = Int32.Parse(result["sec"].ToString());

                    string title = result["title"].ToString();
                    project_time.Content = hGlobal + "h  " + mGlobal + "min  " + sGlobal + "s";
                    project_title.Content = title;
                }
            }
            databaseObject.CloseConnection();
        }

    }
}
