using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.wpf.forms;
using Tests.lib;

namespace Tests
{
    [TestClass]
    public class FileSystemTests
    {
        private static nac.Logging.Logger log = new();

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
                testPathFunctions.updateFileName($"test_{item}.xlsx");
            })
            .FilePathFor("testPath", fileMustExist: false, functions: testPathFunctions,
            onFilePathChanged: (newPath) =>
            {
                log.Info($"New FilePath: {newPath}");
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


    }
}
