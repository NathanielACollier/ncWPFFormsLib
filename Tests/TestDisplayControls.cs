using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests
{
    [TestClass]
    public class DisplayControlsTests
    {
        [TestMethodWPF]
        public void DisplayTextTest()
        {
            var form = new Form();

            form.Text("Hello World!")
                .Display();
        }

        [TestMethodWPF]
        public void TestAddALine()
        {
            new Form()
                .Text("Above Line")
                .Line()
                .Text("Below Line")
                .Display();
        }

        [TestMethodWPF]
        public void TestBusy()
        {
            var form = new Form();
            var busyActions = new nac.wpf.forms.Form.BusyFunctions();

            form.Busy("Loading..", false, functions: busyActions)
                .TextBoxFor("output")
                .ButtonWithLabel("Start", (_o) =>
                {
                    form.Model["output"] = $"Busy is: {busyActions.isBusy()}\nStarting...";
                    busyActions.start();
                })
                .ButtonWithLabel("Stop", (_o) =>
                {
                    form.Model["output"] = $"Busy is: {busyActions.isBusy()}\nStopping...";
                    busyActions.stop();
                })
                .Display();
        }

        [TestMethodWPF]
        public void TestMultilineTextbox()
        {
            var f = new Form();

            f.ButtonWithLabel("Click Me!", (_args) =>
            {

            }).TextBoxFor("text", multiline: true);

            f.Display();
        }
    }
}
