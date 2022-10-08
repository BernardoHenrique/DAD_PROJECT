using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace chatPaxos {
    public class PaxosService : ChatPaxosService.ChatPaxosServiceBase {

        private GrpcChannel channel;

        private Dictionary<string, ArrayList> unorderedCmd =
            new Dictionary<string, ArrayList>();

        private ArrayList output = new ArrayList();

        public PaxosService() {
        }

        public override Task<SendMsgReply> SendMsg(
            SendMsgRequest request, ServerCallContext context) {
            return Task.FromResult(Send(request));
        }

        public SendMsgReply Send(SendMsgRequest request)
        {
             lock (this) {
                unorderedCmd.Add(request.Nick, request.Msg);
            }

            Console.WriteLine($"Msg received from client {request.Nick}");
            SendMsgReply reply = new SendMsgReply();

            reply.Msg = output;

            return reply;
        }

    }
    class Program {
        
        public static void Main(string[] args) {
            const int port = 1004;
            const string hostname = "localhost";
            string startupMessage;
            ServerPort serverPort;

            serverPort = new ServerPort(hostname, port, ServerCredentials.Insecure);
            startupMessage = "Paxos listening on port" + port;

            Server server = new Server
            {
                Services = { ChatServerService.BindService(new PaxosService()) },
                Ports = { serverPort }
            };

            server.Start();

            Console.WriteLine(startupMessage);
            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch(
  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true)
            {
            }
        }
    }
}

