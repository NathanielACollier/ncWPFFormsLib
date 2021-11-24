using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace nac.wpf.forms
{
    public partial class Form
    {
        private void AddRowToHost(FrameworkElement ctrl, string rowLabel)
        {
            DockPanel row = new DockPanel();
            Label label = new Label();

            label.Content = rowLabel;

            row.Children.Add(label);
            row.Children.Add(ctrl);

            DockPanel.SetDock(label, Dock.Left);
            DockPanel.SetDock(ctrl, Dock.Right);

            this.Host.Children.Add(row);
        }

        private void AddRowToHost(FrameworkElement ctrl)
        {
            DockPanel row = new DockPanel();

            row.Children.Add(ctrl);

            this.Host.Children.Add(row);
        }



        private void BindField(string fieldName,
            DependencyObject control,
            DependencyProperty controlProperty,
            BindingMode mode = BindingMode.Default,
            UpdateSourceTrigger trigger = UpdateSourceTrigger.Default)
        {
            Binding bind = new Binding();
            bind.Source = this.Model;
            bind.Path = new PropertyPath(fieldName);
            bind.Mode = mode;
            bind.UpdateSourceTrigger = trigger;
            BindingOperations.SetBinding(control, controlProperty, bind);
        }
    }
}