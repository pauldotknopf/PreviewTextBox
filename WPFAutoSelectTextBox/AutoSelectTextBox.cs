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
        private Key _lastKey;
        private bool _ignoreNextChanged;
        private bool _isDeleting = false;

        public AutoSelectTextBox()
        {
            this.TextChanged += OnTextChanged;
            this.PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            _lastKey = keyEventArgs.Key;

            // do nothing is we don't have auto select text
            if(!HasAutoSelectText)
                return;

            if (_lastKey == Key.Back)
            {
                // if we are deleting,
                // then ignore the on text changed event (which does preview)
                // this allows the user to delete to remove the preview
                // settings the _IsDeleteingFlag causes the real text to be updated
                // after the delete has actually happened
                _isDeleting = true;
                _ignoreNextChanged = true; // dont do any preview logic
                RealText = Text;
            }
            else if (_lastKey == Key.Enter)
            {
                // we want to select the preview text
                // if the real text is different than what is being dispayed,
                // then the user is seeing a preview and whishes to select
                // whe he sees.
                if(RealText != Text)
                {
                    Text = RealText = AutoSelectText;
                    SelectionStart = AutoSelectText.Length;
                    IsPreviewing = false;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            // don't do anthing if we don't have select text
            if (!HasAutoSelectText)
                return;

            // when we leave the textbox, remove the preview
            // by setting the textbox text to what the real text is
            if (RealText != Text)
            {
                _ignoreNextChanged = true; // dont do any preview logic
                Text = RealText;
            }
        }

        public static readonly DependencyProperty AutoSelectTextProperty =
            DependencyProperty.Register("AutoSelectText", typeof(string), typeof(AutoSelectTextBox), new PropertyMetadata(default(string), AutoSelectTextPropertyChangedCallback));

        private static void AutoSelectTextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var selectBox = dependencyObject as AutoSelectTextBox;
            if (selectBox != null) selectBox.AutoSelectTextChanged();
        }

        public string AutoSelectText
        {
            get { return (string)GetValue(AutoSelectTextProperty); }
            set { SetValue(AutoSelectTextProperty, value); }
        }

        public static readonly DependencyProperty RealTextProperty =
            DependencyProperty.Register("RealText", typeof (string), typeof (AutoSelectTextBox), new PropertyMetadata(default(string)));

        public string RealText
        {
            get { return (string) GetValue(RealTextProperty); }
            set { SetValue(RealTextProperty, value); }
        }

        public static readonly DependencyProperty IsPreviewingProperty =
            DependencyProperty.Register("IsPreviewing", typeof (bool), typeof (AutoSelectTextBox), new PropertyMetadata(default(bool)));

        public bool IsPreviewing
        {
            get { return (bool) GetValue(IsPreviewingProperty); }
            set { SetValue(IsPreviewingProperty, value); }
        }

        private void AutoSelectTextChanged()
        {
            bool setCarrotAtEnd = IsPreviewing;
            _ignoreNextChanged = true;
            Text = RealText;
            // we are currently displaying the preview value, update it
            PreviewText(RealText, AutoSelectText);
            if (setCarrotAtEnd)
                SelectionStart = RealText.Length;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            // if the last key that was pressed was a delete,
            // we need to update the real text with what the resulting text is after the delete
            if(_isDeleting)
            {
                RealText = Text;
                _isDeleting = !_isDeleting;
            }

            if(!HasAutoSelectText)
            {
                RealText = Text;
                return;
            }

            if (_ignoreNextChanged)
            {
                Debug.WriteLine("OnTextChanged: Ignoring");
                _ignoreNextChanged = !_ignoreNextChanged;
                return;
            }
            else
            {
                Debug.WriteLine("OnTextChanged: Performing");
            }

            if (Text != RealText)
                RealText = Text;
            Debug.WriteLine("OnTextChanged: Real text is '" + RealText + "'");

            PreviewText(RealText, AutoSelectText);
        }

        /// <summary>
        /// Does a string contain the following?
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        private bool Contains(string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Do we have any auto select text?
        /// </summary>
        private bool HasAutoSelectText
        {
            get
            {
                return !string.IsNullOrEmpty(AutoSelectText);
            }
        }

        /// <summary>
        /// Try to preview the auto select text if conditions are met.
        /// The conditions are:
        /// 1) Are we focused?
        /// 2) Does the textbox have data?
        /// 3) Is the text less than the preview text?
        /// 4) And does the preview text begin with the actual text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="previewText"></param>
        private void PreviewText(string text, string previewText)
        {
            if (IsFocused
                && !string.IsNullOrEmpty(text)
                && text.Length < previewText.Length
                && previewText.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
            {
                IsPreviewing = true;
                Debug.WriteLine("OnTextChanged: The text starts with our auto select phrase. Change text to our auto select text and highlight the preview text");
                // we are settings the text,
                // ignore any of the on changed mumbo jumbo because we are settings additional text to preview
                _ignoreNextChanged = true;
                Text = previewText;
                // highlight the preview text to indicate it is previewing (not actually there)
                SelectionStart = text.Length;
                SelectionLength = previewText.Length - text.Length;
            }
            else
            {
                IsPreviewing = false;
            }
        }
    }
}
