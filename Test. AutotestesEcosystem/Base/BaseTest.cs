using Autotests_ai_ecosystem;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace Autotests_ai_ecosystem.Base
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WebDriverWait Wait { get; private set; } = null!;
        protected TestReport CurrentTestReport { get; private set; } = null!;

        protected virtual string HeadlessOption => "off";

        [SetUp]
        public void Setup()
        {
            // Инициализация отчета
            CurrentTestReport = new TestReport(TestContext.CurrentContext.Test.Name);
            CurrentTestReport.AddStep("Начало настройки тестового окружения", "ИНФО");

            var chromeOptions = new ChromeOptions();

            if (HeadlessOption == "on")
            {
                chromeOptions.AddArgument("--headless");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AddArgument("--window-size=1920,1080");
                CurrentTestReport.AddStep("Режим headless включен", "ИНФО");
            }
            else
            {
                chromeOptions.AddArgument("--start-maximized");
                CurrentTestReport.AddStep("Режим headless выключен, браузер будет развернут", "ИНФО");
            }

            chromeOptions.AddArgument("--disable-notifications");
            chromeOptions.AddArgument("--disable-extensions");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("--disable-web-security");
            chromeOptions.AddArgument("--allow-running-insecure-content");

            try
            {
                Driver = new ChromeDriver(chromeOptions);
                Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

                CurrentTestReport.AddSuccess("Драйвер Chrome успешно инициализирован");
                CurrentTestReport.AddStep($"Неявное ожидание: {Driver.Manage().Timeouts().ImplicitWait.TotalSeconds} сек", "ИНФО");
                CurrentTestReport.AddStep($"Ожидание загрузки страницы: {Driver.Manage().Timeouts().PageLoad.TotalSeconds} сек", "ИНФО");
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Ошибка инициализации ChromeDriver", ex);
                throw;
            }
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                // Добавляем информацию о текущем URL перед завершением
                if (Driver != null)
                {
                    CurrentTestReport.AddUrlInfo(Driver.Url);
                }

                // Создаем скриншот при неудачном тесте
                if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    var screenshotPath = TakeScreenshot(TestContext.CurrentContext.Test.Name);
                    CurrentTestReport.AddScreenshotInfo(screenshotPath);
                    CurrentTestReport.AddError($"Тест завершился с ошибкой: {TestContext.CurrentContext.Result.Message}");
                }
                else
                {
                    CurrentTestReport.AddSuccess("Тест завершен успешно");
                }

                // Добавляем информацию о результате теста
                var result = TestContext.CurrentContext.Result;
                CurrentTestReport.AddTestData("Статус теста", result.Outcome.Status.ToString());

                if (!string.IsNullOrEmpty(result.Message))
                {
                    CurrentTestReport.AddTestData("Сообщение", result.Message);
                }

                if (!string.IsNullOrEmpty(result.StackTrace))
                {
                    CurrentTestReport.AddTestData("Стек вызовов", result.StackTrace);
                }
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Ошибка в методе TearDown", ex);
            }
            finally
            {
                try
                {
                    // Финализируем отчет
                    CurrentTestReport.FinalizeReport();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при финализации отчета: {ex.Message}");
                }

                try
                {
                    // Закрываем драйвер
                    Driver?.Quit();
                    Driver?.Dispose();
                    CurrentTestReport.AddStep("Драйвер браузера закрыт", "ИНФО");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при закрытии драйвера: {ex.Message}");
                }
            }
        }

        protected string TakeScreenshot(string testName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                var fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var screenshotsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Screenshots");
                var screenshotPath = Path.Combine(screenshotsDirectory, fileName);

                Directory.CreateDirectory(screenshotsDirectory);
                screenshot.SaveAsFile(screenshotPath);

                TestContext.AddTestAttachment(screenshotPath);
                CurrentTestReport.AddStep($"Скриншот сохранен: {screenshotPath}", "СКРИНШОТ");

                return screenshotPath;
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Не удалось сделать скриншот", ex);
                return string.Empty;
            }
        }

        protected static void MarkTestAsFailed()
        {
            Console.WriteLine($"ТЕСТ ПРОВАЛЕН: {TestContext.CurrentContext.Test.Name}");
        }

        protected void SetHeadlessMode(string mode)
        {
            CurrentTestReport.AddStep($"Запрошено изменение headless режима на: {mode}", "ИНФО");
        }

        protected void LogInfo(string message)
        {
            CurrentTestReport.AddStep(message, "ИНФО");
        }

        protected void LogSuccess(string message)
        {
            CurrentTestReport.AddSuccess(message);
        }

        protected void LogWarning(string message)
        {
            CurrentTestReport.AddWarning(message);
        }

        protected void LogError(string message, Exception ex = null!)
        {
            CurrentTestReport.AddError(message, ex);
        }
    }
}