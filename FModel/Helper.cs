﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace FModel;

public static class Helper
{
    [StructLayout(LayoutKind.Explicit)]
    private struct NanUnion
    {
        [FieldOffset(0)]
        internal double DoubleValue;
        [FieldOffset(0)]
        internal readonly ulong UlongValue;
    }

    public static void OpenWindow<T>(string windowName, Action action) where T : Window
    {
        if (!IsWindowOpen<T>(windowName))
        {
            action();
        }
        else
        {
            var w = GetOpenedWindow<T>(windowName);
            if (windowName == "Search View") w.WindowState = WindowState.Normal;
            w.Focus();
        }
    }

    public static T GetWindow<T>(string windowName, Action action) where T : Window
    {
        if (!IsWindowOpen<T>(windowName))
        {
            action();
        }

        var ret = (T) GetOpenedWindow<T>(windowName);
        ret.Focus();
        ret.Activate();
        return ret;
    }

    public static void CloseWindow<T>(string windowName) where T : Window
    {
        if (!IsWindowOpen<T>(windowName)) return;
        GetOpenedWindow<T>(windowName).Close();
    }

    private static bool IsWindowOpen<T>(string name = "") where T : Window
    {
        return string.IsNullOrEmpty(name)
            ? Application.Current.Windows.OfType<T>().Any()
            : Application.Current.Windows.OfType<T>().Any(w => w.Title.Equals(name));
    }

    private static Window GetOpenedWindow<T>(string name) where T : Window
    {
        return Application.Current.Windows.OfType<T>().FirstOrDefault(w => w.Title.Equals(name));
    }

    public static bool IsNaN(double value)
    {
        var t = new NanUnion { DoubleValue = value };
        var exp = t.UlongValue & 0xfff0000000000000;
        var man = t.UlongValue & 0x000fffffffffffff;
        return exp is 0x7ff0000000000000 or 0xfff0000000000000 && man != 0;
    }

    public static bool AreVirtuallyEqual(double d1, double d2)
    {
        if (double.IsPositiveInfinity(d1))
            return double.IsPositiveInfinity(d2);

        if (double.IsNegativeInfinity(d1))
            return double.IsNegativeInfinity(d2);

        if (IsNaN(d1))
            return IsNaN(d2);

        var n = d1 - d2;
        var d = (Math.Abs(d1) + Math.Abs(d2) + 10) * 1.0e-15;
        return -d < n && d > n;
    }
}