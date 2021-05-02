using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Updates;
using Jubi.Updates.Types;

namespace Jubi.Console.Api.Types
{
    public class ConsoleUpdateApiProvider : IUpdateApiProvider
    {
        public IApiProvider Provider { get; set; }
        public static int CountUpdates;
        public static string Command;

        public IEnumerable<UpdateInfo> Get()
        {
            for (int i = 0; i < CountUpdates; i++)
            {
                var us = ConsoleProvider.Instance.Users.FirstOrDefault(u => u.Id == (ulong) i);
                if (us != null && us.ThreadPoolActions.Count > 0) continue;
                
                yield return new UpdateInfo
                {
                    Initiator = (ulong)i,
                    UpdateContent = new MessageNewContent { Text = Command }
                };
            }
            
            Thread.Sleep(1000);
        }

        public void OnInit()
        {
            System.Console.Write("Enter count updates: ");
            CountUpdates = int.Parse(System.Console.ReadLine());
            System.Console.Write("\nEnter message: ");
            Command = System.Console.ReadLine();
        }
    }
}