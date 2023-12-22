using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests
{
    [TestClass]
    public class BasicFormTests
    {
        private static nac.Logging.Logger log = new();

        [TestMethodWPF]
        public async Task TestTextBox()
        {
            // idea is to target API like this: http://mscodingblog.blogspot.com/2015/02/introducing-powerforms-for-creating.html
            var result = new Form()
                .TextBoxFor("Field1")
                .Display();

            Assert.IsTrue(!string.IsNullOrEmpty( result.Model["Field1"] as string));
        }


        [TestMethodWPF]
        public void TestTextboxWithTwoQuestions()
        {
            var result = new Form()
                            .TextBoxFor("First Name")
                            .TextBoxFor("Last Name")
                            .Display();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Model["First Name"] as string) &&
                !string.IsNullOrWhiteSpace(result.Model["Last Name"] as string));
        }


        [TestMethodWPF]
        public void TestSimpleAutoSuggest()
        {
            var result = new Form()
                            .AutoSuggestFor<string>("Val1", (textEntered) =>
                            {
                                var source = new[] { "Apple", "Ape", "Alexander", "Andrew", "Animal", "Orange", "Pair", "Water Melon", "Cantilope" };

                                return source.Where(i => i.StartsWith(textEntered, StringComparison.OrdinalIgnoreCase));
                            }).Display();

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Model["Val1"] as string));
        }



        [TestMethodWPF]
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

        [TestMethodWPF]
        public void TestDateForInitialDate()
        {
            var form = new Form()
                        .DateFor("testDate", new DateTime(2000, 01, 01))
                        .Display();
        }



        [TestMethodWPF]
        public void TextBoxWithStartingValue()
        {
            var result = new Form()
                            .TextBoxFor("Email", value: "Apple Orchard")
                            .Display();
        }


        enum Country
        {
            UnitedStates, Argintina, Brazil, Mexico
        }

        [TestMethodWPF]
        public void TestEnumDropDown()
        {


            var result = new Form()
                            .DropDownFor("Country", typeof(Country))
                            .Display();
            Assert.IsTrue(result.Model["Country"].GetType().IsEnum);
            Assert.IsTrue(result.Model["Country"] is Country);
            Assert.IsTrue(result.Model["Country"] as Country? == Country.UnitedStates);
        }




        [TestMethodWPF]
        public void TestTextBoxMultiple()
        {
            var result = new Form()
                            .TextBoxMultipleFor("Groups")
                            .Display();

            var groups = result.Model["Groups"] as ObservableCollection<nac.utilities.BindableDynamicDictionary>;

            Assert.IsTrue(groups.Count > 0);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(groups[0]["Text"] as string));
        }


        [TestMethodWPF]
        public void TestAutoSuggest()
        {
            var result = new Form()
                .AutoSuggestFor<string>("Group Name", (textEntered) =>
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



        [TestMethodWPF]
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

        [TestMethodWPF]
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


        [TestMethodWPF]
        public void TestObjectViewer()
        {
            var stats = new TestObjectViewerStats
            {
                count = 0
            };
            var objectViewerFunctions = new Form.ObjectViewerFunctions<TestObjectViewerStats>();

            var form = new Form()
                        .ObjectViewer(functions: objectViewerFunctions)
                        .ButtonWithLabel("Hit Me!", (_o) =>
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


        [TestMethodWPF]
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
                        .ButtonWithLabel("Hit Me!", (_o) =>
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





        [TestMethodWPF]
        public void TestSimpleButton()
        {
            var form = new Form();
            int clickCount = 0;
            form
                .TextBoxFor("Status")
                .ButtonWithLabel("Click Me!", ( args) =>
                {
                    form.Model["Status"] = $"Clicked {++clickCount} times";
                })
                .Display();
        }


        [TestMethodWPF]
        public void TestSimpleLabel()
        {
            var form = new Form();

            form.TextBoxFor("Original")
                .LabelFor("Original")
                .Display();
        }


        [TestMethodWPF]
        public void TestSelectingFile()
        {
            var form = new Form();

            form.FilePathFor("testPath")
                .Display();

            Assert.IsTrue(System.IO.File.Exists(form.Model["testPath"] as string));
        }

        [TestMethodWPF]
        public void TestSelectingFileThatDoesntExist()
        {
            var form = new Form();

            form.FilePathFor("testPath", fileMustExist: false, initialFileName: "test.xlsx", fileFilter: "Excel (*.xlsx)|*.xlsx",
                onFilePathChanged: (newPath) =>
                {
                    log.Info($"New filepath: {newPath}");
                })
                .Display();

            string path = form.Model["testPath"] as string;
            Assert.IsTrue(!string.Equals(path, "test.txt", StringComparison.OrdinalIgnoreCase)); // path shouldn't be the filename, it should be full path
            Assert.IsTrue(string.Equals(System.IO.Path.GetFileName(path), "test.xlsx", StringComparison.OrdinalIgnoreCase));
        }


        [TestMethodWPF]
        public void TestChangeFilenameBasedOnOtherInputs()
        {
            var form = new Form();

            var testPathFunctions = new Form.FilePathForFunctions();

            form.AutoSuggestFor<string>("fruit", (text) =>
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
            .Display();
        }


        [TestMethodWPF]
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

            var f = new Form();
            f.Model["items"] = items;

                f.Table(itemsSourceModelName: "items")
                .Display();
        }


        [TestMethodWPF]
        public void TableWithButtonCounters()
        {
            var items = new ObservableCollection<nac.utilities.BindableDynamicDictionary>();
            for(int i = 0; i < 10; ++i)
            {
                var item = new nac.utilities.BindableDynamicDictionary();
                item["count"] = 0;
                item["index"] = i;
                items.Add(item);
            }

            var f = new Form();
            f.Model["items"] = items;

            f.Table(itemsSourceModelName: "items",
                columns: new nac.wpf.forms.model.Column[]
                {
                    new nac.wpf.forms.model.Column
                    {
                        Header="",
                        template = row =>
                        {
                            row.ButtonWithLabel("Increment", ( _args) =>
                            {
                                var model = _args as nac.utilities.BindableDynamicDictionary;

                                model["count"] = Convert.ToInt32(model["count"]) + 1;
                                
                            });
                        }
                    },
                    new nac.wpf.forms.model.Column
                    {
                        Header="Index",
                        modelBindingPropertyName="index"
                    },
                    new nac.wpf.forms.model.Column
                    {
                        Header="Count",
                        modelBindingPropertyName="count"
                    }
                })
                .Display();
        }


        [TestMethodWPF]
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



        [TestMethodWPF]
        public void TestTrueFalseButtons()
        {
            var form = new Form();

            form.LabelFor("m1", "Is it ok to go outside?")
                    .ButtonsTrueFalseFor("result")
                    .Display();

            Assert.IsTrue((bool)form.Model["result"]);
        }


        [TestMethodWPF]
        public void DisplayTextTest()
        {
            var form = new Form();

            form.Text("Hello World!")
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
        public void TestHorizontalGroup()
        {
            var form = new Form()
                .HorizontalGroup(f => f.ButtonWithLabel("1", (_o) => { })
                    .ButtonWithLabel("2", (_o) => { })
                    .ButtonWithLabel("3", (_o) => { })
                   ).Display();
        }

        [TestMethodWPF]
        public void TestVerticalSplitGroup()
        {
            var form = new Form()
                .VerticalGroupSplit(f =>

                    f.Text("Entry 1")
                    .Text("Entry 2")
                ).Display();
        }


        [TestMethodWPF]
        public void TestHorizontalGroupSplit()
        {
            var form = new Form()
                .HorizontalGroupSplit(f =>

                f.Text("Hello")
                .Text("Goodbye")
                ).Display();
        }


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
        public void TestHideShowHorizontalGroup()
        {
            new Form()
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


        [TestMethodWPF]
        public void TestHideShowVeritcalGroup()
        {
            new Form()
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


        [TestMethodWPF]
        public void TestExpandingObjectViewer()
        {
            var data = new Dictionary<string, object>();
            var rand = new Random();

            var objFuncs = new Form.ObjectViewerFunctions<Dictionary<string, object>>();

            new Form()
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


        [TestMethodWPF]
        public void TestExpandingObjectViewerInsideTab()
        {
            var data = new Dictionary<string, object>();
            var rand = new Random();

            var objFuncs = new Form.ObjectViewerFunctions<Dictionary<string, object>>();

            new Form()
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
        public void TestMultilineTextbox()
        {
            var f = new Form();

            f.ButtonWithLabel("Click Me!", (_args) =>
            {

            }).TextBoxFor("text",
            multiline: true,
            showFieldNameOnRow: false);

            f.Display();
        }


        [TestMethodWPF]
        public void TestChangeColorOfButton()
        {
            var f = new Form();
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



        [TestMethodWPF]
        public void TestButtonThatClosesWindow()
        {
            var f = new Form();

            f.ButtonWithLabel("Close Me!", (_args) =>
            {
                f.Close();
            }).Display();
        }


        [TestMethodWPF]
        public async Task TestDifferentThreadUIDispatcherShortcut()
        {
            new Form().TextBoxFor("test")
                .Display(onDisplay: (_f) =>
                {
                    Task.Run(() =>
                    {
                        // different thread;
                        // This requires extensions on System.Windows.Threading which is in System.Windows.Presentation
                        _f.BeginInvoke(() =>
                        {
                            _f.Model["test"] = "Hello World!";
                        });
                    });
                });

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
                            Thread.Sleep(200);
                        }
                    });
                }, onClosing: f =>
                {
                    isRequestingTime = false;
                });
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
                    .HorizontalGroup(hori =>
                    {
                        hori.Text("!!!--ERROR--!!!");
                    }, isVisiblePropertyName: "logTabError")
                    .ButtonWithLabel("Test", (_args) =>
                    {
                        log.Info("Header button clicked");
                    });
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
    }
}
