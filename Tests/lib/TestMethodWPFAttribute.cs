﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Tests.lib;

public class TestMethodWPFAttribute : TestMethodAttribute
{
    public override TestResult[] Execute(ITestMethod testMethod)
    {
        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            return Invoke(testMethod);
        }

        TestResult[] result = null;
        var thread = new Thread(() => {
            result = Invoke(testMethod);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return result;
    }

    private TestResult[] Invoke(ITestMethod testMethod)
    {
        return new[] { testMethod.Invoke(null) };
    }
}
