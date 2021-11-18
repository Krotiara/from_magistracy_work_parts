using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using TMPro;

namespace CableWalker.Simulator
{

    
    public class CableWalkerClient : MonoBehaviour
    {
        private string address;
        private int port;
        private NetworkStream serverStream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private Thread listenThread;
        private Socket server;

        public TextMeshProUGUI LogsText { get; set; }


        
        public delegate void CWStatusContainer(Dictionary<string, dynamic> status);
        public delegate void CWCommandContainer(int commandId, ConsoleCommandStatus consoleCommandStatus, string resultMessage);
        public delegate void CwConnection(bool isConnected);
        public event CWStatusContainer onGettingCWStatus;
        public event CWStatusContainer onConnectionGettingCWStatus;
        public event CWCommandContainer onGettingCommandStatus;
        public event CwConnection onConnect;

        private bool isFirstGettingStatus = true;
        private int failsGettingMessageCount = 0;

        public void Send(Message message)
        {
            AddLog($"Sending message: type - {message.messageType}; value - {message.messageValue["cmd"]} \n");
            streamWriter.Write(message.getJsonString());
            streamWriter.Flush(); 
        }

        private void OnApplicationQuit()
        {
            DoClose();
        }



        private void Listen()
        {
            while(true)
            {
                try
                {
                    string data = streamReader.ReadLine();
                    ProcessTheMessage(new Message(data));
                    failsGettingMessageCount = 0;

                }
                catch(Exception e)
                {
                    //Присобачить к событию
                    Debug.Log("Error getting message");
                    Debug.Log(e);
                    failsGettingMessageCount++;
                    if (failsGettingMessageCount > 5)
                        DoClose();
                }
            }
        }

        private void AddLog(string text)
        {
            LogsText.text += text;
        }


        //TODO парсить ресалт на статусы
        private void ProcessTheMessage(Message message)
        {
            var msg = message.messageValue;
            if (message.messageType == "status")
            {
                if (isFirstGettingStatus)
                {
                    isFirstGettingStatus = false;
                    onConnectionGettingCWStatus(msg);
                }
                onGettingCWStatus(msg);
               
            }
            else if(message.messageType == "alert")
            {
                AddLog("alert-" + msg["module"] + ": " + msg["type"]);
            }
            else if (message.messageType == "result")
            {
                var cmd_id = (int)msg["cmd_id"];
                var res = (bool)msg["res"];
                var res_arg = msg["res_arg"];
                AddLog($"Getting message: Command - {msg["cmd"]}; IsDone - {res}; res_arg - {res_arg};\n");

                if (res_arg == "ack")
                {
                    onGettingCommandStatus(cmd_id, ConsoleCommandStatus.Accepted, "Command " + msg["cmd"] + " accepted");
                    //PrintMsg("Command " + msg["cmd"] + " accepted");
                }
                else if (res_arg == "no_module")
                {
                    onGettingCommandStatus(cmd_id, ConsoleCommandStatus.NoModule, "Command " + msg["cmd"] + " not accepted: no such module");
                    // PrintMsg("Command " + msg["cmd"] + " not accepted: no such module");;
                }
                else if (res_arg == "not_op")
                {
                    onGettingCommandStatus(cmd_id, ConsoleCommandStatus.NoTop, "You don't have operator rights!");
                    // PrintMsg("You don't have operator rights!");
                }
                else if (!res)
                {
                    onGettingCommandStatus(cmd_id, ConsoleCommandStatus.Crash, $"problem , ! {res_arg}");
                    //PrintMsg("problem , ! res_arg");
                }
                else if (res)
                {
                    onGettingCommandStatus(cmd_id, ConsoleCommandStatus.Success, "Command " + msg["cmd"] + " success");
                }

                else
                {
                    onGettingCommandStatus(cmd_id, ConsoleCommandStatus.Crash, msg["res_arg"]);
                }


                
            }

        }


        public void DoConnect(string ipAdress, int port)
        {
           
            this.address = ipAdress;
            this.port = port;
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(address), port);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ip);
            }
            catch (SocketException e)
            {
                onConnect(false);
                Debug.Log($"SocketException: {e}");
                return;
            }

            serverStream = new NetworkStream(server);
            streamReader = new StreamReader(serverStream);
            streamWriter = new StreamWriter(serverStream);

            var rand = new System.Random();
            string name = "Simulator" + rand.Next(9999);
            int level = 2;
            Dictionary<string, dynamic> conmsg = new Dictionary<string, dynamic>()
            {
                ["name"] = name,
                ["level"] = level
            };
            Send(new Message("connect", conmsg));
            System.Threading.Thread.Sleep(500);
            Send(new Message("getop", null));
            
            StartListen();
        }

        public void StartListen()
        {
            onConnect(true);
            listenThread = new Thread(Listen);
            listenThread.Start();
        }

        public void DoClose()
        {
            if(listenThread != null)
                listenThread.Abort();
            if(server != null)
                server.Close();
           
        }




    }
}
