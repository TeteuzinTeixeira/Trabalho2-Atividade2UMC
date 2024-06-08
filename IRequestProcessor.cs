using System.Net.Sockets;

namespace Webserver_csharp;

public interface IRequestProcessor
{
    void ProcessRequest(string[] requestFirstLine, string httpVersion, string contentType, string contentEncoding, ref NetworkStream stream);
}