namespace RepoM.ActionMenu.Interface.Scriban;

using System;

public interface IContextRegistration
{
    IContextRegistration CreateOrGetSubRegistration(string key);

    void SetValue(string member, object value, bool readOnly);

    void Add(string key, object value);

    bool ContainsKey(string key);

    void RegisterConstant(string name, object value);

    void RegisterAction(string name, Action action);

    void RegisterAction<T1>(string name, Action<T1> action);

    void RegisterAction<T1, T2>(string name, Action<T1, T2> action);

    void RegisterAction<T1, T2, T3>(string name, Action<T1, T2, T3> action);

    void RegisterAction<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action);

    void RegisterAction<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action);

    void RegisterFunction<T1>(string name, Func<T1> func);

    void RegisterFunction<T1, T2>(string name, Func<T1, T2> func);

    void RegisterFunction<T1, T2, T3>(string name, Func<T1, T2, T3> func);

    void RegisterFunction<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4> func);

    void RegisterFunction<T1, T2, T3, T4, T5>(string name, Func<T1, T2, T3, T4, T5> func);

    void RegisterFunction<T1, T2, T3, T4, T5, T6>(string name, Func<T1, T2, T3, T4, T5, T6> func);

    void RegisterVariable(string name, object value);
}