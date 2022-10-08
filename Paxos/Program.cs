using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace chatPaxos {
    public class PaxosService : ChatPaxosService.ChatPaxosServiceBase {

        private GrpcChannel channel;

        private Dictionary<string, ArrayList> unorderedCmd =
            new Dictionary<string, ArrayList>();

        private Dictionary<int, ArrayList> slots =
            new Dictionary<int, ArrayList>();

        private ArrayList output = new ArrayList();

        private int nrRcvd = 0;

        public PaxosService() {
        }

        public int getNr()
        {
            return nrRcvd;
        }

        public Dictionary<string, ArrayList> getUnorderedList()
        {
            return unorderedCmd;
        }

        public override Task<CompareAndSwapReply> CompareAndSwap(
            CompareAndSwapRequest request, ServerCallContext context) {
            return Task.FromResult(Send(request));
        }

        public CompareAndSwapReply Send(SendMsgRequest request)
        {
             lock (this) {
                unorderedCmd.Add(request.Nick, request.Msg);
            }

            Console.WriteLine($"Msg received from client {request.Nick}");
            CompareAndSwapReply reply = new CompareAndSwapReply();

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

            var reply = null;

            var paxosPorts = new List<int> { 1004, 1005, 1006};
            var paxosStubList = new List<ChatServerService.ChatServerServiceClient>();

            for(int i = 0; i < paxosPorts.Count(); i++)
            {
                channel = GrpcChannel.ForAddress("http://" + serverName + ":" + serverPorts[i].ToString());
                paxosStubList[i] = new ChatServerService.ChatServerServiceClient(channel);
            }

            Console.WriteLine(startupMessage);
            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch(
  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true)
            {
                if(PaxosService.getNr() == 3) { 
                    for(int i = 0; i < paxosPorts.Count(); i++)
                    {
                        reply = paxosStubList[i].Decide(new DecideRequest
                        {
                            Nick = port,
                            Msg = PaxosService.getUnorderedList()
                        });
                    }
                }
            }
        }
    }
}

