# Il2Cpp环境下继承原游戏的类
让你的自定义类继承自你想要继承的类，下面是一个示例
```c#
public class MyClass : SomeIL2CPPClass
{
    public MyClass(IntPtr ptr) : base(ptr) { }
    
    public MyClass() : base(ClassInjector.DerivedConstructorPointer<MyClass>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
    
    // 其他方法
}
```

## 字段/属性初始化
自定义的类在继承Il2Cpp的类后，无法同步继承字段/属性的初始值，所以你需要手动对自己的类的字段/属性进行赋值     
对于自定义组件，可以通过在同一个`GameObject`上`AddComponent<BaseType>()`，并手动将`BaseType`上的字段/属性的值复制到自己的类，可选的，你可以在复制完成后销毁父类组件

## 重写方法
Il2Cpp下，你不能直接在`override`的方法中调用`base.Method()`，这会引起`StackOverflow`，`CustomizeLib`提供了工具类`BaseMethodInvoker`来完成对`base.Method`的调用     
你也可以使用`HarmonyReversePatch`来获取`base`方法的存根，调用这个存根方法不会引起`StackOverflow`，但请注意，受限于Il2Cpp，`HarmonyReversePatch`必须在程序集内进行定义，无法动态生成存根方法

### BaseMethodInvoker

> TODO: 介绍BaseMethodInvoker
