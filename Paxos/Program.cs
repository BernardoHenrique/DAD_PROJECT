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

        private Dictionary<int, ArrayList> slots =
            new Dictionary<int, ArrayList>();

        private ArrayList<string> output = new ArrayList<string>();

        public PaxosService() {
        }

        public Dictionary<int, ArrayList> getSlots()
        {
            return slots;
        }

        public override Task<CompareAndSwapReply> CompareAndSwap(
            CompareAndSwapRequest request, ServerCallContext context) {
            return Task.FromResult(Send(request));
        }

        public CompareAndSwapReply Send(SendMsgRequest request)
        {
             lock (this) {
                if (!slots.ContainsKey(request.Slot)){
                    slots.Add(request.Slot, request.Value);
                    output = request.Value;
                }
             }

            Console.WriteLine($"Msg received from bank}");
            CompareAndSwapReply reply = new CompareAndSwapReply();

            reply.Value = output;

            return reply;
        }

        public ArrayList<string> getOutput()
        {
            return output;
        }

        public override Task<DecideReply> Decide( DecideRequest request, ServerCallContext context){
            return Task.FromResult(DecideAux(request));
        }

        public DecideReply DecideAux(DecideRequest request)
        {
            lock (this)
            {
                íf(!slots.ContainsKey(request.Slot)){
                    slots.Add(request.Slot, request.Value);
                    output = request.Value;
                }
            }
            DecideReply reply = new DecideReply();
            return reply;
        }

    }
    class Program {
        
        public static void Main(string[] args) {
            const int port = args[0];
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

            List<DecideReply> replies = new List<DecideReply>();

            var paxosPorts = new List<int> { 1004, 1005, 1006};
            var paxosStubList = new List<ChatServerService.ChatServerServiceClient>();

            for(int i = 0; i < paxosPorts.Count(); i++)
            {
                if (serverPorts[i] != port)
                {
                    channel = GrpcChannel.ForAddress("http://" + serverName + ":" + serverPorts[i].ToString());
                    paxosStubList[i] = new ChatServerService.ChatServerServiceClient(channel);
                }
            }

            Console.WriteLine(startupMessage);
            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch(
  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true)
            {
                if (PaxosService.getSlots().ContainsKey(PaxosService.getSlots().Count()){ // ACEDER VALUE INDICE SLOTS){ 
                    for(int i = 0; i < paxosPorts.Count(); i++)
                    {
                        replies[i] = paxosStubList[i].Decide(new DecideRequest
                        {   
                            Slot = PaxosService.getSlots().Count(),
                            Value = PaxosService.getOutput()
                        });
                    }
                }
            }
        }
    }
}

