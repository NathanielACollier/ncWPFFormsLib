using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace ncWPFFormsLib
{
    public class Form
    {

        public BindableDynamicDictionary Model { get; set; }
        private StackPanel Host { get; set; }

        public Form()
        {
            this.Model = new BindableDynamicDictionary();
            this.Host = new StackPanel();
            this.Host.Orientation = Orientation.Vertical;
        }

        #region UI Helpers

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

        #endregion


        #region Chain Functions

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


        public Form TextBoxMultipleFor(string fieldName)
        {
            var items = new ObservableCollection<BindableDynamicDictionary>();
            this.Model[fieldName] = items;
            items.Add(new BindableDynamicDictionary()); // start with one blank entry

            ListView lv = new ListView();

            SetupListViewStyleForMultipleItems(lv);

            string dataTemplateXaml = @"
                <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                    <DockPanel >
                        <Button Name=""AddRowButton"" Content=""Add"" DockPanel.Dock=""Right"" />
                        <Button Name=""RemoveRowButton"" Content=""Remove"" DockPanel.Dock=""Right"" />
                        <TextBox Text=""{Binding Path=Text, Mode=TwoWay}"" DockPanel.Dock=""Left"" />
                    </DockPanel>
                </DataTemplate>
            ";
            var template = (DataTemplate)XamlReader.Parse(dataTemplateXaml);
            lv.ItemTemplate = template;

            Binding itemSourceBind = new Binding();
            itemSourceBind.Source = this.Model;
            itemSourceBind.Path = new PropertyPath(fieldName);
            BindingOperations.SetBinding(lv, ListView.ItemsSourceProperty, itemSourceBind);

            var buttonClick = new RoutedEventHandler((s, e) =>
            {
                Button btn = e.OriginalSource as Button;

                if (string.Equals(btn.Name, "AddRowButton"))
                {
                    // this is the add row button
                    items.Add(new BindableDynamicDictionary());
                }
                else if (string.Equals(btn.Name, "RemoveRowButton"))
                {
                    var model = btn.DataContext as BindableDynamicDictionary;
                    if (items.Count > 1)
                    {
                        items.Remove(model);
                    }
                }
            });

            // from: http://stackoverflow.com/questions/19706044/datatemplate-at-runtime-with-event
            lv.AddHandler(Button.ClickEvent, buttonClick);

            this.AddRowToHost(lv, fieldName);

            return this;
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


        #region Auto Suggest
        private string AutoSuggestSourceName(string fieldName)
        {
            return $"{fieldName}_autosuggestSource";
        }

        private string BusyBindModelName(string fieldName)
        {
            return $"{fieldName}_busy";
        }

        private string TimerName(string fieldName)
        {
            return $"{fieldName}_autoCompleteTimer";
        }

        private void PopulateAutoComplete(AutoCompleteBox tb,
                            Func<string, IEnumerable<string>> itemsGenerator,
                            BindableDynamicDictionary model,
                            string itemFieldName)
        {
            var source = model[AutoSuggestSourceName(itemFieldName)] as ObservableCollection<string>;
            string busyName = BusyBindModelName(itemFieldName);

            model[busyName] = true;

            string textBoxTextCopy = tb.Text;

            Thread t = new Thread(() =>
            {
                try
                {
                    var items = itemsGenerator(textBoxTextCopy);

                    tb.Dispatcher.Invoke(() =>
                    {
                        source.Clear();
                        foreach (string i in items)
                        {
                            source.Add(i);
                        }

                    });

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    tb.Dispatcher.Invoke(() =>
                    {
                        model[busyName] = false;
                    });
                }
            });

            t.Start();
        }


        private void SetupTimerForAutoComplete(BindableDynamicDictionary model,
                                        string fieldName,
                                        AutoCompleteBox tb,
                                        Func<string, IEnumerable<string>> itemsGenerator)
        {
            string timerName = TimerName(fieldName);
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(600)
            };

            timer.Tick += (sender, args) =>
            {
                timer.Stop(); // stop the timer, so that it can be started again the next time someone types
                PopulateAutoComplete(tb, itemsGenerator, model, fieldName);

            };

            model[timerName] = timer; // may need to save this...
        }


        public Form AutoSuggestFor(string fieldName, Func<string, IEnumerable<string>> itemsGenerator)
        {
            this.Model[fieldName] = "";
            string autoSuggestSourceFieldName = AutoSuggestSourceName(fieldName);
            this.Model[autoSuggestSourceFieldName] = new ObservableCollection<string>();
            string busyFieldName = BusyBindModelName(fieldName);
            this.Model[busyFieldName] = false;




            var tb = new System.Windows.Controls.AutoCompleteBox();
            tb.IsTextCompletionEnabled = false;
            tb.MinimumPrefixLength = 0;
            tb.FilterMode = AutoCompleteFilterMode.None; // we are going to filter inside the function
            SetupTimerForAutoComplete(this.Model, fieldName, tb, itemsGenerator);

            var busyIndicator = new ncWPFBusyControlLib.BusyIndicatorControl();
            busyIndicator.Width = 20;
            busyIndicator.Height = 20;
            busyIndicator.Visibility = Visibility.Collapsed; // start out hidden

            // see this: http://www.broculos.net/2014/04/wpf-autocompletebox-autocomplete-text.html#.VxeC5zArJaQ

            Binding textBind = new Binding();
            textBind.Source = this.Model;
            textBind.Path = new PropertyPath(fieldName);
            textBind.Mode = BindingMode.TwoWay;
            textBind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(tb, AutoCompleteBox.SelectedItemProperty, textBind);

            Binding itemsBind = new Binding();
            itemsBind.Source = this.Model;
            itemsBind.Path = new PropertyPath(autoSuggestSourceFieldName);
            BindingOperations.SetBinding(tb, AutoCompleteBox.ItemsSourceProperty, itemsBind);

            Binding busyBind = new Binding();
            busyBind.Source = this.Model;
            busyBind.Path = new PropertyPath(busyFieldName);
            busyBind.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(busyIndicator, ncWPFBusyControlLib.BusyIndicatorControl.BusyProperty, busyBind);

            DockPanel halfRow = new DockPanel();
            DockPanel.SetDock(busyIndicator, Dock.Left);
            DockPanel.SetDock(tb, Dock.Right);
            halfRow.Children.Add(busyIndicator);
            halfRow.Children.Add(tb);

            AddRowToHost(halfRow, fieldName);

            PopulateAutoComplete(tb, itemsGenerator, this.Model, fieldName);

            this.Model.PropertyChanged += (sender, args) =>
            {
                if (string.Equals(args.PropertyName, fieldName))
                {
                    // start a timer, or reset a timer to repopulate the drop down
                    var timer = this.Model[TimerName(fieldName)] as System.Windows.Threading.DispatcherTimer;
                    timer.Stop();
                    timer.Start();
                }
            };



            return this;
        }

        #region AutoSuggestMultiple

        private string AutoSuggestMultipleFieldName()
        {
            return "item";
        }

        private string AutoSuggestMultipleTextPath(string fieldName)
        {
            return $"{fieldName}_Text";
        }

        private void AddAutoSuggestMultipleItem(ObservableCollection<BindableDynamicDictionary> items)
        {
            var item = new BindableDynamicDictionary();
            string fieldName = AutoSuggestMultipleFieldName();

            item[AutoSuggestMultipleTextPath(fieldName)] = "";
            item[AutoSuggestSourceName(fieldName)] = new ObservableCollection<string>();
            item[BusyBindModelName(fieldName)] = false; // populating the item source or not...

            items.Add(item);
        }

        public Form AutoSuggestMultipleFor(string fieldName, Func<string, IEnumerable<string>> itemsGenerator)
        {
            var items = new ObservableCollection<BindableDynamicDictionary>();
            this.Model[fieldName] = items;


            ListView lv = new ListView();

            SetupListViewStyleForMultipleItems(lv);



            string dataTemplateXaml = @"
                <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                                xmlns:toolKit=""clr-namespace:System.Windows.Controls;assembly=System.Windows.Controls.Input.Toolkit""
                                xmlns:busyNS=""clr-namespace:WpfBusyIndicator;assembly=WpfBusyIndicator""
                                >
                    <DockPanel >
                        <Button Name=""AddRowButton"" Content=""Add"" DockPanel.Dock=""Right"" />
                        <Button Name=""RemoveRowButton"" Content=""Remove"" DockPanel.Dock=""Right"" />
                        <busyNS:BusyIndicatorControl DockPanel.Dock=""Right"" Busy=""{Binding Path=##BusyPath##, Mode=TwoWay}"" Width=""20"" Height=""20"" Visibility=""Collapsed"" />
                        <toolKit:AutoCompleteBox Name=""AutoComplete"" Text=""{Binding Path=##TextPath##, Mode=TwoWay}"" ItemsSource=""{Binding Path=##ItemsPath##}"" DockPanel.Dock=""Left"" />
                    </DockPanel>
                </DataTemplate>
            ";

            string perItemFieldName = AutoSuggestMultipleFieldName();
            dataTemplateXaml = dataTemplateXaml
                        .Replace("##TextPath##", AutoSuggestMultipleTextPath(perItemFieldName))
                        .Replace("##BusyPath##", BusyBindModelName(perItemFieldName))
                        .Replace("##ItemsPath##", AutoSuggestSourceName(perItemFieldName));

            var template = (DataTemplate)XamlReader.Parse(dataTemplateXaml);
            lv.ItemTemplate = template;

            Binding itemSourceBind = new Binding();
            itemSourceBind.Source = this.Model;
            itemSourceBind.Path = new PropertyPath(fieldName);
            BindingOperations.SetBinding(lv, ListView.ItemsSourceProperty, itemSourceBind);

            var buttonClick = new RoutedEventHandler((s, e) =>
            {
                Button btn = e.OriginalSource as Button;

                if (string.Equals(btn.Name, "AddRowButton"))
                {
                    // this is the add row button
                    AddAutoSuggestMultipleItem(items);
                }
                else if (string.Equals(btn.Name, "RemoveRowButton"))
                {
                    var model = btn.DataContext as BindableDynamicDictionary;
                    if (items.Count > 1)
                    {
                        items.Remove(model);
                    }
                }
            });

            lv.ItemContainerGenerator.StatusChanged += (s, e) =>
            {
                var generator = s as ItemContainerGenerator;
                if (generator.Status == GeneratorStatus.ContainersGenerated)
                {
                    // loop through make sure things are setup
                    foreach (BindableDynamicDictionary model in generator.Items)
                    {
                        if (!model.GetDynamicMemberNames().Contains("TimerSetup"))
                        {
                            model["TimerSetup"] = true;
                            var visualItem = generator.ContainerFromItem(model) as ListViewItem;

                            AutoCompleteBox box = VisualTree.FindVisualChildren<AutoCompleteBox>(visualItem)
                                                    .Single();

                            SetupTimerForAutoComplete(model, AutoSuggestMultipleFieldName(), box, itemsGenerator);
                        }

                    }
                }
            };


            // need this to happen after we've setup the collection changed event
            AddAutoSuggestMultipleItem(items); // need to start off with one item


            var autoCompleteTextChanged = new RoutedEventHandler((s, e) =>
            {
                // there's only one AutoComplete
                AutoCompleteBox box = e.OriginalSource as AutoCompleteBox;
                var model = box.DataContext as BindableDynamicDictionary;

                // start a timer, or reset a timer to repopulate the drop down
                var timer = model[TimerName(perItemFieldName)] as System.Windows.Threading.DispatcherTimer;
                timer.Stop();
                timer.Start();
            });


            // from: http://stackoverflow.com/questions/19706044/datatemplate-at-runtime-with-event
            lv.AddHandler(Button.ClickEvent, buttonClick);
            lv.AddHandler(AutoCompleteBox.TextChangedEvent, autoCompleteTextChanged);

            this.AddRowToHost(lv, fieldName);

            return this;
        }

        #endregion


        #endregion


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


        #region TreeView



        public Form TreeFor(string fieldName, Func<BindableDynamicDictionary, IEnumerable<BindableDynamicDictionary>> generateChildren)
        {
            var rootItems = new ObservableCollection<BindableDynamicDictionary>();
            string rootItemsPath = $"{fieldName}_RootItems";
            this.Model[rootItemsPath] = rootItems;

            string treeViewXaml = @"
        <TreeView xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            xmlns:ncUtil=""clr-namespace:NacWPFUtilities;assembly=NacWPFUtilities""
            ItemsSource=""{Binding Path=##RootItemsPath##}""
            >
            <TreeView.Resources>
                <HierarchicalDataTemplate DataType=""{x:Type ncUtil:BindableDynamicDictionary}"" ItemsSource=""{Binding Path=Children}"">
                    <TextBlock Text=""{Binding Path=Name}"" />
                </HierarchicalDataTemplate>
            </TreeView.Resources>
        </TreeView>
            ";

            treeViewXaml = treeViewXaml.Replace("##RootItemsPath##", rootItemsPath);

            var tree = (TreeView)XamlReader.Parse(treeViewXaml);
            tree.DataContext = this.Model; // this should enable us to bind in the xaml above I think...


            rootItems.Add(new BindableDynamicDictionary(
                new Dictionary<string, object>
                {
                    {"Name", "Root Item 1" }
                }
                ));


            this.AddRowToHost(tree, fieldName);

            return this;
        }



        #endregion


        #region DropDown

        private string DropDownItemSourceName(string fieldName)
        {
            return $"{fieldName}_dropDownItemSource";
        }

        public Form DropDownFor(string fieldName, IEnumerable<string> dropDownSource)
        {
            this.Model[fieldName] = "";
            string itemSourceFieldName = DropDownItemSourceName(fieldName);
            this.Model[itemSourceFieldName] = dropDownSource;

            ComboBox cb = new ComboBox();

            // item source binding
            Binding itemSourceBind = new Binding();
            itemSourceBind.Source = this.Model;
            itemSourceBind.Path = new PropertyPath(itemSourceFieldName);
            BindingOperations.SetBinding(cb, ComboBox.ItemsSourceProperty, itemSourceBind);

            // selected item binding
            Binding itemBind = new Binding();
            itemBind.Source = this.Model;
            itemBind.Path = new PropertyPath(fieldName);
            itemBind.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(cb, ComboBox.SelectedItemProperty, itemBind);

            this.AddRowToHost(cb, fieldName);

            return this;
        }


        public Form DropDownFor(string fieldName, Type enumType)
        {
            // enumType from: http://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum
            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"Type {enumType.Name} is not an enum type.", nameof(enumType));
            }

            string itemSourceName = DropDownItemSourceName(fieldName);
            this.Model[itemSourceName] = Enum.GetValues(enumType);

            ComboBox cb = new ComboBox();

            // item source binding
            Binding itemSourceBind = new Binding();
            itemSourceBind.Source = this.Model;
            itemSourceBind.Path = new PropertyPath(itemSourceName);
            BindingOperations.SetBinding(cb, ComboBox.ItemsSourceProperty, itemSourceBind);

            // selected item binding
            Binding itemBind = new Binding();
            itemBind.Source = this.Model;
            itemBind.Path = new PropertyPath(fieldName);
            itemBind.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(cb, ComboBox.SelectedItemProperty, itemBind);

            this.AddRowToHost(cb, fieldName);

            return this;
        }





        #endregion






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





        public Form Display(int height = 600, int width = 800)
        {
            var win = new Window();
            win.Height = height;
            win.Width = width;
            win.Content = this.Host;

            win.ShowDialog();

            return this;
        }

        #endregion

    }
}
