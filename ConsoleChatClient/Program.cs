// See https://aka.ms/new-console-template for more information
using Grpc.Core;
using Grpc.Net.Client;
using System;


public class ClientService : ChatClientService.ChatClientServiceBase
{

    public override Task<RecvMsgReply> RecvMsg(
        RecvMsgRequest request, ServerCallContext context)
    {
        return Task.FromResult(UpdateGUIwithMsg(request));
    }

    public RecvMsgReply UpdateGUIwithMsg(RecvMsgRequest request)
    {
        lock(Program.ConsoleLock) { 
        Console.WriteLine(request.Msg);
        }
        return new RecvMsgReply
            {
                Ok = true
            };
        }
     }

public class Program {

    public static object ConsoleLock = new object();

    public static void Main(string[] args)
    {
        GrpcChannel channel;

        Console.WriteLine("Insira a sua rassa:");
        string nick = Console.ReadLine();
        Console.WriteLine("Insira o seu porto:");
        string portString = Console.ReadLine();
        AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    
        var serverPorts = new List<int> { 1001, 1002, 1003};

        ChatClientRegisterReply reply = null;

        var serverName = "localhost";
        string hostname = "localhost";
        var chatServerStubList = new List<ChatServerService.ChatServerServiceClient>();
        Server server = new Server
        {
            Services = { ChatClientService.BindService(new ClientService()) },
            Ports = { new ServerPort(hostname, Int32.Parse(portString), ServerCredentials.Insecure) }
        };
        server.Start();

        for(int i = 0; i < serverPorts.Count(); i++)
        {
            channel = GrpcChannel.ForAddress("http://" + serverName + ":" + serverPorts[i].ToString());
            chatServerStubList[i] = new ChatServerService.ChatServerServiceClient(channel);

            reply = chatServerStubList[i].Register(new ChatClientRegisterRequest
            {
                Nick = nick,
                Url = "http://localhost:" + portString
            });
        }

        Console.WriteLine(reply.ToString());
        string msg;
        while (true) {
            msg = Console.ReadLine();

            
        }
    }

    }