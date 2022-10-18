using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;



internal class Program
{
    static void Main(string[] args)
    {
        var botClient = new TelegramBotClient("5721139832:AAG92iyIQvnzP-TeIUv_iSITIxU0nDq2W0k");

        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, receiverOptions, cts.Token);



        Console.WriteLine("Запущен бот " + botClient.GetMeAsync().Result.FirstName);
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();

    }

    /// <summary>
    /// Метод для работы с сообщениями
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        //Проверяем на пустые сообщения
        if (update.Message is not { } message)
            return;
        var chatFN = message.Chat.FirstName;
        var chatId = message.Chat.Id;

        Console.WriteLine($"Получено сообщение {message.Type} '{message.Text}' от {chatFN}.");

        #region Для работы с текстовыми сообщениями

        // Проверяем текстовые сообщения

        if (message.Text is not null)
        {
            if (message.Text.ToLower().Contains("/start"))
            {
                await botClient.SendTextMessageAsync(chatId, "Добро пожаловать!" +
                    "\nЕсли хотите отправить файл на сервер, то просто пришлите мне его)" +
                    "\nЕсли хотите увидеть список файлов на сервере, то напишите - 'Покажи все'");
                return;
            }

            if (message.Text.ToLower().Contains("привет"))
            {
                await botClient.SendTextMessageAsync(chatId, "Привет)");
                return;
            }

            if (message.Text.ToLower().Contains("здорово"))
            {
                await botClient.SendTextMessageAsync(chatId, "Здоровей видали)");
                return;
            }

            if (message.Text.ToLower().Contains("как дела"))
            {
                await botClient.SendTextMessageAsync(chatId, "Спасибо, все хорошо. Как у Вас?");
                return;
            }

            if (message.Text.ToLower().Contains("как сам"))
            {
                await botClient.SendTextMessageAsync(chatId, "Как универсам. Сам как?");
                return;
            }

            if (message.Text.ToLower().Contains("спасибо"))
            {
                await botClient.SendTextMessageAsync(chatId, "Пожалуйста");
                return;
            }

            if (message.Text.ToLower() == "пока")
            {
                await botClient.SendTextMessageAsync(chatId, "До свидания");
                return;
            }
        }

        #endregion;

        #region Для выгрузки файлов
        if (message is not null)
        {
            if (message.Text.ToLower().Contains("покажи все"))
            {
                string[] fileEntries = Directory.GetFiles("../Downloads");
                string fullList = "";
                for (int i = 0; i <= fileEntries.Length - 1; i++)
                {
                    fullList = fullList + "\n" + fileEntries[i];
                }
                await botClient.SendTextMessageAsync(chatId, fullList);
                await botClient.SendTextMessageAsync(chatId, "Какой файл хотите скачать?" +
                    "\nвведит: имя.формат");
                return;
            }
            if (message.Text is not null)
            {
                string fileName = message.Text;
                await using Stream stream = System.IO.File.OpenRead($@"../Downloads/{fileName}");
                await botClient.SendDocumentAsync(chatId, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, $"{fileName}"));
                return;
            }

        }
        #endregion;

        #region Для работы с фото

        if (message.Photo is not null)
        {
            var fileId = message.Photo.Last().FileId;
            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;

            string destinationFilePath = $@"../Downloads/ {fileId}";
            using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();

        }

        #endregion;

        #region Для работы с документами

        if (message.Document is not null)
        {
            var fileId = message.Document.FileId;
            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;

            string destinationFilePath = $@"../Downloads/  {message.Document.FileName}";
            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();

        }

        #endregion;

        #region Для работы с аудио файлами

        if (message.Audio is not null)
        {
            var fileId = message.Audio.FileId;
            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;

            string destinationFilePath = $@"../Downloads/ {fileId}";
            using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();

        }

        #endregion;

    }


    /// <summary>
    /// Метод для работы с ошибками
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}

