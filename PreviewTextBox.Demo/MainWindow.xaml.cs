using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PreviewTextBox.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private bool _flag;

        public MainWindow()
        {
            InitializeComponent();
            BtnChangeText.Click += BtnChangeTextOnClick;
            BtnChangeRealText.Click += BtnChangeRealTextOnClick;
            _timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            _timer.Tick += TimerOnTick;
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Start();
        }

        private void BtnChangeRealTextOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            TbPreviewTextBox.RealText = TbChangeText.Text;
            TbChangeText.Text = null;
        }

        private void BtnChangeTextOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            TbPreviewTextBox.Text = TbChangeText.Text;
            TbChangeText.Text = null;
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (_flag)
            {
                TbPreviewTextBox.SetBinding(Library.PreviewTextBox.PreviewTextProperty, new Binding("Text") { Source = TbPreviewText1 });
            }
            else
            {
                TbPreviewTextBox.SetBinding(Library.PreviewTextBox.PreviewTextProperty, new Binding("Text") { Source = TbPreviewText2 });
            }
            _flag = !_flag;
        }
    }
}
