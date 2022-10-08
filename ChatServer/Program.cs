using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace chatServer {
    // ChatServerService is the namespace defined in the protobuf
    // ChatServerServiceBase is the generated base implementation of the service
    public class ServerService : ChatServerService.ChatServerServiceBase {

        private GrpcChannel channel;
        private Dictionary<string, ChatClientService.ChatClientServiceClient> clientMap =
            new Dictionary<string, ChatClientService.ChatClientServiceClient>();

        private Dictionary<string, double> bankAccounts =
            new Dictionary<string, double>();

        private Dictionary<string, string> unorderedCmd =
            new Dictionary<string, string>();

        private int nrCmd = 0;

        public ServerService() {
        }

        public override Task<ChatClientRegisterReply> Register(
            ChatClientRegisterRequest request, ServerCallContext context) {
            Console.WriteLine("Host: " + context.Host);
            return Task.FromResult(Reg(request));
        }

        public override Task<SendMsgReply> SendMsg(
            SendMsgRequest request, ServerCallContext context) {
            return Task.FromResult(Send(request));
        }

        public SendMsgReply Send(SendMsgRequest request)
        {
             lock (this) {
                unorderedCmd.Add(request.Nick, request.Msg);
                nrCmd += 1;
            }

            Console.WriteLine($"Msg received from client {request.Nick}");
            SendMsgReply reply = new SendMsgReply();

            return reply;
        }


        public ChatClientRegisterReply Reg(ChatClientRegisterRequest request) {
                //Thread.Sleep(5001);
                channel = GrpcChannel.ForAddress(request.Url);
                ChatClientService.ChatClientServiceClient client =
                    new ChatClientService.ChatClientServiceClient(channel);
            lock (this) {
                clientMap.Add(request.Nick, client);
                bankAccounts.Add(request.Nick, 0);
            }
            Console.WriteLine($"Registered client {request.Nick} with URL {request.Url}");
            ChatClientRegisterReply reply = new ChatClientRegisterReply();
            lock (this) {
                foreach (string nick in clientMap.Keys) {
                    reply.Users.Add(new User { Nick = nick });
                }
            }
            return reply;
        }

        public override Task<BcastMsgReply> BcastMsg(BcastMsgRequest request, ServerCallContext context) {
            return Task.FromResult(Bcast(request));
        }


        public BcastMsgReply Bcast(BcastMsgRequest request) {
            // random wait to simulate slow msg broadcast: Thread.Sleep(5000);
            //Console.WriteLine("msg arrived. lazy server waiting for server admin to press key.");
            //Console.ReadKey();
            lock (this) {
                foreach (string nick in clientMap.Keys) {
                    if (nick != request.Nick) {
                        try {
                            clientMap[nick].RecvMsg(new RecvMsgRequest
                            {
                                Msg = request.Nick + ": " + request.Msg 
                            });
                        } catch (Exception e) {
                            Console.WriteLine(e.Message);
                            clientMap.Remove(nick);
                        }
                    }
                }
            }
            Console.WriteLine($"Broadcast message {request.Msg} from {request.Nick}");
            return new BcastMsgReply
            {
                Ok = true
            };
        }
    }
    class Program {
        
        public static void Main(string[] args) {
            const int port = 1001;
            const string hostname = "localhost";
            string startupMessage;
            ServerPort serverPort;

            var paxosPorts = new List<int> { 1004, 1005, 1006};
            var paxosStubList = new List<ChatServerService.ChatServerServiceClient>();

            serverPort = new ServerPort(hostname, port, ServerCredentials.Insecure);
            startupMessage = "Insecure ChatServer server listening on port " + port;

            Server server = new Server
            {
                Services = { ChatServerService.BindService(new ServerService()) },
                Ports = { serverPort }
            };

            server.Start();

            
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
                lock (this)
                {
                    if(nrCmd == 5){
                        for(int i = 0; i < paxosPorts.Count(); i++)
                        {
                            reply = paxosStubList[i].CompareAndSwap(new CompareAndSwapRequest
                            {
                                Nick = port,
                                Msg = unorderedCmd
                            });
                        }
                        nrCmd = 0;
                    }
                }
            }
        }
    }
}

