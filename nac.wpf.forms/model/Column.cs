using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nac.wpf.forms.model;

public class Column
{
    public string Header { get; set; }
    public string modelBindingPropertyName { get; set; }
    public Action<Form> template { get; set; }
}
