using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests;

[TestClass]
public class __MSTest_Setup
{
    private static nac.Logging.Logger log = new();


    [AssemblyInitialize()]
    public static void MyTestInitialize(TestContext testContext)
    {
        nac.Logging.Logger.OnNewMessage += (_s, _e)=>{
            System.Diagnostics.Debug.WriteLine(_e);
        };

        log.Info("Tests Starting");

    }

    [AssemblyCleanup]
    public static void TearDown()
    {


    }


}

