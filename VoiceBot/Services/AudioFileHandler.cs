using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using VoiceBot.Configuration;
using VoiceBot.Utilities;
namespace VoiceBot.Services
{
    class AudioFileHandler : IFileHandler
    {
        private readonly AppSettings _appSettings;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly HttpClient _httpClient;

        public AudioFileHandler(ITelegramBotClient telegramBotClient, AppSettings appSettings, HttpClient httpClient)
        {
            _appSettings = appSettings;
            _telegramBotClient = telegramBotClient;
            _httpClient = httpClient;
        }

        public async Task Download(string fileId, CancellationToken ct)
        {
            // Получаем информацию о файле
            var file = await _telegramBotClient.GetFileAsync(fileId, ct);
            if (file.FilePath == null)
                return;

            // Формируем URL для скачивания файла
            string fileUrl = $"https://api.telegram.org/file/bot{_appSettings.BotToken}/{file.FilePath}";

            // Генерируем путь для сохранения файла
            string inputAudioFilePath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");

            // Скачиваем файл через HttpClient
            using (var response = await _httpClient.GetAsync(fileUrl, ct))
            using (var fs = new FileStream(inputAudioFilePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }
        }

        public string Process(string languageCode)
        {
            string inputAudioPath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");
            string outputAudioPath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.OutputAudioFormat}");

            Console.WriteLine("Начинаем конвертацию...");
            AudioConverter.TryConvert(inputAudioPath, outputAudioPath);
            Console.WriteLine("Файл конвертирован");

            Console.WriteLine("Начинаем распознавание...");
            var speechText = SpeechDetector.DetectSpeech(outputAudioPath, _appSettings.InputAudioBitrate, languageCode);
            Console.WriteLine("Файл распознан.");
            return speechText;
        }
    }
}
