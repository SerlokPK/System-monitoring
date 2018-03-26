using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace PZ3_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static ObservableCollection<Put> listObj;
        private static Dictionary<string, Put> vracanjeBox = new Dictionary<string, Put>();
        private static ObservableCollection<Put> additionalList = new ObservableCollection<Put>();
        private static ObservableCollection<Put> listBoxList = new ObservableCollection<Put>();
        public static ObservableCollection<Put> ListObj { get => listObj; set => listObj = value; }
        public static ObservableCollection<Put> AdditionalList { get => additionalList; set => additionalList = value; }
        public static ObservableCollection<Put> ListBoxList { get => listBoxList; set => listBoxList = value; }
        public static Dictionary<string, Put> VracanjeBox { get => vracanjeBox; set => vracanjeBox = value; }

        private Put draggedItem = null;
        private Put draggedPut = null;
        private bool file = false;
        private bool dragging = false;
        private string searching = "";
        private int naz = -1;

        private string path = @"C:\Users\Serlok\Desktop\Treca god\Interakcija covek-racunar\Vezbe\PZ3_cas_v2\LogFile.txt";
        private int id;
        private int value;
        
        public MainWindow()
        {
            listObj = new ObservableCollection<Put>();
            DataContext = this;
            InitializeComponent();
            dataGrid.CanUserAddRows = false;
            Put p1 = new Put(0, "i34", "IA", "./images/IA.jpg");
            Put p2 = new Put(1, "e-45", "IB", "./images/IB.jpg");
            ListObj.Add(p1);
            ListObj.Add(p2);
            foreach (Put p in ListObj)
            {
                AdditionalList.Add(p);
            }

            
            foreach (Put p in ListObj)
            {
                ListBoxList.Add(p);
            }

            createListener(); //Povezivanje sa serverskom aplikacijom
        }
        
        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(ListObj.Count().ToString());
                            stream.Write(data, 0, data.Length);
                            file = false;
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Objekat_1:272"

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji
                            string[] split = incomming.Split('_', ':');
                            id = Convert.ToInt32(split[1]);
                            value= Convert.ToInt32(split[2]);
                            
                            ListObj[id].Value = value;

                            
                            upis();

                            
                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

       
        private void upis()
        {
            
            if (!file)
            {
                StreamWriter writer;
                using ( writer = new StreamWriter(path))
                {
                    writer.WriteLine("Object_" + id + "\tValue: " + value + "\tTime: " + DateTime.Now.ToString());
                    
                }
               

            }
            else
            {
                StreamWriter writer;
                using ( writer = new StreamWriter(path, true))
                {
                    writer.WriteLine("Object_" + id + "\tValue: " + value + "\tTime: " + DateTime.Now.ToString());
                    
                }
                
            }
            
            file = true;
        }
        private void buttonDel_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedIndex < 0)
            {
                
            }else
            {
                ListObj.RemoveAt(dataGrid.SelectedIndex);
                additionalList.RemoveAt(dataGrid.SelectedIndex);
            }
            
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!dragging)
            {
                dragging = true;
                draggedPut = (Put)listBox.SelectedItem;
                draggedItem = new Put(draggedPut.Id,draggedPut.Broj,draggedPut.Tip,draggedPut.Image);
                DragDrop.DoDragDrop(this, draggedItem, DragDropEffects.Copy | DragDropEffects.Move);
                
            }
        }

        private void listBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggedItem = null;
            draggedPut = null;
            dragging = false;
            listBox.SelectedItem = null;
        }

        private void dragOver(object sender, DragEventArgs e)
        {
            base.OnDragOver(e);
            if (((Canvas)sender).Resources["taken"] != null)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private void drop(object sender, DragEventArgs e)
        {
            base.OnDrop(e);
            if (draggedItem != null)
            {
                if (((Canvas)sender).Resources["taken"] == null)
                {
                    BitmapImage img = new BitmapImage(); 
                    img.BeginInit();
                    img.UriSource = new Uri(draggedItem.Image, UriKind.Relative);
                    img.EndInit();
                    ((Canvas)sender).Background = new ImageBrush(img);
                    if (draggedItem.Tip.Equals("IA"))
                    {
                        if (draggedItem.Value > 15000)
                        {
                            ((Canvas)sender).Background = Brushes.Red;
                        }
                    }
                    else
                    {
                        if (draggedItem.Value > 7000)
                        {
                            ((Canvas)sender).Background = Brushes.Red;
                        }
                    }
                    
                    //((TextBlock)(((Canvas)sender).Children[0])).Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffdaff00"));
                    ((TextBox)((Canvas)sender).Children[0]).Text = draggedPut.Id+" "+draggedPut.Broj;
                    ((Canvas)sender).Resources.Add("taken", true);
                    ListBoxList.RemoveAt(listBox.SelectedIndex);
                    VracanjeBox.Add(((Canvas)sender).Name, draggedItem);
                }
                listBox.SelectedItem = null; 

                dragging = false;
            }
            e.Handled = true;
        }

        private void radioButtonNaz_Checked(object sender, RoutedEventArgs e)
        {
            
            naz = 0;
        }

        private void radioButtonTip_Checked(object sender, RoutedEventArgs e)
        {
            
            naz = 1;
        }

        private void buttonSear_Click(object sender, RoutedEventArgs e)
        {
            searching = textBoxSearch.Text;
            if (searching.Equals(""))
            {
                return;
            }
            
            if(naz == 0)
            {
                AdditionalList.Clear();
                foreach (Put p in ListObj)
                {
                    int temp;
                    if (Int32.TryParse(searching, out temp))
                    {
                        if (p.Id == temp)
                        {
                            AdditionalList.Add(p);
                        }
                    }
                    else
                        return;
                    
                }
            }else if(naz == 1)
            {
                AdditionalList.Clear();
                foreach (Put p in ListObj)
                {
                    if (p.Tip == searching)
                    {
                        AdditionalList.Add(p);
                    }
                }
            }
            //dodati dugme koje vraca staro stanje, a ovu listui isoraznis i stavis u aditional kopiju,
            //ne menjas item source pomocu foreacha 
            
  
        }

        private void buttonRewind_Click(object sender, RoutedEventArgs e)
        {
            AdditionalList.Clear();
            foreach (Put p in ListObj)
            {
                AdditionalList.Add(p);
            }

            
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            tryWind wind = new tryWind();
            wind.ShowDialog();
            AdditionalList.Clear();
            foreach (Put p in ListObj)
            {
                AdditionalList.Add(p);
            }
            ListBoxList.Clear();
            foreach (Put p in ListObj)
            {
                ListBoxList.Add(p);
            }
        }

        private void buttonShow_Click(object sender, RoutedEventArgs e)
        {
            try
            {   // Open the text file using a stream reader.
                string line ;
                using (StreamReader sr = new StreamReader(path))
                {
                    /* string line = sr.ReadToEnd(); 
                     string[] split = line.Split( '\n');
                     int lenght = split[0].Length;
                     int ind;

                     for (int i=0; i<ListObj.Count-1;++i)
                     {
                         ind = 0;
                         while (line.Length - lenght > 0)
                         {
                             if (split[ind].StartsWith("Object_" + i))
                             {
                                 textBoxFile.Text = "Object_" + i + "\n\t\t" + split[ind].Substring(8);
                             }
                             ind++;
                         }                      
                     } */
                    textBoxFile.Text = "";
                    for (int i = 0; i < ListObj.Count; ++i)
                    {
                        sr.DiscardBufferedData();
                        sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                        bool test = true;
                        while ((line = sr.ReadLine()) != null)
                        {
                            
                            if (line.StartsWith("Object_" + i))
                            {                           
                                if(test)
                                {
                                    textBoxFile.Text += ("\nObject_" + i);
                                }
                                test=false;
                                textBoxFile.Text += "\n\t" + line.Substring(8);
                            }
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(ex.Message);
            }
        }

        private void buttonChart_Click(object sender, RoutedEventArgs e)
        {   

            double sirina = canvasChart.Width / listObj.Count - 25;
            Rectangle[] r = new Rectangle[50];
            int bot = 40;
            double left = 30;

            for(int i=0; i<50;++i)
            {
                r[i] = new Rectangle();
            }
            canvasChart.Children.Clear();
            lines();
            for (int i=0; i< listObj.Count;++i)
            {         
                r[i].StrokeThickness = 2;
                Canvas.SetBottom(r[i], bot);
                Canvas.SetLeft(r[i], left);
                r[i].Width = sirina;
                r[i].Height = ListObj[i].Value / 100;
                r[i].Stroke = Brushes.Green;
                r[i].Fill = Brushes.Green;

                if(ListObj[i].Tip.Equals("IA"))
                {
                    if(ListObj[i].Value > 15000)
                    {
                        r[i].Stroke = Brushes.Red;
                        r[i].Fill = Brushes.Red;
                    }
                }else
                {
                    if (ListObj[i].Value > 7000)
                    {
                        r[i].Stroke = Brushes.Red;
                        r[i].Fill = Brushes.Red;
                    }
                }
                canvasChart.Children.Add(r[i]);
                
                
                left += sirina+15;
            }
        }
        //u kanvasu delis sirinu kanvasa sa brojem objekata i razmera ista, 
        //rectangle koristis svaki ima svoju boju
        private void lines()
        {
            Line line1 = new Line();
            Line line2 = new Line();
            line1.Stroke = Brushes.Black;
            line2.Stroke = Brushes.Black;

            line1.X1 = 20;
            line1.X2 = 20;
            line1.Y1 = 1;
            line1.Y2 = 280;

            line2.X1 = 20;
            line2.X2 = 640;
            line2.Y1 = 280;
            line2.Y2 = 280;

            line1.StrokeThickness = 2;
            line2.StrokeThickness = 2;
            canvasChart.Children.Add(line1);
            canvasChart.Children.Add(line2);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (canv1.Resources["taken"] != null)
            {
                canv1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv1.Resources.Remove("taken");
                textBox.Text = "~";

                ListBoxList.Add(VracanjeBox["canv1"]);
                VracanjeBox.Remove("canv1");
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (canv2.Resources["taken"] != null)
            {
                canv2.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv2.Resources.Remove("taken");
                textBox1.Text = "~";
                ListBoxList.Add(vracanjeBox["canv2"]);
                VracanjeBox.Remove("canv2");
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (canv3.Resources["taken"] != null)
            {
                canv3.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv3.Resources.Remove("taken");
                textBox2.Text = "~";
                ListBoxList.Add(vracanjeBox["canv3"]);
                VracanjeBox.Remove("canv3");
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (canv4.Resources["taken"] != null)
            {
                canv4.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv4.Resources.Remove("taken");
                textBox3.Text = "~";
                ListBoxList.Add(vracanjeBox["canv4"]);
                VracanjeBox.Remove("canv4");
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (canv5.Resources["taken"] != null)
            {
                canv5.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv5.Resources.Remove("taken");
                textBox4.Text = "~";
                ListBoxList.Add(vracanjeBox["canv5"]);
                VracanjeBox.Remove("canv5");
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (canv6.Resources["taken"] != null)
            {
                canv6.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv6.Resources.Remove("taken");
                textBox5.Text = "~";
                ListBoxList.Add(vracanjeBox["canv6"]);
                VracanjeBox.Remove("canv6");
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            if (canv7.Resources["taken"] != null)
            {
                canv7.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv7.Resources.Remove("taken");
                textBox6.Text = "~";
                ListBoxList.Add(vracanjeBox["canv7"]);
                VracanjeBox.Remove("canv7");
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            if (canv8.Resources["taken"] != null)
            {
                canv8.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv8.Resources.Remove("taken");
                textBox7.Text = "~";
                ListBoxList.Add(vracanjeBox["canv8"]);
                VracanjeBox.Remove("canv8");
            }
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            if (canv9.Resources["taken"] != null)
            {
                canv9.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0DBDB"));

                canv9.Resources.Remove("taken");
                textBox8.Text = "~";
                ListBoxList.Add(vracanjeBox["canv9"]);
                VracanjeBox.Remove("canv9");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                tabControl.SelectedIndex = 0;
            }
            else if (e.Key == Key.F6)
            {
                tabControl.SelectedIndex = 1;
            }
            else if (e.Key == Key.F7)
            {
                tabControl.SelectedIndex = 2;
            }
            else if (e.Key == Key.F8)
            {
                tabControl.SelectedIndex = 3;
            }

            if (e.Key == Key.W)
            {
                tryWind wind = new tryWind();
                wind.ShowDialog();
                AdditionalList.Clear();
                foreach (Put p in ListObj)
                {
                    AdditionalList.Add(p);
                }
                ListBoxList.Clear();
                foreach (Put p in ListObj)
                {
                    ListBoxList.Add(p);
                }
            }
        }
    }
}
