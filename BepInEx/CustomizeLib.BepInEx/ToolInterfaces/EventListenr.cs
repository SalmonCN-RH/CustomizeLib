using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.ToolInterfaces
{
    public static class EventListenr
    {
        private static Dictionary<ListenerType, List<Action>> Listeners = [];

        /// <summary>
        /// 添加监听器
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="callback">回调</param>
        public static void AddListener(ListenerType type, Action callback)
        {
            if (Listeners.ContainsKey(type))
                Listeners[type].Add(callback);
            else
                Listeners.Add(type, [callback]);
        }

        /// <summary>
        /// 移除监听器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="callback">回调</param>
        public static void RemoveListener(ListenerType type, Action callback)
        {
            if (Listeners.ContainsKey(type) && Listeners[type].Contains(callback))
                Listeners[type].Remove(callback);
        }

        /// <summary>
        /// 触发监听器
        /// </summary>
        /// <param name="type">事件类型</param>
        public static void Trigger(ListenerType type)
        {
            if (!Listeners.TryGetValue(type, out var callbacks)) return;
            foreach (var callback in callbacks)
                callback.Invoke();
        }
    }

    public enum ListenerType
    {
        OnGameLaunch
    }
}
