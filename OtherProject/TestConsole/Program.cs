// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Test
{
    public static unsafe void Main()
    {
        delegate*<int, int, int> ptr = &Add;

        Console.WriteLine($"{(IntPtr)ptr:X}, {ptr(1, -5)}");
    }

    public static int Add(int a, int b) => a + b;
}
