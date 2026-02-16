using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string fallbackTokenPlaceholder = "TG_BOT_TOKEN";
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? fallbackTokenPlaceholder;

if (token == fallbackTokenPlaceholder)
{
    Console.WriteLine("Внимание: не задан TELEGRAM_BOT_TOKEN. Задай переменную окружения или вставь токен в код.");
}

using var cts = new CancellationTokenSource();
// Создание клиента бота
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);

// Проверка токена - получаем информацию о боте
try
{
    var me = await bot.GetMe();
    Console.WriteLine($"Бот @{me.Username} запущен. Нажмите Enter для остановки.");
}
catch (Exception ex)
{
    Console.WriteLine($"Не удалось получить информацию о боте: {ex.Message}");
    return;
}

// Подписка на приходящие сообщения
bot.OnMessage += async (Message msg, UpdateType updateType) =>
{
    try
    {
        if (msg is null) return;
        if (msg.Text is null) return;

        // убираем лишние пробелы в начале/конце
        var text = msg.Text.Trim();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg.Chat.Id}: {text}");

        // Обработка команды /start
        if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
        {
            string welcome =
                "Привет! Я — TaskManagerBot.\n\n" +
                "Я помогу тебе хранить простые задачи в чате.\n" +
                "Пока доступны команды:\n" +
                "/start — это приветствие\n" +
                "(в будущем добавим /add, /list и т.д.)\n\n" +
                "Напиши любую фразу — я повторю её (эхо).";

            // Отправка приветственного сообщения
            await bot.SendMessage(msg.Chat, welcome, cancellationToken: cts.Token);
            return;
        }

        // По умолчанию — эхо 
        await bot.SendMessage(msg.Chat, $"Принял: {text}", cancellationToken: cts.Token);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка в обработчике сообщений: {ex.Message}");
    }
};

// Остановка бота
Console.ReadLine();
cts.Cancel();
Console.WriteLine("Бот остановлен.");
