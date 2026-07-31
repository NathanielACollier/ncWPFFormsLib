using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests;

[TestClass]
public class TabTests
{
    private static nac.Logging.Logger log = new();
    
    
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


    [TestMethodWPF]
    public async Task TestVisualIndicatorThatErroHasOccuredOnTab()
    {
        var mainForm = new Form();

        mainForm
            .AddTab(t =>
            {
                t.Text("Press button below to cause a test log message to be written.")
                    .HorizontalGroup(h =>
                    {
                        h.ButtonWithLabel("Info", (_args) => { log.Info("A normal log message"); })
                            .ButtonWithLabel("Warn", (_args) => { log.Warn("A messing that is a warning"); })
                            .ButtonWithLabel("Error", (_args) => { log.Error("An error message"); });
                    });
            }, tabName: "Main")
            .AddTab(t => { t.LogViewer(onLogReady: () => { log.Info("Logging ready..."); }); }, tabName: "Log",
                populateHeaderForm: tabHeader =>
                {
                    // populate the header for the Log Tab
                    tabHeader.Text("Log")
                        .HorizontalGroup(hori => { hori.Text("!!!--ERROR--!!!"); },
                            isVisiblePropertyName: "logTabError")
                        .ButtonWithLabel("Test", (_args) => { log.Info("Header button clicked"); });
                }, OnFocus: () => mainForm.Model["logTabError"] = false)
            .Display(onDisplay: f =>
            {
                f.Model["logTabError"] = false;
                // watch for anything that is an error and change model
                nac.Logging.Logger.OnNewMessage += (_s, _e) =>
                {
                    bool isInfo = new[] { "info", "debug" }.Contains(_e.Level.ToLower());
                    if (!isInfo)
                    {
                        f.Model["logTabError"] = true;
                    }
                };
            });
    }
    
    
    
    
    
}