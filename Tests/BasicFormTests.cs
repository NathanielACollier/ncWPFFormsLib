using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;

namespace Tests
{
    [TestClass]
    public class BasicFormTests
    {
        [TestMethod]
        public async Task TestTextBox()
        {
            await lib.utility.ShowForm(f =>
            {
                // idea is to target API like this: http://mscodingblog.blogspot.com/2015/02/introducing-powerforms-for-creating.html
                f.TextBoxFor("Field1");
            }, model =>
            {
                Assert.IsTrue(!string.IsNullOrEmpty(model["Field1"] as string));
            });

        }


        [TestMethod]
        public void TestTextboxWithTwoQuestions()
        {
            var result = new Form()
                            .TextBoxFor("First Name")
                            .TextBoxFor("Last Name")
                            .Display();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Model["First Name"] as string) &&
                !string.IsNullOrWhiteSpace(result.Model["Last Name"] as string));
        }


        [TestMethod]
        public void TestSimpleAutoSuggest()
        {
            var result = new Form()
                            .AutoSuggestFor("Val1", (textEntered) =>
                            {
                                var source = new[] { "Apple", "Ape", "Alexander", "Andrew", "Animal", "Orange", "Pair", "Water Melon", "Cantilope" };

                                return source.Where(i => i.StartsWith(textEntered, StringComparison.OrdinalIgnoreCase));
                            }).Display();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Model["Val1"] as string));
        }



        [TestMethod]
        public void SimpleEmployeeInfoForm()
        {
            var result = new Form()
                        .TextBoxFor("First Name")
                        .TextBoxFor("Last Name")
                        .DateFor("Birthday")
                        .DropDownFor("Country", new[] { "United States", "France", "Germany", "England", "Mexico", "Brazil" })
                        .DropDownFor("State", new[] { "Arkansas", "Texas", "Mississippi", "Misouri", "Alabama", "oklahoma" })
                        .Display(width: 400, height: 300);

            var birthDay = result.Model["Birthday"] as DateTime?;

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Model["First Name"] as string)
                        && !string.IsNullOrWhiteSpace(result.Model["Last Name"] as string)
                        && birthDay.HasValue
                        && !string.IsNullOrWhiteSpace(result.Model["Country"] as string)
                        && !string.IsNullOrWhiteSpace(result.Model["State"] as string)
                        );
        }

        [TestMethod]
        public void TestDateForInitialDate()
        {
            var form = new Form()
                        .DateFor("testDate", new DateTime(2000, 01, 01))
                        .Display();
        }



        [TestMethod]
        public void TextBoxWithStartingValue()
        {
            var result = new Form()
                            .TextBoxFor("Email", value: "nathaniel.collier@aecc.com")
                            .Display();
        }


        enum Country
        {
            UnitedStates, Argintina, Brazil, Mexico
        }

        [TestMethod]
        public void TestEnumDropDown()
        {


            var result = new Form()
                            .DropDownFor("Country", typeof(Country))
                            .Display();
            Assert.IsTrue(result.Model["Country"].GetType().IsEnum);
            Assert.IsTrue(result.Model["Country"] is Country);
            Assert.IsTrue(result.Model["Country"] as Country? == Country.UnitedStates);
        }




        [TestMethod]
        public void TestTextBoxMultiple()
        {
            var result = new Form()
                            .TextBoxMultipleFor("Groups")
                            .Display();

            var groups = result.Model["Groups"] as ObservableCollection<nac.utilities.BindableDynamicDictionary>;

            Assert.IsTrue(groups.Count > 0);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(groups[0]["Text"] as string));
        }


        [TestMethod]
        public void TestAutoSuggest()
        {
            var result = new Form()
                .AutoSuggestFor("Group Name", (textEntered) =>
                {
                    List<string> groups = new List<string>();

                    groups.Add("Rolling Stones");
                    groups.Add("Beatles");
                    groups.Add("Metalica");
                    groups.Add("Black Eyed Peas");

                    return groups;
                }).Display();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Model["Group Name"] as string));
        }



        [TestMethod]
        public void TestAutoSuggestMultiple()
        {
            var result = new Form()
                .AutoSuggestMultipleFor("Groups", (textEntered) =>
                {
                    List<string> groups = new List<string>();

                    groups.Add("Blue");
                    groups.Add("Green");
                    groups.Add("Purple");
                    groups.Add("Orange");

                    return groups;
                }).Display();

            var groupList = result.Model["Groups"] as ObservableCollection<nac.utilities.BindableDynamicDictionary>;

            Assert.IsTrue(groupList.Count > 0);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(groupList[0]["item_Text"] as string));
        }

        [TestMethod]
        public void TestSimpleTree()
        {
            var result = new Form()
                                .TreeFor("Test", (parent) =>
                                {
                                    List<nac.utilities.BindableDynamicDictionary> children = new List<nac.utilities.BindableDynamicDictionary>();

                                    if (parent == null)
                                    {
                                        children.Add(new nac.utilities.BindableDynamicDictionary(
                                            new Dictionary<string, object>
                                        {
                                            {"Name", "Parent" }
                                        }));
                                    }
                                    else
                                    {
                                        children.Add(new nac.utilities.BindableDynamicDictionary(
                                        new Dictionary<string, object>
                                        {
                                            {"Name", "Child" }
                                        }));
                                    }

                                    return children;
                                })
                                .Display();
        }




        public class TestObjectViewerStats
        {
            public int count { get; set; }
        }


        [TestMethod]
        public void TestObjectViewer()
        {
            var stats = new TestObjectViewerStats
            {
                count = 0
            };
            var objectViewerFunctions = new Form.ObjectViewerFunctions<TestObjectViewerStats>();

            var form = new Form()
                        .ObjectViewer(functions: objectViewerFunctions)
                        .ButtonWithLabel("Hit Me!", (_s, _o) =>
                        {
                            stats.count++;
                            // update stats
                            objectViewerFunctions.updateValue(stats);
                        })
                        .Display();
        }


        public class TestObjectViewerAgainstDictionaryTestType1
        {
            public int count { get; set; }
            public bool isDone { get; set; }
            public bool isQuery { get; set; }
        }


        [TestMethod]
        public void TestObjectViewerAgainstDictionary()
        {
            var stats = new Dictionary<string, object>
            {
                {"count", 0 },
                {"biscuit", new { Prop1 = 5, Prop2=7 } },
                {"apple", new TestObjectViewerAgainstDictionaryTestType1
                {
                    count = 3, isDone = false, isQuery = true
                } }
            };

            var objectViewerFunctions = new Form.ObjectViewerFunctions<Dictionary<string, object>>();

            var form = new Form()
                        .ObjectViewer(functions: objectViewerFunctions)
                        .ButtonWithLabel("Hit Me!", (_s, _o) =>
                        {
                            stats["count"] = (int)stats["count"] + 1;
                            stats["biscuit"] = new
                            {
                                Prop1 = new Random().Next(0, 10000),
                                Prop2 = new Random().Next(0, 10000)
                            };
                            stats["apple"] = new TestObjectViewerAgainstDictionaryTestType1
                            {
                                count = new Random().Next(0, 100),
                                isDone = new Random().Next(-100, 100) > 0 ? true : false,
                                isQuery = new Random().Next(-1, 1) > 0 ? true : false
                            };
                            // update stats
                            objectViewerFunctions.updateValue(stats);
                        })
                        .Display();
        }





        [TestMethod]
        public void TestSimpleButton()
        {
            var form = new Form();
            int clickCount = 0;
            form
                .TextBoxFor("Status")
                .ButtonWithLabel("Click Me!", (sender, args) =>
                {
                    form.Model["Status"] = $"Clicked {++clickCount} times";
                })
                .Display();
        }


        [TestMethod]
        public void TestSimpleLabel()
        {
            var form = new Form();

            form.TextBoxFor("Original")
                .LabelFor("Original")
                .Display();
        }


        [TestMethod]
        public void TestSelectingFile()
        {
            var form = new Form();

            form.FilePathFor("testPath")
                .Display();

            Assert.IsTrue(System.IO.File.Exists(form.Model["testPath"] as string));
        }

        [TestMethod]
        public void TestSelectingFileThatDoesntExist()
        {
            var form = new Form();

            form.FilePathFor("testPath", fileMustExist: false, initialFileName: "test.xlsx", fileFilter: "Excel (*.xlsx)|*.xlsx",
                onFilePathChanged: (newPath) =>
                {
                    log.Info($"New filepath: {newPath}");
                })
                .LogViewer()
                .Display();

            string path = form.Model["testPath"] as string;
            Assert.IsTrue(!string.Equals(path, "test.txt", StringComparison.OrdinalIgnoreCase)); // path shouldn't be the filename, it should be full path
            Assert.IsTrue(string.Equals(System.IO.Path.GetFileName(path), "test.xlsx", StringComparison.OrdinalIgnoreCase));
        }


        [TestMethod]
        public void TestChangeFilenameBasedOnOtherInputs()
        {
            var form = new Form();

            var testPathFunctions = new Form.FilePathForFunctions();

            form.AutoSuggestFor("fruit", (text) =>
            {
                return new[] { "orange", "apple", "watermellon" }
                        .Where(i => i.Contains(text));
            }, onSelected: (item) =>
            {
                testPathFunctions.updateFileName($"{item}.xlsx");
            })
            .FilePathFor("testPath", fileMustExist: false, functions: testPathFunctions,
            onFilePathChanged: (newPath) =>
            {
                log.Info($"New FilePath: {newPath}");
            })
            .LogViewer()
            .Display();
        }


        [TestMethod]
        public void TestTable()
        {
            var items = new[]
            {
                new Dictionary<string,object>{ { "prop1", 0}, { "prop2", "Hello World!"} },
                new Dictionary<string, object>{ { "prop1", 1 }, { "prop2", "Orange"} }
            }.Select(dict => new
            {
                prop1 = (int)dict["prop1"],
                prop2 = dict["prop2"] as string
            });

            new Form()
                .Table(items)
                .Display();
        }


        [TestMethod]
        public void TestSelectingFolder()
        {
            var form = new Form();

            form.DirectoryPathFor("path1")
                .HorizontalGroup(f =>
                    f.Text("Path Selected is: ")
                     .TextFor("path1")
                )
                .Display();

            Assert.IsTrue(System.IO.Directory.Exists(form.Model["path1"] as string));
        }



        [TestMethod]
        public void TestTrueFalseButtons()
        {
            var form = new Form();

            form.LabelFor("m1", "Is it ok to go outside?")
                    .ButtonsTrueFalseFor("result")
                    .Display();

            Assert.IsTrue((bool)form.Model["result"]);
        }


        [TestMethod]
        public void DisplayTextTest()
        {
            var form = new Form();

            form.Text("Hello World!")
                .Display();
        }



        [TestMethod]
        public void TestBusy()
        {
            var form = new Form();
            var busyActions = new nac.wpf.forms.Form.BusyFunctions();

            form.Busy("Loading..", false, functions: busyActions)
                .TextBoxFor("output")
                .ButtonWithLabel("Start", (_s,_o) =>
                {
                    form.Model["output"] = $"Busy is: {busyActions.isBusy()}\nStarting...";
                    busyActions.start();
                })
                .ButtonWithLabel("Stop", (_s, _o) =>
                {
                    form.Model["output"] = $"Busy is: {busyActions.isBusy()}\nStopping...";
                    busyActions.stop();
                })
                .Display();

        }


        [TestMethod]
        public void TestMultipleTabs()
        {
            var form = new NacFormsWPFLib.Form()
                .AddTab((newF) => newF.TextBoxFor("tb1", "Hello World!"))
                .AddTab((newF) =>
                    newF.DateFor("Christmass", new DateTime(DateTime.Now.Year, 12, 31))
                    , tabName: "Christmas")
                .Display();
        }


        [TestMethod]
        public void AddTabLater()
        {
            var form = new NacFormsWPFLib.Form()
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


        [TestMethod]
        public void TestHorizontalGroup()
        {
            var form = new NacFormsWPFLib.Form()
                .HorizontalGroup(f => f.ButtonWithLabel("1", (_o) => { })
                    .ButtonWithLabel("2", (_o) => { })
                    .ButtonWithLabel("3", (_o) => { })
                   ).Display();
        }

        [TestMethod]
        public void TestVerticalSplitGroup()
        {
            var form = new NacFormsWPFLib.Form()
                .VerticalGroupSplit(f =>

                    f.Text("Entry 1")
                    .Text("Entry 2")
                ).Display();
        }


        [TestMethod]
        public void TestHorizontalGroupSplit()
        {
            var form = new NacFormsWPFLib.Form()
                .HorizontalGroupSplit(f =>

                f.Text("Hello")
                .Text("Goodbye")
                ).Display();
        }


        [TestMethod]
        public void TestStartUI()
        {
            NacFormsWPFLib.Form.StartUI(f =>

                f.Text("Hello World!")
            );
        }


        [TestMethod]
        public void TestLogReadyInLogViewer()
        {
            new NacFormsWPFLib.Form()
                .LogViewer(onLogReady: () =>
                {
                    log.Info("Hello World! -- Successfull test of log is ready!");
                })
                .Display();
        }


        [TestMethod]
        public void TestHideShowHorizontalGroup()
        {
            new NacFormsWPFLib.Form()
                .HorizontalGroup(f =>
                    f.ButtonWithLabel("Hide", (_o) =>
                    {
                        f.Model["row2IsVis"] = false;
                    })
                    .ButtonWithLabel("Show", (_o) =>
                    {
                        f.Model["row2IsVis"] = true;
                    })
                )
                .HorizontalGroup(f =>
                    f.Text("Hello World!")
                , isVisiblePropertyName: "row2IsVis")
                .Display();
        }


        [TestMethod]
        public void TestHideShowVeritcalGroup()
        {
            new NacFormsWPFLib.Form()
                .HorizontalGroup(f =>
                    f.ButtonWithLabel("Hide", (_o) =>
                    {
                        f.Model["row2IsVis"] = false;
                    })
                    .ButtonWithLabel("Show", (_o) =>
                    {
                        f.Model["row2IsVis"] = true;
                    })
                )
                .VerticalGroup(f =>
                    f.Text("Hello World!")
                    .TextBoxFor("f1", "Merry Christmas")
                    .LabelFor("f1")
                    .DateFor("christmass", new DateTime(DateTime.Now.Year, 12, 31))
                , isVisiblePropertyName: "row2IsVis")
                .Text("I'm below vertical group")
                .Display();
        }


        [TestMethod]
        public void TestExpandingObjectViewer()
        {
            var data = new Dictionary<string, object>();
            var rand = new Random();

            var objFuncs = new NacFormsWPFLib.Form.ObjectViewerFunctions<Dictionary<string, object>>();

            new NacFormsWPFLib.Form()
                .ButtonWithLabel("Add Entry", (_o) =>
                {
                    data[$"Item_{rand.Next(0, 10000)}"] = new
                    {
                        Prop1 = rand.Next(0, 10000),
                        Prop2 = rand.Next(0, 10000)
                    };
                    objFuncs.updateValue(data);
                })
                .ObjectViewer(functions: objFuncs)
                .Display();
        }


        [TestMethod]
        public void TestExpandingObjectViewerInsideTab()
        {
            var data = new Dictionary<string, object>();
            var rand = new Random();

            var objFuncs = new NacFormsWPFLib.Form.ObjectViewerFunctions<Dictionary<string, object>>();

            new NacFormsWPFLib.Form()
                .AddTab(f =>
                    f.ButtonWithLabel("Add Entry", (_o) =>
                    {
                        data[$"Item_{rand.Next(0, 10000)}"] = new
                        {
                            Prop1 = rand.Next(0, 10000),
                            Prop2 = rand.Next(0, 10000)
                        };
                        objFuncs.updateValue(data);
                    })
                    .ObjectViewer(functions: objFuncs)
                    , tabName: "Main"
                )
                .AddTab(f =>
                    f.LogViewer()
                    , tabName: "Log"
                )
                .Display();
        }


        [TestMethod]
        public void TestLogViewer()
        {
            var form = new NacFormsWPFLib.Form();
            form.TextBoxFor("message", "Hello World!")
                .ButtonWithLabel("Log Message", (_o) =>
                {
                    log.Info(form.Model["message"] as string);
                })
                .LogViewer()
                .Display();
        }



        [TestMethod]
        public void TestBasicList()
        {
            var form = new NacFormsWPFLib.Form();
            var items = new ObservableCollection<NacWPFUtilities.BindableDynamicDictionary>();
            form.Model["list1"] = items;

            var newItemFactory = new Func<NacWPFUtilities.BindableDynamicDictionary>(() =>
            {
                var newItem = new NacWPFUtilities.BindableDynamicDictionary();
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
                    form.Model["checkedCount"] = items.Count(i => (bool)i["isChecked"] == true);
                })
                .TextFor("currentDate")
                .ButtonWithLabel("Click Me!", (_o) =>
                {
                    var model = _o as NacWPFUtilities.BindableDynamicDictionary;

                    model["currentDate"] = DateTime.Now.ToLongTimeString();
                })
            );

            string xaml = form.Xaml;

            form
            .Display();
        }


        [TestMethod]
        public void TestTextBoxKeyUp()
        {
            var f = new NacFormsWPFLib.Form();

            f.TextBoxFor("t1", onKeyUp: (_args) =>
            {
                f.Model["msg"] = $"You pressed: {_args.Key}";
            })
            .TextFor("msg")
            .Display();
        }


        [TestMethod]
        public void TestListInVerticalGroup()
        {
            var f = new NacFormsWPFLib.Form();

            var items = new ObservableCollection<NacWPFUtilities.BindableDynamicDictionary>();
            f.Model["list1"] = items;
            var rand = new Random();
            for (int i = 0; i < 1000; ++i)
            {
                var item = new NacWPFUtilities.BindableDynamicDictionary();
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


        [TestMethod]
        public void TestListInSplitVerticalGroup()
        {
            var f = new NacFormsWPFLib.Form();

            var items = new ObservableCollection<NacWPFUtilities.BindableDynamicDictionary>();
            f.Model["list1"] = items;
            var rand = new Random();
            for (int i = 0; i < 1000; ++i)
            {
                var item = new NacWPFUtilities.BindableDynamicDictionary();
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


        [TestMethod]
        public void TestAddALine()
        {
            new NacFormsWPFLib.Form()
                .Text("Above Line")
                .Line()
                .Text("Below Line")
                .Display();
        }


        [TestMethod]
        public void TestMultilineTextbox()
        {
            var f = new NacFormsWPFLib.Form();

            f.ButtonWithLabel("Click Me!", (_args) =>
            {

            }).TextBoxFor("text",
            multiline: true,
            showFieldNameOnRow: false);

            f.Display();
        }


        [TestMethod]
        public void TestChangeColorOfButton()
        {
            var f = new NacFormsWPFLib.Form();
            var defaultButtonBackground = new System.Windows.Controls.Button().Background;
            System.Windows.Controls.Button btnRef = null;

            f.ButtonWithLabel("Click Me!", (_args) =>
            {
                if (btnRef.Background == defaultButtonBackground)
                {
                    btnRef.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
                }
                else
                {
                    btnRef.Background = defaultButtonBackground;
                }
            }, onControlReady: btn => btnRef = btn)
            .Display();
        }



        [TestMethod]
        public void TestButtonThatClosesWindow()
        {
            var f = new NacFormsWPFLib.Form();

            f.ButtonWithLabel("Close Me!", (_args) =>
            {
                f.Close();
            }).Display();
        }


        [TestMethod]
        public void TestDifferentThreadUIDispatcherShortcut()
        {
            // test access wpf outside the UI Thread
            NacFormsWPFLib.Form.StartUI(f =>
            {
                f.TextBoxFor("test");

                Task.Run(() =>
                {
                    // different thread;
                    // This requires extensions on System.Windows.Threading which is in System.Windows.Presentation
                    f.BeginInvoke(() =>
                    {
                        f.Model["test"] = "Hello World!";
                    });
                });
            });
        }


        [TestMethod]
        public void TestFormClosingEvent()
        {
            NacFormsWPFLib.Form.StartUI(f =>
            {
                f.Text("Close me to get a debug message");
            }, onClosing: (f) =>
            {
                System.Diagnostics.Debug.WriteLine("***************************************");
                System.Diagnostics.Debug.WriteLine("--           Form Closed             --");
                System.Diagnostics.Debug.WriteLine("***************************************");
            });
        }


        [TestMethod]
        public void TestUpdateUIOnDifferentThread()
        {
            bool isRequestingTime = true;
            NacFormsWPFLib.Form.StartUI(f =>
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
                        Thread.Sleep(200);
                    }
                });

                f.TextBoxFor("time");


            }, onClosing: (f) => isRequestingTime = false);
        }


        [TestMethod]
        public void TestVisualIndicatorThatErroHasOccuredOnTab()
        {

            NacFormsWPFLib.Form.StartUI(f =>
            {
                f.Model["logTabError"] = false;
                Log4NetHelpers.CodeConfiguredUtilities.AddNotifyAppender((_s, _args) => {
                    if (_args.SourceEvent.Level > log4net.Core.Level.Info)
                    {
                        f.Model["logTabError"] = true;
                    }
                });


                f.AddTab(t =>
                {
                    t.Text("Press button below to cause a test log message to be written.")
                    .HorizontalGroup(h =>
                    {
                        h.ButtonWithLabel("Info", (_args) =>
                        {
                            log.Info("A normal log message");
                        }).ButtonWithLabel("Warn", (_args) =>
                        {
                            log.Warn("A messing that is a warning");
                        }).ButtonWithLabel("Error", (_args) =>
                        {
                            log.Error("An error message");
                        });
                    });
                }, tabName: "Main")
                .AddTab(t =>
                {
                    t.LogViewer(onLogReady: () =>
                    {
                        log.Info("Logging ready...");
                    });
                }, tabName: "Log", populateHeaderForm: tabHeader =>
                {
                    // populate the header for the Log Tab
                    tabHeader.Text("Log")
                    .HorizontalGroup(hori => {
                        hori.Text("!!!--ERROR--!!!");
                    }, isVisiblePropertyName: "logTabError")
                    .ButtonWithLabel("Test", (_args) =>
                    {
                        log.Info("Header button clicked");
                    });
                }, OnFocus: () => f.Model["logTabError"] = false);

            });
        }



        [TestMethod]
        public void TestDisplayImage()
        {
            string imagePath = @"C:\Users\ncollier\Desktop\temp\2020-02-27\output.png";

            var img = System.ConvertNCWPF.ToWPFBitmapImage(System.IO.File.ReadAllBytes(imagePath));

            NacFormsWPFLib.Form.StartUI(f =>
            {
                f.Model["imgToDisplay"] = img;

                f.Image("imgToDisplay");
            });
        }
    }
}
