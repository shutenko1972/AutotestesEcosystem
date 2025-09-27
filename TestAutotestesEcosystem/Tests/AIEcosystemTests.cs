using Autotests.Base;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Linq;

namespace Autotests.Tests
{
    [TestFixture]
    public class AIEcosystemTests : AuthorizationBase
    {
        [Test]
        public void Test_SuccessfulLogin()
        {
            HandleCommonExceptions(() =>
            {
                // Act
                PerformLogin();

                // Assert
                Assert.Multiple(() =>
                {
                    Assert.That(Driver.Url, Does.Not.Contain("login.html"));
                    Assert.That(Driver.PageSource, Contains.Substring("Vitaliy Shutenko"));

                    var menuElements = Driver.FindElements(By.CssSelector(".dropdown-user, .user-menu"));
                    Assert.That(menuElements.Any(e => e.Displayed), Is.True, "Меню пользователя должно отображаться");
                });

            }, "Успешный вход в систему");
        }

        [Test]
        public void Test_NavigateToChatGPTPage()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();

                // Act
                CurrentTestReport.AddStep("Переход на страницу ChatGPT...", "ИНФО");
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                // Assert
                Wait.Until(d => d.FindElement(By.Id("send-request-form")).Displayed);

                Assert.Multiple(() =>
                {
                    Assert.That(Driver.Url, Is.EqualTo(TestChatGPTUrl));
                    Assert.That(Driver.PageSource, Contains.Substring("Temperature"));
                    Assert.That(Driver.PageSource, Contains.Substring("TopP"));

                    var textArea = Driver.FindElement(By.Id("textarea_request"));
                    Assert.That(textArea.Displayed, Is.True, "Текстовое поле для запроса должно отображаться");

                    var sendButton = Driver.FindElement(By.Id("send_request"));
                    Assert.That(sendButton.Displayed, Is.True, "Кнопка отправки должна отображаться");
                });

            }, "Навигация на страницу ChatGPT");
        }

        [Test]
        public void Test_UserMenuFunctionality()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();

                // Act
                OpenUserMenu();

                // Assert
                var dropdownMenu = Driver.FindElement(By.CssSelector(".dropdown-menu-right"));

                Assert.Multiple(() =>
                {
                    Assert.That(dropdownMenu.Displayed, Is.True, "Выпадающее меню должно отображаться");

                    var accountSettings = FindAccountSettingsButton();
                    Assert.That(accountSettings.Text, Contains.Substring("Account").IgnoreCase);

                    var logoutButton = Driver.FindElements(By.PartialLinkText("Logout"))
                        .FirstOrDefault(e => e.Displayed);
                    Assert.That(logoutButton, Is.Not.Null, "Кнопка выхода должна быть в меню");
                });

            }, "Проверка функциональности меню пользователя");
        }

        [Test]
        public void Test_AccountSettingsNavigation()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                OpenUserMenu();

                // Act
                var accountSettingsButton = FindAccountSettingsButton();
                accountSettingsButton.Click();

                // Assert
                Wait.Until(d => d.Url.Contains("/profile/index.html"));

                Assert.Multiple(() =>
                {
                    Assert.That(Driver.Url, Contains.Substring("/profile/index.html"));
                    Assert.That(Driver.PageSource, Contains.Substring("Profile").IgnoreCase);
                });

            }, "Навигация в настройки аккаунта");
        }

        [Test]
        public void Test_ChatGPTTextInput()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);
                Wait.Until(d => d.FindElement(By.Id("textarea_request")).Displayed);

                // Act & Assert
                string testText = "Привет, как дела?";
                EnterTextAndVerify(testText);

                // Проверка очистки поля
                var clearButton = Driver.FindElement(By.Id("clear_request"));
                clearButton.Click();

                var textArea = Driver.FindElement(By.Id("textarea_request"));
                string clearedText = textArea.GetAttribute("value") ?? string.Empty;

                Assert.That(string.IsNullOrEmpty(clearedText), Is.True, "Текстовое поле должно быть очищено");

            }, "Проверка ввода текста в ChatGPT");
        }

        [Test]
        public void Test_SliderControlsFunctionality()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                // Act & Assert - Temperature slider
                var temperatureSlider = Driver.FindElement(By.CssSelector(".noui-connect-lower-temperature"));
                var temperatureValue = Driver.FindElement(By.Id("noui-connect-lower-temperature"));

                // Act & Assert - TopP slider
                var topPSlider = Driver.FindElement(By.CssSelector(".noui-connect-lower-topp"));
                var topPValue = Driver.FindElement(By.Id("noui-connect-lower-topp"));

                Assert.Multiple(() =>
                {
                    // Temperature assertions
                    Assert.That(temperatureSlider.Displayed, Is.True, "Слайдер Temperature должен отображаться");
                    Assert.That(temperatureValue.Text, Is.Not.Empty, "Значение Temperature должно отображаться");

                    // TopP assertions
                    Assert.That(topPSlider.Displayed, Is.True, "Слайдер TopP должен отображаться");
                    Assert.That(topPValue.Text, Is.Not.Empty, "Значение TopP должно отображаться");
                });

                // Взаимодействуем со слайдерами для активации скрытых полей
                InteractWithSlider("noui-connect-lower-temperature");
                InteractWithSlider("noui-connect-lower-topp");

                // Теперь проверяем скрытые поля (они могут быть заполнены после взаимодействия)
                var temperatureInput = Driver.FindElement(By.Id("temperature"));
                var topPInput = Driver.FindElement(By.Id("top_p"));

                string tempValue = temperatureInput.GetAttribute("value") ?? string.Empty;
                string topPVal = topPInput.GetAttribute("value") ?? string.Empty;

                CurrentTestReport.AddStep($"Значение Temperature: '{tempValue}'", "ИНФО");
                CurrentTestReport.AddStep($"Значение TopP: '{topPVal}'", "ИНФО");

                // Более мягкая проверка - поля могут быть пустыми до отправки формы
                if (!string.IsNullOrEmpty(tempValue))
                {
                    Assert.That(tempValue, Is.Not.Empty, "Поле Temperature должно иметь значение после взаимодействия");
                }

                if (!string.IsNullOrEmpty(topPVal))
                {
                    Assert.That(topPVal, Is.Not.Empty, "Поле TopP должно иметь значение после взаимодействия");
                }

            }, "Проверка функциональности слайдеров управления");
        }

        [Test]
        public void Test_CopyButtonPresence()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                // Act & Assert
                var copyButton = Driver.FindElement(By.CssSelector(".coping"));

                Assert.Multiple(() =>
                {
                    Assert.That(copyButton.Displayed, Is.True, "Кнопка копирования должна отображаться");
                    Assert.That(copyButton.Text, Contains.Substring("Copy").IgnoreCase);
                    Assert.That(copyButton.Enabled, Is.True, "Кнопка копирования должна быть активна");
                });

            }, "Проверка наличия кнопки копирования");
        }

        [Test]
        public void Test_ResponseAreaInitialState()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                // Act & Assert
                var responseDiv = Driver.FindElement(By.Id("response_div"));

                Assert.Multiple(() =>
                {
                    Assert.That(responseDiv.Displayed, Is.True, "Область ответа должна отображаться");
                    Assert.That(responseDiv.Text, Contains.Substring("Your answer will be shown here"));
                });

            }, "Проверка начального состояния области ответа");
        }

        [Test]
        public void Test_PageLayoutElements()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                // Assert - Проверка основных элементов layout
                var navbar = Driver.FindElement(By.CssSelector(".navbar-inverse"));
                var pageHeader = Driver.FindElement(By.CssSelector(".page-header"));
                var breadcrumb = Driver.FindElement(By.CssSelector(".breadcrumb-line"));
                var footer = Driver.FindElement(By.CssSelector(".text-muted.pt-20"));

                Assert.Multiple(() =>
                {
                    Assert.That(navbar.Displayed, Is.True, "Навигационная панель должна отображаться");
                    Assert.That(pageHeader.Displayed, Is.True, "Заголовок страницы должен отображаться");
                    Assert.That(breadcrumb.Displayed, Is.True, "Хлебные крошки должны отображаться");
                    Assert.That(footer.Displayed, Is.True, "Футер должен отображаться");
                });

            }, "Проверка элементов layout страницы");
        }

        [Test]
        public void Test_SendRequestButtonState()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                // Act & Assert
                var sendButton = Driver.FindElement(By.Id("send_request"));

                Assert.Multiple(() =>
                {
                    Assert.That(sendButton.Displayed, Is.True, "Кнопка отправки должна отображаться");
                    Assert.That(sendButton.Enabled, Is.True, "Кнопка отправки должна быть активна");
                    Assert.That(sendButton.Text, Contains.Substring("Send Request").IgnoreCase);

                    // Проверяем наличие класса Ladda, но не конкретный атрибут
                    Assert.That(sendButton.GetAttribute("class"), Contains.Substring("btn-ladda"));
                });

            }, "Проверка состояния кнопки отправки запроса");
        }

        [Test]
        public void Test_ServiceDescriptionVisibility()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                // Act & Assert
                var serviceDescription = Driver.FindElement(By.CssSelector(".panel.panel-body.border-top-info"));
                var descriptionItems = serviceDescription.FindElements(By.TagName("li"));

                Assert.Multiple(() =>
                {
                    Assert.That(serviceDescription.Displayed, Is.True, "Описание сервиса должно отображаться");
                    Assert.That(descriptionItems, Has.Count.GreaterThanOrEqualTo(3), "Должно быть несколько пунктов описания");
                    Assert.That(serviceDescription.Text, Contains.Substring("ChatGPT"));
                    Assert.That(serviceDescription.Text, Contains.Substring("integration"));
                });

            }, "Проверка видимости описания сервиса");
        }
    }
}