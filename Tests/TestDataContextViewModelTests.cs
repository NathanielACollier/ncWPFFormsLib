using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;
using Tests.Models;

namespace Tests
{
    [TestClass]
    public class TestDataContextViewModel
    {
        [TestMethodWPF]
        public void DataContextReadsFromViewModelProperty()
        {
            var vm = new TestViewModel { Name = "John" };
            var form = new Form
            {
                DataContext = vm
            };

            var result = form.getModelValue("Name");

            Assert.IsFalse(result.invalid);
            Assert.AreEqual("John", result.Value);
        }

        [TestMethodWPF]
        public void DataContextWritesToViewModelPropertyThroughBindingSource()
        {
            var vm = new TestViewModel();
            var form = new Form
            {
                DataContext = vm
            };

            form.setModelValue("Name", "Jane");

            Assert.AreEqual("Jane", vm.Name);
        }

        [TestMethodWPF]
        public void DataContextDoesNotClobberPresetValue()
        {
            var vm = new TestViewModel { Name = "Existing" };
            var form = new Form
            {
                DataContext = vm
            };

            form.setModelIfNull("Name", "Replacement");

            Assert.AreEqual("Existing", vm.Name);
        }

        [TestMethodWPF]
        public void FallsBackToModelWhenNoDataContext()
        {
            var form = new Form();
            form.Model["Name"] = "FromDict";

            var result = form.getModelValue("Name");

            Assert.IsFalse(result.invalid);
            Assert.AreEqual("FromDict", result.Value);
        }

        [TestMethodWPF]
        public void ViewModelPropertyChangedFlowsToFormCallback()
        {
            var vm = new TestViewModel();
            var form = new Form
            {
                DataContext = vm
            };

            string lastValue = null;
            form.notifyOnModelChange("Name", (_bindingSource, value) =>
            {
                lastValue = value as string;
            });

            vm.Name = "AfterChange";

            Assert.AreEqual("AfterChange", lastValue);
        }
    }
}
