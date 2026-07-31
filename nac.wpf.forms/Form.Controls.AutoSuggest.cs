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

        private void PopulateAutoComplete<T>(AutoCompleteBox tb,
            Func<string, IEnumerable<T>> itemsGenerator,
            nac.utilities.BindableDynamicDictionary model,
            string itemFieldName)
        {
            var source = model[AutoSuggestSourceName(itemFieldName)] as ObservableCollection<T>;
            if (source == null)
            {
                throw new Exception("Source was null. This means you've defined source in AutoSuggestFor as a different type than observablecollection<T> probably.  Go check!");
            }
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
                        foreach (T i in items)
                        {
                            source.Add(i);
                        }

                    });

                }
                catch (Exception ex)
                {
                    log.Error($"Exception producing generated items.  {ex}");
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


        private void SetupTimerForAutoComplete<T>(nac.utilities.BindableDynamicDictionary model,
                                        string fieldName,
                                        AutoCompleteBox tb,
                                        Func<string, IEnumerable<T>> itemsGenerator)
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


        public Form AutoSuggestFor<T>(string fieldName, Func<string, IEnumerable<T>> itemsGenerator,
            Action<T> onSelected = null)
        {
            this.Model[fieldName] = "";
            string autoSuggestSourceFieldName = AutoSuggestSourceName(fieldName);
            this.Model[autoSuggestSourceFieldName] = new ObservableCollection<T>();
            string busyFieldName = BusyBindModelName(fieldName);
            this.Model[busyFieldName] = false;


            

            var tb = new System.Windows.Controls.AutoCompleteBox();
            tb.IsTextCompletionEnabled = false;
            tb.MinimumPrefixLength = 0;
            tb.FilterMode = AutoCompleteFilterMode.None; // we are going to filter inside the function
            SetupTimerForAutoComplete(this.Model, fieldName, tb, itemsGenerator);

            var busyIndicator = new nac.wpf.controls.BusyControl.BusyIndicatorControl();
            busyIndicator.Width = 20;
            busyIndicator.Height = 20;
            busyIndicator.Visibility = Visibility.Collapsed; // start out hidden

            // see this: http://www.broculos.net/2014/04/wpf-autocompletebox-autocomplete-text.html#.VxeC5zArJaQ

            Helper_BindField(fieldName, tb, AutoCompleteBox.SelectedItemProperty, BindingMode.TwoWay);

            Helper_BindField(autoSuggestSourceFieldName, tb, AutoCompleteBox.ItemsSourceProperty);

            Helper_BindField(busyFieldName, busyIndicator, nac.wpf.controls.BusyControl.BusyIndicatorControl.BusyProperty, BindingMode.TwoWay);

            if (onSelected != null)
            {
                tb.SelectionChanged += (_sender, _args) =>
                {
                    if (tb.SelectedItem is T item && item != null)
                    {
                        onSelected(item);
                    }
                };
            }

            DockPanel halfRow = new DockPanel();
            DockPanel.SetDock(busyIndicator, Dock.Left);
            DockPanel.SetDock(tb, Dock.Right);
            halfRow.Children.Add(busyIndicator);
            halfRow.Children.Add(tb);

            Helper_AddRowToHost(halfRow, fieldName);

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