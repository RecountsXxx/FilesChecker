using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ехам
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> words = new List<string>() { "школа", "привет" };
        private List<FileInfo> pathes = new List<FileInfo>();
        private string[] logicalAllDrives = new string[20];
        private object locker = new object();
        private void GetFiles(string path)
        {
            lock (locker)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(path);

                    FileInfo[] files = dir.GetFiles("*.txt");
                    foreach (FileInfo f in files)
                    {
                        pathes.Add(f);
                    }
                    foreach (DirectoryInfo d in dir.GetDirectories())
                    {
                        GetFiles(path + d.Name + @"\");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
        }

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                logicalAllDrives = Directory.GetLogicalDrives();
                foreach (var item in logicalAllDrives)
                {

                    Thread thread = new Thread(() => GetFiles(item));
                    thread.Start();
                    thread.Join();


                }
            }
            catch
            {

            }
        }
    }
}
