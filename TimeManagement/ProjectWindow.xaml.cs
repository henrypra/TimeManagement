﻿using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Text;
using Microsoft.VisualBasic;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace TimeManagement
{
    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        private int hGlobal, minGlobal, secGlobal;
        private int hLocal, minLocal, secLocal;
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

            id = _id;
            RetrieveData(id);
        }

        static ProjectWindow()
        {
            PropertyChangedCallback formatChangedCallback = new PropertyChangedCallback(FormatChanged);
            FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(ProjectWindow), new UIPropertyMetadata("HH:mm:ss", formatChangedCallback));
            IntervalProperty = DependencyProperty.Register("Interval", typeof(int), typeof(ProjectWindow), new UIPropertyMetadata(1000));
        }

        static void FormatChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ProjectWindow thisStopWatch = (ProjectWindow)sender;
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

        public StopWatchState State
        {
            get { return this.state; }
        }

        public void Start()
        {
            if (this.state == StopWatchState.Started)
            {

                return;
            }

            if (this.state == StopWatchState.Stopped)
            {

                timer.Tick += timer_Tick;
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
            }

            else if (this.state == StopWatchState.Paused)
            {
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks).Subtract(pausedTimeSpan).Add(startedTimeSpan);
                pausedTimeSpan = TimeSpan.Zero;
            }

            if (timer.Interval.Ticks == 0)
            {
                timer.Interval = new TimeSpan(this.Interval * 10000);
            }

            timer.Start();
            state = StopWatchState.Started;
            btn_start.BorderBrush = Brushes.Red;
            btn_pause.BorderBrush = Brushes.Transparent;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime date = DateTime.Now.Subtract(startedTimeSpan);

            hLocal = Int32.Parse(date.ToString("HH"));
            String h = hLocal.ToString("00");

            minLocal = Int32.Parse(date.ToString("mm"));
            String m = minLocal.ToString("00");

            secLocal = Int32.Parse(date.ToString("ss"));
            String s = secLocal.ToString("00");

            timerLabel.Content = h + " : " + m + " : " + s;
        }


        private void headerThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Left = Left + e.HorizontalChange;
            Top = Top + e.VerticalChange;
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
                case "btn_delete":
                    DeleteProject();
                    break;
                case "btn_edit":
                    EditProjectname();
                    break;
                case "btn_erase":
                    ResetMainTime();
                    break;
                case "btn_back":
                    BackToMainMenu();
                    break;
                case "btn_close":
                    this.Close();
                    break;
                case "btn_minimize":
                    this.WindowState = WindowState.Minimized;
                    break;
            }
        }

        private void BackToMainMenu()
        {
            if (state == StopWatchState.Started || state == StopWatchState.Paused)
            {
                var msgBox = MessageBox.Show("Möchten Sie zum Hauptmenü zurückkehren und ihre aktuelle Sitzung speichern?", "Sitzung speichern?", MessageBoxButton.YesNoCancel);
                if (msgBox == MessageBoxResult.Yes)
                {
                    Save();
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else if (msgBox == MessageBoxResult.No)
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
            else
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        public void Pause()
        {
            if (this.state != StopWatchState.Started)
            {
                return;
            }

            timer.Stop();
            state = StopWatchState.Paused;
            pausedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
            btn_start.BorderBrush = Brushes.Transparent;
            btn_pause.BorderBrush = Brushes.Red;
        }

        public void Save()
        {
            Database databaseObject = new Database();

            //INSERT INTO DATABASE
            string query = "UPDATE projects SET h= '" + (hLocal + hGlobal) + "', min='" + (minLocal + minGlobal) + "', sec='" + (secLocal + secGlobal) + "' WHERE id = " + id;
            hLocal = 0;
            minLocal = 0;
            secLocal = 0;
            SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
            databaseObject.OpenConnection();
            var result = command.ExecuteNonQuery();
            databaseObject.CloseConnection();
            timer.Stop();
            timer.Tick -= timer_Tick;
            timerLabel.Content = "00 : 00 : 00";
            if (state == StopWatchState.Started)
            {
                timer.Tick += timer_Tick;
                startedTimeSpan = new TimeSpan(DateTime.Now.Ticks);
                timer.Start();
                state = StopWatchState.Started;
                btn_pause.BorderBrush = Brushes.Transparent;
                btn_start.BorderBrush = Brushes.Red;
            }
            else
            {
                btn_pause.BorderBrush = Brushes.Transparent;
                btn_start.BorderBrush = Brushes.Transparent;
                state = StopWatchState.Stopped;
            }

            RetrieveData(id);
        }

        public void Reset()
        {
            timer.Stop();
            timer.Tick -= timer_Tick;
            timerLabel.Content = "00 : 00 : 00";
            state = StopWatchState.Stopped;
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= timer_Tick;
            }
            btn_pause.BorderBrush = Brushes.Transparent;
            btn_start.BorderBrush = Brushes.Transparent;
        }

        private void ResetMainTime()
        {
            var msgBox = MessageBox.Show("Sind Sie sicher, dass Sie die insgesamt investierte Zeit für das Projekt '" + project_title.Content + "' zurücksetzen wollen?", "Zeit zurücksetzen?", MessageBoxButton.YesNo);
            if (msgBox == MessageBoxResult.Yes)
            {
                Database databaseObject = new Database();

                //INSERT INTO DATABASE
                string query = "UPDATE projects SET h = '0', min = '0', sec = '0' WHERE id = " + id;
                SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
                databaseObject.OpenConnection();
                var result = command.ExecuteNonQuery();
                databaseObject.CloseConnection();

                project_time.Content = "00 : 00 : 00";
                RetrieveData(id);
            }

        }

        private void EditProjectname()
        {
            string projectName;
            string message, title, defaultValue;
            message = "Geben Sie Ihrem  Projekt einen Namen";
            title = "Projektname";
            defaultValue = project_title.Content.ToString();//Display message, title, and default value.
            projectName = Interaction.InputBox(message, title, defaultValue);

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (projectName != "")
            {
                Database databaseObject = new Database();

                //INSERT INTO DATABASE
                string query = "UPDATE projects SET title = '" + projectName + "' WHERE id = " + id;
                SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
                databaseObject.OpenConnection();
                var result = command.ExecuteNonQuery();
                databaseObject.CloseConnection();

                project_title.Content = projectName;
            }
        }

        private void DeleteProject()
        {
            var msgBox = MessageBox.Show("Sind Sie sicher, dass Sie das Projekt '" + project_title.Content + "' löschen wollen?", "Projekt löschen?", MessageBoxButton.YesNo);
            if (msgBox == MessageBoxResult.Yes)
            {
                Database databaseObject = new Database();

                string query = "DELETE FROM projects WHERE id=" + id;

                SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
                databaseObject.OpenConnection();
                SQLiteDataReader result = command.ExecuteReader();
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
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
                    secGlobal = Int32.Parse(result["sec"].ToString());
                    minGlobal = Int32.Parse(result["min"].ToString());
                    hGlobal = Int32.Parse(result["h"].ToString());


                    if (secGlobal >= 60)
                    {
                        secGlobal = secGlobal % 60;
                        minGlobal++;
                    }

                    if (minGlobal >= 60)
                    {
                        minGlobal = minGlobal % 60;
                        hGlobal++;
                    }

                    String s = secGlobal.ToString("00");

                    String m = minGlobal.ToString("00");

                    String h = hGlobal.ToString("00");

                    string title = result["title"].ToString();
                    project_time.Content = h + " : " + m + " : " + s;
                    project_title.Content = title;
                }
            }
            databaseObject.CloseConnection();
        }

    }
}
