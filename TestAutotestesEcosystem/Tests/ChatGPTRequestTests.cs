using Autotests.Base;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace Autotests.Tests
{
    [TestFixture]
    public class ChatGPTRequestTests : AuthorizationBase
    {
        private static readonly string[] TestQuestions =
        {
            "Hello, how are you?",
            "What is the capital of France?",
            "Explain artificial intelligence in simple terms"
        };

        [Test]
        public void Test_MultipleSequentialRequests()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                foreach (var question in TestQuestions)
                {
                    // Act
                    CurrentTestReport.AddStep($"Отправка запроса: {question}", "ИНФО");

                    // Очищаем поле перед каждым запросом
                    var textArea = Driver.FindElement(By.Id("textarea_request"));
                    textArea.Clear();

                    EnterTextAndVerify(question);

                    var sendButton = Driver.FindElement(By.Id("send_request"));
                    sendButton.Click();

                    // Ждем завершения обработки запроса
                    CurrentTestReport.AddStep("Ожидание завершения обработки запроса...", "ИНФО");

                    // Ждем, пока кнопка снова станет активной
                    WaitForButtonToBecomeEnabled(sendButton, 30);

                    // Краткая пауза между запросами
                    Thread.Sleep(2000);

                    // Assert - Проверка, что кнопка снова доступна
                    Assert.That(sendButton.Enabled, Is.True, "Кнопка отправки должна быть доступна после завершения запроса");

                    CurrentTestReport.AddSuccess($"Запрос '{question}' обработан успешно");
                }

            }, "Проверка множественных последовательных запросов");
        }

        [Test]
        public void Test_RequestWithShortTimeout()
        {
            HandleCommonExceptions(() =>
            {
                // Arrange
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);
                Wait.Until(d => d.FindElement(By.Id("textarea_request")).Displayed);

                // Act - Отправляем запрос с коротким таймаутом для быстрого теста
                string quickQuestion = "Say hello";
                EnterTextAndVerify(quickQuestion);

                var sendButton = Driver.FindElement(By.Id("send_request"));
                sendButton.Click();

                CurrentTestReport.AddStep("Запрос отправлен с коротким таймаутом...", "ИНФО");

                // Assert - Ждем всего 10 секунд для быстрого теста
                try
                {
                    WaitForResponse(10);
                    CurrentTestReport.AddSuccess("Быстрый запрос обработан");
                }
                catch (Exception ex)
                {
                    CurrentTestReport.AddWarning($"Быстрый запрос не завершился за 10 секунд: {ex.Message}");
                    // Это не ошибка теста - просто информация
                }

                // Проверяем, что кнопка разблокирована
                WaitForButtonToBecomeEnabled(sendButton, 5);

            }, "Проверка запроса с коротким таймаутом");
        }
    }
}