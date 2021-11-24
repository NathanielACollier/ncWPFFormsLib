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
            // bind in code: http://stackoverflow.com/questions/7525185/how-to-set-a-binding-in-code
            Binding bind = new Binding();
            bind.Source = this.Model;
            bind.Path = new PropertyPath(fieldName);
            bind.Mode = BindingMode.TwoWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(tb, TextBox.TextProperty, bind);

            this.AddRowToHost(tb, fieldName);

            return this;
        }

        public Form LabelFor(string fieldName, string value = "")
        {
            this.Model[fieldName] = value;
            Label label = new Label();

            Binding bind = new Binding();
            bind.Source = this.Model;
            bind.Path = new PropertyPath(fieldName);
            bind.Mode = BindingMode.TwoWay;
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(label, Label.ContentProperty, bind);

            this.AddRowToHost(label, fieldName);

            return this;
        }


        public Form ButtonWithLabel(string labelText, RoutedEventHandler onClick)
        {
            Button btn = new Button();
            btn.Content = labelText;
            btn.Click += onClick;
            this.AddRowToHost(btn, "");
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

            this.AddRowToHost(box, fieldName);

            return this;
        }


        public Form DateFor(string fieldName)
        {
            this.Model[fieldName] = new DateTime?(); // just init a date in there

            DatePicker dp = new DatePicker();
            Binding bind = new Binding();
            bind.Source = this.Model;
            bind.Path = new PropertyPath(fieldName);
            bind.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(dp, DatePicker.SelectedDateProperty, bind);

            this.AddRowToHost(dp, fieldName);

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

            AddRowToHost(sp, fieldName);

            return this;
        }


        public Form Text(string text)
        {
            var lbl = new Label();
            lbl.Content = text;
            AddRowToHost(lbl);

            return this;
        }
        
        
        
        
        
    }
}