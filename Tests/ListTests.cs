using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests;

[TestClass]
public class ListTests
{
    
    [TestMethodWPF]
    public void TestBasicList()
    {
        var form = new Form();
        var items = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();
        form.Model["list1"] = items;

        var newItemFactory = new Func<nac.utilities.BindableDynamicDictionary>(() =>
        {
            var newItem = new nac.utilities.BindableDynamicDictionary();
            newItem["isChecked"] = false;
            newItem["currentDate"] = "";
            return newItem;
        });

        items.Add(newItemFactory());

        form.ButtonWithLabel("Add Item", (_o) =>
            {
                items.Add(newItemFactory());
            })
            .HorizontalGroup(f =>
                f.Text("Check Count: ")
                    .TextFor("checkedCount")
            )
            .List("list1", f =>
                f
                    .CheckBoxFor("isChecked", checkChangedAction: (_o) =>
                    {
                        var model = _o as nac.utilities.BindableDynamicDictionary;
                        form.Model["checkedCount"] = items.Count(i => (bool)i["isChecked"] == true);
                    })
                    .TextFor("currentDate")
                    .ButtonWithLabel("Click Me!", (_o) =>
                    {
                        var model = _o as nac.utilities.BindableDynamicDictionary;
                        model["currentDate"] = DateTime.Now.ToLongTimeString();
                    })
            );

        string xaml = form.Xaml;

        form
            .Display();
    }
    
    
    
    
    
    [TestMethodWPF]
    public void TestListInVerticalGroup()
    {
        var f = new Form();

        var items = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();
        f.Model["list1"] = items;
        var rand = new Random();
        for (int i = 0; i < 1000; ++i)
        {
            var item = new nac.utilities.BindableDynamicDictionary();
            item["Number"] = rand.Next(0, 10000);
            items.Add(item);
        }

        f.VerticalGroup(v =>
        {
            v.Text("Hello World!")
                .List("list1", (itemRow) =>
                {
                    itemRow.HorizontalGroup(h =>
                    {
                        h.Text("Number is: ")
                            .TextBoxFor("Number");
                    });
                });
        }).Display();
    }


    [TestMethodWPF]
    public void TestListInSplitVerticalGroup()
    {
        var f = new Form();

        var items = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();
        f.Model["list1"] = items;
        var rand = new Random();
        for (int i = 0; i < 1000; ++i)
        {
            var item = new nac.utilities.BindableDynamicDictionary();
            item["Number"] = rand.Next(0, 10000);
            items.Add(item);
        }

        f.VerticalGroupSplit(v =>
        {
            v.Text("Hello World!")
                .List("list1", (itemRow) =>
                {
                    itemRow.HorizontalGroup(h =>
                    {
                        h.Text("Number is: ")
                            .TextBoxFor("Number");
                    });
                });
        }).Display();
    }
    
    
    
    
    
    
}