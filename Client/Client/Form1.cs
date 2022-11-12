using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        string ip = "localhost";
        string port = "1111";
        int portNum;

        string name;
        bool terminating = false;
        bool connected = false;

        bool isNameRegistered = false;

        Socket clientSocket;

        public void ensureNameRegistered()
        {
            if (!isNameRegistered)
            {
                //send client name to check if it is unique
                //format: 'operation:name'
                //e.g. 'registerName:Alper'
                string messageRegister = "registerName" + ":" + name;
                Byte[] buffer = Encoding.Default.GetBytes(messageRegister);
                clientSocket.Send(buffer);
            }
        }

        public Form1()
        {
            this.FormClosing += new FormClosingEventHandler(closeForm);
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void closeForm(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void ThreadReceiveFunction()
        {
            //first, try to authenticate ourselves
            if (!isNameRegistered)
            {
                while (connected)
                {
                    Byte[] bufferReceived = new Byte[64];
                    clientSocket.Receive(bufferReceived);

                    string messageReceived = Encoding.Default.GetString(bufferReceived);
                    messageReceived = messageReceived.Trim('\0');

                    if (messageReceived == "VALID-NAME")
                    {
                        isNameRegistered = true;
                        Byte[] bufferReceivedNameRegister = new Byte[64];
                        richTextBox_logs.AppendText("Successful register.");

                    }
                    else if (messageReceived == "INVALID-NAME")
                    {
                        richTextBox_logs.AppendText("This name is already taken! Please provide another name.\n");
                    }
                }
            }
            else
            {
                while (connected)
                {
                    //this will keep receiving new messages while it is connected to server
                    try { 
                        //Lets load those received messages into buffer
                        Byte[] bufferReceived = new Byte[64];
                        clientSocket.Receive(bufferReceived);



                        //convert buffer data into string, by default ASCII
                        string messageReceived = Encoding.Default.GetString(bufferReceived);
                        richTextBox_logs.AppendText("Server message: " + messageReceived + "\n");



                        //Byte[] bufferToSend = Encoding.Default.GetBytes(messageToSend);
                        //clientSocket.Send(bufferToSend);

                    }
                    catch
                    {
                        if (!terminating)
                        {
                            //this means there is a problem, but it is related with server (e.g. server stops listening)
                            button_connect.Enabled = true;

                        }
                        else
                        {
                            //this means there is a problem, and it is related with client (e.g. client closes the window)
                        }
                        connected = false;
                        clientSocket.Close();
                        richTextBox_logs.AppendText("A client has disconnected.\n");
                    }
                }
            }
            
        }



        private void button_connect_Click(object sender, EventArgs e)
        {
            terminating = false;
            ///TODO
            ip = textBox_ip.Text;
            port = textBox_port.Text;
            name = textBox_name.Text;

            if (name != "")
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (Int32.TryParse(port, out portNum))
                {
                    try
                    {
                        clientSocket.Connect(ip, portNum); //connect and send your name

                        button_connect.Enabled = false;
                        button_disconnect.Enabled = true;
                        button_send.Enabled = true;
                        textBox_message.Enabled = true;
                        textBox_ip.Enabled = false;
                        textBox_port.Enabled = false;
                        textBox_name.Enabled = false;
                        connected = true;

    
                        richTextBox_logs.AppendText("Connected to the server." + "\n");


                        ensureNameRegistered();


                        Thread receiveThread = new Thread(ThreadReceiveFunction);
                        receiveThread.Start();
                    }
                    catch (Exception)
                    {
                        richTextBox_logs.AppendText("Something went wrong..." + "\n");
                    }

                }
                else
                {
                    richTextBox_logs.AppendText("Invalid port!" + "\n");
                }
            }
            else
            {
                richTextBox_logs.AppendText("Please enter a valid name!\n");
            }

        }

        private void button_disconnect_Click(object sender, EventArgs e)
        {
            connected = false;
            button_connect.Enabled=true;
            button_disconnect.Enabled=false;

            button_send.Enabled=false;
            textBox_message.Enabled=false;
            textBox_ip.Enabled = false;
            textBox_port.Enabled = false;
            textBox_name.Enabled = false;
        }
    }
}