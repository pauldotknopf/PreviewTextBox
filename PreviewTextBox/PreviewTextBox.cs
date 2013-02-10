using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PreviewTextBox.Library
{
    /// <summary>
    /// This is a textbox that has the ability to display previewed text that can be selected by pressening enter.
    /// </summary>
    public class PreviewTextBox : TextBox
    {
        private bool _ignoreNextChanged;
        private bool _isDeleting = false;

        /// <summary>
        /// Ctor
        /// </summary>
        public PreviewTextBox()
        {
            TextChanged += OnTextChanged;
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            // do nothing is we don't have preview text
            if (!HasPreviewText)
                return;

            if (keyEventArgs.Key == Key.Back)
            {
                // if we are deleting,
                // then ignore the on text changed event (which does preview)
                // this allows the user to delete to remove the preview
                // settings the _IsDeleteingFlag causes the real text to be updated
                // after the delete has actually happened
                _isDeleting = true;
                SetRealText(Text);
            }
            else if (keyEventArgs.Key == Key.Enter)
            {
                // we want to select the preview text
                // if the real text is different than what is being dispayed,
                // then the user is seeing a preview and whishes to select
                // whe he sees.
                if (RealText != Text)
                {
                    SetRealText(PreviewText);
                    Text = RealText;
                    SelectionStart = PreviewText.Length;
                    IsPreviewing = false;
                }
            }else if(keyEventArgs.Key == Key.Left 
                || keyEventArgs.Key == Key.Right 
                || keyEventArgs.Key == Key.Up 
                || keyEventArgs.Key == Key.Down)
            {
                // we are trying to navigate within the textbox, remove the preview text
                if(IsPreviewing)
                {
                    _ignoreNextChanged = true;
                    Text = RealText;
                    SelectionStart = Text.Length;
                    IsPreviewing = false;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            // don't do anthing if we don't have select text
            if (!HasPreviewText)
                return;

            // when we leave the textbox, remove the preview
            // by setting the textbox text to what the real text is
            if (RealText != Text)
            {
                _ignoreNextChanged = true; // dont do any preview logic
                Text = RealText;
            }
        }

        /// <summary>
        /// See <see cref="PreviewText"/>
        /// </summary>
        public static readonly DependencyProperty PreviewTextProperty =
            DependencyProperty.Register("PreviewText", typeof(string), typeof(PreviewTextBox), new PropertyMetadata(default(string), PreviewTextPropertyChangedCallback));
        /// <summary>
        /// Raised when the PreviewText property changes
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="dependencyPropertyChangedEventArgs"></param>
        private static void PreviewTextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var selectBox = dependencyObject as PreviewTextBox;
            if (selectBox != null) selectBox.PreviewTextChanged();
        }

        /// <summary>
        /// The text that will be previewed to the user for selection by pressing enter
        /// </summary>
        public string PreviewText
        {
            get { return (string)GetValue(PreviewTextProperty); }
            set { SetValue(PreviewTextProperty, value); }
        }

        /// <summary>
        /// See <see cref="RealText"/>
        /// </summary>
        public static readonly DependencyProperty RealTextProperty =
                DependencyProperty.Register(
                        "RealText",
                        typeof(string),
                        typeof(PreviewTextBox), 
                        new FrameworkPropertyMetadata(string.Empty,
                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, RealTextPropertyChangedCallback));
        private static void RealTextPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var selectBox = dependencyObject as PreviewTextBox;
            if (selectBox != null) selectBox.RealTextChanged();
        }

        /// <summary>
        /// This is the real intended input for the user.
        /// Since the Text can be, at times, filled with additional characters for previewing,
        /// this value will always contain the actual text that the user intended on using.
        /// Binding to this property from your view models
        /// </summary>
        public string RealText
        {
            get { return (string)GetValue(RealTextProperty); }
            set { SetValue(RealTextProperty, value); }
        }

        /// <summary>
        /// See <see cref="IsPreviewing"/>
        /// </summary>
        public static readonly DependencyProperty IsPreviewingProperty =
            DependencyProperty.Register("IsPreviewing", typeof(bool), typeof(PreviewTextBox), new PropertyMetadata(default(bool)));
        /// <summary>
        /// Is there currently a text preview going on?
        /// This means that the Text property contains additional characters that the user may not intend on using.
        /// In this case, the RealText will have the data the user is intending on using
        /// </summary>
        public bool IsPreviewing
        {
            get { return (bool)GetValue(IsPreviewingProperty); }
            set { SetValue(IsPreviewingProperty, value); }
        }

        /// <summary>
        /// This is raised when the PreviewText text changes
        /// </summary>
        private void PreviewTextChanged()
        {
            // Set the new text flat out (don't preview yet)
            _ignoreNextChanged = true;
            Text = RealText;

            // try to preview the text with the new preview text (if any)
            TryPreviewText(RealText, PreviewText);

            // set the carrot at the end of the real text
            SelectionStart = RealText.Length;
        }

        private bool _isInternalRealTextSet;
        /// <summary>
        /// Raised when the preview text is changed
        /// </summary>
        private void RealTextChanged()
        {
            if(_isInternalRealTextSet)
            {
                // we don't want to update the text property when realtext is changed internally.
                // however, if third party sources update RealText, the intend on changing the
                // entire contents of the textbox, so we set Text in that case.
                _isInternalRealTextSet = !_isInternalRealTextSet;
                return;
            }

            Text = RealText;
        }

        /// <summary>
        /// When the text changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="textChangedEventArgs"></param>
        private void OnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            // if the last key that was pressed was a delete,
            // we need to update the real text with what the resulting text is after the delete
            if (_isDeleting)
            {
                SetRealText(Text);
                _isDeleting = !_isDeleting;
                return;
            }

            if(!IsFocused)
            {
                // we update the text from another source, maybe from the view model, who cares,
                // but we don't do anything
                SetRealText(Text);
                return;
            }

            if (!HasPreviewText)
            {
                // keep the real text insync, event though there is no preview text
                // this makes it so that we can always trust RealText, even if we
                // aren't planning on previewing text
                SetRealText(Text);
                return;
            }

            // in some cases, we don't want to preview the text on changed,
            // if so, stop here
            if (_ignoreNextChanged)
            {
                // don't stop the next time though!
                _ignoreNextChanged = !_ignoreNextChanged;
                return;
            }

            // at this point, the text should be the intended users input
            // ensure that the real text is updated if needed.
            if (Text != RealText)
                SetRealText(Text);

            // Try to preview the text
            TryPreviewText(RealText, PreviewText);
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
        /// Do we have any preview text?
        /// </summary>
        private bool HasPreviewText
        {
            get
            {
                return !string.IsNullOrEmpty(PreviewText);
            }
        }

        /// <summary>
        /// Try to preview the text if conditions are met.
        /// The conditions are:
        /// 1) Are we focused?
        /// 2) Does the textbox have data?
        /// 3) Is the text less than the preview text?
        /// 4) And does the preview text begin with the actual text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="previewText"></param>
        private void TryPreviewText(string text, string previewText)
        {
            if (IsFocused
                && !string.IsNullOrEmpty(text)
                && text.Length < previewText.Length
                && previewText.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
            {
                IsPreviewing = true;
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

        /// <summary>
        /// Set RealText while not updating that actual text
        /// </summary>
        /// <param name="realText"></param>
        private void SetRealText(string realText)
        {
            _isInternalRealTextSet = true;
            RealText = realText;
        }
    }
}
