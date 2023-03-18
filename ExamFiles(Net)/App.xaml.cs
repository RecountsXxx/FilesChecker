using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace ExamFiles_Net_
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly Mutex _mutex = new Mutex(false, "ExamFiles(Net)");

        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("Gellp");
           
            if (!_mutex.WaitOne(500, false))
            {
                MessageBox.Show("Same application's copy is working!", "Error",MessageBoxButton.OK,MessageBoxImage.Warning);
                Environment.Exit(0);
            }
            else
            {
                base.OnStartup(e);
            }
        }
       
    }
}
