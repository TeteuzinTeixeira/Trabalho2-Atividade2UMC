using System.Net.Sockets;
using System.Text;

namespace Webserver_csharp
{
    public class GetMatriz
    {
        public static void ProcessMatrizRequest(string[] requestFirstLine, string httpVersion, string contentType,
            string contentEncoding, ref NetworkStream stream)
        {
            // Extrair os parâmetros da URL
            var req = requestFirstLine[1];
            string[] requestLine = req.Split("?");
            var requestedPath = requestLine[0];
            Dictionary<string, string> parameters = ParseParameters(requestLine.Length > 1 ? requestLine[1] : "");

            if (parameters.ContainsKey("A") && parameters.ContainsKey("b") && parameters.ContainsKey("algorithm"))
            {
                string matrixA = parameters["A"];
                string vectorB = parameters["b"];
                string algorithm = parameters["algorithm"];

                if (IsValidAlgorithm(algorithm))
                {
                    string responseBody = $"Matrix A: {matrixA}\r\nVector b: {vectorB}\r\nAlgorithm: {algorithm}";
                    SendResponse(httpVersion, 200, "OK", contentType, contentEncoding, responseBody, ref stream);
                }
                else
                {
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

        public static bool IsValidAlgorithm(string algorithm)
        {
            List<string> validAlgorithms = new List<string> { "lu", "jacobi", "gauss-seidel" /* Adicione outras opções conforme necessário */ };

            return validAlgorithms.Contains(algorithm);
        }

        public static (Dictionary<string, string> headers, string requestType) ParseHeaders(string headerString)
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

        public static Dictionary<string, string> ParseParameters(string parametersString)
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

        public static string SendResponse(string httpVersion, int statusCode, string statusMsg, string contentType, string contentEncoding, string responseBody, ref NetworkStream networkStream)
        {
            string responseHeaderBuffer = $"HTTP/1.1 {statusCode} {statusMsg}\r\n" +
                                          $"Connection: Keep-Alive\r\n" +
                                          $"Date: {DateTime.UtcNow.ToString()}\r\n" +
                                          $"Server: MacOs PC \r\n" +
                                          $"Etag: \"{Program.serverEtag}\"\r\n" +
                                          $"Content-Encoding: {contentEncoding}\r\n" +
                                          "X-Content-Type-Options: nosniff\r\n" +
                                          $"Content-Type: {contentType}\r\n" +
                                          $"Content-Length: {Encoding.UTF8.GetByteCount(responseBody)}\r\n\r\n";

            byte[] responseHeaderBytes = Encoding.UTF8.GetBytes(responseHeaderBuffer);
            byte[] responseBodyBytes = Encoding.UTF8.GetBytes(responseBody);

            networkStream.Write(responseHeaderBytes, 0, responseHeaderBytes.Length);
            networkStream.Write(responseBodyBytes, 0, responseBodyBytes.Length);

            return responseHeaderBuffer + responseBody;
        }
    }
}
