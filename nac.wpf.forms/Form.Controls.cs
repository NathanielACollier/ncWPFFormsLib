using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace nac.wpf.forms
{
    public partial class Form
    {
        public Form TextBoxFor(string fieldName, string value = "")
        {
            this.Model[fieldName] = value;

            TextBox tb = new TextBox();

            Helper_BindField(fieldName, tb, TextBox.TextProperty, BindingMode.TwoWay);

            this.Helper_AddRowToHost(tb, fieldName);

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


        public Form DateFor(string fieldName)
        {
            this.Model[fieldName] = new DateTime?(); // just init a date in there

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
        
        
        
        
        
    }
}