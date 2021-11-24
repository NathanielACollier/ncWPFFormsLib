using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace nac.wpf.forms
{
    public partial class Form
    {
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
        
        
        
        
        
    }
}