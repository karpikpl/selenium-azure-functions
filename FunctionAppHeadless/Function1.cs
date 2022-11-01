using FunctionAppHeadless.Tests;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FunctionAppHeadless
{
    public class SeleniumTrigger1
    {
        #region Field(s)
        private static readonly object SyncRoot = new();
        private readonly List<BaseTest> Tests;
        #endregion

        #region Constructor(s)
        public SeleniumTrigger1(TelemetryConfiguration telemetryConfiguration)
        {
            string @namespace = typeof(BaseTest).Namespace;

            Tests = typeof(SeleniumTrigger1)
                        .Assembly
                        .GetTypes()
                        .Where(x => x.Namespace == @namespace
                            && typeof(BaseTest).IsAssignableFrom(x)
                            && !x.IsAbstract
                        )
                        .Select(x =>
                        {
                            return (BaseTest)Activator.CreateInstance(x, telemetryConfiguration);
                        })
                        .ToList();
        }
        #endregion

        [FunctionName("WebtestTimerTrigger")]
        public void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)//Function runs every 5 minutes
        {
            try
            {
                if (!Monitor.TryEnter(SyncRoot))
                    return; // already running


                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("headless");
                

                var whereToSearch =Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
                var chromeFile = SearchAccessibleFiles(whereToSearch, "chromedriver.exe");

                if (chromeFile is null)
                {
                    throw new FileNotFoundException("Could not find chromedriver.exe in the current directory or any of its subdirectories.");
                }

                //using IWebDriver driver = new ChromeDriver(Path.GetDirectoryName(chromeFile), chromeOptions);
                using IWebDriver driver = new ChromeDriver(Directory.GetParent(chromeFile).FullName, chromeOptions);

                var Browser = driver;
                var options = Browser.Manage();
                var timeouts = options.Timeouts();
                timeouts.PageLoad = TimeSpan.FromSeconds(5);

                var WebDriverWait = new WebDriverWait(Browser, TimeSpan.FromSeconds(5));

                foreach (var test in Tests)
                {
                    using (test)
                    {
                        test.Run(myTimer, log, Browser);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Exception thrown for Selenium.");
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        string? SearchAccessibleFiles(string root, string fileName)
        {
            Console.WriteLine($"Searching for {fileName} in {root}.");
            var file = Directory.EnumerateFiles(root).FirstOrDefault(m => m.EndsWith(fileName));

            if (file is not null)
            {
                return file;
            }

            foreach (var subDir in Directory.EnumerateDirectories(root))
            {
                try
                {
                    var fileFound = SearchAccessibleFiles(subDir, fileName);

                    if (fileFound is not null)
                    {
                        return fileFound;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // ...
                }
            }

            return null;
        }
    }
}
