using System.Net.Sockets;

namespace Webserver_csharp
{
    public class GetMatrizAdapter : IRequestProcessor
    {
        private readonly GetMatriz _getMatriz;

        public GetMatrizAdapter(GetMatriz getMatriz)
        {
            _getMatriz = getMatriz;
        }

        public void ProcessRequest(string[] requestFirstLine, string httpVersion, string contentType, string contentEncoding, ref NetworkStream stream)
        {
            GetMatriz.ProcessMatrizRequest(requestFirstLine, httpVersion, contentType, contentEncoding, ref stream);
        }
        
    }
}