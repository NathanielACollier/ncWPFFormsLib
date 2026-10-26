using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace nac.wpf.forms;

public partial class Form
{

    public class FilePathForFunctions
    {
        public Action<string> updateFileName { get; set; }
    }


        public Form FilePathFor(string fieldName, string fileFilter = null, string initialFileName = null, bool fileMustExist = true,
                Action<string> onFilePathChanged = null,
                FilePathForFunctions functions = null)
        {
            setModelIfNull(fieldName, ""); // init a value

        var filePicker = new nac.wpf.Controls.FilePicker();
        filePicker.FileMustExist = fileMustExist;

        SetupFilePathFileNameBinding(fieldName, functions, filePicker);

        // filename filtering see: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.filedialog.filter
        if (!string.IsNullOrWhiteSpace(fileFilter))
        {
            filePicker.FileNameFilter = fileFilter;
        }
        else
        {
            filePicker.FileNameFilter = "All files (*.*)|*.*";
        }

        Helper_BindField(fieldName, filePicker, nac.wpf.Controls.FilePicker.FilePathProperty, mode: BindingMode.TwoWay);

        if (onFilePathChanged != null)
        {
            filePicker.FilePathChanged += (_sender, _args) =>
            {
                onFilePathChanged(filePicker.FilePath);
            };
        }

        this.Helper_AddRowToHost(filePicker, fieldName);

        return this;
    }

    private void SetupFilePathFileNameBinding(string fieldName, FilePathForFunctions functions, nac.wpf.Controls.FilePicker filePicker)
    {
        string fileNameFieldName = $"{fieldName}_fileName";
        this.Model[fileNameFieldName] = ""; // init
        Helper_BindModelField(fileNameFieldName, filePicker, nac.wpf.Controls.FilePicker.FileNameProperty, BindingMode.TwoWay);

        if (functions != null)
        {
            // provide a function that can update the filename
            functions.updateFileName = (newFileName) =>
            {
                setModelValue(fileNameFieldName, newFileName);
                // set the filename of the current path if we have a current path
                var pathResult = getModelValue(fieldName);
                string currentPath = pathResult.Value as string;
                if (!string.IsNullOrWhiteSpace(currentPath))
                {
                    string currentDirPath = System.IO.Path.GetDirectoryName(currentPath);
                    setModelValue(fieldName, System.IO.Path.Combine(currentDirPath, newFileName));
                }
            };
        }
    }

        public Form DirectoryPathFor(string fieldName)
        {
            setModelIfNull(fieldName, "");

        var dirPicker = new nac.wpf.Controls.DirectoryPicker();
        Helper_BindField(fieldName, dirPicker, nac.wpf.Controls.DirectoryPicker.DirectoryPathProperty, mode: BindingMode.TwoWay);

        this.Helper_AddRowToHost(dirPicker, fieldName);

        return this;
    }


}
