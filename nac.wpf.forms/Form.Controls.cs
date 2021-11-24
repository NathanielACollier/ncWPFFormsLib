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
    }
}