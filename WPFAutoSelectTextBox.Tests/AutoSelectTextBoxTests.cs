using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using NUnit.Framework;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace WPFAutoSelectTextBox.Tests
{
    [TestFixture]
    public class AutoSelectTextBoxTests
    {
        private Application _application;
        private Window _window;

        [Test]
        public void Can_preview_text()
        {
            var tb = BuildTextBox();
            tb.AutoSelectText = "test";
            RaiseKeyPressed(tb, Key.T);
            RaiseKeyPressed(tb, Key.L);
            Assert.AreEqual("l", tb.Text);
        }

        private void RaiseKeyPressed(AutoSelectTextBox textBox, Key key)
        {
            textBox.Focus();
            SendMessage(new WindowInteropHelper(_window).Handle, 0x0102, MapVirtualKey((int)key, MAPVK_VK_TO_CHAR), 0);
        }

        private AutoSelectTextBox BuildTextBox()
        {
            _window = new Window();
            
            var stackPanel = new StackPanel();
            var textBox = new AutoSelectTextBox();
            stackPanel.Children.Add(textBox);

            _window.Content = stackPanel;
            _window.Show();
            textBox.Focus();
            return textBox;
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


        const uint MAPVK_VK_TO_CHAR = 0x02;
        [DllImport("user32.dll")]
        static extern int MapVirtualKey(int uCode, uint uMapType);
    }
}
