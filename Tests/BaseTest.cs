using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Reflection;

namespace Tests
{
    public abstract class BaseTest<T>
    {
        private ILogger<T> _logger;
        private IWebDriver _browser;

        protected ILogger<T> Logger => _logger;
        protected IWebDriver Browser => _browser;

        [OneTimeSetUp]
        public void Setup()
        {
            // setup logging
            _logger = LoggingSetup.GetLogger<T>();

            //TestContext.Error = new LoggingSetup.LoggerRedirect(_logger, LogLevel.Error);

            // chromedriver for linux, chromedriver.exe for windows
            string chromeDriverName = "chromedriver" + (Environment.OSVersion.Platform == PlatformID.Win32NT ? ".exe" : "");

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");

            // this can possibly be simplified - we're searching for chrome driver
            var whereToSearch = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
            var chromeFile = SearchAccessibleFiles(whereToSearch, chromeDriverName, _logger);

            if (chromeFile is null)
            {
                throw new FileNotFoundException($"Could not find {chromeDriverName} in the current directory or any of its subdirectories.");
            }
            else
            {
                _logger.LogInformation($"Found {chromeDriverName} at {chromeFile}. Will provide parent folder to Driver {Directory.GetParent(chromeFile).FullName}");
            }

            _browser = new ChromeDriver(Directory.GetParent(chromeFile).FullName, chromeOptions);

            var options = _browser.Manage();
            var timeouts = options.Timeouts();
            timeouts.PageLoad = TimeSpan.FromSeconds(5);

            var _ = new WebDriverWait(_browser, TimeSpan.FromSeconds(5));
        }

        [OneTimeTearDown]
        public void GlobbalTearDown()
        {
            _browser.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object> {
            { "TestName", TestContext.CurrentContext.Test.Name },
            { "FullName", TestContext.CurrentContext.Test.FullName }
        });
            var result = TestContext.CurrentContext.Result;

            switch (result.Outcome.Status)
            {
                case NUnit.Framework.Interfaces.TestStatus.Passed:
                    _logger.LogInformation("Test Passed");
                    break;
                case NUnit.Framework.Interfaces.TestStatus.Inconclusive:
                    _logger.LogWarning("Test Result was Inconclusive");

                    break;
                case NUnit.Framework.Interfaces.TestStatus.Skipped:
                    _logger.LogInformation("Test was skipped");

                    break;
                case NUnit.Framework.Interfaces.TestStatus.Failed:
                    _logger.LogError(new AssertionException(result.Message), "Test Failed. Error:{error}, StackTrace:{stackTrace}", result.Message, result.StackTrace);
                    break;
            }
        }

        private string? SearchAccessibleFiles(string root, string fileName, ILogger<T> log)
        {
            _logger.LogInformation($"Searching for {fileName} in {root}.");
            var file = Directory.EnumerateFiles(root).FirstOrDefault(m => m.EndsWith(fileName));

            if (file is not null)
            {
                return file;
            }

            foreach (var subDir in Directory.EnumerateDirectories(root))
            {
                try
                {
                    var fileFound = SearchAccessibleFiles(subDir, fileName, log);

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
