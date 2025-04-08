using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TimeServer
{
    private const int Port = 8888;
    private static readonly CultureInfo ukrainianCulture = new CultureInfo("uk-UA");

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Thread.CurrentThread.CurrentCulture = ukrainianCulture;
        Thread.CurrentThread.CurrentUICulture = ukrainianCulture;

        Console.WriteLine("Сервер дати та часу запускається...");
        Console.WriteLine("Створено культуру: " + Thread.CurrentThread.CurrentCulture.Name);

        TcpListener listener = null;
        try
        {
            listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();
            Console.WriteLine($"Сервер слухає на порті {Port}...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Підключено клієнта.");
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }
        finally
        {
            listener?.Stop();
        }
    }

    private static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;
        TimeZoneInfo clientTimeZone = TimeZoneInfo.Utc;
        string dateFormat = "G"; // Default format

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Отримано запит: {request}");

                string response = ProcessRequest(request, ref clientTimeZone, ref dateFormat);
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
                Console.WriteLine($"Надіслано відповідь: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка обробки клієнта: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("З'єднання з клієнтом закрито.");
        }
    }

    private static string ProcessRequest(string request, ref TimeZoneInfo timeZone, ref string dateFormat)
    {
        string[] parts = request.Split('|');
        string command = parts[0].ToUpper();

        switch (command)
        {
            case "TIME":
                DateTime utcNow = DateTime.UtcNow;
                DateTime clientTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
                return clientTime.ToString(dateFormat, CultureInfo.InvariantCulture);

            case "SETTZ":
                if (parts.Length > 1)
                {
                    try
                    {
                        timeZone = TimeZoneInfo.FindSystemTimeZoneById(parts[1]);
                        return "Часовий пояс успішно змінено.";
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        return "Помилка: Невірний часовий пояс.";
                    }
                }
                return "Помилка: Вкажіть часовий пояс.";

            case "SETFORMAT":
                if (parts.Length > 1)
                {
                    dateFormat = parts[1];
                    return "Формат дати успішно змінено.";
                }
                return "Помилка: Вкажіть формат дати.";

            case "LISTTZ":
                var timeZones = TimeZoneInfo.GetSystemTimeZones();
                var sb = new StringBuilder();
                foreach (var tz in timeZones)
                {
                    sb.AppendLine(tz.Id);
                }
                return sb.ToString();

            case "QUIT":
                return "До побачення!";

            default:
                return "Невідома команда. Доступні команди: TIME, SETTZ, SETFORMAT, LISTTZ, QUIT";
        }
    }
}