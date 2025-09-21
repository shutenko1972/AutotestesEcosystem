using Autotests.Base;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Autotests.Tests
{
    [TestFixture]
    [Category("Authorization")]
    [Category("Smoke")]
    [Category("CriticalPath")]
    public class BaseAuthenticationFlowSessionValidation : AuthorizationBase
    {
        [Test]
        [Order(1)]
        public void SuccessfulUserAuthentication_WithValidCredentials_FromBaseConfiguration()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep($"Инициализация процесса аутентификации", "INFO");
                CurrentTestReport.AddStep($"Учетные данные: Login={ValidLogin}, Password={MaskSensitiveData(ValidPassword)}", "DEBUG");

                // Шаг 1: Навигация на страницу аутентификации
                CurrentTestReport.AddStep("Навигация на страницу аутентификации", "INFO");
                Driver.Navigate().GoToUrl(TestLoginUrl);
                Wait.Until(d => d.FindElement(By.Id("login-form")).Displayed);
                CurrentTestReport.AddSuccess("Страница аутентификации успешно загружена");

                // Шаг 2: Оптимизация окна браузера
                CurrentTestReport.AddStep("Конфигурация размеров окна браузера", "INFO");
                Driver.Manage().Window.Size = new System.Drawing.Size(1936, 1048);
                CurrentTestReport.AddSuccess("Размеры окна браузера оптимизированы");

                // Шаг 3: Ввод учетных данных
                ExecuteCredentialInputSequence();

                // Шаг 4: Инициализация процесса аутентификации
                CurrentTestReport.AddStep("Инициация процесса аутентификации", "INFO");
                var loginButton = Driver.FindElement(By.Name("login-button"));
                SimulateUserInteraction(loginButton);
                loginButton.Click();
                CurrentTestReport.AddSuccess("Запрос аутентификации отправлен");

                // Шаг 5: Валидация успешной аутентификации
                ValidateAuthenticationSuccess();

                CurrentTestReport.AddSuccess("Аутентификация пользователя выполнена успешно");

            }, "Тест успешной аутентификации с валидными учетными данными");
        }

        [Test]
        [Order(2)]
        public void BaseAuthMethod_ValidatesLoginSuccess()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Инициализация процесса аутентификации через базовый метод", "INFO");

                // Использование базового метода аутентификации
                PerformLogin();

                // Базовая проверка успешного входа
                ValidateBasicLoginSuccess();

                CurrentTestReport.AddSuccess("Аутентификация через базовый метод завершена успешно");

            }, "Тест входа через базовый метод аутентификации");
        }

        [Test]
        [Order(3)]
        public void Test222_LogoutSequence_FromSeleniumIDE()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест последовательности выхода на основе Selenium IDE", "INFO");

                // Шаг 1: Сначала выполняем вход в систему
                CurrentTestReport.AddStep("Предварительная аутентификация пользователя", "INFO");
                PerformLogin();
                CurrentTestReport.AddSuccess("Пользователь успешно аутентифицирован");

                // Шаг 2: Навигация на страницу модели (тестовый URL)
                CurrentTestReport.AddStep("Навигация на тестовую страницу модели", "INFO");
                Driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/request/model.html");

                // Ожидание загрузки страницы
                Wait.Until(d => d.Url.Contains("model.html"));
                CurrentTestReport.AddSuccess("Тестовая страница модели успешно загружена");

                // Шаг 3: Оптимизация окна браузера
                CurrentTestReport.AddStep("Конфигурация размеров окна браузера", "INFO");
                Driver.Manage().Window.Size = new System.Drawing.Size(1936, 1048);
                CurrentTestReport.AddSuccess("Размеры окна браузера оптимизированы");

                // Шаг 4: Ожидание появления меню пользователя
                CurrentTestReport.AddStep("Ожидание загрузки элементов интерфейса", "INFO");
                Wait.Until(d => d.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user, .user-name"))
                    .Any(e => e.Displayed && e.Enabled));
                CurrentTestReport.AddSuccess("Элементы интерфейса загружены");

                // Шаг 5: Клик по выпадающему меню пользователя
                CurrentTestReport.AddStep("Открытие выпадающего меню пользователя", "INFO");

                // Поиск и клик по меню пользователя
                var dropdownToggle = Wait.Until(d =>
                {
                    var elements = d.FindElements(By.CssSelector(".dropdown-toggle > span:nth-child(1)"));
                    return elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                });

                dropdownToggle.Click();
                CurrentTestReport.AddSuccess("Выпадающее меню открыто");

                // Шаг 6: Выбор пункта Logout
                CurrentTestReport.AddStep("Выбор пункта 'Logout' из меню", "INFO");
                var logoutLink = Wait.Until(d =>
                {
                    var links = d.FindElements(By.LinkText("Logout"));
                    return links.FirstOrDefault(e => e.Displayed && e.Enabled);
                });

                logoutLink.Click();
                CurrentTestReport.AddSuccess("Запрос выхода из системы отправлен");

                // Шаг 7: Ожидание перенаправления на страницу логина
                Wait.Until(d => d.Url.Contains("login") || d.FindElements(By.Id("loginform-login")).Any(e => e.Displayed));
                CurrentTestReport.AddSuccess("Перенаправление на страницу логина выполнено");

                // Шаг 8: Валидация успешного выхода
                ValidateLogoutSuccess();

                CurrentTestReport.AddSuccess("Тест последовательности выхода завершен успешно");

            }, "Тест последовательности выхода на основе Selenium IDE");
        }

        [Test]
        [Order(4)]
        public void Test222_FullLogoutAndLoginSequence_FromSeleniumIDE()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Тест полной последовательности выхода и входа на основе Selenium IDE", "INFO");

                // Шаг 1: Сначала выполняем вход в систему
                CurrentTestReport.AddStep("Предварительная аутентификация пользователя", "INFO");
                PerformLogin();
                CurrentTestReport.AddSuccess("Пользователь успешно аутентифицирован");

                // Шаг 2: Навигация на страницу модели (тестовый URL)
                CurrentTestReport.AddStep("Навигация на тестовую страницу модели", "INFO");
                Driver.Navigate().GoToUrl("https://ai-ecosystem-test.janusww.com:9999/request/model.html");

                // Ожидание загрузки страницы
                Wait.Until(d => d.Url.Contains("model.html"));
                CurrentTestReport.AddSuccess("Тестовая страница модели успешно загружена");

                // Шаг 3: Оптимизация окна браузера
                CurrentTestReport.AddStep("Конфигурация размеров окна браузера", "INFO");
                Driver.Manage().Window.Size = new System.Drawing.Size(1936, 1048);
                CurrentTestReport.AddSuccess("Размеры окна браузера оптимизированы");

                // Шаг 4: Ожидание появления меню пользователя
                CurrentTestReport.AddStep("Ожидание загрузки элементов интерфейса", "INFO");
                Wait.Until(d => d.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user, .user-name"))
                    .Any(e => e.Displayed && e.Enabled));
                CurrentTestReport.AddSuccess("Элементы интерфейса загружены");

                // Шаг 5: Клик по выпадающему меню пользователя
                CurrentTestReport.AddStep("Открытие выпадающего меню пользователя", "INFO");

                // Поиск и клик по меню пользователя
                var dropdownToggle = Wait.Until(d =>
                {
                    var elements = d.FindElements(By.CssSelector(".dropdown-toggle > span:nth-child(1)"));
                    return elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                });

                dropdownToggle.Click();
                CurrentTestReport.AddSuccess("Выпадающее меню открыто");

                // Шаг 6: Выбор пункта Logout
                CurrentTestReport.AddStep("Выбор пункта 'Logout' из меню", "INFO");
                var logoutLink = Wait.Until(d =>
                {
                    var links = d.FindElements(By.LinkText("Logout"));
                    return links.FirstOrDefault(e => e.Displayed && e.Enabled);
                });

                logoutLink.Click();
                CurrentTestReport.AddSuccess("Запрос выхода из системы отправлен");

                // Шаг 7: Ожидание перенаправления на страницу логина
                Wait.Until(d => d.Url.Contains("login") || d.FindElements(By.Id("loginform-login")).Any(e => e.Displayed));
                CurrentTestReport.AddSuccess("Перенаправление на страницу логина выполнено");

                // Шаг 8: Ввод учетных данных для повторного входа
                CurrentTestReport.AddStep("Ввод учетных данных для повторного входа", "INFO");

                var loginField = Wait.Until(d => d.FindElement(By.Id("loginform-login")));
                loginField.Clear();
                loginField.SendKeys(ValidLogin);
                CurrentTestReport.AddStep($"Введен логин: {MaskSensitiveData(ValidLogin)}", "DEBUG");

                var passwordField = Driver.FindElement(By.Id("loginform-password"));
                passwordField.Clear();
                passwordField.SendKeys(ValidPassword);
                CurrentTestReport.AddStep("Введен пароль", "DEBUG");

                CurrentTestReport.AddSuccess("Учетные данные введены успешно");

                // Шаг 9: Завершение входа
                CurrentTestReport.AddStep("Завершение процесса входа", "INFO");
                var loginButton = Driver.FindElement(By.CssSelector(".icon-circle-right2, [type='submit'], button"));
                loginButton.Click();
                CurrentTestReport.AddSuccess("Кнопка входа нажата");

                // Шаг 10: Валидация успешного входа
                ValidateAuthenticationSuccess();

                CurrentTestReport.AddSuccess("Тест полной последовательности выхода и входа завершен успешно");

            }, "Тест полной последовательности выхода и входа на основе Selenium IDE");
        }

        private void ExecuteCredentialInputSequence()
        {
            CurrentTestReport.AddStep("Выполнение последовательности ввода учетных данных", "INFO");

            var loginField = Driver.FindElement(By.Id("loginform-login"));
            var passwordField = Driver.FindElement(By.Id("loginform-password"));
            var loginForm = Driver.FindElement(By.Id("login-form"));

            // Последовательность ввода логина
            loginField.Clear();
            loginField.SendKeys(ValidLogin);
            CurrentTestReport.AddStep("Ввод логина пользователя", "DEBUG");

            // Последовательность ввода пароля
            passwordField.Clear();
            passwordField.SendKeys(ValidPassword);
            CurrentTestReport.AddStep("Ввод пароля пользователя", "DEBUG");

            // Дополнительные взаимодействия (имитация пользовательского поведения)
            PerformAdditionalFieldInteractions(loginField, passwordField, loginForm);

            CurrentTestReport.AddSuccess("Последовательность ввода учетных данных выполнена");
        }

        private void PerformAdditionalFieldInteractions(IWebElement loginField, IWebElement passwordField, IWebElement loginForm)
        {
            CurrentTestReport.AddStep("Выполнение дополнительных взаимодействий с полями формы", "DEBUG");

            // Имитация пользовательского поведения из Selenium IDE экспорта
            loginField.Click();
            loginForm.Click();

            loginField.Clear();
            loginField.SendKeys(ValidLogin);

            loginForm.Click();
            passwordField.Click();

            passwordField.Clear();
            passwordField.SendKeys(ValidPassword);

            loginForm.Click();

            CurrentTestReport.AddStep("Дополнительные взаимодействия завершены", "DEBUG");
        }

        private void SimulateUserInteraction(IWebElement element)
        {
            try
            {
                var actions = new OpenQA.Selenium.Interactions.Actions(Driver);

                // Имитация наведения курсора
                actions.MoveToElement(element).Perform();
                CurrentTestReport.AddStep("Наведение курсора на элемент аутентификации", "DEBUG");
                Thread.Sleep(300);

                // Имитация увода курсора
                actions.MoveToElement(Driver.FindElement(By.Id("login-form"))).Perform();
                CurrentTestReport.AddStep("Отведение курсора от элемента аутентификации", "DEBUG");
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                CurrentTestReport.AddWarning($"Не удалось выполнить симуляцию пользовательского взаимодействия: {ex.Message}");
            }
        }

        private void ValidateAuthenticationSuccess()
        {
            CurrentTestReport.AddStep("Валидация успешной аутентификации пользователя", "INFO");

            // Проверка изменения URL - основной признак успешного входа
            Wait.Until(d => d.Url != TestLoginUrl && !d.Url.Contains("login"));
            Assert.That(Driver.Url, Does.Not.Contain("login"),
                "URL должен измениться после успешной аутентификации");

            // Проверка что мы не на странице логина
            var loginFormElements = Driver.FindElements(By.Id("login-form"));
            Assert.That(loginFormElements, Is.Empty,
                "Форма логина не должна отображаться после успешного входа");

            CurrentTestReport.AddSuccess("Валидация аутентификации выполнена успешно");
        }

        private void ValidateBasicLoginSuccess()
        {
            CurrentTestReport.AddStep("Базовая проверка успешного входа в систему", "INFO");

            // Проверка что URL изменился и мы не на странице логина
            Assert.That(Driver.Url, Does.Not.Contain("login"),
                "После успешного входа URL не должен содержать 'login'");

            // Проверка что отсутствует форма логина
            var loginFields = Driver.FindElements(By.CssSelector("#loginform-login, #loginform-password"));
            Assert.That(loginFields, Is.Empty,
                "Поля логина и пароля не должны отображаться после успешного входа");

            CurrentTestReport.AddSuccess("Базовая проверка входа выполнена успешно");
        }

        private void ValidateLogoutSuccess()
        {
            CurrentTestReport.AddStep("Валидация успешного выхода из системы", "INFO");

            // Проверка что мы на странице логина
            Assert.That(Driver.Url, Does.Contain("login"),
                "После выхода из системы URL должен содержать 'login'");

            // Проверка что присутствует форма логина
            var loginFormElements = Driver.FindElements(By.Id("login-form"));
            Assert.That(loginFormElements, Is.Not.Empty,
                "Форма логина должна отображаться после выхода из системы");

            CurrentTestReport.AddSuccess("Валидация выхода из системы выполнена успешно");
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
        public void AuthenticationTestCleanup()
        {
            CurrentTestReport.AddStep("Завершение теста аутентификации", "INFO");
        }
    }
}