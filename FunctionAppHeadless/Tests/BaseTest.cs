using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace FunctionAppHeadless.Tests
{
    public abstract class BaseTest
    {
        #region Properties
        protected TelemetryClient TelemetryClient { get; private set; }
        protected TelemetryConfiguration TelemetryConfiguration { get; private set; }

        internal IWebDriver Browser { get; private set; }
        internal WebDriverWait WebDriverWait;

        protected abstract string TestDescription { get; }
        #endregion

        #region Constructor(s)
        public BaseTest(TelemetryConfiguration telemetryConfiguration)
        {
            TelemetryConfiguration = telemetryConfiguration;
            TelemetryClient = new TelemetryClient(telemetryConfiguration);
        }
        #endregion

        #region Method(s)
        public void Run(TimerInfo timer, ILogger log, IWebDriver webDriver)
        {
            Browser = webDriver;
            WebDriverWait = new WebDriverWait(Browser, TimeSpan.FromSeconds(5));
         
            var stopwatch = Stopwatch.StartNew();
            var @event = new EventTelemetry(TestDescription);

            TelemetryClient.TrackEvent(@event);

            log.LogInformation($"Starting to execute test: {TestDescription}.");

            try
            {
                RunTest(timer, log);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Exception thrown for {TestDescription}.");
            }

            log.LogInformation($"Completed executing test: {TestDescription}.  Took: {FormatElapsedTime(stopwatch.ElapsedMilliseconds)}");
            log.LogInformation($"==============================================================================");
        }        

        public abstract void RunTest(TimerInfo timer, ILogger log);
        #endregion

        #region Helper(s)
        private static string FormatElapsedTime(double milliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

            return timeSpan.ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s");
        }
        #endregion
    }
}