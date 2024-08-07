using System;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ConsoleApp1
{
    internal class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "Credit to Imistrz21 on Github";
        Console.WriteLine("Credit to Imistrz21 on Github");
        Console.WriteLine("This tool was made for educational purposes ONLY! The author is not responisble for any harm done using this!!!");
        Console.WriteLine("By pressing any key you agree to the eula availible at the github, where you downloaded this from.");
        Console.WriteLine("Press any Key to continue...");
        Console.ReadKey();
        Console.Title = "Credit to Imistrz21 on Github";
        Console.Clear();

        Console.Title = "Input an IP address";
        Console.WriteLine("Input an IP address (e.g., 192.168.1)");
        string ip = Console.ReadLine();
        Console.Title = "Input an IP address";
        Console.Clear();

        Console.Title = "Input an IP address range";
        Console.WriteLine("Input an IP address range (e.g., 254 for 192.168.1.1 - 192.168.1.254)");
        int ipr = Convert.ToInt32(Console.ReadLine());
        Console.Title = "Input an IP address range";
        Console.Clear();

        Console.Title = "Input the number of threads";
        Console.WriteLine("Input the number of threads:");
        int numThreads = Convert.ToInt32(Console.ReadLine());
        Console.Title = "Input the number of threads:";
        Console.Clear();

        Console.Title = "Input the Max Time";
        Console.WriteLine("Input the Max Time:");
        int maxTime = Convert.ToInt32(Console.ReadLine());
        Console.Title = "Max Time...";
        Console.Clear();

        var results = new ConcurrentBag<string>();
        var hosts = new ConcurrentBag<string>();

        int rangePerThread = (int)Math.Ceiling((double)ipr / numThreads);
        Task[] tasks = new Task[numThreads];

        for (int i = 0; i < numThreads; i++)
        {
            int start = i * rangePerThread + 1;
            int end = Math.Min(start + rangePerThread - 1, ipr);
            tasks[i] = Task.Run(() => PingRange(ip, start, end, results, hosts));
        }

        await Task.WhenAll(tasks);

        Console.Title = "Press any key to continue...";
        Console.WriteLine("Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"Hosts found: {hosts.Count}");
        Console.WriteLine("===============================");

        foreach (var result in results)
        {
            Console.Title = $"Hosts found: {hosts.Count}";
            Console.WriteLine(result);
        }

        Console.WriteLine("Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    private static async Task PingRange(string baseIp, int start, int end, ConcurrentBag<string> results, ConcurrentBag<string> hosts)
    {
        var tasks = new List<Task>();

        for (int i = start; i <= end; i++)
        {
            string ipu = baseIp + "." + i;
            tasks.Add(Task.Run(async () =>
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ipu, 1000);
                    Console.WriteLine($"ping send {ipu}");
                    if (reply.Status == IPStatus.Success)
                    {
                        var result = $"===============================\n" +
                                     $"Pinging with server {ipu}:\n" +
                                     $"Reply: {reply.Status}\n" +
                                     $"Ping time: {reply.RoundtripTime} ms\n" +
                                     $"Buffer: {reply.Buffer.Length} bytes\n" +
                                     $"===============================";

                        results.Add(result);
                        hosts.Add(ipu);

                        await CheckOpenPorts(ipu, results);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    private static async Task CheckOpenPorts(string ip, ConcurrentBag<string> results)
    {
        int[] ports = { 80, 443, 8080, 22, 21, 23, 25, 110, 16, 67, 69, 123, 137, 161, 1900, 2000, 2049, 27017, 3389, 5353, 5432, 5900, 53, 135, 548, 587, 636, 389, 1433, 1521, 3306, 5439, 5984, 6379, 8081, 8443, 9200, 27015, 5060, 10000, 11211, 27018, 3388, 5433 };

        var portDefinitions = new Dictionary<int, string>
        {
            { 80, "HTTP" }, { 443, "HTTPS" }, { 8080, "HTTP Alternative" }, { 22, "SSH" }, { 21, "FTP" }, { 23, "Telnet" },
            { 25, "SMTP" }, { 110, "POP3" }, { 16, "Reserved" }, { 67, "DHCP" }, { 69, "TFTP" }, { 123, "NTP" },
            { 137, "NetBIOS" }, { 161, "SNMP" }, { 1900, "UPnP" }, { 2000, "Cisco SCCP" }, { 2049, "NFS" },
            { 27017, "MongoDB" }, { 3389, "RDP" }, { 5353, "mDNS" }, { 5432, "PostgreSQL" }, { 5900, "VNC" },
            { 53, "DNS" }, { 135, "RPC" }, { 548, "AFP" }, { 587, "SMTP" }, { 636, "LDAP over SSL" }, { 389, "LDAP" },
            { 1433, "SQL Server" }, { 1521, "Oracle DB" }, { 3306, "MySQL" }, { 5439, "Amazon Redshift" }, { 5984, "CouchDB" },
            { 6379, "Redis" }, { 8081, "HTTP Alternate" }, { 8443, "HTTPS Alternate" }, { 9200, "Elasticsearch" },
            { 27015, "Source Engine" }, { 5060, "SIP" }, { 10000, "Webmin" }, { 11211, "Memcached" }, { 27018, "MongoDB" },
            { 3388, "Microsoft Terminal Services" }, { 5433, "PostgreSQL" }
        };

        var tasks = new List<Task>();

        foreach (int port in ports)
        {
            tasks.Add(Task.Run(async () =>
            {
                var scanner = new PortScanner(ip, port);
                var isOpen = await scanner.ScanAsync();

                if (isOpen)
                {
                    portDefinitions.TryGetValue(port, out string portName);
                    string openPortInfo = $"Open port found on {ip}:{port} ({portName ?? "Not well known"})";
                    results.Add(openPortInfo);
                }
            }));
        }

        await Task.WhenAll(tasks);
    }
}

public class PortScanner
{
    private readonly string _ip;
    private readonly int _port;

    public PortScanner(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }

    public async Task<bool> ScanAsync()
    {
        try
        {
            using (var client = new System.Net.Sockets.TcpClient())
            {
                var task = client.ConnectAsync(_ip, _port);
                bool connected = await Task.WhenAny(task, Task.Delay(80000)) == task;
                return connected;
            }
        }
        catch
        {
            return false;
        }
    }
}
}
