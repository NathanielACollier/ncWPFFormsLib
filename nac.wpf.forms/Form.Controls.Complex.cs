using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace nac.wpf.forms;

public partial class Form
{

    public Form TreeFor(string fieldName, Func<nac.utilities.BindableDynamicDictionary, IEnumerable<nac.utilities.BindableDynamicDictionary>> generateChildren)
    {
        var rootItems = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();
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


        rootItems.Add(new nac.utilities.BindableDynamicDictionary(
            new Dictionary<string, object>
            {
                    {"Name", "Root Item 1" }
            }
            ));


        this.Helper_AddRowToHost(tree, fieldName);

        return this;
    }



    public Form TextBoxMultipleFor(string fieldName)
    {
        var items = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();
        this.Model[fieldName] = items;
        items.Add(new nac.utilities.BindableDynamicDictionary()); // start with one blank entry

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
                items.Add(new nac.utilities.BindableDynamicDictionary());
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

        // from: http://stackoverflow.com/questions/19706044/datatemplate-at-runtime-with-event
        lv.AddHandler(Button.ClickEvent, buttonClick);

        this.Helper_AddRowToHost(lv, fieldName);

        return this;
    }



    public Form Table<T>(IEnumerable<T> items)
    {
        var dg = new DataGrid();
        string tableItemsSourceFieldName = $"table_{Guid.NewGuid().ToString("N")}";

        this.Model[tableItemsSourceFieldName] = items;

        Helper_BindField(tableItemsSourceFieldName, dg, DataGrid.ItemsSourceProperty);

        Helper_AddRowToHost(dg, rowAutoHeight: false);
        return this;
    }


    public Form List(string itemSourcePropertyName, Action<Form> populateItemRow)
    {
        var horizontalGroupForm = new Form(_parentForm: this);

        horizontalGroupForm.HorizontalGroup(f => populateItemRow(f));

        var grid = (horizontalGroupForm.Host.Children[0] as DockPanel).Children[0] as Grid;

        var xaml = nac.wpf.utilities.SaveXaml.SaveXamlUtility.SaveXaml(grid);

        var itemsCtrl = new ItemsControl();
        Helper_BindField(itemSourcePropertyName, itemsCtrl, ItemsControl.ItemsSourceProperty);

        // setup item template
        var itemTemplate = Helper_GetDatatemplate($"<DataTemplate>{xaml}</DataTemplate>");
        itemsCtrl.ItemTemplate = itemTemplate;

        // list should be scrollable and ItemsControl doesn't have built in scrollviewer
        var listScroller = new ScrollViewer();
        listScroller.Content = itemsCtrl;

        Helper_AddRowToHost(listScroller, rowAutoHeight: false);
        return this;
    }
}
