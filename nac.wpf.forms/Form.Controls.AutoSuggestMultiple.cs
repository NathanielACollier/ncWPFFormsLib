using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace nac.wpf.forms
{
    public partial class Form
    {
        private string AutoSuggestMultipleFieldName()
        {
            return "item";
        }

        private string AutoSuggestMultipleTextPath(string fieldName)
        {
            return $"{fieldName}_Text";
        }

        private void AddAutoSuggestMultipleItem(ObservableCollection<nac.utilities.BindableDynamicDictionary> items)
        {
            var item = new nac.utilities.BindableDynamicDictionary();
            string fieldName = AutoSuggestMultipleFieldName();

            item[AutoSuggestMultipleTextPath(fieldName)] = "";
            item[AutoSuggestSourceName(fieldName)] = new ObservableCollection<string>();
            item[BusyBindModelName(fieldName)] = false; // populating the item source or not...

            items.Add(item);
        }

        public Form AutoSuggestMultipleFor(string fieldName, Func<string, IEnumerable<string>> itemsGenerator)
        {
            var items = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();
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

            Helper_BindField(fieldName, lv, ListView.ItemsSourceProperty);

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
                    var model = btn.DataContext as nac.utilities.BindableDynamicDictionary;
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
                    foreach (nac.utilities.BindableDynamicDictionary model in generator.Items)
                    {
                        if (!model.GetDynamicMemberNames().Contains("TimerSetup"))
                        {
                            model["TimerSetup"] = true;
                            var visualItem = generator.ContainerFromItem(model) as ListViewItem;

                            AutoCompleteBox box = nac.wpf.utilities.VisualTree.FindVisualChildren<AutoCompleteBox>(visualItem)
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
                var model = box.DataContext as nac.utilities.BindableDynamicDictionary;

                // start a timer, or reset a timer to repopulate the drop down
                var timer = model[TimerName(perItemFieldName)] as System.Windows.Threading.DispatcherTimer;
                timer.Stop();
                timer.Start();
            });


            // from: http://stackoverflow.com/questions/19706044/datatemplate-at-runtime-with-event
            lv.AddHandler(Button.ClickEvent, buttonClick);
            lv.AddHandler(AutoCompleteBox.TextChangedEvent, autoCompleteTextChanged);

            this.Helper_AddRowToHost(lv, fieldName);

            return this;
        }
    }
}