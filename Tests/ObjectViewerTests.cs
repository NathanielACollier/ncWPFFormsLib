using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests;

[TestClass]
public class ObjectViewerTests
{
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
            { "count", 0 },
            { "biscuit", new { Prop1 = 5, Prop2 = 7 } },
            {
                "apple", new TestObjectViewerAgainstDictionaryTestType1
                {
                    count = 3, isDone = false, isQuery = true
                }
            }
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
    
    
    
    
    
}