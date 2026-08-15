using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueShootingLib.BepInEx
{
    public partial class CustomConfig
    {
        public static IntPtr GetConsPtr<T>() where T : CustomConfig =>
            GetConsPtr(typeof(T));
        public static IntPtr GetConsPtr(Type type) =>
            IL2CPP.il2cpp_object_new(Il2CppClassPointerStore.GetNativeClassPointer(type));
    }
}
