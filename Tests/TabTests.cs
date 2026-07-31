using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests;

[TestClass]
public class TabTests
{
    [TestMethodWPF]
    public void TestMultipleTabs()
    {
        var form = new Form()
            .AddTab((newF) => newF.TextBoxFor("tb1", "Hello World!"))
            .AddTab((newF) =>
                    newF.DateFor("Christmass", new DateTime(DateTime.Now.Year, 12, 31))
                , tabName: "Christmas")
            .Display();
    }


    [TestMethodWPF]
    public void AddTabLater()
    {
        var form = new Form()
            .AddTab((f) => f.Text("Hello World!.  The label below should have it's value shared with last tab...")
                    .LabelFor("var1") // make sure the model is shared accross tabs
                ,
                tabName: "TabA"
            );

        form.AddTab(f => f.Text("Hey There!")
            , tabName: "TabB");

        // test without a tab name
        form.AddTab(f => f.TextBoxFor("var1", "Hello")
            .LabelFor("var1")
            .DateFor("christmas")
        );

        form.Display();
    }
}