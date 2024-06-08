using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Webserver_csharp
{
    class Program
    {
        private static TcpListener myListener;
        private static int port = 5051;
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        private static string WWWWebServerPath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\www";
        internal static string serverEtag = Guid.NewGuid().ToString("N");

        static void Main(string[] args)
        {
            try
            {
                myListener = new TcpListener(localAddr, port);
                myListener.Start();
                Console.WriteLine($"Web Server Running on {localAddr.ToString()} on port {port}... Press ^C to Stop...");
                Thread th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }catch (System.Exception) {
                Console.WriteLine("erro");
                throw;
            }
        }

        private static void StartListen()
        {
            while (true)
            {
                TcpClient client = myListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                
                byte[] requestBytes = new byte[1024];
                int bytesRead = stream.Read(requestBytes, 0, requestBytes.Length);

                string request = Encoding.UTF8.GetString(requestBytes, 0, bytesRead);
                Console.WriteLine("--------------------------- REQUEST  ------------------------------------");
                Console.WriteLine(request);
                var requestHeaders = GetMatriz.ParseHeaders(request);

                string[] requestFirstLine = requestHeaders.requestType.Split(" ");
                string httpVersion = requestFirstLine.LastOrDefault();
                string contentType = requestHeaders.headers.GetValueOrDefault("Accept");
                string contentEncoding = requestHeaders.headers.GetValueOrDefault("Acept-Encoding");

                if (request.StartsWith("GET"))
                {
                    IRequestProcessor requestProcessor = new GetMatrizAdapter(new GetMatriz());
                    requestProcessor.ProcessRequest(requestFirstLine, httpVersion, contentType, contentEncoding, ref stream);
                }
                client.Close();
            }
        }
    }
}
