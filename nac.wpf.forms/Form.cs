using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace nac.wpf.forms
{
    public partial class Form
    {
        
        public nac.utilities.BindableDynamicDictionary Model { get; set; }
        private lib.NacFormHostGrid Host { get; set; }
        private Window window = null;
        private Dictionary<string, FrameworkElement> controlsIndex;
        private System.Windows.Threading.Dispatcher dispatcher = null;

        public Form(Form _parentForm = null)
        {
            var g = new lib.NacFormHostGrid();
            var gridCol = new ColumnDefinition();
            g.ColumnDefinitions.Add(gridCol);

            this.Host = g;

            this.controlsIndex = new Dictionary<string, FrameworkElement>();

            if (_parentForm != null)
            {
                this.dispatcher = _parentForm.dispatcher; // keep using parent form's dispatcher
                this.Model = _parentForm.Model; // use same model as parent
            }
            else
            {
                this.Model = new nac.utilities.BindableDynamicDictionary();
                // only the origin host has the data context set
                g.DataContext = this.Model; // for commands need to be able to bind to DataContext
                this.dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            }
        }



        public void Close()
        {
            if (this.window != null)
            {
                this.window.Close();
            }
            else
            {
                throw new Exception("This form has not been displayed, or something bad is wrong.  No reference was saved to the window object that Display sets up");
            }

        }

        public Task<bool> BeginInvoke(Action codeToRunOnUIThread)
        {
            var promise = new TaskCompletionSource<bool>();
            this.dispatcher.BeginInvoke(() =>
            {
                codeToRunOnUIThread();
                promise.SetResult(true);
            });
            return promise.Task;
        }


        public Form Display(int height = 600, int width = 800,
                Action<Form> onClosing = null)
        {
            var win = new Window();
            return Display(win: win, height: height, width: width,
                        onClosing: onClosing);
        }

        public Form Display(Window win, int height = 600, int width = 800,
                Action<Form> onClosing = null,
                bool windowCallShowDialog = true)
        {
            this.window = win; // save a ref to what window we are using
            win.Height = height;
            win.Width = width;
            win.Content = this.Host;

            win.Closing += (_s, _args) =>
            {
                if (onClosing != null)
                {
                    onClosing(this);
                }
            };

            // if we are managing the wpf thread ourselves it is helpfull to do the show of the window in that code
            if (windowCallShowDialog)
            {
                win.ShowDialog();
            }


            return this;
        }


        public static Task<nac.wpf.utilities.Run.RunResult> StartUI(Action<Form> workWithNewForm, int height = 600, int width = 800,
            Action<Form> onClosing = null)
        {
            return nac.wpf.utilities.Run.Run.runWithUIThread(async (win) =>
            {
                var newForm = new Form();
                workWithNewForm(newForm);
                newForm.Display(win, width: width, height: height, onClosing: onClosing,
                        windowCallShowDialog: false);
            });
        }



    }
}
