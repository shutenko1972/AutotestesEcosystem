using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Autotests
{
    public class TestReport
    {
        private readonly string _testName;
        private readonly List<string> _steps = [];
        private readonly StringBuilder _reportContent = new();
        private bool _testPassed;
        private readonly string _reportDirectory;
        private readonly DateTime _startTime;
        private DateTime _endTime;

        // Статические поля для общего файла отчета и статистики
        private static readonly object _lockObject = new object();
        private static string? _globalReportFilePath;
        private static bool _isGlobalReportInitialized = false;
        private static int _totalTests = 0;
        private static int _passedTests = 0;
        private static int _failedTests = 0;
        private static readonly List<string> _testResults = [];
        private static bool _sessionFinalized = false;

        public TestReport(string testName)
        {
            _testName = testName ?? "НеизвестныйТест";
            _testPassed = true;
            _startTime = DateTime.Now;
            _reportDirectory = GetReportsDirectory();

            EnsureReportDirectoryExists();

            // Инициализируем общий отчет только если сессия еще не завершена
            if (!_sessionFinalized)
            {
                InitializeGlobalReport();
            }

            InitializeReport();
        }

        private static string GetReportsDirectory()
        {
            try
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var projectDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.FullName;

                if (string.IsNullOrEmpty(projectDirectory) || !Directory.Exists(projectDirectory))
                {
                    projectDirectory = baseDirectory;
                }

                return Path.Combine(projectDirectory, "Reports");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при определении директории отчетов: {ex.Message}");
                return Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            }
        }

        private void EnsureReportDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_reportDirectory))
                {
                    Directory.CreateDirectory(_reportDirectory);
                    Console.WriteLine($"Создана директория для отчетов: {_reportDirectory}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании директории отчетов: {ex.Message}");
            }
        }

        private void InitializeGlobalReport()
        {
            lock (_lockObject)
            {
                if (!_isGlobalReportInitialized && !_sessionFinalized)
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    _globalReportFilePath = Path.Combine(_reportDirectory, $"TestReport_{timestamp}.txt");

                    // Создаем заголовок общего отчета
                    var header = GenerateReportHeader();

                    File.WriteAllText(_globalReportFilePath, header, Encoding.UTF8);
                    _isGlobalReportInitialized = true;

                    Console.WriteLine($"✓ Общий отчет создан: {_globalReportFilePath}");

                    // Добавляем обработчик для завершения сессии при выходе
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => FinalizeTestSession();
                    AppDomain.CurrentDomain.DomainUnload += (s, e) => FinalizeTestSession();
                }
            }
        }

        private static string GenerateReportHeader()
        {
            var header = new StringBuilder();
            header.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            header.AppendLine("║                             ОБЩИЙ ОТЧЕТ ТЕСТОВ                              ║");
            header.AppendLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            header.AppendLine($"║ Создан: {DateTime.Now:yyyy-MM-dd HH:mm:ss,-60} ║");
            header.AppendLine($"║ Директория: {Path.GetDirectoryName(_globalReportFilePath)?.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? "Reports",-52} ║");
            header.AppendLine("╠══════════════════════════════════════════════════════════════════════════════╣");
            header.AppendLine($"║ Всего тестов: {_totalTests,-3}                                                          ║");
            header.AppendLine($"║ Пройдено:     {_passedTests,-3}                                                          ║");
            header.AppendLine($"║ Провалено:    {_failedTests,-3}                                                          ║");
            header.AppendLine($"║ Успешность:   {(_totalTests > 0 ? (_passedTests * 100.0 / _totalTests).ToString("F1") : "0.0"),-5}%                                                       ║");
            header.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            header.AppendLine();
            header.AppendLine("ТЕСТЫ:");
            header.AppendLine(new string('=', 80));
            header.AppendLine();

            return header.ToString();
        }

        private void UpdateGlobalStatistics()
        {
            lock (_lockObject)
            {
                if (_sessionFinalized) return;

                _totalTests++;
                if (_testPassed)
                {
                    _passedTests++;
                }
                else
                {
                    _failedTests++;
                }

                // Обновляем заголовок с новой статистикой
                if (_globalReportFilePath != null && File.Exists(_globalReportFilePath))
                {
                    try
                    {
                        var currentContent = File.ReadAllText(_globalReportFilePath);

                        // Находим начало основного содержимого (после заголовка)
                        var testsSectionIndex = currentContent.IndexOf("ТЕСТЫ:");
                        if (testsSectionIndex > 0)
                        {
                            var mainContent = currentContent.Substring(testsSectionIndex);
                            var newHeader = GenerateReportHeader();
                            File.WriteAllText(_globalReportFilePath, newHeader + mainContent, Encoding.UTF8);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не удалось обновить заголовок отчета: {ex.Message}");
                    }
                }
            }
        }

        private void InitializeReport()
        {
            if (_sessionFinalized) return;

            // Добавляем информацию о начале теста в общую статистику
            lock (_lockObject)
            {
                _testResults.Add($"🔹 {_testName} - ЗАПУЩЕН");
            }

            AddStep($"Инициализация отчета для теста: {_testName}", "ИНФО");
            AddStep($"Время начала: {_startTime:yyyy-MM-dd HH:mm:ss.fff}", "ИНФО");
        }

        public void AddStep(string stepDescription, string status = "ИНФО")
        {
            if (_sessionFinalized) return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var step = $"[{timestamp}] [{status}] {stepDescription}";
            _steps.Add(step);
            _reportContent.AppendLine(step);

            // Также выводим в консоль для реального времени
            Console.WriteLine(step);

            // Записываем шаг в общий файл
            WriteToGlobalFile(step);
        }

        private void WriteToGlobalFile(string content)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_globalReportFilePath != null && File.Exists(_globalReportFilePath) && !_sessionFinalized)
                    {
                        File.AppendAllText(_globalReportFilePath, content + Environment.NewLine, Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось записать в общий файл отчета: {ex.Message}");
            }
        }

        public void AddSuccess(string message)
        {
            AddStep($"УСПЕХ: {message}", "УСПЕХ");
        }

        public void AddWarning(string message)
        {
            AddStep($"ПРЕДУПРЕЖДЕНИЕ: {message}", "ПРЕДУПРЕЖДЕНИЕ");
        }

        public void AddError(string message, Exception? exception = null)
        {
            _testPassed = false;
            var errorMessage = $"ОШИБКА: {message}";

            if (exception != null)
            {
                errorMessage += $"\nИсключение: {exception.Message}\nТрассировка стека: {exception.StackTrace}";
            }

            AddStep(errorMessage, "ОШИБКА");
        }

        public void AddScreenshotInfo(string screenshotPath)
        {
            if (!string.IsNullOrEmpty(screenshotPath))
            {
                AddStep($"Скриншот сохранен: {screenshotPath}", "СКРИНШОТ");
            }
        }

        public void AddUrlInfo(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                AddStep($"Текущий URL: {url}", "ИНФО");
            }
        }

        public void AddTestData(string key, string value)
        {
            AddStep($"Данные теста: {key} = {value}", "ДАННЫЕ");
        }

        public void FinalizeReport()
        {
            if (_sessionFinalized) return;

            SaveReport();
        }

        public void SaveReport()
        {
            try
            {
                if (_sessionFinalized) return;

                _endTime = DateTime.Now;
                var duration = _endTime - _startTime;
                var status = _testPassed ? "ПРОЙДЕН" : "ПРОВАЛЕН";

                // Обновляем статистику
                UpdateGlobalStatistics();

                // Обновляем результат теста в списке
                lock (_lockObject)
                {
                    var testIndex = _testResults.FindIndex(r => r.Contains(_testName) && r.Contains("ЗАПУЩЕН"));
                    if (testIndex >= 0)
                    {
                        var statusIcon = _testPassed ? "✅" : "❌";
                        _testResults[testIndex] = $"{statusIcon} {_testName} - {status} ({duration.TotalSeconds:F1} сек)";
                    }
                }

                // Формируем итоговую статистику теста
                var testSummary = new StringBuilder();
                testSummary.AppendLine();
                testSummary.AppendLine(new string('-', 80));
                testSummary.AppendLine($"ИТОГ ТЕСТА: {_testName}");
                testSummary.AppendLine($"СТАТУС: {status}");
                testSummary.AppendLine($"ВРЕМЯ НАЧАЛА: {_startTime:yyyy-MM-dd HH:mm:ss.fff}");
                testSummary.AppendLine($"ВРЕМЯ ОКОНЧАНИЯ: {_endTime:yyyy-MM-dd HH:mm:ss.fff}");
                testSummary.AppendLine($"ПРОДОЛЖИТЕЛЬНОСТЬ: {duration.TotalSeconds:F2} секунд");
                testSummary.AppendLine($"ВСЕГО ШАГОВ: {_steps.Count}");
                testSummary.AppendLine($"УСПЕШНЫХ: {_steps.Count(s => s.Contains("[УСПЕХ]"))}");
                testSummary.AppendLine($"ОШИБОК: {_steps.Count(s => s.Contains("[ОШИБКА]"))}");
                testSummary.AppendLine($"ПРЕДУПРЕЖДЕНИЙ: {_steps.Count(s => s.Contains("[ПРЕДУПРЕЖДЕНИЕ]"))}");
                testSummary.AppendLine(new string('-', 80));
                testSummary.AppendLine();

                // Записываем итоги теста в общий файл
                WriteToGlobalFile(testSummary.ToString());

                // Добавляем информацию о завершении теста
                AddStep($"Тест завершен: {status}", "ОТЧЕТ");

                Console.WriteLine($"✓ Тест '{_testName}' завершен со статусом: {status}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось сохранить отчет: {ex.Message}");
            }
        }

        public bool IsTestPassed()
        {
            return _testPassed;
        }

        public string GetTestName()
        {
            return _testName;
        }

        public int GetStepCount()
        {
            return _steps.Count;
        }

        // Статический метод для получения пути к текущему отчету
        public static string? GetCurrentReportPath()
        {
            return _globalReportFilePath;
        }

        // Метод для завершения всей сессии тестирования
        public static void FinalizeTestSession()
        {
            lock (_lockObject)
            {
                if (_sessionFinalized) return;

                if (_globalReportFilePath != null && File.Exists(_globalReportFilePath))
                {
                    try
                    {
                        var footer = new StringBuilder();
                        footer.AppendLine();
                        footer.AppendLine(new string('=', 80));
                        footer.AppendLine("СЕССИЯ ТЕСТИРОВАНИЯ ЗАВЕРШЕНА");
                        footer.AppendLine($"Время завершения: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        footer.AppendLine();

                        // Добавляем список всех тестов
                        footer.AppendLine("СПИСОК ТЕСТОВ:");
                        foreach (var result in _testResults)
                        {
                            footer.AppendLine(result);
                        }

                        footer.AppendLine(new string('=', 80));

                        File.AppendAllText(_globalReportFilePath, footer.ToString(), Encoding.UTF8);

                        Console.WriteLine($"✓ Сессия тестирования завершена");
                        Console.WriteLine($"✓ Всего тестов: {_totalTests}, Пройдено: {_passedTests}, Провалено: {_failedTests}");
                        Console.WriteLine($"✓ Общий отчет сохранен: {_globalReportFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не удалось завершить сессию тестирования: {ex.Message}");
                    }
                }

                _sessionFinalized = true;
            }
        }

        // Метод для принудительного создания нового отчета (если нужно)
        public static void StartNewSession()
        {
            lock (_lockObject)
            {
                _sessionFinalized = false;
                _isGlobalReportInitialized = false;
                _totalTests = 0;
                _passedTests = 0;
                _failedTests = 0;
                _testResults.Clear();
                _globalReportFilePath = null;
            }
        }
    }
}