using System;
using System.IO.Ports;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server; // Usa WebSocketSharp (ligera y compatible)

//THIS PROGRAM READS THE COM PORTS AND SENDS INFORMATION BY WEBSOCKETS 


namespace WebSocket
{

    //WE CREATE THE CLASS OF THE WEBSOCKET
    public static class ServerReference
    {
        public static WebSocketServer Server;
    }


    //We create the clase Handler and we use heritage to give some existing methods to this new class
    public class STM32Handler : WebSocketBehavior
    {
        //We create an object serial port unique for whole class(static) and useful only inside this class(private)
        private static SerialPort serialPort;


        protected override void OnOpen()
        {
            Console.WriteLine("Connection successfully!");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            serialPort.WriteLine(e.Data);
        }

        public static void Start()
        {
            serialPort = new SerialPort("COM3", 9600)
            {
                DtrEnable = true,
                RtsEnable = true,
                NewLine = "\n"
            };

            serialPort.DataReceived += (s, e) =>
            {
                try
                {
                    string data = serialPort.ReadExisting();
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        Console.WriteLine("STM32 Says: " + data);

                        foreach (var session in ServerReference.Server.WebSocketServices["/"].Sessions.Sessions)
                        {
                            session.Context.WebSocket.Send(data);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Error leyendo del puerto serie: " + ex.Message);
                }
            };

            try
            {
                serialPort.Open();
                Console.WriteLine("Port COM3 was openned correctly!.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("We could not open port COM3: " + ex.Message);
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {


            var wssv = new WebSocketServer(3000);
            ServerReference.Server = wssv; // Guardas la instancia aquí

            wssv.AddWebSocketService<STM32Handler>("/");

            //WE START THE SOCKET
            wssv.Start();


            //WE OPEN UP THE SERIAL PORT AFTER CREATING THE SOCKET
            STM32Handler.Start();


            Console.WriteLine("Server websocket is ready! ...");
            
            
            //PRESS ENTER TWICE TO CLOSE WINDOW
            Console.ReadLine();
            wssv.Stop();


        }
    }
}
