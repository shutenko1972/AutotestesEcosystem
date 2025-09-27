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

        // Статическое поле для общего файла отчета
        private static readonly object _lockObject = new object();
        private static string? _globalReportFilePath;
        private static bool _isGlobalReportInitialized = false;

        public TestReport(string testName)
        {
            _testName = testName ?? "НеизвестныйТест";
            _testPassed = true;
            _startTime = DateTime.Now;
            _reportDirectory = GetReportsDirectory();

            EnsureReportDirectoryExists();
            InitializeGlobalReport();
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
                var tempDirectory = Path.Combine(Path.GetTempPath(), "TestReports");
                Directory.CreateDirectory(tempDirectory);
                Console.WriteLine($"Используется временная директория: {tempDirectory}");
            }
        }

        private void InitializeGlobalReport()
        {
            lock (_lockObject)
            {
                if (!_isGlobalReportInitialized)
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    _globalReportFilePath = Path.Combine(_reportDirectory, $"TestReport_{timestamp}.txt");

                    // Создаем заголовок общего отчета
                    var header = $"ОБЩИЙ ОТЧЕТ ТЕСТОВ\n";
                    header += $"Создан: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                    header += $"Директория: {_reportDirectory}\n";
                    header += new string('=', 80) + "\n\n";

                    File.WriteAllText(_globalReportFilePath, header, Encoding.UTF8);
                    _isGlobalReportInitialized = true;

                    Console.WriteLine($"✓ Общий отчет создан: {_globalReportFilePath}");
                }
            }
        }

        private void InitializeReport()
        {
            AddStep($"Инициализация отчета для теста: {_testName}", "ИНФО");
            AddStep($"Время начала: {_startTime:yyyy-MM-dd HH:mm:ss.fff}", "ИНФО");
        }

        public void AddStep(string stepDescription, string status = "ИНФО")
        {
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
                    if (_globalReportFilePath != null)
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
            SaveReport();
        }

        public void SaveReport()
        {
            try
            {
                _endTime = DateTime.Now;
                var duration = _endTime - _startTime;
                var status = _testPassed ? "ПРОЙДЕН" : "ПРОВАЛЕН";

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
                Console.WriteLine($"✓ Результаты записаны в общий отчет: {_globalReportFilePath}");

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
                if (_globalReportFilePath != null && File.Exists(_globalReportFilePath))
                {
                    var footer = new StringBuilder();
                    footer.AppendLine(new string('=', 80));
                    footer.AppendLine($"СЕССИЯ ТЕСТИРОВАНИЯ ЗАВЕРШЕНА");
                    footer.AppendLine($"Время завершения: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    footer.AppendLine(new string('=', 80));

                    File.AppendAllText(_globalReportFilePath, footer.ToString(), Encoding.UTF8);

                    Console.WriteLine($"✓ Сессия тестирования завершена");
                    Console.WriteLine($"✓ Общий отчет сохранен: {_globalReportFilePath}");
                }
            }
        }
    }
}