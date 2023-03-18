using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using MessageBox = System.Windows.MessageBox;

namespace ExamFiles_Net_
{ 
    public partial class MainWindow : System.Windows.Window
    {
        private object locker = new object();

        private List<FileInfo> pathes = new List<FileInfo>();
        private List<string> pathesNew = new List<string>();
        private List<string> words = new List<string>();

        private List<FileInformation> fileInformation = new List<FileInformation>();

        private string[] logicalDisksArray = new string[20];

        private string pathSave = null;
        private string pathWords = null;

        private Thread threadGetFiles;
        private Thread threadGetListFiles;

        private new System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        
        public MainWindow()
        {
            InitializeComponent();

            logicalDisksArray = Directory.GetLogicalDrives();

            timer.Interval = 3000;
            timer.Tick += Timer_Tick_DefaultLabel;

            #region CMD
            string[] args = Environment.GetCommandLineArgs();
            if (args != null & args.Length == 3)
            {
                this.Hide();

                pathSave = args[1];
                pathWords = args[2];
                using (StreamReader reader = new StreamReader(pathWords))
                {
                    string line;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        words.Add(line);
                    }
                }
                foreach (var item in logicalDisksArray)
                {
                    threadGetFiles = new Thread(() => GetFiles(item));
                    threadGetFiles.Start();
                    threadGetFiles.Join();
                }
                pathes.Remove(pathes.Where(x => x.FullName == pathWords).Select(x => x).First());
                threadGetListFiles = new Thread(GetListFiles);
                threadGetListFiles.Start();
            }
            #endregion

        }

        #region Timer
        private async void Timer_Tick_DefaultLabel(object sender, EventArgs e)
        {
            await SlowOpactityLabel(2);
            ErorrLabel.Foreground = Brushes.Red;
            timer.Stop();
        }
        #endregion

        #region UI Funcs
        private async void ResetUIAndLists()
        {
            listBoxPathesWhereContainsWord.Items.Clear();
            fileInformation.Clear();
            pathes.Clear();
            pathesNew.Clear();
            ErorrLabel.Opacity = 0;
            progressBarScaningPathes.Value = 0;
            await SlowOpactityProgressBar(1);
        }
        private async Task SlowOpactityLabel(int variant)
        {
            if (variant == 1)
            {
                await Task.Factory.StartNew(() =>
                {
                    for (double i = 0.0; i < 1.1; i += 0.1)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ErorrLabel.Opacity = i;
                        }));
                        Thread.Sleep(25);

                    }
                });
            }
            if (variant == 2)
            {
                await Task.Factory.StartNew(() =>
                {
                    for (double i = 1.0; i > 0.0; i -= 0.1)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ErorrLabel.Opacity = i;
                        }));
                        Thread.Sleep(25);
                    }
                });
            }
        }
        private async Task SlowOpactityProgressBar(int variant)
        {
            if (variant == 1)
            {
                await Task.Factory.StartNew(() =>
                {
                    for (double i = 0.0; i < 1.1; i += 0.1)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressBarScaningPathes.Opacity = i;
                        }));
                        Thread.Sleep(25);

                    }
                });
            }
            if (variant == 2)
            {
                await Task.Factory.StartNew(() =>
                {
                    for (double i = 1.0; i > 0.0; i -= 0.1)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressBarScaningPathes.Opacity = i;
                        }));
                        Thread.Sleep(25);
                    }
                });
            }

        }
        #endregion

        #region Pathes buttons
        private void savingPathButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            if(folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pathSave = folder.SelectedPath;
                savingPathButton.Content = pathSave;
            }
        }
        private async void searhWordButton_Click(object sender, RoutedEventArgs e)
        {
            if (pathSave != null)
            {
                if(pathWords != null)
                {
                    ResetUIAndLists();

                    await Task.Factory.StartNew(() =>
                    {
                        foreach (var item in logicalDisksArray)
                        {
                            threadGetFiles = new Thread(() => GetFiles(item));
                            threadGetFiles.Start();
                            threadGetFiles.Join();
                        }
                    });

                    pathes.Remove(pathes.Where(x=>x.FullName == pathWords).Select(x=>x).First());
                    threadGetListFiles = new Thread(GetListFiles);
                    threadGetListFiles.Start();
                }
                else
                {
                    ErorrLabel.Content = "Please select file";
                    timer.Start();
                    await SlowOpactityLabel(1);
                }   
            }
            else
            {
                ErorrLabel.Content = "Please select folder";
                timer.Start();
                await SlowOpactityLabel(1);

            }

        }
        private void importWordsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog folder = new OpenFileDialog();
            folder.Filter = "Text Files | *.txt";
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pathWords = folder.FileName;
                importWordsButton.Content = pathWords;
                using (StreamReader reader = new StreamReader(pathWords))
                {
                    string line;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        words.Add(line);
                    }
                }
            }
        }
        #endregion

        #region Work in file system
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
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        progressBarScaningPathes.Value += 0.45;
                    }));

                }
            }
        }
        public async void GetListFiles()
        {
            lock (locker)
            {
                if (File.Exists(pathSave + "/ReportFile.txt"))
                    File.Delete(pathSave + "/ReportFile.txt");

                foreach (var item in pathes)
                {
                    try
                    {
                        AmountWordsInFilePerEveryWord(item);
                        int wordCount = CountWordInFile(item.FullName);
                      
                        if (wordCount > 0)
                        {
                            string report = string.Format("Path -  {0}, Size - {1}, Count replaces - {2}, Top 10 - {3}\r\n", item.FullName, item.Length, wordCount, wordCount);
                            using (StreamWriter writer = new StreamWriter(pathSave + "/ReportFile.txt", true))
                            {
                                writer.WriteLine(report);
                            }
                            Thread thread = new Thread(() => CopyFile(item.FullName));
                            thread.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            timer.Start();
            await Task.Factory.StartNew(() =>
            {
                for (double i = 0.0; i < 1.1; i += 0.1)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ErorrLabel.Opacity = i;
                        ErorrLabel.Foreground = Brushes.GreenYellow;
                        ErorrLabel.Content = "Succesfull!";
                    }));
                    Thread.Sleep(25);

                }
            });
            await SlowOpactityProgressBar(2);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                progressBarScaningPathes.Value = 100;
            }));

        }

        public void ReplaceWordInFile()
        {
            lock (locker)
            {
                foreach (var item in pathesNew)
                {
                    string str = File.ReadAllText(item);
                    for (int j = 0; j < words.Count; j++)
                    {
                        for (int i = 0; i < words[j].Length; i++)
                        {
                            string asterisks = new string('*', words[j].Length);
                            str = str.Replace(words[j], asterisks);
                            File.WriteAllText(item, str);
                        }
                    }
                }
            }

                var top10 = fileInformation.OrderByDescending(x => x.wordAndCount.count).Take(10).Select(x => x);
            if (File.Exists(pathSave + "/Top10Words.txt"))
                File.Delete(pathSave + "/Top10Words.txt");
            foreach (var word in top10)
            {
                string str = "Path: " + word.path + ", Word: " + word.wordAndCount.word + ", Count: " + word.wordAndCount.count + "\r\n";
                using(StreamWriter writer = new StreamWriter(pathSave + "/Top10Words.txt",true))
                {
                    writer.WriteLine(str);
                }
            }

        }
        public void CopyFile(string item)
        {
            Random rd = new Random();
            lock (locker)
            {
                string newPath = null;
                    FileInfo infoFile = new FileInfo(item);
                    newPath = infoFile.Name.Remove(infoFile.Name.Length - 4, 4) + rd.Next(0, 100000) + ".txt";
                if (!File.Exists(pathSave + "\\" + newPath))
                {
                    Thread.Sleep(100);
                    rd = new Random();
                    newPath = infoFile.Name.Remove(infoFile.Name.Length - 4, 4) + rd.Next(0, 1000000) + ".txt";
                }
                    File.Copy(item, pathSave + "\\" + newPath);
                    pathesNew.Add(pathSave + "\\" + newPath);
            }
            Thread thread = new Thread(ReplaceWordInFile);
            thread.Start();
            thread.Join();
        }

        private void AmountWordsInFilePerEveryWord(FileInfo path)
        {
            try
            {
                int wordCount = 0;
                if (File.Exists(path.FullName))
                {
                    string fileContent = File.ReadAllText(path.FullName);
                    foreach (var temp in words)
                    {
                        wordCount = Regex.Matches(fileContent, "\\b" + temp + "\\b").Count;
                        if (wordCount > 0)
                        {
                            FileInformation file = new FileInformation(path.FullName);
                            file.wordAndCount = new WordInformation(temp,wordCount);
                            fileInformation.Add(file);
                            string str = $"Путь: '{path} 'Слово '{temp}' встречается в файле {wordCount} раз(а).";
                            Dispatcher.BeginInvoke(new Action(() => { listBoxPathesWhereContainsWord.Items.Add(str); }));
                        }
                    }
                }
            }
            catch
            {

            }
        }
        public int CountWordInFile(string filePath)
        {
            int count = 0;
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] wordsTemp = line.Split(' ');

                foreach (string w in wordsTemp)
                {
                    foreach(var item in words)
                    {
                        if (w.Equals(item, StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }
        #endregion

        #region Windows buttons
        private void Window_Closed(object sender, EventArgs e)
        {
            //threadGetFiles.Abort();
            //threadGetListFiles.Abort();
            Environment.Exit(0);
            this.Close();
        }
        private void Window_DragDrop(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void hideButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Thread buttons
        private void pauseThreadButton_Click(object sender, RoutedEventArgs e)
        {
            threadGetFiles.Suspend();
        }
        private void stopThreadButton_Click(object sender, RoutedEventArgs e)
        {
            
                threadGetFiles.Abort();
            progressBarScaningPathes.Value = 100;


        }
        private void ResumeThreadButton_Click(object sender, RoutedEventArgs e)
        {
            if (threadGetFiles.ThreadState != System.Threading.ThreadState.Stopped)
            {
                threadGetFiles.Resume();
            }
        }
        #endregion

    }
    public class FileInformation
    {
        public string path { get; set; }
        public WordInformation wordAndCount { get; set; }
        public FileInformation(string path)
        {
            this.path = path;
        }
    }
    public class WordInformation
    {
        public string word { get; set; }
        public int count { get; set; }
        public WordInformation(string word, int count)
        {
            this.word = word;
            this.count = count;
        }
    }

}
