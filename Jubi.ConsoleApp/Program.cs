﻿using System;
using System.Reflection;
using Jubi.Abstracts;
using Jubi.Models;
using Jubi.Telegram;
using Jubi.VKontakte;
using SimpleIni;

namespace Jubi.ConsoleApp
{
    // It - example bot, which work through Jubi

    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot(
                "config.ini", 
                new SiteProvider[]{ new VKontakteProvider(), new TelegramProvider() }, 
                new ExecutorInformation
                {
                    Namespace = "Jubi.ConsoleApp.Executors",
                    Assembly = Assembly.GetExecutingAssembly()
                });
            
            bot.Start();
        }
    }
}