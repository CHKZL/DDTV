using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auxiliary.FLVEx
{
    public interface ILogger
    {
        void Log(string message);

        [StringFormatMethod("format")]
        void Log(string format, params object[] args);

        [StringFormatMethod("format")]
        void Log(string format, object arg1);

        [StringFormatMethod("format")]
        void Log(string format, object arg1, object arg2);

        [StringFormatMethod("format")]
        void Log(string format, object arg1, object arg2, object arg3);
    }

    public class VoidLogger : ILogger
    {
        public void Log(string message) { }

        public void Log(string format, params object[] args) { }

        public void Log(string format, object arg1) { }

        public void Log(string format, object arg1, object arg2) { }

        public void Log(string format, object arg1, object arg2, object arg3) { }
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Log(string format, object arg1)
        {
            Console.WriteLine(format, arg1);
        }

        public void Log(string format, object arg1, object arg2)
        {
            Console.WriteLine(format, arg1, arg2);
        }

        public void Log(string format, object arg1, object arg2, object arg3)
        {
            Console.WriteLine(format, arg1, arg2, arg3);
        }
    }
}

namespace JetBrains.Annotations
{
    /// <summary>
    /// Indicates that the marked method builds string by format pattern and (optional) arguments.
    /// Parameter, which contains format string, should be given in constructor. The format string
    /// should be in <see cref="string.Format(IFormatProvider,string,object[])"/>-like form.
    /// </summary>
    /// <example><code>
    /// [StringFormatMethod("message")]
    /// void ShowError(string message, params object[] args) { /* do something */ }
    /// 
    /// void Foo() {
    ///   ShowError("Failed: {0}"); // Warning: Non-existing argument in format string
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
    public sealed class StringFormatMethodAttribute : Attribute
    {
        /// <param name="formatParameterName">
        /// Specifies which parameter of an annotated method should be treated as format-string
        /// </param>
        public StringFormatMethodAttribute(string formatParameterName)
        {
            FormatParameterName = formatParameterName;
        }

        public string FormatParameterName { get; private set; }
    }
}
