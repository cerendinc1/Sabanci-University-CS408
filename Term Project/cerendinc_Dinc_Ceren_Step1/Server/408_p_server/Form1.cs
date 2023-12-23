using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _408_p_server
{
    public partial class Form1 : Form
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Socket object
        List<Socket> clientSockets = new List<Socket>(); // Socket list for clients created
        List<String> names = new List<String>(); // list for names of the clients created

        bool terminating = false;
        bool listening = false;
        bool connected = true;
        //int btn_cnt = 9;
        //int count = 0;
        //string winner = "";
        //int playercnt = 0;

        List<List<char>> board = new List<List<char>>(){ // board for the game created
            new List<char>() { ' ', ' ', ' ' },
            new List<char>() { ' ', ' ', ' ' },
            new List<char>() { ' ', ' ', ' ' }
        };

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_listen_Click(object sender, EventArgs e)
        {
            int serverPort; // stores the entered port number

            if (Int32.TryParse(textBox1_port.Text, out serverPort)) // attempts to parse entered port number into an integer
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort); // represents combination of ip address and port number
                serverSocket.Bind(endPoint); // associating the socket with the specified ip address and port number
                serverSocket.Listen(3);

                listening = true; // server listening for incoming connections
                button_listen.Enabled = false; // while server is already running it prevents multiple clicks to the button

                Thread acceptThread = new Thread(Accept); // to handle incoming client connections
                acceptThread.Start();

                richTextBox1.AppendText("Started listening on port: " + serverPort + "\n");
            }
            else
            {
                richTextBox1.AppendText("Please check port number \n"); // parsing fails
            }
        }
        private void Accept()
        { // handles incoming client connections
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept(); // when a client connects new socket object is created

                    if (names.Count < 2) // only two clients can connect
                    {
                        // receive the name of the client and store it in a byte array
                        Byte[] name_buffer = new Byte[1024];
                        newClient.Receive(name_buffer);

                        string receivedName = Encoding.Default.GetString(name_buffer); // convert received bytes into string
                        receivedName = receivedName.Replace("\0", string.Empty); // remove null characters

                        if (names.Contains(receivedName))
                        {
                            // if the entered name is not unique
                            richTextBox1.AppendText("this client is already exist! \n");
                            send_message(newClient, "-");
                        }
                        else
                        {
                            // new client has successfully connected and added to the list
                            names.Add(receivedName);
                            richTextBox1.AppendText(receivedName + " is connected.\n");
                            clientSockets.Add(newClient);
                            send_message(newClient, "c");
                            if (names.Count == 2)
                            {
                                Thread receiveThread = new Thread(() => Game_Receive(clientSockets)); // thread responsible for receiving game data from clients
                                receiveThread.Start();
                            }
                        }
                    }
                    else
                    {
                        richTextBox1.AppendText("Game is full!");
                        newClient.Close(); // closes the socket for new client as there are already two clients 
                    }
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        richTextBox1.AppendText("The socket stopped working.\n");
                    }
                }
            }
        }

        private void Game_Receive(List<Socket> socket_list) // updated
        {
            connected = true;
            string status = "";
            while (connected && !terminating)
            {
                string message1 = "X"; // x sent to first client
                string message2 = "O"; // o sent to second client
                send_message(clientSockets[0], message1);
                send_message(clientSockets[1], message2);

                try
                {
                    int turn = 0;
                    while (!game_over() && turn < 9) // loops until one of the clients win or there is a draw
                    {
                        Byte[] buffer = new Byte[64];
                        clientSockets[turn % 2].Receive(buffer); // receives move of the client 

                        string incomingMessage = Encoding.Default.GetString(buffer);
                        incomingMessage = incomingMessage.Replace("\0", string.Empty);
                        int num = incomingMessage[0] - '0'; // chosen move 
                        board[(num - 1) / 3][(num - 1) % 3] = (turn % 2 == 0) ? 'X' : 'O'; // updating the game board based on the move and symbol of the player
                        //string move_msg = "";

                        status = num.ToString(); // status repesents current move of the player
                        richTextBox1.AppendText(((turn % 2 == 0) ? "X" : "O") + ": " + status + "\n");

                        // status sent back to clients
                        send_message(clientSockets[0], status);
                        send_message(clientSockets[1], status);

                        turn++; // switch to next player
                    }
                    status = "F";
                    if (game_over()) // game results
                    {
                        if (turn % 2 == 1) status = "X wins\n";
                        else status = "O wins\n";
                        richTextBox1.AppendText(status);
                    }
                    else richTextBox1.AppendText("Draw\n");
                    send_message(clientSockets[0], status);
                    send_message(clientSockets[1], status);
                    close_game();
                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("A client has disconnected\n");
                    }
                    foreach (Socket socket in socket_list)
                    {
                        socket.Close();

                    }
                    connected = false;
                }
            }
        }

        private bool game_over()
        {
            //horizontal
            for (int i = 0; i < 3; i++)
                if (board[i][0] != ' ' && board[i][0] == board[i][1] && board[i][0] == board[i][2]) return true;
            //vertical
            for (int i = 0; i < 3; i++)
                if (board[0][i] != ' ' && board[0][i] == board[1][i] && board[0][i] == board[2][i]) return true;
            //diagonal
            if (board[0][0] != ' ' && board[0][0] == board[1][1] && board[0][0] == board[2][2]) return true;
            if (board[0][2] != ' ' && board[0][2] == board[1][1] && board[0][2] == board[2][0]) return true;
            return false;
        }

        private void send_message(Socket thisClient, string message) // sending a message to a specific client
        {
            Byte[] buffer = new Byte[1000];
            buffer = Encoding.UTF8.GetBytes(message);
            thisClient.Send(buffer);
        }

        private void close_game() // closing the game and cleaning resources up
        {
            foreach (Socket x in clientSockets)
            {
                x.Close();
            }
            connected = false;
            names = new List<String>();
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }
    }

}