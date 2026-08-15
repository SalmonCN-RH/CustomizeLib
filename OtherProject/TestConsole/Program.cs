// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Runtime.InteropServices;
using TestConsole;

public class ExecuteMachineCodeExample
{
    [DllImport("kernel32.dll")]
    static extern bool VirtualProtectEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        UIntPtr dwSize,
        uint flNewProtect,
        out uint lpflOldProtect
    );

    // 定义托管函数签名，用于调用机器码
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int AddFunction(int x, int y);

    const uint PAGE_EXECUTE_READWRITE = 0x40;

    public static unsafe void Main()
    {
        // 这就是你从反汇编窗口复制来的终极优化版机器码！
        // 8D 04 11 对应 lea eax, [rcx+rdx] 
        // C3 对应 ret
        byte[] machineCode = { 0x8D, 0x04, 0x11, 0xC3 };

        unsafe
        {
            fixed (byte* ptr = machineCode)
            {
                IntPtr memoryAddress = (IntPtr)ptr;

                // 修改内存页属性为可读可写可执行
                if (!VirtualProtectEx(
                    Process.GetCurrentProcess().Handle,
                    memoryAddress,
                    (UIntPtr)machineCode.Length,
                    PAGE_EXECUTE_READWRITE,
                    out uint _))
                {
                    throw new System.ComponentModel.Win32Exception();
                }

                // 创建委托并执行
                AddFunction add = Marshal.GetDelegateForFunctionPointer<AddFunction>(memoryAddress);
                int result = add(10, -15);
                Console.WriteLine($"计算结果: {result}"); // 完美输出: -5
            }
        }

        var handle = HookCore.AllocMemmory(machineCode);
        Console.WriteLine((int)HookCore.RunAssemblyCode<AddFunction>((byte*)handle.AddrOfPinnedObject(), 4, -2147483648, -1));
        HookCore.SetAssemblyCode((byte*)handle.AddrOfPinnedObject(), 0, new byte[]
        {
            0x89, 0xC8, // mov eax, ecx
            0x90, // nop
            0xC3 // ret
        });
        Console.WriteLine((int)HookCore.RunAssemblyCode<AddFunction>((byte*)handle.AddrOfPinnedObject(), 4, -2147483648, -1));
        handle.Free();

        foreach (var item in machineCode)
            Console.WriteLine($"0x{item:X}");
    }

    
}
