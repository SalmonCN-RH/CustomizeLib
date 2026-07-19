using BepInEx;
using BepInEx.Unity.IL2CPP;
using Cysharp.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;

namespace Il2CppTask.BepInEx
{
    [BepInPlugin("salmon.test.il2cpptask", "Il2CppTaskTest", "1.0")]
    public class TestCore : BasePlugin
    {
        public override void Load()
        {
            PlayerLoopHelper.Initialize();
            // 测试
            // 调用异步方法并阻塞等待结果
            // int result = await Test();
            // Console.WriteLine($"结果: {result}");
            // Console.WriteLine(await ComputeAsync());
        }

        //public static async MyTask<int> Test()
        //{
        //    Console.WriteLine($"call, {Time.time}");
        //    await Task.Delay(1000);
        //    Console.WriteLine($"after, {Time.time}");
        //    return 100;
        //}
        //static async Il2CppTask ComputeAsync()
        //{
        //    Console.WriteLine($"ComputeAsync 开始， {Environment.TickCount64}");
        //    await Il2CppTask.Delay(1500);          // 等待 0.5 秒
        //    Console.WriteLine($"延迟结束, {Environment.TickCount64}");
        //}
        //public static MyTask<object> MyDelay(int milliseconds)
        //{
        //    var source = new MyTaskSource<object>();

        //    var timer = new System.Threading.Timer(_ =>
        //    {
        //        source.TrySetResult(null!); // 时间到了，设置结果，触发回调
        //    }, null, milliseconds, Timeout.Infinite);

        //    return source.Task;
        //}
    }
}
