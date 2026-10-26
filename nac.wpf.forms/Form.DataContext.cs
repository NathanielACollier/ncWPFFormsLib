using System;
using System.ComponentModel;
using nac.wpf.forms.model;

namespace nac.wpf.forms
{
    public partial class Form
    {
        /*
         When a DataContext (any INotifyPropertyChanged object, e.g. a ViewModelBase) is
         set on the model, field bindings target that object's properties instead of the
         BindableDynamicDictionary.  WPF's binding engine already honors INotifyPropertyChanged,
         so this is a simple source redirect rather than the reactive wiring Avalonia needs.
         */
        private INotifyPropertyChanged getBindingSource()
        {
            if (this.Model.HasKey(model.SpecialModelKeys.DataContext))
            {
                var dataContext = this.Model[model.SpecialModelKeys.DataContext];

                if (dataContext is INotifyPropertyChanged inpc)
                {
                    return inpc;
                }

                throw new Exception(
                    $"Special DataContext used in model, but DataContext is not INotifyPropertyChanged. Type given is {dataContext?.GetType().Name ?? "null"}.  There is no way to read properties without INotifyPropertyChanged");
            }

            return this.Model;
        }


        public void notifyOnModelChange(string modelFieldName, CodeToRunOnModelChange codeToRunOnModelChange)
        {
            var bindingSource = getBindingSource();

            var valueResult = getDataContextValue(parentContext: null,
                datacontext: bindingSource,
                modelFieldName: modelFieldName);

            if (valueResult.invalid)
            {
                log.Warn($"Cannot watch model field [{modelFieldName}] on binding source");
                return;
            }

            var initialResult = valueResult;
            initialResult.DataContext.PropertyChanged += (_s, args) =>
            {
                if (!string.Equals(initialResult.CurrentFieldName, args.PropertyName))
                {
                    return;
                }

                var changedValue = getDataContextValue(parentContext: null,
                    datacontext: initialResult.DataContext,
                    modelFieldName: args.PropertyName);

                if (changedValue.invalid)
                {
                    log.Warn($"PropertyChanged: Field: [{args.PropertyName}] is not valid for datacontext");
                    return;
                }

                codeToRunOnModelChange(changedValue, changedValue.Value);
            };
        }

        public delegate void CodeToRunOnModelChange(model.DataContextValueResult bindingSource, object value);


        private model.DataContextValueResult getDataContextValue(model.DataContextValueResult parentContext,
            INotifyPropertyChanged datacontext, string modelFieldName)
        {
            DataContextValueResult result = new()
            {
                ParentContext = parentContext,
                DataContext = datacontext
            };

            var fieldPath = new model.ModelFieldNamePathInfo(modelFieldName);
            result.CurrentFieldName = fieldPath.Current;
            result.RelativeBindingPath = fieldPath.ChildPath;

            if (datacontext == null)
            {
                result.invalid = true;
                log.Warn($"Error accessing [field: {modelFieldName}]. DataContext is null, or is not INotifyPropertyChanged");
                return result;
            }

            if (datacontext is nac.utilities.BindableDynamicDictionary dynDict)
            {
                if (!dynDict.HasKey(fieldPath.Current))
                {
                    result.invalid = true;
                    log.Warn($"Field [{fieldPath.Current}] does not exist on DataContext object");
                    return result;
                }

                result.Value = dynDict[fieldPath.Current];
                if (fieldPath.ChildPath.Length > 0)
                {
                    return getDataContextValue(parentContext: result,
                        datacontext: result.Value as INotifyPropertyChanged,
                        modelFieldName: fieldPath.ChildPath);
                }

                return result;
            }

            Type dcType = datacontext.GetType();
            var prop = dcType.GetProperty(fieldPath.Current);
            if (prop == null)
            {
                result.invalid = true;
                log.Warn($"Field [{fieldPath.Current}] does not exist on DataContext object of [type: {dcType.Name}]");
                return result;
            }

            result.Value = prop.GetValue(datacontext);

            if (fieldPath.ChildPath.Length > 0)
            {
                if (result.Value == null)
                {
                    result.invalid = true;
                    log.Warn(
                        $"DataContext warning.  Child Path found [FullPath: {modelFieldName}] but current [Path: {fieldPath.Current}] is null.  Would not be able to find [child path: {fieldPath.ChildPath}] while parent is null.");
                    return result;
                }

                return getDataContextValue(parentContext: result,
                    datacontext: result.Value as INotifyPropertyChanged,
                    modelFieldName: fieldPath.ChildPath);
            }

            return result;
        }


        private void setDataContextValue(INotifyPropertyChanged dataContext, string modelFieldName, object val)
        {
            var fieldPath = new model.ModelFieldNamePathInfo(modelFieldName);

            if (fieldPath.ChildPath.Length > 0)
            {
                if (dataContext is nac.utilities.BindableDynamicDictionary dynDict)
                {
                    if (!dynDict.HasKey(fieldPath.Current))
                    {
                        throw new Exception($"Field [{fieldPath.Current}] does not exist on DataContext object");
                    }

                    var currentPathVal = dynDict[fieldPath.Current] as INotifyPropertyChanged;
                    setDataContextValue(currentPathVal, fieldPath.ChildPath, val);
                }
                else
                {
                    Type dcType = dataContext.GetType();
                    var prop = dcType.GetProperty(fieldPath.Current);
                    if (prop == null)
                    {
                        throw new Exception(
                            $"Field [{fieldPath.Current}] does not exist on DataContext object of [type: {dcType.Name}]");
                    }

                    var currentPathVal = prop.GetValue(dataContext) as INotifyPropertyChanged;
                    setDataContextValue(currentPathVal, fieldPath.ChildPath, val);
                }
            }
            else
            {
                if (dataContext is nac.utilities.BindableDynamicDictionary dynDict2)
                {
                    dynDict2[fieldPath.Current] = val;
                }
                else
                {
                    Type dcType = dataContext.GetType();
                    var prop = dcType.GetProperty(fieldPath.Current);
                    if (prop == null)
                    {
                        throw new Exception(
                            $"Field [{fieldPath.Current}] does not exist on DataContext object of [type: {dcType.Name}]");
                    }

                    prop.SetValue(dataContext, val);
                }
            }
        }


        /*
         Public access used by the Table/List controls to reach an item source
         even when it lives on the DataContext instead of the dict.
         */
        public model.DataContextValueResult getModelValue(string modelFieldName)
        {
            var datacontext = getBindingSource();

            return getDataContextValue(parentContext: null,
                datacontext: datacontext,
                modelFieldName: modelFieldName);
        }


        public void setModelValue(string modelFieldName, object val)
        {
            var datacontext = getBindingSource();

            setDataContextValue(datacontext, modelFieldName, val);
        }


        public void setModelIfNull(string modelFieldName, object val)
        {
            var result = getModelValue(modelFieldName);

            if (result.invalid)
            {
                log.Warn($"Trying to set model field: [{modelFieldName}] but it is invalid for DataContext");
                return;
            }

            if (result.Value is null)
            {
                setDataContextValue(result.DataContext, modelFieldName, val);
            }
        }
    }
}
