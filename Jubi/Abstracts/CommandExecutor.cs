using System;
using System.Collections.Generic;
using Jubi.Response;

namespace Jubi.Abstracts
{
    public abstract class CommandExecutor
    {
        /// <summary>
        /// Name of command
        /// </summary>
        /// 
        /// Reserved alias: page. Please, don't use reserved name.
        public abstract string Alias { get; }
        
        /// <summary>
        /// Can user call this executor if received request is not from keyboard? 
        /// </summary>
        public virtual bool IsHidden { get; }

        /// <summary>
        /// Need for middlewares
        /// </summary>
        /// <param name="user">User, who call it</param>
        /// <param name="args">Arguments</param>
        public delegate Message? ExecutorDelegate(User user, string[] args);

        /// <summary>
        /// Ways, which bot passes, before call Execute method
        /// If way returned not null - send returned Message and break
        /// </summary>
        public virtual List<ExecutorDelegate> Middlewares { get; set; }
            = new List<ExecutorDelegate>();

        /// <summary>
        /// Calling, while user send messae with prefix and aliase this Executor.
        /// </summary>
        /// <param name="user">User, who call it</param>
        /// <param name="args">Arguments</param>
        /// <returns>Response to user</returns>
        public abstract Message? Execute(User user, string[] args);

        public Bot BotInstance;

        public Message? GetError(string error)
        {
            return BotInstance.Configuration["prefix_error"]["default"] + " " + error;
        } 
        
        public Message? GetSyntaxError(string error)
        {
            return BotInstance.Configuration["prefix_error"]["syntax"] +  $" /{Alias} " + error;
        }
    }
}