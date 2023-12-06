using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace nac.wpf.forms;

public partial class Form
{

    public class ObjectViewerFunctions<T>
    {
        public Action<T> updateValue;
    }


    private void ObjectViewerUpdateTreeView<T>(TreeView tv, T newItem)
    {
        try
        {
            tv.Dispatcher.Invoke(() =>
            {
                nac.wpf.utilities.WPFTreeViewObjectPropertiesBuilder.BuildTree(tv, "ObjectView", newItem,
                expandSettings: new nac.wpf.utilities.WPFTreeViewObjectPropertiesBuilder.NodeExpandSettings(expandAll: true)
                );
            });
        }
        catch (Exception ex)
        {
            log.Error($"Failed to update treeview with newItem argument.  Exception: {ex}");
        }
    }


    public Form ObjectViewer<T>(T initialItemValue = null,
        ObjectViewerFunctions<T> functions = null)
        where T : class
    {
        var tv = new TreeView();

        if (initialItemValue != null)
        {
            ObjectViewerUpdateTreeView(tv, initialItemValue);
        }

        if (functions != null)
        {
            functions.updateValue = (newItem) =>
            {
                ObjectViewerUpdateTreeView(tv, newItem);
            };
        }

        Helper_AddRowToHost(tv, rowAutoHeight: false);
        return this;
    }


}
