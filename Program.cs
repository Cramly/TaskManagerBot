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
// Хранилище задач в памяти
var tasks = new Dictionary<long, List<string>>();


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
                // Команда /add
        if (text.StartsWith("/add", StringComparison.OrdinalIgnoreCase))
        {
            var parts = text.Split(' ', 2);

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                await bot.SendMessage(msg.Chat, 
                    "После команды /add напиши текст задачи.\nПример:\n/add Купить хлеб", 
                    cancellationToken: cts.Token);
                return;
            }

            var taskText = parts[1].Trim();

            if (!tasks.ContainsKey(msg.Chat.Id))
                tasks[msg.Chat.Id] = new List<string>();

            tasks[msg.Chat.Id].Add(taskText);

            await bot.SendMessage(msg.Chat, $"Задача добавлена: {taskText}", cancellationToken: cts.Token);
            return;
        }

                // Команда /list
        if (text.Equals("/list", StringComparison.OrdinalIgnoreCase))
        {
            if (!tasks.ContainsKey(msg.Chat.Id) || tasks[msg.Chat.Id].Count == 0)
            {
                await bot.SendMessage(msg.Chat, "У тебя пока нет задач.", cancellationToken: cts.Token);
                return;
            }

            var userTasks = tasks[msg.Chat.Id];
            var response = "Твои задачи:\n\n";

            for (int i = 0; i < userTasks.Count; i++)
            {
                response += $"{i + 1}. {userTasks[i]}\n";
            }

            await bot.SendMessage(msg.Chat, response, cancellationToken: cts.Token);
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
