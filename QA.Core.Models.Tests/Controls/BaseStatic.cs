using System;
using System.Runtime.CompilerServices;

namespace QA.Core.Models.Tests.Controls
{
    public class Tester
    {
        static Tester() { }
        public static bool state;

        public static DateTime Call()
        {
            Console.WriteLine("call");
            state = true;
            return DateTime.Now;
        }
    }

    public class Base
    {
        static Base()
        {
            Console.WriteLine("base");
        }
    }

    public class Class1 : Base
    {
        static Class1() { }
        public static readonly object prop = Tester.Call();

        public virtual object Prop { get { return prop; } }
    }

    public class Class2
    {
        static Class2()
        {
            prop = Tester.Call();
        }

        public static object prop;

        public virtual object Prop { get { return prop; } }
    }
}

