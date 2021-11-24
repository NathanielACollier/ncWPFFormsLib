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

        public BindableDynamicDictionary Model { get; set; }
        private StackPanel Host { get; set; }

        public Form()
        {
            this.Model = new BindableDynamicDictionary();
            this.Host = new StackPanel();
            this.Host.Orientation = Orientation.Vertical;
        }
        
        
        public Form Display(int height = 600, int width = 800)
        {
            var win = new Window();
            win.Height = height;
            win.Width = width;
            win.Content = this.Host;

            win.ShowDialog();

            return this;
        }



    }
}
