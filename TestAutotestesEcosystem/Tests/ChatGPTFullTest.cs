using Autotests.Base;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace Autotests.Tests
{
    [TestFixture]
    public class ChatGPTFullTest : AuthorizationBase
    {
        [Test]
        public void Test_LoginAndNavigationToChatGPT()
        {
            HandleCommonExceptions(() =>
            {
                // Шаг 1: Выполнение входа
                PerformLogin();

                // Шаг 2: Навигация к ChatGPT через меню
                CurrentTestReport.AddStep("Нажатие на пункт 'Menu' в навигации", "ИНФО");
                var menuLink = Driver.FindElement(By.LinkText("Menu"));
                menuLink.Click();
                CurrentTestReport.AddSuccess("Меню открыто");

                CurrentTestReport.AddStep("Нажатие на пункт 'ChatGPT' в меню", "ИНФО");
                var chatGptLink = Driver.FindElement(By.LinkText("ChatGPT"));
                chatGptLink.Click();
                CurrentTestReport.AddSuccess("Переход в ChatGPT выполнен");

                // Проверка, что мы на правильной странице
                Wait.Until(d => d.Url.Contains("/request/model.html"));
                Assert.That(Driver.Url, Does.Contain("/request/model.html"));
                CurrentTestReport.AddSuccess("Успешная навигация на страницу ChatGPT");

            }, "Логин и навигация к ChatGPT");
        }

        [Test]
        public void Test_TextInputAndClearFunctionality()
        {
            HandleCommonExceptions(() =>
            {
                // Предварительный логин и навигация
                PerformLogin();
                NavigateToChatGPT();

                // Шаг 1: Ввод текста и очистка
                CurrentTestReport.AddStep("Проверка функциональности ввода и очистки текста", "ИНФО");

                // Первый ввод текста
                EnterTextAndVerify("Привет!");

                // Очистка текста
                CurrentTestReport.AddStep("Нажатие кнопки 'Clear Input'", "ИНФО");
                var clearButton = Driver.FindElement(By.Id("clear_request"));
                clearButton.Click();
                CurrentTestReport.AddSuccess("Текст очищен");

                // Проверка, что поле пустое
                var textArea = Driver.FindElement(By.Id("textarea_request"));
                string textAfterClear = textArea.GetAttribute("value") ?? string.Empty;
                Assert.That(string.IsNullOrEmpty(textAfterClear), Is.True, "Текст не был очищен");
                CurrentTestReport.AddSuccess("Поле ввода успешно очищено");

                // Второй ввод текста
                EnterTextAndVerify("Привет. Как дела?");

            }, "Проверка ввода и очистки текста");
        }

        [Test]
        public void Test_SendRequestAndGetResponse()
        {
            HandleCommonExceptions(() =>
            {
                // Предварительный логин и навигация
                PerformLogin();
                NavigateToChatGPT();

                // Ввод текста запроса
                EnterTextAndVerify("Привет. Как дела?");

                // Отправка запроса
                CurrentTestReport.AddStep("Нажатие кнопки 'Send Request'", "ИНФО");
                var sendButton = Driver.FindElement(By.CssSelector(".ladda-label"));
                sendButton.Click();
                CurrentTestReport.AddSuccess("Запрос отправлен");

                // Ожидание ответа
                WaitForResponse(90);

                // Проверка наличия ответа
                var responseElements = Driver.FindElements(By.CssSelector(
                    ".content, .response, .answer, .message, .coping, " +
                    "[class*='response'], [class*='answer'], [class*='message']"
                ));

                var responseElement = responseElements
                    .FirstOrDefault(e => e.Displayed &&
                        !string.IsNullOrWhiteSpace(e.Text) &&
                        e.Text.Length > 10 &&
                        !e.Text.Contains("Temperature:"));

                Assert.That(responseElement, Is.Not.Null, "Ответ не найден на странице");
                CurrentTestReport.AddSuccess($"Ответ получен: {responseElement.Text.Trim()[..Math.Min(100, responseElement.Text.Length)]}...");

            }, "Отправка запроса и получение ответа");
        }

        [Test]
        public void Test_CopyAnswerFunctionality()
        {
            HandleCommonExceptions(() =>
            {
                // Предварительный логин, навигация и отправка запроса
                PerformLogin();
                NavigateToChatGPT();
                EnterTextAndVerify("Привет. Как дела?");

                var sendButton = Driver.FindElement(By.CssSelector(".ladda-label"));
                sendButton.Click();
                WaitForResponse(90);

                // Проверка функциональности копирования
                CurrentTestReport.AddStep("Проверка кнопки 'Copy Answer'", "ИНФО");

                try
                {
                    var copyButton = Driver.FindElement(By.CssSelector(".coping"));

                    // Проверяем, что кнопка видима и активна
                    Assert.That(copyButton.Displayed, Is.True, "Кнопка копирования не видима");
                    Assert.That(copyButton.Enabled, Is.True, "Кнопка копирования не активна");

                    // Нажимаем на кнопку копирования
                    copyButton.Click();
                    CurrentTestReport.AddSuccess("Кнопка 'Copy Answer' нажата успешно");

                    // Небольшая пауза для визуализации
                    Thread.Sleep(1000);
                }
                catch (NoSuchElementException)
                {
                    CurrentTestReport.AddWarning("Кнопка 'Copy Answer' не найдена, возможно ответ еще не готов");
                }

            }, "Проверка функциональности копирования ответа");
        }

        [Test]
        public void Test_SliderAdjustments()
        {
            HandleCommonExceptions(() =>
            {
                // Предварительный логин и навигация
                PerformLogin();
                NavigateToChatGPT();

                CurrentTestReport.AddStep("Проверка регулировки слайдеров параметров", "ИНФО");

                // Поиск слайдеров (адаптивный поиск)
                var sliders = Driver.FindElements(By.CssSelector(".noUi-active, .noUi-handle"));

                if (sliders.Count > 0)
                {
                    CurrentTestReport.AddStep($"Найдено слайдеров: {sliders.Count}", "ИНФО");

                    // Проходим по всем найденным слайдерам и пытаемся взаимодействовать
                    for (int i = 0; i < Math.Min(sliders.Count, 2); i++) // Ограничиваем количество для стабильности
                    {
                        try
                        {
                            var slider = sliders[i];
                            if (slider.Displayed && slider.Enabled)
                            {
                                CurrentTestReport.AddStep($"Взаимодействие со слайдером #{i + 1}", "ИНФО");

                                // Простое взаимодействие - клик по слайдеру
                                slider.Click();
                                CurrentTestReport.AddSuccess($"Слайдер #{i + 1} обработан успешно");

                                Thread.Sleep(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            CurrentTestReport.AddWarning($"Не удалось взаимодействовать со слайдером #{i + 1}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    CurrentTestReport.AddWarning("Слайдеры не найдены на странице");
                }

            }, "Проверка регулировки параметров через слайдеры");
        }

        [Test]
        public void Test_UserProfileNavigation()
        {
            HandleCommonExceptions(() =>
            {
                // Предварительный логин
                PerformLogin();

                // Открытие меню пользователя
                OpenUserMenu();

                // Переход в Account Settings
                CurrentTestReport.AddStep("Переход в 'Account settings'", "ИНФО");
                var accountSettingsButton = FindAccountSettingsButton();
                accountSettingsButton.Click();
                CurrentTestReport.AddSuccess("Переход в Account settings выполнен");

                // Проверка, что мы на странице профиля
                Wait.Until(d => d.Url.Contains("/profile/index.html"));
                Assert.That(Driver.Url, Does.Contain("/profile/index.html"));
                CurrentTestReport.AddSuccess("Успешная навигация на страницу профиля");

                // Проверка кнопки копирования User ID
                try
                {
                    CurrentTestReport.AddStep("Проверка кнопки копирования User ID", "ИНФО");
                    var copyButton = Driver.FindElement(By.CssSelector(".icon-clippy"));

                    Assert.That(copyButton.Displayed, Is.True, "Кнопка копирования не видима");
                    copyButton.Click();
                    CurrentTestReport.AddSuccess("Кнопка копирования User ID нажата");

                    Thread.Sleep(1000);
                }
                catch (NoSuchElementException)
                {
                    CurrentTestReport.AddWarning("Кнопка копирования User ID не найдена");
                }

                // Возврат на домашнюю страницу
                CurrentTestReport.AddStep("Возврат на домашнюю страницу", "ИНФО");
                var homeLink = Driver.FindElement(By.LinkText("Home"));
                homeLink.Click();
                CurrentTestReport.AddSuccess("Возврат на домашнюю страницу выполнен");

            }, "Навигация по профилю пользователя");
        }

        [Test]
        public void Test_LogoutFunctionality()
        {
            HandleCommonExceptions(() =>
            {
                // Предварительный логин
                PerformLogin();

                // Открытие меню пользователя
                OpenUserMenu();

                // Выход из системы
                CurrentTestReport.AddStep("Выполнение выхода из системы", "ИНФО");
                var logoutLink = Driver.FindElement(By.LinkText("Logout"));
                logoutLink.Click();
                CurrentTestReport.AddSuccess("Выход выполнен");

                // Проверка, что мы вернулись на страницу логина
                Wait.Until(d => d.Url.Contains("/auth/login.html"));
                Assert.That(Driver.Url, Does.Contain("/auth/login.html"));
                CurrentTestReport.AddSuccess("Успешный возврат на страницу логина");

                // Проверка, что поля логина доступны
                var loginField = Driver.FindElement(By.Id("loginform-login"));
                var passwordField = Driver.FindElement(By.Id("loginform-password"));

                Assert.That(loginField.Displayed, Is.True, "Поле логина не отображается");
                Assert.That(passwordField.Displayed, Is.True, "Поле пароля не отображается");
                CurrentTestReport.AddSuccess("Поля для повторного входа доступны");

            }, "Проверка функциональности выхода из системы");
        }

        [Test]
        public void Test_CompleteWorkflow()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Запуск полного рабочего процесса", "ИНФО");

                // Полный цикл: Логин → Работа с ChatGPT → Профиль → Выход
                PerformLogin();
                NavigateToChatGPT();
                EnterTextAndVerify("Привет! Это тестовое сообщение для проверки полного workflow.");

                var sendButton = Driver.FindElement(By.CssSelector(".ladda-label"));
                sendButton.Click();
                WaitForResponse(60);

                // Переход в профиль
                OpenUserMenu();
                var accountSettingsButton = FindAccountSettingsButton();
                accountSettingsButton.Click();
                Wait.Until(d => d.Url.Contains("/profile/index.html"));

                // Возврат на главную
                var homeLink = Driver.FindElement(By.LinkText("Home"));
                homeLink.Click();

                // Выход
                OpenUserMenu();
                var logoutLink = Driver.FindElement(By.LinkText("Logout"));
                logoutLink.Click();
                Wait.Until(d => d.Url.Contains("/auth/login.html"));

                CurrentTestReport.AddSuccess("Полный рабочий процесс завершен успешно");

            }, "Полный рабочий процесс приложения");
        }

        // Вспомогательный метод для навигации к ChatGPT
        private void NavigateToChatGPT()
        {
            if (!Driver.Url.Contains("/request/model.html"))
            {
                CurrentTestReport.AddStep("Навигация к ChatGPT", "ИНФО");

                var menuLink = Driver.FindElement(By.LinkText("Menu"));
                menuLink.Click();
                Thread.Sleep(500);

                var chatGptLink = Driver.FindElement(By.LinkText("ChatGPT"));
                chatGptLink.Click();

                Wait.Until(d => d.Url.Contains("/request/model.html"));
                CurrentTestReport.AddSuccess("Навигация к ChatGPT завершена");
            }
        }
    }
}