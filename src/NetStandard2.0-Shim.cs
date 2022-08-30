// Copyright © myCSharp.de - all rights reserved

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue) => this.ReturnValue = returnValue;

        public bool ReturnValue { get; }
    }
}

namespace MyCSharp.HttpUserAgentParser
{
    using System;
    using System.Collections.Generic;

    internal static class Extensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        public static bool Contains(this string instance, string value, StringComparison comparison)
            => instance.IndexOf(value, comparison) >= 0;
    }
}
