using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests
{
    [TestClass]
    public class TestLayoutControls
    {
        [TestMethodWPF]
        public void TestHorizontalGroup()
        {
            var form = new Form()
                .HorizontalGroup(f => f.ButtonWithLabel("1", (_o) => { })
                    .ButtonWithLabel("2", (_o) => { })
                    .ButtonWithLabel("3", (_o) => { }).Display());
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
    }
}
