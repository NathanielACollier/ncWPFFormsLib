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
        
        
        private static void SetupListViewStyleForMultipleItems(ListView lv)
        {
            // style it to stretch list items
            // see: http://stackoverflow.com/questions/10300228/set-itemcontainerstyle-from-code
            // see: http://stackoverflow.com/questions/1080479/how-to-make-dockpanel-fill-available-space
            var lvStyle = new Style(typeof(ListView));
            var horizontalContentAlignmentSetter = new Setter
            {
                Property = ListViewItem.HorizontalContentAlignmentProperty,
                Value = HorizontalAlignment.Stretch
            };
            lvStyle.Setters.Add(horizontalContentAlignmentSetter);
            lv.Style = lvStyle;
        }
        
        
        
    }
}