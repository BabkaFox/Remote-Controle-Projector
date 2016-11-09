using System;
using System.Threading;
using System.Windows;
using System.Xml;

namespace TurnOnProjector
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        System.Net.IPAddress prIPAddress = System.Net.IPAddress.Parse("0.0.0.0");
        public Settings()
        {
            InitializeComponent();

            try //Открыть файл, получить данные о проекторе
            {
                string prType = "", prIP = "";

                using (XmlReader reader = XmlReader.Create(MainWindow.FILE_SETTINGS))
                {
                    while (reader.Read())
                    {
                        switch (reader.Name.ToString())
                        {
                            case "pr":
                                prType = reader.ReadString();
                                break;
                            case "ippr":
                                prIP = reader.ReadString();
                                break;
                        }

                    }
                }

                switch (prType)
                {
                    case "0":
                        cbProjector.SelectedIndex = 0;
                        tbIP1.Text = prIP.Split("."[0])[0];
                        tbIP2.Text = prIP.Split("."[0])[1];
                        tbIP3.Text = prIP.Split("."[0])[2];
                        tbIP4.Text = prIP.Split("."[0])[3];
                        break;
                    case "1":
                        cbProjector.SelectedIndex = 1;
                        tbIP1.Text = prIP.Split("."[0])[0];
                        tbIP2.Text = prIP.Split("."[0])[1];
                        tbIP3.Text = prIP.Split("."[0])[2];
                        tbIP4.Text = prIP.Split("."[0])[3];
                        break;
                    default:
                        //System.Windows.MessageBox.Show("Ошибка чтения файла. В файле кривые настройки");
                        break;
                }

            }
            catch (XmlException)
            {
                Console.WriteLine("В настройках беда");
            }
            catch (Exception)
            {
                Console.WriteLine("В настройках беда. Нет файла");

            }


        }

        private void testProjector_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                prIPAddress = System.Net.IPAddress.Parse((tbIP1.Text + "." + tbIP2.Text + "." + tbIP3.Text + "." + tbIP4.Text));
            }
            catch (FormatException)
            {
                MessageBox.Show("Неправильно указан IP адрес!");
                return;
            }

            try
            {
                string s = "<xml><pr>" + cbProjector.SelectedIndex.ToString() + "</pr><ippr>" + prIPAddress + "</ippr></xml>";
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(s);
                xdoc.Save(MainWindow.FILE_SETTINGS);
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка записи в файл :(");
            }
       
        }

        private void tbIP_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
          /*  if (btnSave != null)
            {
                btnSave.IsEnabled = false;
            }*/
        }

        private void comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnSave.IsEnabled = true;         
        }
    }
}
