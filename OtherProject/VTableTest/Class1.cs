using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using Il2CppInterop.Runtime.Runtime.VersionSpecific.Class;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VTableTest
{
    public class Class1
    {

    }

    public static unsafe class VTableHelper
    {
        public static Il2CppMethodInfo* ConvertMethodInfo(MethodInfo method, INativeClassStruct declaring)
        {
            var cls = IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "", "UltimateSpring");
            var strc = UnityVersionHandler.Wrap((Il2CppClass*)cls);
            var me = ((VirtualInvokeData*)strc.VTable)[61];
            var info = UnityVersionHandler.Wrap(me.method);
            Console.WriteLine($"{Marshal.PtrToStringUTF8(info.Name)}, {me.methodPtr:X}");
        }
    }
}
