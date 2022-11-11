using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using OpenQA.Selenium;

namespace Tests;

[TestFixture]
public class UnitTests : BaseTest<UnitTests>
{
    [Test]
    public void Test1()
    {
        using var scope = Logger.BeginScope(new Dictionary<string, object> {
            { "TestName", TestContext.CurrentContext.Test.Name },
            { "FullName", TestContext.CurrentContext.Test.FullName }
        });

        Logger.LogInformation("Testing selenium has Begun");
        DateTime start = DateTime.UtcNow;

        Logger.LogInformation("We will now attempt selenium test");

        Browser.Navigate().GoToUrl("https://www.selenium.dev/selenium/web/web-form.html");
        Logger.LogInformation("The Website we are on is {0}", Browser.Url);

        Browser.Title.Should().Be("Web form");

        Browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);


        var textBox = Browser.FindElement(By.Name("my-text"));
        var submitButton = Browser.FindElement(By.CssSelector("button"));

        textBox.SendKeys("Selenium");
        submitButton.Click();

        var message = Browser.FindElement(By.Id("message"));

        message.Text.Should().Be("Received!");
    }

    [Test]
    public void Test2_Failing()
    {
        using var scope = Logger.BeginScope(new Dictionary<string, object> {
            { "TestName", "Selenium.fail" }
        });
        Logger.LogInformation("Testing selenium has Begun");
        DateTime start = DateTime.UtcNow;

        Logger.LogInformation("We will now attempt selenium test");

        Browser.Navigate().GoToUrl("https://www.selenium.dev/selenium/web/web-form.html");
        Logger.LogInformation("The Website we are on is {0}", Browser.Url);

        Browser.Title.Should().Be("Web form");

        Browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

        Console.WriteLine("hello");

        var textBox = Browser.FindElement(By.Name("my-text"));
        var submitButton = Browser.FindElement(By.CssSelector("button"));

        textBox.SendKeys("Selenium");
        submitButton.Click();

        var message = Browser.FindElement(By.Id("message"));

        message.Text.Should().Be("This is going to fail");
    }


}