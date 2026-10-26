using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests
{
    [TestClass]
    public class ThreadingAndCallbackTests
    {
        private static nac.Logging.Logger log = new();

        /*
         !!NOTE!!
             + This test doesn't have the WPF attribute, because it's testing making the WPF thread setup stuff
         */
        [TestMethod]
        public async Task TestStartUI()
        {
            await Form.StartUI(f =>

                f.Text("Hello World!")
            );
        }

        [TestMethodWPF]
        public async Task TestFormClosingEvent()
        {
            new Form()
                .Text("Close me to get a debug message")
                .Display(onClosing: (f) =>
                {
                    System.Diagnostics.Debug.WriteLine("***************************************");
                    System.Diagnostics.Debug.WriteLine("--           Form Closed             --");
                    System.Diagnostics.Debug.WriteLine("***************************************");
                });
        }

        [TestMethodWPF]
        public async Task TestUpdateUIOnDifferentThread()
        {
            bool isRequestingTime = true;

            new Form()
                .TextBoxFor("time")
                .Display(onDisplay: f =>
                {
                    // start thread to keep date/time updated
                    Task.Run(async () =>
                    {
                        while (isRequestingTime)
                        {
                            await f.BeginInvoke(() =>
                            {
                                f.Model["time"] = DateTime.Now.ToString();
                            });
                            System.Threading.Thread.Sleep(200);
                        }
                    });
                }, onClosing: f =>
                {
                    isRequestingTime = false;
                });
        }

        [TestMethodWPF]
        public async Task TestDisplayImage()
        {
            string imagePath = System.Environment.ExpandEnvironmentVariables(@"%userprofile%\desktop\temp\pictures\winter.png");

            var img = nac.wpf.utilities.Convert.ToWPFBitmapImage(System.IO.File.ReadAllBytes(imagePath));

            new Form()
                .Image("imgToDisplay")
                .Display(onDisplay: f =>
                {
                    f.Model["imgToDisplay"] = img;
                });
        }

        [TestMethodWPF]
        public void TestTextBoxKeyUp()
        {
            var f = new Form();

            f.TextBoxFor("t1", onKeyUp: (_args) =>
            {
                f.Model["msg"] = $"You pressed: {_args.Key}";
            })
            .TextFor("msg")
            .Display();
        }

        [TestMethodWPF]
        public void TestButtonThatClosesWindow()
        {
            var f = new Form();

            f.ButtonWithLabel("Close Me!", (_args) =>
            {
                f.Close();
            }).Display();
        }
    }
}
