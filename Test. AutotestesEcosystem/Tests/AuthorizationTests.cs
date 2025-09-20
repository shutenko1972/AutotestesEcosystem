using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading;
using Autotests_ai_ecosystem.Base;

namespace Autotests_ai_ecosystem.Tests
{
    [TestFixture]
    public class AuthorizationTests : AuthorizationBase
    {
        [Test]
        public void AuthorizationLogInTest()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Начало теста авторизации", "ИНФО");

                PerformLogin();

                Assert.That(Driver.Url.Contains("login") || Driver.Url.Contains("auth/login"), Is.False,
                    "Остались на странице входа после успешного логина");

                CurrentTestReport.AddSuccess("Проверка URL после логина пройдена успешно");
                CurrentTestReport.AddTestData("Финальный URL", Driver.Url);

            }, "AuthorizationLogInTest");
        }

        [Test]
        public void AuthorizationLogInLogoutTest()
        {
            HandleCommonExceptions(() =>
            {
                CurrentTestReport.AddStep("Начало теста авторизации и выхода", "ИНФО");

                PerformLogin();
                Thread.Sleep(2000);
                CurrentTestReport.AddStep("Ожидание 2 секунды после логина", "ИНФО");

                OpenUserMenu();
                Thread.Sleep(1000);
                CurrentTestReport.AddStep("Ожидание 1 секунды после открытия меню", "ИНФО");

                var logoutButton = Driver.FindElements(By.LinkText("Logout"))
                    .FirstOrDefault(e => e.Displayed);

                if (logoutButton == null)
                {
                    CurrentTestReport.AddWarning("Кнопка 'Logout' не найдена по точному тексту, поиск по частичному совпадению");
                    logoutButton = Driver.FindElements(By.PartialLinkText("Logout"))
                        .FirstOrDefault(e => e.Displayed);
                }

                Assert.That(logoutButton, Is.Not.Null, "Кнопка выхода не найдена");
                CurrentTestReport.AddSuccess("Кнопка выхода найдена");

                logoutButton.Click();
                CurrentTestReport.AddSuccess("Кнопка выхода нажата");

                Wait.Until(d => d.FindElement(By.Id("loginform-login")).Displayed);
                Assert.That(Driver.FindElement(By.Id("loginform-login")).Displayed, Is.True,
                    "Не вернулись на страницу входа после выхода");

                CurrentTestReport.AddSuccess("Успешно вернулись на страницу входа после выхода");
                CurrentTestReport.AddTestData("URL после выхода", Driver.Url);

            }, "AuthorizationLogInLogoutTest");
        }
    }
}