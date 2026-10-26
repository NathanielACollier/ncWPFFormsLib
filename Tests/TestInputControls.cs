using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests
{
    [TestClass]
    public class InputControlsTests
    {
        private static nac.Logging.Logger log = new();

        [TestMethodWPF]
        public async Task TestTextBox()
        {
            var result = new Form()
                .TextBoxFor("Field1")
                .Display();

            Assert.IsTrue(!string.IsNullOrEmpty(result.Model["Field1"] as string));
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
        public void SimpleEmployeeInfoForm()
        {
            var result = new Form()
                        .TextBoxFor("First Name")
                        .TextBoxFor("Last Name")
                        .DateFor("Birthday")
                        .DropDownFor("Country", new[] { "United States", "France" })
                        .DropDownFor("State", new[] { "Arkansas", "Texas" })
                        .Display(width: 400, height: 300);

            var birthDay = result.Model["Birthday"] as DateTime?;

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result.Model["First Name"] as string)
                        && !string.IsNullOrWhiteSpace(result.Model["Last Name"] as string)
                        && birthDay.HasValue
                        && !string.IsNullOrWhiteSpace(result.Model["Country"] as string)
                        && !string.IsNullOrWhiteSpace(result.Model["State"] as string));
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
        }

        [TestMethodWPF]
        public void TestTextBoxMultiple()
        {
            var result = new Form()
                        .TextBoxMultipleFor("Groups")
                        .Display();

            var groups = result.Model["Groups"] as System.Collections.ObjectModel.ObservableCollection<nac.utilities.BindableDynamicDictionary>;

            Assert.IsTrue(groups.Count > 0);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(groups[0]["Text"] as string));
        }
    }
}
