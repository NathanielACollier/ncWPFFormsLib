using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests;

[TestClass]
public class AutosuggestTests
{
    
    
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
    
    
    
    
    
}