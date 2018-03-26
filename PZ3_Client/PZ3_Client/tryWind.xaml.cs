using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace PZ3_Client
{
    /// <summary>
    /// Interaction logic for tryWind.xaml
    /// </summary>
    public partial class tryWind : Window
    {
       // public static MainWindow mejn = new MainWindow();
        public tryWind()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string filename;

            if (comboxic.SelectedIndex == 0)
            {

                filename = "./images/IA.jpg";
            }
            else
            {
                filename = "./images/IB.jpg";
            }

            textBoxImage.Text = filename;
            var uri = new Uri(filename, UriKind.Relative);
            image.Source = new BitmapImage(uri);
        }

        private void comboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            data.Add("IA");
            data.Add("IB");

            comboxic.ItemsSource = data;
            comboxic.SelectedIndex = 0;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            
            if (validate())
            {
                  MainWindow.ListObj.Add(new Put(Int32.Parse(textBoxID.Text), textBoxVal.Text, comboxic.Text, textBoxImage.Text));

                this.Close();
            }
            else
            {
                MessageBox.Show("There was an error. Try again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool validate()
        {

            bool status = true;


            if (textBoxID.Text.Trim().Equals(""))
            {
                labelErID.Content = "U can't leave this blank.";
                textBoxID.BorderBrush = Brushes.Red;
                status = false;
            }
            else
            {
                textBoxID.BorderBrush = Brushes.Gray;
                labelErID.Content = "";

                
                try
                {
                    if (Double.Parse(textBoxID.Text) < 0 || Double.Parse(textBoxID.Text) > 1000)
                    {
                        labelErID.Content = "U didn't enter a valid number.";
                        textBoxID.BorderBrush = Brushes.Red;

                        status = false;
                    }

                    foreach(Put p in MainWindow.ListObj)
                    {
                        if(p.Id == Double.Parse(textBoxID.Text))
                        {
                            labelErID.Content = "ID already exists.";
                            textBoxID.BorderBrush = Brushes.Red;

                            status = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    labelErID.Content = "U didn't enter a valid number.";
                    textBoxID.BorderBrush = Brushes.Red;
                    Console.WriteLine(ex.Message);
                    status = false;
                }
            }

            if (textBoxVal.Text.Trim().Equals(""))
            {
                labelErrVal.Content = "U can't leave this blank.";
                textBoxVal.BorderBrush = Brushes.Red;
                status = false;
            }
            else
            {
                textBoxVal.BorderBrush = Brushes.Gray;
                labelErrVal.Content = "";
            }

            return status;
        }
    }
}
