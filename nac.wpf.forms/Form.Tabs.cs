using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace nac.wpf.forms;

public partial class Form
{
    private void AddFormToTab(Form f, TabControl tc, string tabName,
            Action<Form> populateHeaderForm,
            Action onFocus
        )
    {
        var item = new TabItem();

        if (populateHeaderForm != null)
        {
            setupTabItemHeaderForm(populateHeaderForm: populateHeaderForm, item: item);
        }
        else
        {
            // only do a string for the header if no header form is sent in
            item.Header = tabName;
        }

        item.GotFocus += (_s, _args) =>
        {
            if (onFocus != null)
            {
                onFocus();
            }
        };

        var dp = new DockPanel();
        item.Content = dp;

        dp.Children.Add(f.Host);

        tc.Items.Add(item);
    }


    private void setupTabItemHeaderForm(Action<Form> populateHeaderForm, TabItem item)
    {
        var tabHeaderForm = new Form(_parentForm: this);
        populateHeaderForm(tabHeaderForm);

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var childControls = tabHeaderForm.Host.Children.OfType<UIElement>().ToList();
        foreach (var child in childControls)
        {
            tabHeaderForm.Host.Children.Remove(child); // get the child out of the form so we can move it
            header.Children.Add(child);
        }

        item.Header = header;
    }


    private string getTabsControlIndexName(string tabControlName = "")
    {
        return $"{tabControlName}_tabs";
    }

    private TabControl getTabsControl(string tabControlName = "")
    {
        string index = getTabsControlIndexName(tabControlName: tabControlName);
        if (!this.controlsIndex.ContainsKey(index))
        {
            var tabcontrol = new TabControl();

            Helper_AddRowToHost(tabcontrol, ctrlIndex: getTabsControlIndexName(),
                rowAutoHeight: false
                );
            this.controlsIndex[index] = tabcontrol;
            return tabcontrol;
        }

        return this.controlsIndex[index] as TabControl;
    }


    public Form AddTab(Action<Form> setupNewTabForm, string tabName = "", string tabControlIndex = "",
        Action<Form> populateHeaderForm = null,
        Action OnFocus = null
    )
    {
        var tabControl = getTabsControl(tabControlIndex);
        var newTabForm = new Form(_parentForm: this);
        setupNewTabForm(newTabForm);

        AddFormToTab(newTabForm, tabControl, tabName: tabName, populateHeaderForm: populateHeaderForm,
            onFocus: OnFocus
        );

        return this;
    }

}
