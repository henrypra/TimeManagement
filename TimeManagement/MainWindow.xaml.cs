using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;

namespace TimeManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool exists = false;
        int childDetected;
        Button buttonProject;
        Label lbTitle, lbTime, lbTimeText;
        private string projectName;
        private int count;
        private int numberOfChildren;

        //Database
        Database databaseObject;
        SQLiteCommand command;
        SQLiteDataReader dataReader;
        string query;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitDatabase();
            generateButtons();
        }

        private void InputProjectTitle()
        {
            string message, title, defaultValue;
            message = "Geben Sie Ihrem neuen Projekt einen Namen";
            title = "Projektname";
            defaultValue = "[Projekt]";//Display message, title, and default value.
            projectName = Interaction.InputBox(message, title, defaultValue);

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (projectName != "")
            {
                InsertData();
            }
        }

        private void headerThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Left = Left + e.HorizontalChange;
            Top = Top + e.VerticalChange;
        }

        private void generateButtons()
        {
            count = 0;
            int usedSlots = 0;
            while (count < 9)
            {
                exists = false;
                while (!exists && count < 9)
                {
                    count++;
                    RetrieveData(count);
                }
                if (exists)
                {
                    buttonProject.Name = "prj_" + count.ToString();
                    buttonProject.HorizontalContentAlignment = HorizontalAlignment.Center;
                    buttonProject.FontSize = 24;
                    buttonProject.Click += Button_Click;
                    buttonProject.Background = new SolidColorBrush(Color.FromRgb(220, 236, 236));
                    buttonProject.BorderBrush = new SolidColorBrush(Color.FromRgb(77, 77, 77));
                    buttonProject.BorderThickness = new Thickness(.5);

                    Grid.SetColumn(buttonProject, usedSlots % 3);
                    Grid.SetRow(buttonProject, usedSlots / 3);
                    gridMain.Children.Add(buttonProject);

                    lbTitle.FontSize = 24;
                    lbTitle.Margin = new Thickness(0, 12, 0, 0);
                    lbTitle.HorizontalAlignment = HorizontalAlignment.Center;
                    lbTitle.VerticalAlignment = VerticalAlignment.Top;
                    lbTitle.FontFamily = new FontFamily("Trebuchet MS");
                    lbTitle.IsHitTestVisible = false;
                    Grid.SetColumn(lbTitle, usedSlots % 3);
                    Grid.SetRow(lbTitle, usedSlots / 3);
                    gridMain.Children.Add(lbTitle);

                    lbTime.FontSize = 32;
                    lbTime.Margin = new Thickness(0, 75, 0, 0);
                    lbTime.HorizontalAlignment = HorizontalAlignment.Center;
                    lbTime.FontFamily = new FontFamily("Digital-7 Mono Regular");
                    lbTime.VerticalAlignment = VerticalAlignment.Top;
                    lbTime.IsHitTestVisible = false;
                    Grid.SetColumn(lbTime, usedSlots % 3);
                    Grid.SetRow(lbTime, usedSlots / 3);
                    gridMain.Children.Add(lbTime);

                    lbTimeText.FontSize = 16;
                    lbTimeText.Margin = new Thickness(0, 120, 0, 0);
                    lbTimeText.HorizontalAlignment = HorizontalAlignment.Center;
                    lbTimeText.VerticalAlignment = VerticalAlignment.Top;
                    lbTimeText.FontFamily = new FontFamily("Trebuchet MS");
                    lbTimeText.IsHitTestVisible = false;
                    lbTimeText.Content = "Std.\tMin.\tSek.";
                    Grid.SetColumn(lbTimeText, usedSlots % 3);
                    Grid.SetRow(lbTimeText, usedSlots / 3);
                    gridMain.Children.Add(lbTimeText);

                    usedSlots++;
                }
                else if (usedSlots < 9)
                {
                    Button buttonCancel = new Button();
                    buttonCancel.Content = "+";
                    buttonCancel.FontSize = 65;
                    buttonCancel.Name = "Add";
                    buttonCancel.Background = new SolidColorBrush(Color.FromRgb(220, 236, 236));//(77,77,77)
                    buttonCancel.BorderBrush = System.Windows.Media.Brushes.Teal;
                    buttonCancel.BorderThickness = new Thickness(.5);
                    buttonCancel.Click += Button_Click;
                    Grid.SetColumn(buttonCancel, usedSlots % 3);
                    Grid.SetRow(buttonCancel, usedSlots / 3);
                    gridMain.Children.Add(buttonCancel);
                    return;
                }
            }
        }

        private void InsertData()
        {

            databaseObject = new Database();

            //INSERT INTO DATABASE

            query = "INSERT INTO projects ('title', 'h', 'min', 'sec') VALUES (@title, @h, @min, @sec)";

            command = new SQLiteCommand(query, databaseObject.connection);
            databaseObject.OpenConnection();
            command.Parameters.AddWithValue("@title", projectName);
            command.Parameters.AddWithValue("@h", 0);
            command.Parameters.AddWithValue("@min", 0);
            command.Parameters.AddWithValue("@sec", 0);
            var result = command.ExecuteNonQuery();
            databaseObject.CloseConnection();
            generateButtons();
        }

        private void RetrieveData(int id)
        {
            databaseObject = new Database();

            //RETRIEVE FROM DATABASE
            query = "SELECT * FROM projects WHERE id ='" + id + "'";

            command = new SQLiteCommand(query, databaseObject.connection);
            databaseObject.OpenConnection();
            dataReader = command.ExecuteReader();


            if (dataReader.HasRows)
            {
                exists = true;
                while (dataReader.Read())
                {
                    buttonProject = new Button();
                    lbTitle = new Label();
                    lbTime = new Label();
                    lbTimeText = new Label();

                    childDetected++;
                    string title = dataReader["title"].ToString();
                    int h = Int32.Parse(dataReader["h"].ToString());
                    int m = Int32.Parse(dataReader["min"].ToString());
                    int s = Int32.Parse(dataReader["sec"].ToString());


                    if (s >= 60)
                    {
                        s = s % 60;
                        m++;
                    }

                    if (m >= 60)
                    {
                        m = m % 60;
                        h++;
                    }

                    String _s = s.ToString("00");
                    String _m = m.ToString("00");
                    String _h = h.ToString("00");

                    lbTitle.Content = title;
                    lbTime.Content = _h + " : " + _m + " : " + _s;

                    dataReader.Close();

                    databaseObject.CloseConnection();
                    return;
                }

            }
            else
            {
                exists = false;
            }
            dataReader.Close();
            databaseObject.CloseConnection();
        }

        private void InitDatabase()
        {
            databaseObject = new Database();

            //COUNT ROWS
            query = "SELECT * FROM projects";

            command = new SQLiteCommand(query, databaseObject.connection);
            databaseObject.OpenConnection();
            dataReader = command.ExecuteReader();

            if (dataReader.HasRows)
            {
                numberOfChildren = 0;
                while (dataReader.Read())
                {
                    numberOfChildren++;
                }
            }
            dataReader.Close();
            databaseObject.CloseConnection();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            switch (b.Name)
            {
                case "Add":
                    InputProjectTitle();
                    break;
                case "btn_close":
                    this.Close();
                    break;
                case "btn_minimize":
                    this.WindowState = WindowState.Minimized;
                    break;
                default:
                    int id = Int32.Parse(b.Name.Substring(4).ToString());
                    ProjectWindow projectWindow = new ProjectWindow(id);
                    projectWindow.Show();
                    this.Close();
                    break;
            }
        }

    }
}
