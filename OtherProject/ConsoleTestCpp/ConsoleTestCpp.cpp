#include <windows.h>
#include <stdio.h>
#include <iostream>

class MyClass {
public:
    virtual void Func(int num) {
        printf("[原始] 执行Func，参数为：%d\n", num);
    }
};

// 1. 定义函数指针类型（用于保存原函数）
typedef void(__thiscall* OriginalFunc)(void* pThis, int num);

// 2. 保存原始虚函数地址（全局变量）
OriginalFunc g_OriginalFunc = nullptr;

// 3. 你的替代函数（注意必须使用 __thiscall，或者 __stdcall 配合指针）
void __stdcall HookedFunc(void* pThis, int num) {
    // --- 增强逻辑 A：前置增强（Before Advice） ---
    printf("[Hook] 前置拦截：参数 num = %d\n", num);

    // 参数校验/修改（你可以篡改传入的参数）
    if (num < 0) {
        printf("[Hook] 检测到负数，修正为 0\n");
        num = 0;
    }

    // --- 核心：调用原函数（执行原始逻辑） ---
    // 注意：必须将 this 指针（pThis）原封不动地传回去
    g_OriginalFunc(pThis, num);

    // --- 增强逻辑 B：后置增强（After Advice） ---
    printf("[Hook] 后置拦截：原函数执行完毕\n");
}

void InstallHook(MyClass* obj) {
    // 获取虚表
    uintptr_t* vptr = *(uintptr_t**)obj;

    // 假设 Func 是第一个虚函数（索引 0）
    // 先保存原地址（这一步必须在篡改前执行）
    g_OriginalFunc = (OriginalFunc)vptr[0];

    // 篡改虚表（改为我们的 HookedFunc）
    DWORD oldProtect;
    VirtualProtect(&vptr[0], sizeof(uintptr_t), PAGE_READWRITE, &oldProtect);

    // 注意：这里直接赋值的地址是 __stdcall 函数，兼容性最好
    vptr[0] = (uintptr_t)HookedFunc;

    VirtualProtect(&vptr[0], sizeof(uintptr_t), oldProtect, &oldProtect);
}

int main() {
    MyClass obj;
    // InstallHook(&obj);
    auto addr = &InstallHook;
    std::cout << addr << std::endl;
    // 调用被增强后的函数
    obj.Func(100);
    obj.Func(-5);
    return 0;
}