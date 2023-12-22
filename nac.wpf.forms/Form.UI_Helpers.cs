using System.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Documents;

namespace nac.wpf.forms
{
    public partial class Form
    {
        private static nac.Logging.Logger log = new();


        public string Xaml
        {
            get
            {
                // this is for debugging
                return nac.wpf.utilities.SaveXaml.SaveXamlUtility.SaveXaml(Host);
            }
        }


        public void Helper_setupControlCommand(FrameworkElement ctrl, DependencyProperty commandProperty, Action<object> cmdAction,
            DependencyProperty commandParameterProperty = null)
        {
            string commandPath = $"Command_{Guid.NewGuid().ToString("N")}";
            this.Model[commandPath] = new nac.wpf.utilities.RelayCommand(cmdAction);

            // command binding
            var cmdBinding = new Binding();
            cmdBinding.Path = new PropertyPath($"DataContext.{commandPath}");
            // there is only 1 host grid in the whole application, so we make sure we are using it as the Source
            //  This keeps commands working even if they are part of a DataTemplate
            //   - See the List function, it needs a xaml serialization compatible way of binding
            //       RelativeSource doesn't normally serialize but NacWPFUtilities has a custom addition to the serialization
            cmdBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(lib.NacFormHostGrid), 1);

            BindingOperations.SetBinding(ctrl, commandProperty, cmdBinding);

            // if command parameterProperty gets passed the bind it to the data context which is an empty binding
            if (commandParameterProperty != null)
            {
                var paramBinding = new Binding();
                BindingOperations.SetBinding(ctrl, commandParameterProperty, paramBinding);
            }
        }


        public DataTemplate Helper_GetDatatemplate(string fromstring)
        {
            ParserContext context = new ParserContext();
            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            return (DataTemplate)XamlReader.Parse(fromstring, context);
        }

        private class DataTemplateResult
        {
            public DataTemplate Template;
            public nac.utilities.BindableDynamicDictionary Model;
        }

        private DataTemplateResult Helper_GetDataTemplateFromFormBuilder(Action<Form> formBuilderAction)
        {
            var f = new Form();
            formBuilderAction(f);

            var formXAML = nac.wpf.utilities.SaveXaml.SaveXamlUtility.SaveXaml(f.Host);

            string dataTemplateXaml = $@"
                <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                {formXAML}
                </DataTemplate>
            ";

            var template = (DataTemplate)XamlReader.Parse(dataTemplateXaml);

            return new DataTemplateResult
            {
                Template=template,
                Model=f.Model
            };
        }



        public class AddRowToHostFunctions
        {
            public Action show;
            public Action hide;
        }

        private void SetupAddRowToHostFunctionsIfAny(DockPanel row, AddRowToHostFunctions functions = null)
        {
            if (functions != null)
            {
                functions.show = () =>
                {
                    row.Visibility = Visibility.Visible;
                };
                functions.hide = () =>
                {
                    row.Visibility = Visibility.Collapsed;
                };
            }
        }

        private void handleAddingControlToIndexIfRequested(FrameworkElement ctrl, string ctrlIndex = null)
        {
            if (!string.IsNullOrWhiteSpace(ctrlIndex))
            {
                if (this.controlsIndex.ContainsKey(ctrlIndex))
                {
                    throw new Exception($"Control index [{ctrlIndex}] already exists");
                }

                this.controlsIndex[ctrlIndex] = ctrl;
            }
        }

        private void AddHostChild(FrameworkElement ctrl, bool rowAutoHeight)
        {
            var row = new RowDefinition();
            if (rowAutoHeight)
            {
                row.Height = GridLength.Auto;
            }
            log.Info($"New Host Child of Type: [{ctrl.GetType().Name}].  It's Row height is: [{row.Height}]");

            this.Host.RowDefinitions.Add(row);
            int colIndex = 0;
            int rowIndex = this.Host.RowDefinitions.Count - 1;
            Grid.SetRow(ctrl, rowIndex);
            Grid.SetColumn(ctrl, colIndex);
            this.Host.Children.Add(ctrl);
        }


        public void Helper_AddRowToHost(FrameworkElement ctrl, string rowLabel,
            AddRowToHostFunctions functions = null, string ctrlIndex = null,
            bool rowAutoHeight = true)
        {
            DockPanel row = new DockPanel();
            Label label = new Label();

            handleAddingControlToIndexIfRequested(ctrl, ctrlIndex);

            label.Content = rowLabel;

            row.Children.Add(label);
            row.Children.Add(ctrl);

            DockPanel.SetDock(label, Dock.Left);
            DockPanel.SetDock(ctrl, Dock.Right);

            AddHostChild(row, rowAutoHeight);

            SetupAddRowToHostFunctionsIfAny(row, functions);
        }

        public void Helper_AddRowToHost(FrameworkElement ctrl,
            AddRowToHostFunctions functions = null, string ctrlIndex = null,
            bool rowAutoHeight = true)
        {
            DockPanel row = new DockPanel();
            handleAddingControlToIndexIfRequested(ctrl, ctrlIndex);

            row.Children.Add(ctrl);

            AddHostChild(row, rowAutoHeight);
            SetupAddRowToHostFunctionsIfAny(row, functions);
        }


        private void addVisibilityTrigger<T>(T ctrl, string boolIsVisiblePropertyName)
            where T : FrameworkElement
        {
            // only default to true if it doesn't have a value
            if (!this.Model.GetDynamicMemberNames().Contains(boolIsVisiblePropertyName))
            {
                this.Model[boolIsVisiblePropertyName] = true; // default value to true
            }

            // is visible
            DataTrigger isVisibleTrig = new DataTrigger();
            isVisibleTrig.Binding = createModelPropertyBinding(boolIsVisiblePropertyName);
            isVisibleTrig.Value = true;
            var isVisibleSetter = new Setter(FrameworkElement.VisibilityProperty, Visibility.Visible);
            isVisibleTrig.Setters.Add(isVisibleSetter);

            // is !!not!! visible
            DataTrigger isHiddenTrig = new DataTrigger();
            isHiddenTrig.Binding = createModelPropertyBinding(boolIsVisiblePropertyName);
            isHiddenTrig.Value = false;
            var isHiddenSetter = new Setter(FrameworkElement.VisibilityProperty, Visibility.Collapsed);
            isHiddenTrig.Setters.Add(isHiddenSetter);

            var style = new Style(ctrl.GetType(), ctrl.Style);
            style.Triggers.Add(isVisibleTrig);
            style.Triggers.Add(isHiddenTrig);
            ctrl.Style = style;
        }


        private Binding createModelPropertyBinding(string fieldName,
            BindingMode mode = BindingMode.Default,
                        UpdateSourceTrigger trigger = UpdateSourceTrigger.Default)
        {
            Binding bind = new Binding();
            bind.Source = this.Model;
            bind.Path = new PropertyPath(fieldName);
            bind.Mode = mode;
            bind.UpdateSourceTrigger = trigger;
            return bind;
        }


        public void Helper_BindField(string fieldName,
                        DependencyObject control,
                        DependencyProperty controlProperty,
                        BindingMode mode = BindingMode.Default,
                        UpdateSourceTrigger trigger = UpdateSourceTrigger.Default)
        {
            var bind = createModelPropertyBinding(fieldName, mode, trigger);
            BindingOperations.SetBinding(control, controlProperty, bind);
        }


        private static void SetupListViewStyleForMultipleItems(ListView lv)
        {
            // style it to stretch list items
            // see: http://stackoverflow.com/questions/10300228/set-itemcontainerstyle-from-code
            // see: http://stackoverflow.com/questions/1080479/how-to-make-dockpanel-fill-available-space
            var lvStyle = new Style(typeof(ListView));
            var horizontalContentAlignmentSetter = new Setter
            {
                Property = ListViewItem.HorizontalContentAlignmentProperty,
                Value = HorizontalAlignment.Stretch
            };
            lvStyle.Setters.Add(horizontalContentAlignmentSetter);
            lv.Style = lvStyle;
        }


        private IEnumerable<KeyValuePair<string, nac.wpf.utilities.RelayCommand>> GetRelayCommands(nac.utilities.BindableDynamicDictionary model)
        {
            foreach (var key in model.GetDynamicMemberNames())
            {
                if (model[key] is nac.wpf.utilities.RelayCommand cmd)
                {
                    yield return new KeyValuePair<string, nac.wpf.utilities.RelayCommand>(key: key,
                        value: cmd);
                }
            }
        }


        private void Helper_SetupItemsModelForRelayCommands(ItemCollection itemCollection, DataTemplateResult dataTemplate)
        {
            var relayCommands = GetRelayCommands(dataTemplate.Model);

            // each of the existing items needs the relay commands
            foreach (var i in itemCollection.OfType<nac.utilities.BindableDynamicDictionary>())
            {
                foreach (var cmd in relayCommands)
                {
                    i[cmd.Key] = cmd.Value;
                }
            }


            // each item needs the relay commands
            ((INotifyCollectionChanged)itemCollection).CollectionChanged += (_s, _args) =>
            {
                foreach (var i in _args.NewItems.OfType<nac.utilities.BindableDynamicDictionary>())
                {
                    foreach (var cmd in relayCommands)
                    {
                        i[cmd.Key] = cmd.Value;
                    }
                }
            };
        }






















    }
}