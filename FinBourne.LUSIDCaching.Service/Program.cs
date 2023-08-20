using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinBourne.LUSIDCaching;
using log4net;
using System.Collections.Specialized;
using System.Runtime.Caching;
using log4net.Config;
using System.Reflection;

namespace FinBourne.LUSIDCaching.Service
{
    public class Program
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This is just a basic simulation of three threads using the caching component
        /// the main thread just adds a simple item and trys to look it up much later, the item wont exist
        /// the first thread will add an object, wait a bit then check if it exists again, it should exist
        /// the second thread will just keep adding objects non stop with no regard for checking.
        /// 
        /// the threads should not collide or have issues with accessing the memory cache due to a mutex lock on both add and get. Along with utilizing the MemoryCache systems object
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {

            Console.WriteLine("Hello, World!");

            XmlConfigurator.Configure(new FileInfo("log4net.config"));


            var firstItem = LusidDynamicCache.AddItem("First", "First", a =>
            {
                log.Debug($"Thread 0 {a.Key} Removed Last Accessed { a.LastAccessed }");
            });
            log.Debug($"Thread 0 Added {firstItem.Key} Last Accessed {firstItem.LastAccessed}");

            // Thread 1 checks for last item they add every 1.5 seconds
            Thread thread = new Thread(e =>
            {
                LusidDynamicCacheObject lastItem = null;
                while (true)
                {
                    if(lastItem != null)
                    {
                        var item = LusidDynamicCache.GetItem(lastItem.Key);
                        if(lastItem.Value == item)
                        {
                            log.Debug("Item still exists!");
                        }
                    }

                    lastItem = LusidDynamicCache.AddItem(Guid.NewGuid().ToString(), "wow", a =>
                    {
                        log.Debug($"Thread 1 {a.Key} Removed Last Accessed { a.LastAccessed }");
                    });
                    log.Debug($"Thread 1 Added {lastItem.Key} Last Accessed {lastItem.LastAccessed}");
                    Thread.Sleep(1500);
                }
            });
            thread.Start();

            // thread 2 is just conerned with adding items every 500ms
            Thread thread2 = new Thread(e =>
            {
                while (true)
                { 
                    var item = LusidDynamicCache.AddItem(Guid.NewGuid().ToString(), "wow", a =>
                    {
                        log.Debug($"Thread 2 {a.Key} Removed Last Accessed { a.LastAccessed }");
                    });
                    log.Debug($"Thread 2 Added {item.Key} Last Accessed {item.LastAccessed}");
                    Thread.Sleep(500);
                }
            });
            thread2.Start();

            // The main thread will add an item at the start then check for its existance later on down the line, the item should not exist!
            Thread.Sleep(15000);

            var i = LusidDynamicCache.GetItem("First");
            if(i == null) 
                log.Debug($"Main Threads item no longer exists"); 

        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
