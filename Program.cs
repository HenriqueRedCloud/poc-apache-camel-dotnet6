using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

using Apache.NMS;
using Apache.NMS.ActiveMQ;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static string BrokerUri;
    private static string QueueName;
    private static string erpBaseUrl;
    private static string ordersEndpoint;
    private static string spreadsheetId;
    private static string range;

    static async Task Main(string[] args)
    {
        // Build the configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Retrieve configuration values
        erpBaseUrl = configuration["ERPApi:BaseUrl"];
        ordersEndpoint = configuration["ERPApi:OrdersEndpoint"];
        spreadsheetId = configuration["GoogleSheets:SpreadsheetId"];
        range = configuration["GoogleSheets:Range"];
        BrokerUri = configuration["ActiveMQ:BrokerUri"];
        QueueName = configuration["ActiveMQ:QueueName"];

        // Output configuration values to verify they're loaded correctly
        Console.WriteLine($"ERP Base URL: {erpBaseUrl}");
        Console.WriteLine($"Orders Endpoint: {ordersEndpoint}");
        Console.WriteLine($"Google Spreadsheet ID: {spreadsheetId}");
        Console.WriteLine($"Range: {range}");
        Console.WriteLine($"ActiveMQ Broker URI: {BrokerUri}");
        Console.WriteLine($"Queue Name: {QueueName}");

        await FetchOrdersFromERP();
        await ReadDataFromGoogleSheets();

        SendMessage("Hello Apache Camel with .NET!");
        ReceiveMessage();
    }

    static async Task FetchOrdersFromERP()
    {
        try
        {
            string apiUrl = $"{erpBaseUrl}{ordersEndpoint}"; // Combine base URL and endpoint
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Fetched orders from ERP: {responseBody}");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
    }

    static async Task ReadDataFromGoogleSheets()
    {
        UserCredential credential;
        using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                new[] { SheetsService.Scope.SpreadsheetsReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));
            Console.WriteLine("Credential file saved to: " + credPath);
        }

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Google Sheets API .NET Quickstart",
        });

        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();
        var values = response.Values;

        if (values != null && values.Count > 0)
        {
            foreach (var row in values)
            {
                Console.WriteLine($"{row[0]}, {row[1]}, {row[2]}, {row[3]}");
            }
        }
        else
        {
            Console.WriteLine("No data found in Google Sheets.");
        }
    }

    static void SendMessage(string message)
    {
        IConnectionFactory factory = new ConnectionFactory(BrokerUri);
        using IConnection connection = factory.CreateConnection();
        using ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

        IDestination destination = session.GetQueue(QueueName);
        using IMessageProducer producer = session.CreateProducer(destination);

        ITextMessage textMessage = session.CreateTextMessage(message);
        producer.Send(textMessage);

        Console.WriteLine($"Message sent: {message}");
    }

    static void ReceiveMessage()
    {
        IConnectionFactory factory = new ConnectionFactory(BrokerUri);
        using IConnection connection = factory.CreateConnection();
        using ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

        IDestination destination = session.GetQueue(QueueName);
        using IMessageConsumer consumer = session.CreateConsumer(destination);

        connection.Start();
        ITextMessage message = consumer.Receive() as ITextMessage;

        if (message != null)
        {
            Console.WriteLine($"Message received: {message.Text}");
        }
        else
        {
            Console.WriteLine("No message received!");
        }
    }
}
