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
        ChatServerService.ChatServerServiceClient chatServerStub;

        Console.WriteLine("Insira o seu nick:");
        string nick = Console.ReadLine();
        Console.WriteLine("Insira o seu porto:");
        string portString = Console.ReadLine();
        AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        string serverHostname = "localhost";
        object serverPort = "1001";
        channel = GrpcChannel.ForAddress("http://" + serverHostname + ":" + serverPort.ToString());
        chatServerStub = new ChatServerService.ChatServerServiceClient(channel);
        string hostname = "localhost";
        Server server = new Server
        {
            Services = { ChatClientService.BindService(new ClientService()) },
            Ports = { new ServerPort(hostname, Int32.Parse(portString), ServerCredentials.Insecure) }
        };
        server.Start();

        ChatClientRegisterReply reply = chatServerStub.Register(new ChatClientRegisterRequest
        {
            Nick = nick,
            Url = "http://localhost:" + portString
        });
        Console.WriteLine(reply.ToString());
        string msg;
        while (true) {
            Console.Write("me: ");
            msg = Console.ReadLine();
            chatServerStub.BcastMsg(new BcastMsgRequest { Nick = nick, Msg = msg});
        }
    }

    }