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


    }
}
