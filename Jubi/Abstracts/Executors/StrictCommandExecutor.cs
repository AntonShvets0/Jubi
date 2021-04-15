using System;

namespace Jubi.Abstracts.Executors
{
    public abstract class CommandExecutor<T> : CommandExecutor
    {
        protected override Type[] PatternArgs { get; } = { typeof(T) };
    }
    
    public abstract class CommandExecutor<T, T2> : CommandExecutor
    {
        protected override Type[] PatternArgs { get; } = { typeof(T), typeof(T2) };
    }
    
    public abstract class CommandExecutor<T, T2, T3> : CommandExecutor
    {
        protected override Type[] PatternArgs { get; } = { typeof(T), typeof(T2), typeof(T3) };
    }
    
    public abstract class CommandExecutor<T, T2, T3, T4> : CommandExecutor
    {
        protected override Type[] PatternArgs { get; } = { typeof(T), typeof(T2), typeof(T3), typeof(T4) };
    }
    
    public abstract class CommandExecutor<T, T2, T3, T4, T5> : CommandExecutor
    {
        protected override Type[] PatternArgs { get; } = { typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
    }
    
    public abstract class CommandExecutor<T, T2, T3, T4, T5, T6> : CommandExecutor
    {
        protected override Type[] PatternArgs { get; } = { typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
    }
}