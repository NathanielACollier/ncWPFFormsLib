using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace nac.wpf.forms;

public partial class Form
{
    public class BusyFunctions
    {
        public Action stop;
        public Action start;
        public Func<bool> isBusy;
    }

    public Form Busy(string displayText, bool startIsBusy = false, BusyFunctions functions = null)
    {
        string busyFieldName = "busy_" + Guid.NewGuid().ToString("N");

        var busyCtrl = new nac.wpf.controls.BusyControl.BusyIndicatorControl();
        busyCtrl.Width = 20;
        busyCtrl.Height = 20;

        Helper_BindField(busyFieldName, busyCtrl, nac.wpf.controls.BusyControl.BusyIndicatorControl.BusyProperty, BindingMode.TwoWay);

        var addRowToHostFunctions = new AddRowToHostFunctions();
        Helper_AddRowToHost(busyCtrl, displayText, functions: addRowToHostFunctions);


        this.Model[busyFieldName] = startIsBusy; // start it off busy???

        if (!startIsBusy)
        {
            addRowToHostFunctions.hide();
        }

        if (functions != null)
        {
            functions.start = () =>
            {
                this.Model[busyFieldName] = true;
                addRowToHostFunctions.show();
            };
            functions.stop = () =>
            {
                this.Model[busyFieldName] = false;
                addRowToHostFunctions.hide();
            };

            functions.isBusy = () =>
            {
                return (bool)this.Model[busyFieldName];
            };
        }


        return this;
    }
}
