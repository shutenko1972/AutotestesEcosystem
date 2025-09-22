using Autotests.Base;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Autotests.Tests.Navigation
{
    [TestFixture]
    [Category("Navigation")]
    [Category("Smoke")]
    [Category("UserInterface")]
    [Category("CriticalPath")]
    public class NavigationFlowValidation : AuthorizationBase
    {
        private const string OptimizedWindowSize = "1936, 1048";

        [Test]
        [Order(1)]
        [Description("Verify Menu navigation to ChatGPT functionality")]
        public void VerifyMenuNavigationToChatGPT()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест навигации Menu -> ChatGPT", "INFO");

                PerformLogin();
                SetBrowserSize();

                var menuLink = Wait.Until(d => d.FindElement(By.LinkText("Menu")));
                menuLink.Click();
                CurrentTestReport.AddStep("Menu открыто", "DEBUG");

                var chatGptLink = Wait.Until(d => d.FindElement(By.LinkText("ChatGPT")));
                chatGptLink.Click();
                CurrentTestReport.AddStep("ChatGPT выбран", "DEBUG");

                Assert.That(Driver.Url, Does.Not.Contain("login"), "Должны остаться аутентифицированными");
                CurrentTestReport.AddSuccess("Навигация Menu -> ChatGPT завершена успешно");

            }, "Тест навигации Menu -> ChatGPT");
        }

        [Test]
        [Order(2)]
        [Description("Verify user dropdown navigation to Account Settings")]
        public void VerifyUserDropdownToAccountSettings()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест навигации User Dropdown -> Account Settings", "INFO");

                PerformLogin();
                SetBrowserSize();

                OpenUserDropdown();

                var accountSettingsLink = Wait.Until(d => d.FindElement(By.LinkText("Account settings")));
                accountSettingsLink.Click();
                CurrentTestReport.AddStep("Account Settings выбран", "DEBUG");

                Wait.Until(d => d.Url.Contains("profile/index.html"));
                Assert.That(Driver.Url, Does.Contain("profile/index.html"), "Должны быть на странице профиля");
                CurrentTestReport.AddSuccess("Навигация к Account Settings завершена успешно");

            }, "Тест навигации к Account Settings");
        }

        [Test]
        [Order(3)]
        [Description("Verify clipboard functionality on Account Settings page")]
        public void VerifyAccountSettingsClipboardFunctionality()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест функциональности буфера обмена", "INFO");

                PerformLogin();
                SetBrowserSize();
                NavigateToAccountSettings();

                var clipboardButton = Wait.Until(d => d.FindElement(By.CssSelector(".icon-clippy")));
                Assert.That(clipboardButton.Displayed, "Кнопка буфера обмена должна отображаться");
                Assert.That(clipboardButton.Enabled, "Кнопка буфера обмена должна быть активна");

                clipboardButton.Click();
                CurrentTestReport.AddStep("Функция буфера обмена активирована", "DEBUG");
                CurrentTestReport.AddSuccess("Тест буфера обмена завершен успешно");

            }, "Тест функциональности буфера обмена");
        }

        [Test]
        [Order(4)]
        [Description("Verify navigation from Account Settings back to Home")]
        public void VerifyAccountSettingsToHomeNavigation()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест навигации Account Settings -> Home", "INFO");

                PerformLogin();
                SetBrowserSize();
                NavigateToAccountSettings();

                var homeLink = Wait.Until(d => d.FindElement(By.LinkText("Home")));
                homeLink.Click();
                CurrentTestReport.AddStep("Home выбран", "DEBUG");

                Wait.Until(d => d.Url.Contains("model.html"));
                Assert.That(Driver.Url, Does.Contain("model.html"), "Должны вернуться на главную страницу");
                CurrentTestReport.AddSuccess("Навигация к Home завершена успешно");

            }, "Тест навигации к Home");
        }

        [Test]
        [Order(5)]
        [Description("Verify text input functionality on model page")]
        public void VerifyModelPageTextInputFunctionality()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест ввода текста на странице модели", "INFO");

                PerformLogin();
                SetBrowserSize();

                var textArea = Wait.Until(d => d.FindElement(By.Id("textarea_request")));

                textArea.Click();
                textArea.Clear();

                string testText = "b906170e-d802-4a11-b3a5-f22714f854ba";
                textArea.SendKeys(testText);
                CurrentTestReport.AddStep($"Текст '{MaskSensitiveData(testText)}' введен", "DEBUG");

                Assert.That(textArea.GetAttribute("value"), Is.EqualTo(testText),
                    "Введенный текст должен соответствовать ожидаемому");
                CurrentTestReport.AddSuccess("Текст корректно отображается в поле");

            }, "Тест ввода текста");
        }

        [Test]
        [Order(6)]
        [Description("Verify AI Ecosystem and Home navigation flow")]
        public void VerifyAIEcosystemAndHomeNavigation()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест навигации AI Ecosystem -> Home", "INFO");

                PerformLogin();
                SetBrowserSize();

                var ecosystemLink = Wait.Until(d => d.FindElement(By.LinkText("AI - ecosystem")));
                ecosystemLink.Click();
                CurrentTestReport.AddStep("AI Ecosystem выбран", "DEBUG");

                var homeLink = Wait.Until(d => d.FindElement(By.LinkText("Home")));
                homeLink.Click();
                CurrentTestReport.AddStep("Home выбран", "DEBUG");

                Assert.That(Driver.Url, Does.Contain("model.html"), "Должны быть на главной странице");
                CurrentTestReport.AddSuccess("Навигация AI Ecosystem -> Home завершена успешно");

            }, "Тест навигации AI Ecosystem -> Home");
        }

        [Test]
        [Order(7)]
        [Description("Verify complete user dropdown navigation flow")]
        public void VerifyCompleteUserDropdownNavigation()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест полной навигации через User Dropdown", "INFO");

                PerformLogin();
                SetBrowserSize();

                OpenUserDropdown();

                var accountSettingsLink = Wait.Until(d => d.FindElement(By.LinkText("Account settings")));
                accountSettingsLink.Click();
                CurrentTestReport.AddStep("Account Settings выбран", "DEBUG");

                Wait.Until(d => d.Url.Contains("profile/index.html"));

                var ecosystemLink = Wait.Until(d => d.FindElement(By.LinkText("AI - ecosystem")));
                ecosystemLink.Click();
                CurrentTestReport.AddStep("AI Ecosystem выбран", "DEBUG");

                Assert.That(Driver.Url, Does.Not.Contain("login"), "Должны остаться аутентифицированными");
                CurrentTestReport.AddSuccess("Полная навигация завершена успешно");

            }, "Тест полной навигации через User Dropdown");
        }

        #region Helper Methods

        private void SetBrowserSize()
        {
            Driver.Manage().Window.Size = new System.Drawing.Size(1936, 1048);
            CurrentTestReport.AddStep($"Размер окна установлен: {OptimizedWindowSize}", "DEBUG");
        }

        private void OpenUserDropdown()
        {
            var dropdownToggle = Wait.Until(d => d.FindElement(By.CssSelector(".dropdown-toggle > span:nth-child(1)")));
            dropdownToggle.Click();
            CurrentTestReport.AddStep("User dropdown открыт", "DEBUG");
        }

        private void NavigateToAccountSettings()
        {
            OpenUserDropdown();

            var accountSettingsLink = Wait.Until(d => d.FindElement(By.LinkText("Account settings")));
            accountSettingsLink.Click();
            Wait.Until(d => d.Url.Contains("profile/index.html"));
            CurrentTestReport.AddStep("На странице Account Settings", "DEBUG");
        }

        private static string MaskSensitiveData(string sensitiveData)
        {
            if (string.IsNullOrEmpty(sensitiveData))
                return "******";

            return sensitiveData.Length <= 4 ?
                new string('*', sensitiveData.Length) :
                sensitiveData[..2] + new string('*', sensitiveData.Length - 4) + sensitiveData[^2..];
        }

        [TearDown]
        public void NavigationTestCleanup()
        {
            CurrentTestReport.AddStep("Завершение тестов навигации", "INFO");
        }

        #endregion
    }
}