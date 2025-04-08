using System;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TimeClient
{
    private const string Server = "127.0.0.1";
    private const int Port = 8888;
    private static readonly CultureInfo ukrainianCulture = new CultureInfo("uk-UA");

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Thread.CurrentThread.CurrentCulture = ukrainianCulture;
        Thread.CurrentThread.CurrentUICulture = ukrainianCulture;

        Console.WriteLine("Клієнт дати та часу запускається...");
        Console.WriteLine("Створено культуру: " + Thread.CurrentThread.CurrentCulture.Name);

        TimeZoneInfo timeZone = TimeZoneInfo.Utc;
        string dateFormat = "G";

        try
        {
            using (TcpClient client = new TcpClient(Server, Port))
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Підключено до сервера.");

                bool running = true;
                while (running)
                {
                    DisplayMenu();
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Будь ласка, введіть команду.");
                        continue;
                    }

                    string command = input.ToUpper();
                    string request = command;

                    if (command == "SETTZ" || command == "SETFORMAT")
                    {
                        Console.Write("Введіть значення: ");
                        string value = Console.ReadLine();
                        request += "|" + value;
                    }

                    byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                    stream.Write(requestBytes, 0, requestBytes.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine("Відповідь сервера: " + response);

                    if (command == "QUIT")
                    {
                        running = false;
                    }
                    else if (command == "LISTTZ")
                    {
                        Console.WriteLine("Доступні часові пояси:");
                        Console.WriteLine(response);
                    }
                }
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Помилка підключення: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }

        Console.WriteLine("Клієнт завершує роботу. Натисніть будь-яку клавішу...");
        Console.ReadKey();
    }

    private static void DisplayMenu()
    {
        Console.WriteLine("\nОберіть дію:");
        Console.WriteLine("1. Отримати поточний час (TIME)");
        Console.WriteLine("2. Змінити часовий пояс (SETTZ)");
        Console.WriteLine("3. Змінити формат дати (SETFORMAT)");
        Console.WriteLine("4. Показати список часових поясів (LISTTZ)");
        Console.WriteLine("5. Вийти (QUIT)");
        Console.Write("Ваш вибір: ");
    }
}