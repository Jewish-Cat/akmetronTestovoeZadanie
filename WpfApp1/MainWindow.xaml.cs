using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<string> Types;
        CancellationTokenSource _tokenSource;
        public MainWindow()
        {
            InitializeComponent();
            Types = new ObservableCollection<string> { "Синусоидальный сигнал", "FM - сигнал" };
            signalTypes.ItemsSource = Types;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SignalBuilder builder = new SignalBuilder();
            Signal signal = new Signal();
            switch (signalTypes.SelectedIndex)
            {

                case 0:
                    Generator SinGenerator = new SinusoidalGenerator();
                    signal = builder.Transmit(SinGenerator, signalParam1TextBox.Text, signalParam2TextBox.Text, signalParam3TextBox.Text, signalParam4TextBox.Text);
                    signalsDataGrid.Items.Add(signal);
                    break;
                case 1:
                    Generator FMgenerator = new FMGenerator();
                    signal = builder.Transmit(FMgenerator, signalParam1TextBox.Text, signalParam2TextBox.Text, signalParam3TextBox.Text, signalParam4TextBox.Text);
                    signalsDataGrid.Items.Add(signal);
                    break;
            }

        }

        private void SignalTypeSelect(object sender, RoutedEventArgs e)
        {
            switch (signalTypes.SelectedIndex)
            {
                case 0:
                    signalParam1Label.Content = "Частота";
                    signalParam2Label.Content = "Амплитуда";
                    signalParam3Label.Content = "Фазовый сдвиг";
                    break;
                case 1:
                    signalParam1Label.Content = "Частота несущей";
                    signalParam2Label.Content = "Амплитуда несущей";
                    signalParam3Label.Content = "Частота модулированного сигнала";
                    break;
            }
        }

        public async void ProcedureStart(object sender, RoutedEventArgs e)
        {
            ButtonAdd.IsEnabled = false;
            ButtonSave.IsEnabled = false;
            ButtonLoad.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            Progress<string> _progress = new Progress<string>(text => logWindow.Text = text);
            _tokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = _tokenSource.Token;


            try
            {
                procedureTimerLabel.Content = "0";
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += new EventHandler(timer_Tick);
                timer.Interval = new TimeSpan(0, 0, 0, 1);
                timer.Start();
                logWindow.Text =  await Task.Run(() => _procedure(cancelToken, _progress), cancelToken);
                timer.Stop();

            }
            catch (Exception ex)
            {
                logWindow.Text = logWindow.Text + $"При выполнении процедуры произошла ошибка {ex.Message}";
            }

            ButtonAdd.IsEnabled = true;
            ButtonSave.IsEnabled = true;
            ButtonLoad.IsEnabled = true;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        public void ProcedureStop(object sender, RoutedEventArgs e)
        {

            _tokenSource.Cancel();

            ButtonAdd.IsEnabled = true;
            ButtonSave.IsEnabled = true;
            ButtonLoad.IsEnabled = true;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private string _procedure(CancellationToken cancelToken, IProgress<string> progess)
        {

            StringBuilder sb = new StringBuilder();
            try
            {
                
                foreach (Signal item in signalsDataGrid.Items)
                {
                    string signalInfo = item.Type + " " + item.param1 + " " + item.param2 + " " + item.param3 + " " + item.duration;
                    sb.AppendLine(signalInfo);

                    progess.Report(sb.ToString());
                    cancelToken.WaitHandle.WaitOne(Convert.ToInt32(item.duration));

                    cancelToken.ThrowIfCancellationRequested();
                }

                sb.AppendLine("Процедура завершена");

                return sb.ToString();
            }
            catch (OperationCanceledException)
            {
                sb.AppendLine("Выполнение процедуры прервано");
                return sb.ToString();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current second
            procedureTimerLabel.Content = Convert.ToInt32(procedureTimerLabel.Content) + 1;

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public void ProcedureLoad(object sender, RoutedEventArgs e)
        {
            //signalsDataGrid.Items.Add()
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Stack<Signal>));
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                signalsDataGrid.Items.Clear();
                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    fs.Position = 0;


                    Stack<Signal> signalsList = (Stack<Signal>)serializer.ReadObject(fs);
                    foreach(Signal item in signalsList)
                    {
                        signalsDataGrid.Items.Add(item);
                    }
                    
                    MessageBox.Show($"Файл {openFileDialog.FileName} успешно загружен");
                }
            }
                //logWindow.Text = File.ReadAllText(openFileDialog.FileName);
        }

        public void ProcedureSave(object sender, RoutedEventArgs e)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Stack<Signal>));
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    Stack<Signal> signalsList = new Stack<Signal>();
                    foreach (Signal item in signalsDataGrid.Items)
                    {
                        signalsList.Push(item);
                    }


                    serializer.WriteObject(fs, signalsList);

                    MessageBox.Show("Сохранение успешно");
                }
            }
        }

    }
}
