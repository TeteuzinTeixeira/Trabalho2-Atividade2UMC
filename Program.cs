using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Webserver_csharp
{
    class Program
    {
        private static TcpListener myListener;
        private static int port = 5051;
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        private static string WWWWebServerPath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\www";
        private static string serverEtag = Guid.NewGuid().ToString("N");

        static void Main(string[] args)
        {
            try
            {

                myListener = new TcpListener(localAddr, port);
                myListener.Start();
                Console.WriteLine($"Web Server Running on {localAddr.ToString()} on port {port}... Press ^C to Stop...");
                Thread th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private static void StartListen()
        {
            while (true)
            {

                TcpClient client = myListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                //read request 
                byte[] requestBytes = new byte[1024];
                int bytesRead = stream.Read(requestBytes, 0, requestBytes.Length);

                string request = Encoding.UTF8.GetString(requestBytes, 0, bytesRead);
                Console.WriteLine("--------------------------- REQUEST  ------------------------------------");
                Console.WriteLine(request);
                var requestHeaders = ParseHeaders(request);

                string[] requestFirstLine = requestHeaders.requestType.Split(" ");
                string httpVersion = requestFirstLine.LastOrDefault();
                string contentType = requestHeaders.headers.GetValueOrDefault("Accept");
                string contentEncoding = requestHeaders.headers.GetValueOrDefault("Acept-Encoding");

                if (request.StartsWith("GET"))
                {
                    // Extrair os parâmetros da URL
                    var req = requestFirstLine[1];
                    string[] requestLine = req.Split("?");
                    var requestedPath = requestLine[0];
                    Dictionary<string, string> parameters = ParseParameters(requestLine.Length > 1 ? requestLine[1] : "");

                    // Verificar se os parâmetros incluem A, b e algorithm
                    if (parameters.ContainsKey("A") && parameters.ContainsKey("b") && parameters.ContainsKey("algorithm"))
                    {
                        string matrixA = parameters["A"];
                        string vectorB = parameters["b"];
                        string algorithm = parameters["algorithm"];

                        // Verificar se o algoritmo especificado é válido
                        if (IsValidAlgorithm(algorithm))
                        {
                            // Montar a resposta com os valores da matriz A, vetor b e algoritmo de resolução
                            string responseBody = $"Matrix A: {matrixA}\r\nVector b: {vectorB}\r\nAlgorithm: {algorithm}";
                            
                            // Enviar a resposta
                            SendResponse(httpVersion, 200, "OK", contentType, contentEncoding, responseBody, ref stream);
                        }
                        else
                        {
                            // Se o algoritmo especificado não for válido, retorne um erro 400 (Bad Request)
                            string responseBody = "Invalid algorithm specified.";
                            SendResponse(httpVersion, 400, "Bad Request", contentType, contentEncoding, responseBody, ref stream);
                        }
                    }
                    else
                    {
                        // Se algum dos parâmetros estiver faltando, retorne um erro 400 (Bad Request)
                        string responseBody = "Missing parameters.";
                        SendResponse(httpVersion, 400, "Bad Request", contentType, contentEncoding, responseBody, ref stream);
                    }
                }
                else
                {
                    string header = SendResponse(httpVersion, 405, "Method Not Allowed", contentType, contentEncoding, "0", ref stream);
                    Console.WriteLine(header);
                }

                client.Close();
            }
        }

        private static Dictionary<string, string> ParseParameters(string parametersString)
        {
            var parameters = new Dictionary<string, string>();
            string[] pairs = parametersString.Split("&");
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split("=");
                if (keyValue.Length == 2)
                {
                    string key = Uri.UnescapeDataString(keyValue[0]);
                    string value = Uri.UnescapeDataString(keyValue[1]);
                    parameters[key] = value;
                }
            }
            return parameters;
        }

        private static string SendResponse(string httpVersion, int statusCode, string statusMsg, string contentType, string contentEncoding, string responseBody, ref NetworkStream networkStream)
        {
            string responseHeaderBuffer = $"HTTP/1.1 {statusCode} {statusMsg}\r\n" +
                                          $"Connection: Keep-Alive\r\n" +
                                          $"Date: {DateTime.UtcNow.ToString()}\r\n" +
                                          $"Server: MacOs PC \r\n" +
                                          $"Etag: \"{serverEtag}\"\r\n" +
                                          $"Content-Encoding: {contentEncoding}\r\n" +
                                          "X-Content-Type-Options: nosniff" +
                                          $"Content-Type: {contentType}\r\n" +
                                          $"Content-Length: {Encoding.UTF8.GetByteCount(responseBody)}\r\n\r\n";

            byte[] responseHeaderBytes = Encoding.UTF8.GetBytes(responseHeaderBuffer);
            byte[] responseBodyBytes = Encoding.UTF8.GetBytes(responseBody);

            networkStream.Write(responseHeaderBytes, 0, responseHeaderBytes.Length);
            networkStream.Write(responseBodyBytes, 0, responseBodyBytes.Length);

            return responseHeaderBuffer + responseBody;
        }

        private static bool IsValidAlgorithm(string algorithm)
        {
            // Lista de opções de algoritmo válidas
            List<string> validAlgorithms = new List<string> { "lu", "jacobi", "gauss-seidel" /* Adicione outras opções conforme necessário */ };

            // Verificar se o algoritmo está na lista de opções válidas
            return validAlgorithms.Contains(algorithm);
        }

        private static (Dictionary<string, string> headers, string requestType) ParseHeaders(string headerString)
        {
            var headerLines = headerString.Split('\r', '\n');
            string firstLine = headerLines[0];
            var headerValues = new Dictionary<string, string>();
            foreach (var headerLine in headerLines)
            {
                var headerDetail = headerLine.Trim();
                var delimiterIndex = headerLine.IndexOf(':');
                if (delimiterIndex >= 0)
                {
                    var headerName = headerLine.Substring(0, delimiterIndex).Trim();
                    var headerValue = headerLine.Substring(delimiterIndex + 1).Trim();
                    headerValues.Add(headerName, headerValue);
                }
            }
            return (headerValues, firstLine);
        }
    }
}
