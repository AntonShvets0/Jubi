using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jubi.Attributes;
using Jubi.Enums;
using Jubi.Exceptions;
using Jubi.Response;

namespace Jubi.Abstracts.Executors
{
    public abstract class CommandExecutor
    {
        /// <summary>
        /// Can user call this executor if received request is not from keyboard? 
        /// </summary>
        public virtual bool IsHidden { get; }

        /// <summary>
        /// Need for middlewares
        /// </summary>
        public delegate bool ExecutorDelegate(CommandExecutor executor);

        /// <summary>
        /// Ways, which bot passes, before call Execute method
        /// If way returned not false - Execute() will never be called
        /// </summary>
        public virtual ExecutorDelegate[] Middlewares { get; }
            = Array.Empty<ExecutorDelegate>();
        
        public virtual CommandExecutor[] Subcommands { get; set; }
        
        public object[] Args { get; set; }
        
        public User User { get; set; }

        public virtual string Alias => GetType().GetCustomAttribute<CommandAttribute>()?.Alias;
        
        public string QueryData { get; set; }

        public virtual CommandScope Scope { get; set; } = CommandScope.PrivateChat;
        
        public string FullAlias => Parent == null ?
            Alias : Parent.FullAlias + " " + Alias;

        public string Syntax => string.Join(" ",
                PatternArgs.Select(t => $"<{GetTypeName(t)}>"));
        
        protected virtual Type[] PatternArgs { get; }

        public CommandExecutor Parent { get; set; }

        public Bot BotInstance;

        public bool PreProcessData()
        {
            if (PatternArgs == null) return true;
            if (PatternArgs.Length != Args.Length) 
                throw new SyntaxErrorException(this, Syntax);

            var args = new List<object>();
            for (int i = 0; i < Args.Length; i++)
            {
                if (!TryParse((string)Args[i], PatternArgs[i], out object arg)) 
                    throw new SyntaxErrorException(this, Syntax);
                args.Add(arg);
            }

            Args = args.ToArray();

            return true;
        }

        private string GetTypeName(Type type)
            => BotInstance.Configuration["types"][type.FullName];

        private bool TryParse(string s, Type type, out object arg)
        {
            try
            {
                if (type == typeof(int)) arg = int.Parse(s);
                else if (type == typeof(long)) arg = long.Parse(s);
                else if (type == typeof(decimal)) arg = decimal.Parse(s);
                else if (type == typeof(uint)) arg = uint.Parse(s);
                else if (type == typeof(ulong)) arg = ulong.Parse(s);
                else if (type == typeof(bool)) arg = bool.Parse(s);
                else if (type == typeof(char)) arg = s[0];
                else arg = s;
            }
            catch (Exception)
            {
                arg = null;
                return false;
            }
            
            return true;
        }

        public string Get(int index) => Args[index].ToString();
        public T Get<T>(int index) => (T) Args[index];

        /// <summary>
        /// Calling, while user send messae with prefix and aliase this Executor.
        /// </summary>
        /// <returns>Response to user</returns>
        public virtual Message? Execute()
        {
            if (Subcommands.Length == 0)
                throw new JubiException($"{GetType().Name} class does not override Execute() method");

            var dict = Subcommands.ToDictionary(
                k => k.Alias, 
                v => v);
            
            if (Args.Length < 1 || !dict.ContainsKey(Get<string>(0)))
                throw new SyntaxErrorException(this, $"<{string.Join("/", dict.Keys)}>");
                    
            var executor = dict[Get<string>(0)];
            executor.User = User;
            executor.Parent = this;

            var args = new List<object>(Args);
            args.RemoveAt(0);

            executor.Args = args.ToArray();

            foreach (var middleware in executor.Middlewares)
            {
                if (!middleware(executor)) return null;
            }
            if (!executor.PreProcessData()) return null;

            return executor.Execute();
        }
    }
}