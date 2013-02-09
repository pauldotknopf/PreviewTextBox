using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFAutoSelectTextBox
{
    public class AutoSelectTextBox : TextBox
    {
        private string _realText = string.Empty;
        private Key _lastKey;
        private bool _ignoreNextChanged;

        public AutoSelectTextBox()
        {
            this.TextChanged += OnTextChanged;
            this.PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            _lastKey = keyEventArgs.Key;
            if(_lastKey == Key.Back)
            {
                _ignoreNextChanged = true;
            }else if (_lastKey == Key.Enter)
            {
                // we want to select the preview text
                if (!string.IsNullOrEmpty(AutoSelectText))
                {
                    Text = AutoSelectText;
                    SelectionStart = AutoSelectText.Length;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if(!string.IsNullOrEmpty(AutoSelectText))
                Text = _realText;
        }

        public static readonly DependencyProperty AutoSelectTextProperty =
            DependencyProperty.Register("AutoSelectText", typeof (string), typeof (AutoSelectTextBox), new PropertyMetadata(default(string)));

        public string AutoSelectText
        {
            get { return (string) GetValue(AutoSelectTextProperty); }
            set { SetValue(AutoSelectTextProperty, value); }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if(_ignoreNextChanged)
            {
                Debug.WriteLine("OnTextChanged: Ignoring");
                _ignoreNextChanged = !_ignoreNextChanged;
                return;
            }else
            {
                Debug.WriteLine("OnTextChanged: Performing");
            }
            
            _realText = Text;
            Debug.WriteLine("OnTextChanged: Real text is '" + _realText + "'");

            if(AutoSelectText.StartsWith(Text, StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.WriteLine("OnTextChanged: The text starts with our auto select phrase. Change text to our auto select text and highlight the preview text");
                // it matches
                _ignoreNextChanged = true;
                Text = AutoSelectText;
                SelectionStart = _realText.Length;
                SelectionLength = AutoSelectText.Length - _realText.Length;
            }
        }

        private bool Contains(string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
