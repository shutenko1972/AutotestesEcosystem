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

        // Статическое поле для хранения всех ошибок из всех тестов
        private static readonly List<string> _allErrors = [];
        private static readonly object _lockObject = new object();
        private static string? _errorsFilePath; // Добавляем nullable

        public TestReport(string testName)
        {
            _testName = testName ?? "НеизвестныйТест";
            _testPassed = true;
            _startTime = DateTime.Now;
            _reportDirectory = GetReportsDirectory();

            EnsureReportDirectoryExists();
            // Инициализация файла ошибок один раз
            InitializeErrorsFile();
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

        private void InitializeReport()
        {
            AddStep($"Инициализация отчета для теста: {_testName}", "ИНФО");
            AddStep($"Время начала: {_startTime:yyyy-MM-dd HH:mm:ss.fff}", "ИНФО");
            AddStep($"Директория отчетов: {_reportDirectory}", "ИНФО");
        }

        private void InitializeErrorsFile()
        {
            lock (_lockObject)
            {
                if (_errorsFilePath == null)
                {
                    _errorsFilePath = Path.Combine(_reportDirectory, "AllErrors.txt");

                    // Создаем заголовок файла ошибок при первом использовании
                    if (!File.Exists(_errorsFilePath))
                    {
                        var header = $"ФАЙЛ ОШИБОК ТЕСТОВ\nСоздан: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{new string('=', 80)}\n\n";
                        File.WriteAllText(_errorsFilePath, header, Encoding.UTF8);
                    }
                }
            }
        }

        public void AddStep(string stepDescription, string status = "ИНФО")
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var step = $"[{timestamp}] [{status}] {stepDescription}";
            _steps.Add(step);
            _reportContent.AppendLine(step);

            // Также выводим в консоль для реального времени
            Console.WriteLine(step);
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

            // Добавляем ошибку в общий файл
            AddErrorToGlobalFile(errorMessage);
        }

        private void AddErrorToGlobalFile(string errorMessage)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_errorsFilePath == null) return;

                    var errorEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ТЕСТ: {_testName}\n{errorMessage}\n{new string('-', 60)}\n";

                    // Добавляем ошибку в список
                    _allErrors.Add(errorEntry);

                    // Записываем в файл
                    File.AppendAllText(_errorsFilePath, errorEntry, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось записать ошибку в общий файл: {ex.Message}");
            }
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

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var status = _testPassed ? "ПРОЙДЕН" : "ПРОВАЛЕН";

                var safeTestName = new string(_testName
                    .Where(c => !Path.GetInvalidFileNameChars().Contains(c))
                    .ToArray());

                var fileName = $"{safeTestName}_{status}_{timestamp}.txt";
                var filePath = Path.Combine(_reportDirectory, fileName);

                var reportHeader = new StringBuilder();
                reportHeader.AppendLine("=".PadRight(100, '='));
                reportHeader.AppendLine($"ОТЧЕТ О ТЕСТЕ: {_testName}");
                reportHeader.AppendLine($"СТАТУС: {status}");
                reportHeader.AppendLine($"ВРЕМЯ НАЧАЛА: {_startTime:yyyy-MM-dd HH:mm:ss.fff}");
                reportHeader.AppendLine($"ВРЕМЯ ОКОНЧАНИЯ: {_endTime:yyyy-MM-dd HH:mm:ss.fff}");
                reportHeader.AppendLine($"ПРОДОЛЖИТЕЛЬНОСТЬ: {duration.TotalSeconds:F2} секунд");
                reportHeader.AppendLine($"ДИРЕКТОРИЯ ОТЧЕТОВ: {_reportDirectory}");
                reportHeader.AppendLine("=".PadRight(100, '='));
                reportHeader.AppendLine();

                var statistics = new StringBuilder();
                statistics.AppendLine();
                statistics.AppendLine("-".PadRight(100, '-'));
                statistics.AppendLine("СТАТИСТИКА ТЕСТА:");
                statistics.AppendLine($"Всего шагов: {_steps.Count}");
                statistics.AppendLine($"Успешных шагов: {_steps.Count(s => s.Contains("[УСПЕХ]"))}");
                statistics.AppendLine($"Шагов с предупреждениями: {_steps.Count(s => s.Contains("[ПРЕДУПРЕЖДЕНИЕ]"))}");
                statistics.AppendLine($"Шагов с ошибками: {_steps.Count(s => s.Contains("[ОШИБКА]"))}");
                statistics.AppendLine($"Информационных шагов: {_steps.Count(s => s.Contains("[ИНФО]"))}");
                statistics.AppendLine($"РЕЗУЛЬТАТ: {status}");
                statistics.AppendLine("-".PadRight(100, '-'));

                // Добавляем ссылку на файл ошибок, если тест провален
                if (!_testPassed && _errorsFilePath != null)
                {
                    statistics.AppendLine($"Ошибка также записана в общий файл: {Path.GetFileName(_errorsFilePath)}");
                    statistics.AppendLine("-".PadRight(100, '-'));
                }

                var fullReport = reportHeader + _reportContent.ToString() + statistics;

                File.WriteAllText(filePath, fullReport, Encoding.UTF8);

                // Добавляем информацию о сохранении отчета
                AddStep($"Отчет сохранен: {filePath}", "ОТЧЕТ");

                Console.WriteLine($"✓ Отчет сохранен: {filePath}");
                Console.WriteLine($"✓ Директория отчетов: {_reportDirectory}");

                // Также создаем HTML версию для удобства просмотра
                CreateHtmlReport(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось сохранить отчет: {ex.Message}");
                // Пытаемся сохранить хотя бы в временную директорию
                TrySaveToTempDirectory();
            }
        }

        private void TrySaveToTempDirectory()
        {
            try
            {
                var tempDirectory = Path.Combine(Path.GetTempPath(), "TestReports");
                Directory.CreateDirectory(tempDirectory);

                var tempFilePath = Path.Combine(tempDirectory, $"{_testName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(tempFilePath, _reportContent.ToString(), Encoding.UTF8);

                Console.WriteLine($"✓ Отчет сохранен во временную директорию: {tempFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось сохранить отчет даже во временную директорию: {ex.Message}");
            }
        }

        private void CreateHtmlReport(string txtFilePath)
        {
            try
            {
                var htmlFilePath = Path.ChangeExtension(txtFilePath, ".html");

                var htmlContent = $@"<!DOCTYPE html>
<html lang='ru'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Отчет теста: {_testName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }}
        .container {{ background: white; padding: 20px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }}
        .header {{ background: #2c3e50; color: white; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .step {{ margin: 5px 0; padding: 5px; border-left: 4px solid #ddd; }}
        .success {{ border-left-color: #27ae60; background-color: #e8f5e8; }}
        .error {{ border-left-color: #e74c3c; background-color: #ffe6e6; }}
        .warning {{ border-left-color: #f39c12; background-color: #fff3cd; }}
        .info {{ border-left-color: #3498db; background-color: #e3f2fd; }}
        .timestamp {{ color: #7f8c8d; font-size: 0.9em; }}
        .statistics {{ background: #ecf0f1; padding: 15px; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Отчет теста: {_testName}</h1>
            <p>Статус: <strong>{(_testPassed ? "ПРОЙДЕН" : "ПРОВАЛЕН")}</strong></p>
            <p>Время: {_startTime:yyyy-MM-dd HH:mm:ss} - {_endTime:yyyy-MM-dd HH:mm:ss}</p>
        </div>
        
        <h2>Шаги выполнения:</h2>
        {GenerateHtmlSteps()}
        
        <div class='statistics'>
            <h3>Статистика</h3>
            <p>Всего шагов: {_steps.Count}</p>
            <p>Успешных: {_steps.Count(s => s.Contains("[УСПЕХ]"))}</p>
            <p>Ошибок: {_steps.Count(s => s.Contains("[ОШИБКА]"))}</p>
            <p>Предупреждений: {_steps.Count(s => s.Contains("[ПРЕДУПРЕЖДЕНИЕ]"))}</p>
        </div>
    </div>
</body>
</html>";

                File.WriteAllText(htmlFilePath, htmlContent, Encoding.UTF8);
                Console.WriteLine($"✓ HTML отчет сохранен: {htmlFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось создать HTML отчет: {ex.Message}");
            }
        }

        private string GenerateHtmlSteps()
        {
            var htmlSteps = new StringBuilder();

            foreach (var step in _steps)
            {
                var cssClass = "step";
                if (step.Contains("[УСПЕХ]")) cssClass += " success";
                else if (step.Contains("[ОШИБКА]")) cssClass += " error";
                else if (step.Contains("[ПРЕДУПРЕЖДЕНИЕ]")) cssClass += " warning";
                else if (step.Contains("[ИНФО]")) cssClass += " info";

                htmlSteps.AppendLine($"<div class='{cssClass}'>");

                // Извлекаем timestamp
                var timestampStart = step.IndexOf('[') + 1;
                var timestampEnd = step.IndexOf(']', timestampStart);
                var timestamp = step[timestampStart..timestampEnd];

                htmlSteps.AppendLine($"<span class='timestamp'>[{timestamp}]</span> ");

                // Извлекаем оставшуюся часть сообщения (после второго закрывающей скобки)
                var messageStart = step.IndexOf(']', timestampEnd + 1) + 2;
                var message = step[messageStart..];

                // Заменяем переносы строк на HTML теги
                message = message.Replace("\n", "<br/>");

                htmlSteps.AppendLine(message);
                htmlSteps.AppendLine("</div>");
            }

            return htmlSteps.ToString();
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

        // Новый статический метод для получения статистики ошибок
        public static string GetErrorsSummary()
        {
            lock (_lockObject)
            {
                return $"Всего ошибок в сессии: {_allErrors.Count}";
            }
        }

        // Метод для очистки файла ошибок (опционально)
        public static void ClearErrorsFile()
        {
            lock (_lockObject)
            {
                _allErrors.Clear();
                if (_errorsFilePath != null && File.Exists(_errorsFilePath))
                {
                    var header = $"ФАЙЛ ОШИБОК ТЕСТОВ\nОчищен и пересоздан: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{new string('=', 80)}\n\n";
                    File.WriteAllText(_errorsFilePath, header, Encoding.UTF8);
                }
            }
        }
    }
}