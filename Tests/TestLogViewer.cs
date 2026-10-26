using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests
{
    [TestClass]
    public class LogTests
    {
        private static nac.Logging.Logger log = new();

        [TestMethodWPF]
        public void TestLogReadyInLogViewer()
        {
            new Form()
                .LogViewer(onLogReady: () =>
                {
                    log.Info("Hello World! -- Successfull test of log is ready!");
                })
                .Display();
        }

        [TestMethodWPF]
        public void TestLogViewer()
        {
            var form = new Form();
            form.TextBoxFor("message", "Hello World!")
                .ButtonWithLabel("Log Message", (_o) =>
                {
                    log.Info(form.Model["message"] as string);
                })
                .LogViewer()
                .Display();
        }

        [TestMethodWPF]
        public void TestMultipleTabsWithLogViewer()
        {
            var mainForm = new Form();

            mainForm.AddTab(t =>
            {
                t.Text("Press button below to cause a test log message to be written.")
                    .HorizontalGroup(h =>
                    {
                        h.ButtonWithLabel("Info", (_args) => { log.Info("A normal log message"); })
                            .ButtonWithLabel("Warn", (_args) => { log.Warn("A message that is a warning"); })
                            .ButtonWithLabel("Error", (_args) => { log.Error("An error message"); });
                    });
            }, tabName: "Main")
            .AddTab(t => { t.LogViewer(onLogReady: () => { log.Info("Logging ready..."); }); }, tabName: "Log");

            mainForm.Display();
        }
    }
}
