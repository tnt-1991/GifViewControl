using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace GifViewControlApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileStream fs;

        public MainWindow()
        {
            InitializeComponent();

            string link = (@"Images/giphy.gif");
            fs = new FileStream(link, FileMode.Open, FileAccess.Read);
            Byte[] imgByte = new byte[fs.Length];
            fs.Read(imgByte, 0, System.Convert.ToInt32(fs.Length));

            GifViewControl gifViewControl = new GifViewControl();
            gifViewControl.GifSource = fs;
            gifViewControl.Stretch = Stretch.Fill;
            gifViewControl.SpeedRatio = 1.0;
            gifViewControl.AutoStart = true;

            main.Children.Add(gifViewControl);

            this.Closed += new EventHandler(MainWindow_Closed);
        }

        /// <summary>
        /// disposing FileStream on close
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            fs.Close();
        }
    }
}