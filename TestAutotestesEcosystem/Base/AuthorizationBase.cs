using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace Autotests.Base
{
    public abstract class AuthorizationBase : BaseTest
    {
        protected const string ValidLogin = "v_shutenko";
        protected const string ValidPassword = "8nEThznM";
        protected const string TestLoginUrl = "https://ai-ecosystem-test.janusww.com:9999/auth/login.html";
        protected const string ProdLoginUrl = "https://ai-ecosystem-test.janusww.com:9999/auth/login.html";
        protected const string TestChatGPTUrl = "https://ai-ecosystem-test.janusww.com:9999/request/model.html";
        protected const string ExpectedUserId = "b906170e-d802-4a11-b3a5-f22714f854ba";

        // Для ВКЛЮЧЕНИЯ визуализации (браузер видимый):
        protected override string HeadlessOption => "off";

        protected void HandleCommonExceptions(Action testAction, string testName)
        {
            try
            {
                CurrentTestReport.AddStep($"Начало выполнения теста: {testName}", "ИНФО");
                testAction();
                CurrentTestReport.AddSuccess($"ТЕСТ ПРОЙДЕН: {testName}");

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError($"ТЕСТ НЕ ПРОЙДЕН: {testName} - {ex.Message}", ex);
                CurrentTestReport.AddStep($"Текущий URL: {Driver.Url}", "ИНФО");
                TakeScreenshot(testName + "_error");
                throw;
            }
        }

        protected void PerformLogin(string? loginUrl = null)
        {
            string url = loginUrl ?? TestLoginUrl;
            CurrentTestReport.AddStep($"Переход на страницу входа: {url}", "ИНФО");

            try
            {
                Driver.Navigate().GoToUrl(url);
                Wait.Until(d => d.FindElement(By.Id("loginform-login")).Displayed);
                CurrentTestReport.AddSuccess("Страница входа успешно загружена");

                CurrentTestReport.AddStep("Ввод логина...", "ИНФО");
                var loginField = Driver.FindElement(By.Id("loginform-login"));
                loginField.SendKeys(ValidLogin);
                CurrentTestReport.AddSuccess($"Логин '{ValidLogin}' введен успешно");

                CurrentTestReport.AddStep("Ввод пароля...", "ИНФО");
                var passwordField = Driver.FindElement(By.Id("loginform-password"));
                passwordField.SendKeys(ValidPassword);
                CurrentTestReport.AddSuccess("Пароль введен успешно");

                CurrentTestReport.AddStep("Нажатие кнопки входа...", "ИНФО");
                var loginButton = Driver.FindElement(By.CssSelector(".icon-circle-right2"));
                loginButton.Click();
                CurrentTestReport.AddSuccess("Кнопка входа нажата");

                Wait.Until(d => d.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user"))
                    .Any(e => e.Displayed));

                CurrentTestReport.AddSuccess("Вход выполнен успешно");
                CurrentTestReport.AddStep($"Текущий URL после входа: {Driver.Url}", "ИНФО");

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Ошибка при выполнении логина", ex);
                throw;
            }
        }

        protected void OpenUserMenu()
        {
            CurrentTestReport.AddStep("Поиск меню пользователя...", "ИНФО");

            try
            {
                var userMenu = Driver.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user, [data-toggle='dropdown'], .user-name"))
                    .FirstOrDefault(e => e.Displayed && e.Text.Contains("Vitaliy", StringComparison.OrdinalIgnoreCase));

                if (userMenu == null)
                {
                    CurrentTestReport.AddWarning("Меню пользователя 'Vitaliy' не найдено, поиск альтернативного меню");
                    userMenu = Driver.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user"))
                        .FirstOrDefault(e => e.Displayed);
                }

                Assert.That(userMenu, Is.Not.Null, "Меню пользователя 'Vitaliy Shutenko' не найдено");
                CurrentTestReport.AddSuccess("Меню пользователя найдено");

                CurrentTestReport.AddStep("Открытие меню пользователя...", "ИНФО");
                userMenu!.Click();
                CurrentTestReport.AddSuccess("Меню пользователя открыто");

                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Ошибка при открытии меню пользователя", ex);
                throw;
            }
        }

        protected IWebElement FindAccountSettingsButton()
        {
            CurrentTestReport.AddStep("Поиск кнопки Account Settings...", "ИНФО");

            try
            {
                var accountSettingsButton = Driver.FindElements(By.LinkText("Account settings"))
                    .FirstOrDefault(e => e.Displayed);

                if (accountSettingsButton == null)
                {
                    CurrentTestReport.AddWarning("Кнопка 'Account settings' не найдена по точному тексту, поиск по частичному совпадению");
                    accountSettingsButton = Driver.FindElements(By.PartialLinkText("Account"))
                        .FirstOrDefault(e => e.Displayed && e.Text.Contains("Account", StringComparison.OrdinalIgnoreCase));
                }

                Assert.That(accountSettingsButton, Is.Not.Null, "Кнопка 'Account Settings' не найдена в меню");
                Assert.That(accountSettingsButton!.Enabled, Is.True, "Кнопка 'Account Settings' не активна");

                CurrentTestReport.AddSuccess("Кнопка 'Account Settings' найдена и активна");
                return accountSettingsButton!;
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Ошибка при поиске кнопки Account Settings", ex);
                throw;
            }
        }

        protected void EnterTextAndVerify(string text, string elementId = "textarea_request")
        {
            CurrentTestReport.AddStep($"Ввод текста: {text}", "ИНФО");

            try
            {
                var textArea = Driver.FindElement(By.Id(elementId));
                textArea.Click();
                textArea.Clear();
                textArea.SendKeys(text);

                string enteredText = textArea.GetAttribute("value") ?? string.Empty;
                Assert.That(enteredText, Is.EqualTo(text), $"Текст не был введен корректно: ожидалось '{text}', получено '{enteredText}'");

                CurrentTestReport.AddSuccess($"Текст '{text}' успешно введен и проверен");

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Ошибка при вводе текста", ex);
                throw;
            }
        }

        protected void WaitForResponse(int timeoutSeconds = 90)
        {
            CurrentTestReport.AddStep($"Ожидание ответа (таймаут: {timeoutSeconds} секунд)...", "ИНФО");

            try
            {
                bool responseReceived = false;
                DateTime startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds && !responseReceived)
                {
                    try
                    {
                        // Ищем любой контент в области ответа, кроме заглушки
                        var responseDiv = Driver.FindElement(By.Id("response_div"));
                        var responseElements = responseDiv.FindElements(By.CssSelector("*"));

                        var meaningfulContent = responseElements
                            .Where(e => e.Displayed &&
                                      !string.IsNullOrWhiteSpace(e.Text) &&
                                      e.Text.Length > 10 &&
                                      !e.Text.Contains("Your answer will be shown here") &&
                                      !e.Text.Contains("Temperature:") &&
                                      !e.Text.Contains("Copy Answer"))
                            .ToList();

                        if (meaningfulContent.Any())
                        {
                            responseReceived = true;
                            var firstContent = meaningfulContent.First();
                            CurrentTestReport.AddSuccess($"Ответ получен: {firstContent.Text.Trim()[..Math.Min(50, firstContent.Text.Length)]}...");
                            break;
                        }

                        // Проверяем, исчезли ли индикаторы загрузки
                        var loadingIndicators = Driver.FindElements(By.CssSelector(
                            ".ladda-spinner, .loading, .spinner, [class*='loading'], .btn-ladda-spinner"
                        ));

                        bool isLoading = loadingIndicators.Any(e => e.Displayed && e.GetAttribute("style")?.Contains("display: none") == false);

                        if (!isLoading)
                        {
                            // Проверяем, есть ли хотя бы кнопка копирования (признак полученного ответа)
                            var copyButton = Driver.FindElements(By.CssSelector(".coping"));
                            if (copyButton.Any(e => e.Displayed))
                            {
                                responseReceived = true;
                                CurrentTestReport.AddSuccess("Индикаторы загрузки исчезли, ответ предположительно получен");
                                break;
                            }
                        }

                        Thread.Sleep(2000);
                        CurrentTestReport.AddStep("Ожидание ответа... (2 секунды прошло)", "ИНФО");
                    }
                    catch (Exception ex)
                    {
                        CurrentTestReport.AddWarning($"Исключение при ожидании ответа: {ex.Message}");
                        Thread.Sleep(2000);
                    }
                }

                if (!responseReceived)
                {
                    throw new Exception($"Ответ не получен в течение {timeoutSeconds} секунд");
                }
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddError("Ошибка при ожидании ответа", ex);
                throw;
            }
        }

        protected void WaitForButtonToBecomeEnabled(IWebElement button, int timeoutSeconds = 30)
        {
            CurrentTestReport.AddStep($"Ожидание активации кнопки (таймаут: {timeoutSeconds} секунд)...", "ИНФО");

            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                if (button.Enabled)
                {
                    CurrentTestReport.AddSuccess("Кнопка снова активна");
                    return;
                }
                Thread.Sleep(1000);
            }

            throw new Exception($"Кнопка не стала активной в течение {timeoutSeconds} секунд");
        }

        protected void InteractWithSlider(string sliderClass)
        {
            try
            {
                var slider = Driver.FindElement(By.CssSelector($".{sliderClass}"));
                var handle = slider.FindElement(By.CssSelector(".noUi-handle"));

                // Просто кликаем на слайдер, чтобы активировать его
                handle.Click();
                Thread.Sleep(500);

                CurrentTestReport.AddSuccess($"Слайдер {sliderClass} активирован");
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddWarning($"Не удалось взаимодействовать со слайдером {sliderClass}: {ex.Message}");
            }
        }
    }
}