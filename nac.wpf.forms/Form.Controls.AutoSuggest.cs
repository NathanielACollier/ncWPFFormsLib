using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace nac.wpf.forms
{
    public partial class Form
    {
        private string AutoSuggestSourceName(string fieldName)
        {
            return $"{fieldName}_autosuggestSource";
        }

        private string BusyBindModelName(string fieldName)
        {
            return $"{fieldName}_busy";
        }

        private string TimerName(string fieldName)
        {
            return $"{fieldName}_autoCompleteTimer";
        }

        private void PopulateAutoComplete(AutoCompleteBox tb,
                            Func<string, IEnumerable<string>> itemsGenerator,
                            BindableDynamicDictionary model,
                            string itemFieldName)
        {
            var source = model[AutoSuggestSourceName(itemFieldName)] as ObservableCollection<string>;
            string busyName = BusyBindModelName(itemFieldName);

            model[busyName] = true;

            string textBoxTextCopy = tb.Text;

            Thread t = new Thread(() =>
            {
                try
                {
                    var items = itemsGenerator(textBoxTextCopy);

                    tb.Dispatcher.Invoke(() =>
                    {
                        source.Clear();
                        foreach (string i in items)
                        {
                            source.Add(i);
                        }

                    });

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    tb.Dispatcher.Invoke(() =>
                    {
                        model[busyName] = false;
                    });
                }
            });

            t.Start();
        }


        private void SetupTimerForAutoComplete(BindableDynamicDictionary model,
                                        string fieldName,
                                        AutoCompleteBox tb,
                                        Func<string, IEnumerable<string>> itemsGenerator)
        {
            string timerName = TimerName(fieldName);
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(600)
            };

            timer.Tick += (sender, args) =>
            {
                timer.Stop(); // stop the timer, so that it can be started again the next time someone types
                PopulateAutoComplete(tb, itemsGenerator, model, fieldName);

            };

            model[timerName] = timer; // may need to save this...
        }


        public Form AutoSuggestFor(string fieldName, Func<string, IEnumerable<string>> itemsGenerator)
        {
            this.Model[fieldName] = "";
            string autoSuggestSourceFieldName = AutoSuggestSourceName(fieldName);
            this.Model[autoSuggestSourceFieldName] = new ObservableCollection<string>();
            string busyFieldName = BusyBindModelName(fieldName);
            this.Model[busyFieldName] = false;




            var tb = new System.Windows.Controls.AutoCompleteBox();
            tb.IsTextCompletionEnabled = false;
            tb.MinimumPrefixLength = 0;
            tb.FilterMode = AutoCompleteFilterMode.None; // we are going to filter inside the function
            SetupTimerForAutoComplete(this.Model, fieldName, tb, itemsGenerator);

            var busyIndicator = new ncWPFBusyControlLib.BusyIndicatorControl();
            busyIndicator.Width = 20;
            busyIndicator.Height = 20;
            busyIndicator.Visibility = Visibility.Collapsed; // start out hidden

            // see this: http://www.broculos.net/2014/04/wpf-autocompletebox-autocomplete-text.html#.VxeC5zArJaQ

            Binding textBind = new Binding();
            textBind.Source = this.Model;
            textBind.Path = new PropertyPath(fieldName);
            textBind.Mode = BindingMode.TwoWay;
            textBind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(tb, AutoCompleteBox.SelectedItemProperty, textBind);

            Binding itemsBind = new Binding();
            itemsBind.Source = this.Model;
            itemsBind.Path = new PropertyPath(autoSuggestSourceFieldName);
            BindingOperations.SetBinding(tb, AutoCompleteBox.ItemsSourceProperty, itemsBind);

            Binding busyBind = new Binding();
            busyBind.Source = this.Model;
            busyBind.Path = new PropertyPath(busyFieldName);
            busyBind.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(busyIndicator, ncWPFBusyControlLib.BusyIndicatorControl.BusyProperty, busyBind);

            DockPanel halfRow = new DockPanel();
            DockPanel.SetDock(busyIndicator, Dock.Left);
            DockPanel.SetDock(tb, Dock.Right);
            halfRow.Children.Add(busyIndicator);
            halfRow.Children.Add(tb);

            AddRowToHost(halfRow, fieldName);

            PopulateAutoComplete(tb, itemsGenerator, this.Model, fieldName);

            this.Model.PropertyChanged += (sender, args) =>
            {
                if (string.Equals(args.PropertyName, fieldName))
                {
                    // start a timer, or reset a timer to repopulate the drop down
                    var timer = this.Model[TimerName(fieldName)] as System.Windows.Threading.DispatcherTimer;
                    timer.Stop();
                    timer.Start();
                }
            };



            return this;
        }
        
        
        
        
        
    }
}