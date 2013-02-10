﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
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

namespace WPFAutoSelectTextBox
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
            _timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            _timer.Tick += TimerOnTick;
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if(_flag)
            {
                TbAutoSelectTextBox.SetBinding(AutoSelectTextBox.AutoSelectTextProperty, new Binding("Text") { Source = TbAutoSelectText1 });
            }else
            {
                TbAutoSelectTextBox.SetBinding(AutoSelectTextBox.AutoSelectTextProperty, new Binding("Text") { Source = TbAutoSelectText2 });
            }
            _flag = !_flag;
        }
    }
}
