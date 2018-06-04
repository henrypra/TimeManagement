using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
using Microsoft.VisualBasic;

namespace TimeManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Button buttonProject;
        private string projectName;
        private int count;
        private int numberOfChildren;

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
                numberOfChildren++;
                InsertData();
            }
        }

        private void generateButtons()
        {
            count = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    count++;
                    if (count <= numberOfChildren)
                    {
                        buttonProject = new Button();

                        buttonProject.Name = "prj_" + count.ToString();
                        RetrieveData(count);
                        buttonProject.HorizontalContentAlignment = HorizontalAlignment.Center;
                        buttonProject.FontSize = 24;
                        buttonProject.Click += Button_Click;
                        Grid.SetColumn(buttonProject, j);
                        Grid.SetRow(buttonProject, i);
                    }
                    else
                    {
                        Button buttonCancel = new Button();
                        buttonCancel.Content = "+";
                        buttonCancel.FontSize = 65;
                        buttonCancel.Name = "Add";
                        buttonCancel.Click += Button_Click;
                        Grid.SetColumn(buttonCancel, j);
                        Grid.SetRow(buttonCancel, i);
                        gridMain.Children.Add(buttonCancel);
                        return;
                    }

                }
            }
        }

        private void InsertData()
        {
            Database databaseObject = new Database();

            //INSERT INTO DATABASE
            string query = "INSERT INTO projects ('title', 'h', 'min', 'sec') VALUES (@title, @h, @min, @sec)";

            SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
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
                    string title = result["title"].ToString();
                    int h = Int32.Parse(result["h"].ToString());
                    int m = Int32.Parse(result["min"].ToString());
                    int s = Int32.Parse(result["sec"].ToString());

                    buttonProject.Content = title+"\n"+h+"h  "+m+"min  "+s+"s";
                    gridMain.Children.Add(buttonProject);
                }
            }
            databaseObject.CloseConnection();
        }

        private void InitDatabase()
        {
            Database databaseObject = new Database();

            //COUNT ROWS
            string query = "SELECT * FROM projects";

            SQLiteCommand command = new SQLiteCommand(query, databaseObject.connection);
            databaseObject.OpenConnection();
            SQLiteDataReader result = command.ExecuteReader();

            if (result.HasRows)
            {
                numberOfChildren = 0;
                while (result.Read())
                {
                    numberOfChildren++;
                }
            }
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
