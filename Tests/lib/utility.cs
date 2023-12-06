using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.lib;

internal static class utility
{

    public static async Task ShowForm(Action<nac.wpf.forms.Form> buildFormAction,
            Action<nac.utilities.BindableDynamicDictionary> workWithModelAfterFormAction)
    {
        nac.utilities.BindableDynamicDictionary formModel = null;

        await nac.wpf.forms.Form.StartUI(f =>
        {
            buildFormAction(f);
            formModel = f.Model; // save reference so we can do tests against it
        });

        workWithModelAfterFormAction(formModel);
    }
}
