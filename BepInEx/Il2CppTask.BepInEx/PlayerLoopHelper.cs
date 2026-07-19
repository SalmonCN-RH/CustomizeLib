using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Il2CppTask.BepInEx
{
    public static class PlayerLoopHelper
    {
        internal class Test
        {
            public static void Do()
            {
                Console.WriteLine("before update");
            }
        }

        internal static void Initialize()
        {
            ClassInjector.RegisterTypeInIl2Cpp<Test>();
            var insert = new PlayerLoopSystem();
            var action = Test.Do;
            insert.updateDelegate = action;
            insert.type = Il2CppType.From(typeof(Test));
            insert.subSystemList = null;
            insert.loopConditionFunction = IntPtr.Zero;
            InsertPlayerLoop(insert, typeof(Update));
        }

        private static void InsertPlayerLoop(PlayerLoopSystem loopSystem, Type targetType)
        {
            var origin = PlayerLoop.GetCurrentPlayerLoop();
            if (origin == null) return;
            if (origin.subSystemList == null) origin.subSystemList = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PlayerLoopSystem>(0);
            for (int i = 0; i < origin.subSystemList.Length; i++)
            {
                var item = origin.subSystemList[i];
                if (item.type == Il2CppType.From(targetType))
                {
                    var oldSystems = origin.subSystemList;
                    var newSystems = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PlayerLoopSystem>(oldSystems.Length + 1);
                    newSystems[0] = loopSystem;
                    oldSystems.CopyTo(newSystems, 1);
                    item.subSystemList = newSystems;
                    origin.subSystemList[i] = item;
                    break;
                }
            }
            PlayerLoop.SetPlayerLoop(origin);
        }
    }
}
