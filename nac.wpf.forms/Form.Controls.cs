using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace nac.wpf.forms
{
    public partial class Form
    {
        public Form TextBoxFor(string fieldName, string value = "",
            Action<KeyEventArgs> onKeyUp = null,
            bool showFieldNameOnRow = true,
            bool multiline = false)
        {
            this.Model[fieldName] = value;

            TextBox tb = new TextBox();

            tb.KeyUp += (_s, _args) =>
            {
                if (onKeyUp != null)
                {
                    onKeyUp(_args);
                }
            };

            Helper_BindField(fieldName, tb, TextBox.TextProperty, BindingMode.TwoWay);

            if (multiline)
            {
                var sv = new ScrollViewer();
                sv.Content = tb;

                tb.TextWrapping = TextWrapping.Wrap;
                tb.AcceptsReturn = true;
                tb.AcceptsTab = true;
                tb.SpellCheck.IsEnabled = true;

                if (showFieldNameOnRow)
                {
                    this.Helper_AddRowToHost(sv, rowLabel: fieldName, rowAutoHeight: false);
                }
                else
                {
                    this.Helper_AddRowToHost(sv, rowAutoHeight: false);
                }

            }
            else
            {
                if (showFieldNameOnRow)
                {
                    this.Helper_AddRowToHost(tb, fieldName);
                }
                else
                {
                    this.Helper_AddRowToHost(tb);
                }
            }

            return this;
        }

        public Form LabelFor(string fieldName, string value = "")
        {
            this.Model[fieldName] = value;
            Label label = new Label();

            Helper_BindField(fieldName, label, Label.ContentProperty, BindingMode.TwoWay);

            this.Helper_AddRowToHost(label, fieldName);

            return this;
        }


        public Form ButtonWithLabel(string labelText, RoutedEventHandler onClick)
        {
            Button btn = new Button();
            btn.Content = labelText;
            btn.Click += onClick;
            this.Helper_AddRowToHost(btn, "");
            return this;
        }
        
        
        public Form PasswordFor(string fieldName)
        {
            this.Model[fieldName] = new System.Security.SecureString();

            PasswordBox box = new PasswordBox();
            // It's impossible to bind to PasswordBox so we have to do it this way
            //          see: http://stackoverflow.com/questions/1483892/how-to-bind-to-a-passwordbox-in-mvvm

            box.PasswordChanged += (sender, args) =>
            {
                this.Model[fieldName] = ((PasswordBox)sender).SecurePassword;
            };

            this.Helper_AddRowToHost(box, fieldName);

            return this;
        }


        public Form DateFor(string fieldName, DateTime? initialDate = null)
        {
            this.Model[fieldName] = new DateTime?(); // just init a date in there

            if (initialDate.HasValue)
            {
                this.Model[fieldName] = initialDate.Value;
            }

            DatePicker dp = new DatePicker();
            Helper_BindField(fieldName, dp, DatePicker.SelectedDateProperty, BindingMode.TwoWay);

            this.Helper_AddRowToHost(dp, fieldName);

            return this;
        }



        public Form ButtonsTrueFalseFor(string fieldName)
        {
            this.Model[fieldName] = false;
            var selectedBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            var unselectedBrush = (new Button()).Background;

            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            var falseButton = new Button();
            var trueButton = new Button();
            // since we start the model out as false, then start the false button out as colored selected
            falseButton.Background = selectedBrush;

            falseButton.Width = 30;
            falseButton.Content = "False";
            falseButton.Click += (s, e) =>
            {
                this.Model[fieldName] = false;
                falseButton.Background = selectedBrush;
                trueButton.Background = unselectedBrush;
            };

            trueButton.Width = 30;
            trueButton.Content = "True";
            trueButton.Click += (s, e) =>
            {
                this.Model[fieldName] = true;
                trueButton.Background = selectedBrush;
                falseButton.Background = unselectedBrush;
            };
            trueButton.Margin = new Thickness(0, 0, 7, 0);

            sp.Children.Add(trueButton);
            sp.Children.Add(falseButton);

            Helper_AddRowToHost(sp, fieldName);

            return this;
        }


        public Form Text(string text)
        {
            var lbl = new Label();
            lbl.Content = text;
            Helper_AddRowToHost(lbl);

            return this;
        }



        public Form TextFor(string fieldName)
        {
            var lbl = new Label();

            Helper_BindField(fieldName, lbl, Label.ContentProperty);

            Helper_AddRowToHost(lbl);
            return this;
        }



        public Form CheckBoxFor(string fieldName, Action<object> checkChangedAction = null)
        {
            var cb = new CheckBox();

            if (checkChangedAction != null)
            {
                Helper_setupControlCommand(cb, CheckBox.CommandProperty, checkChangedAction, commandParameterProperty: CheckBox.CommandParameterProperty);
            }

            Helper_BindField(fieldName, cb, CheckBox.IsCheckedProperty, BindingMode.TwoWay);

            this.Helper_AddRowToHost(cb);
            return this;

        }


        public Form Line()
        {
            var seperator = new Separator();

            this.Helper_AddRowToHost(seperator);

            return this;
        }



        public Form Image(string fieldName)
        {
            var img = new System.Windows.Controls.Image();

            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            scrollViewer.Content = img;

            Helper_BindField(fieldName, img, System.Windows.Controls.Image.SourceProperty, BindingMode.TwoWay);

            this.Helper_AddRowToHost(scrollViewer, rowAutoHeight: false);

            return this;
        }



    }
}