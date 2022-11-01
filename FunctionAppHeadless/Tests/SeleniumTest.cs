using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace FunctionAppHeadless.Tests
{
    public class SeleniumTest : BaseTest
    {
        protected override string TestDescription => "Bing Search Test";

        public SeleniumTest(TelemetryConfiguration telemetryConfiguration) : base(telemetryConfiguration) { }
        public override void RunTest(TimerInfo timer, ILogger log)
        {
            log.LogInformation("Testing selenium has Begun");
            DateTime start = DateTime.UtcNow;

            try
            {
                log.LogInformation("We will now attempt selenium test");

                Browser.Navigate().GoToUrl("https://www.selenium.dev/selenium/web/web-form.html");
                log.LogInformation("The Website we are on is {0}", Browser.Url);

                var title = Browser.Title;
                if (title == "Web form")
                {
                    log.LogInformation("Title is OK");
                }
                else
                {
                    log.LogError("Test failed - title doesn't match");
                    return;
                }

                Browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);


                var textBox = Browser.FindElement(By.Name("my-text"));
                var submitButton = Browser.FindElement(By.CssSelector("button"));

                textBox.SendKeys("Selenium");
                submitButton.Click();

                var message = Browser.FindElement(By.Id("message"));
                var value = message.Text;

                if (value == "Received!")
                {
                    log.LogInformation("Message is OK");
                }
                else
                {
                    log.LogError("Test failed - message doesn't match");
                    return;
                }
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex);
            }
        }

    }
}
