using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Il2CppTask.BepInEx
{
    [AsyncMethodBuilder(typeof(Il2CppTaskBuilder<>))]
    public partial struct Il2CppTask<T>
    {
        internal Il2CppTaskSource<T> Source;
        internal Il2CppTask(Il2CppTaskSource<T> Source) => this.Source = Source;
    }

    public struct Il2CppTaskAwaiter<T> : INotifyCompletion, ICriticalNotifyCompletion
    {
        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }
        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
    }

    public struct Il2CppTaskBuilder<T>
    {
        internal Il2CppTaskSource<T> Source;
        public Il2CppTask<T> Task => new(Source);

        public static Il2CppTaskBuilder<T> Create() => new();
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
            stateMachine.MoveNext();
        public void SetResult(T result) => Source.SetResult(result);
        public void SetException(Exception e) => Source.SetException(e);

        internal Il2CppTaskBuilder(Il2CppTaskSource<T> source) => Source = source;
    }

    internal class Il2CppTaskSource<T>
    {
        internal Action OnCompleted;

        internal void SetResult(T result)
        {

        }
        internal void SetException(Exception e)
        {

        }
    }
}
