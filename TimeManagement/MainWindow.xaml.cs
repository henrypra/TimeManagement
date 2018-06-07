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
                    Grid.SetColumn(buttonProject, usedSlots % 3);
                    Grid.SetRow(buttonProject, usedSlots / 3);
                    gridMain.Children.Add(buttonProject);
                    usedSlots++;
                }
                else if (usedSlots < 9)
                {
                    Button buttonCancel = new Button();
                    buttonCancel.Content = "+";
                    buttonCancel.FontSize = 65;
                    buttonCancel.Name = "Add";
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

                    childDetected++;
                    string title = dataReader["title"].ToString();
                    int h = Int32.Parse(dataReader["h"].ToString());
                    int m = Int32.Parse(dataReader["min"].ToString());
                    int s = Int32.Parse(dataReader["sec"].ToString());

                    buttonProject.Content = title + "\n" + h + "h  " + m + "min  " + s + "s";

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
