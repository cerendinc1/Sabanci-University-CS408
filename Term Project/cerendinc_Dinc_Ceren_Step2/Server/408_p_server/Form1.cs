using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
        Dictionary<String, List<int>> scores = new Dictionary<String, List<int>>(); // scoreboard of players, name -> {win, draw, lose}
        int gamesPlayed = 0;

        bool terminating = false;
        bool listening = false;

        List<List<char>> board = new List<List<char>>(){ // game board
            new List<char>() { ' ', ' ', ' ' },
            new List<char>() { ' ', ' ', ' ' },
            new List<char>() { ' ', ' ', ' ' }
        };
        public int turn()
        {
            int count = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    if (board[row][col] != ' ') { count++; }
                }
            }
            return count;
        }
        public int connectedCount()
        {
            return clientSockets.Count;
        }
        public void clientClosed(int idx) // deletes client from names and sockets lists
        {
            names.RemoveAt(idx);
            clientSockets.RemoveAt(idx);
            if(idx == 0)
            {
                // putting player O, to O again
                if (connectedCount() >= 2)
                {
                    String tempName = names[0];
                    names[0] = names[1];
                    names[1] = tempName;

                    Socket tempSocket = clientSockets[0];
                    clientSockets[0] = clientSockets[1];
                    clientSockets[1] = tempSocket;
                }
            }
        }

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
                serverSocket.Listen(6);

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

                    if (connectedCount() < 4) // only two clients can connect
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
                            if (!scores.Keys.Contains(receivedName))
                            {
                                scores[receivedName] = new List<int>() { 0, 0, 0 };
                            }
                            names.Add(receivedName);
                            clientSockets.Add(newClient);
                            richTextBox1.AppendText(receivedName + " is connected.\n");
                            send_message(newClient, "" + (clientSockets.Count - 1));
                            // start thread
                            Thread receiveThread = new Thread(() => Game_Receive(newClient)); // thread responsible for receiving game data from clients
                            receiveThread.Start();
                            // sends current state of board to new players
                            String msg_moves = "";
                            for (int row = 0; row < 3; row++)
                            {
                                for(int col = 0; col < 3; col++)
                                {
                                    msg_moves += ((row * 3) + col + 1).ToString() + board[row][col];
                                }
                            }
                            send_message(newClient, msg_moves);

                            foreach (Socket cl in clientSockets)
                            {
                                if (cl != newClient)
                                {
                                    send_message(cl, "g" + receivedName); // new client joined
                                }
                            }

                            if (connectedCount() == 2)
                            {
                                foreach (Socket cl in clientSockets)
                                {
                                    send_message(cl, "s"); // game started
                                }
                            } else if(connectedCount() > 2)
                            {
                                send_message(newClient, "s"); // game started
                            }
                        }
                    }
                    else
                    {
                        richTextBox1.AppendText("Game is full!\n");
                        newClient.Close(); // closes the socket for new client as there are already two clients 
                    }
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                        foreach (Socket cl in clientSockets)
                        {
                            send_message(cl, "q"); // server quits
                        }
                    }
                    else
                    {
                        richTextBox1.AppendText("The socket stopped working.\n");
                    }
                }
            }
        }

        private void Game_Receive(Socket sock)
        {
            while (sock.Connected)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    sock.Receive(buffer);
                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Replace("\0", string.Empty);
                    int idxOfClient = clientSockets.IndexOf(sock);
                    if (incomingMessage == "q") // client quit
                    {
                        string namnam = names[idxOfClient];
                        // client disconnected
                        richTextBox1.AppendText(names[idxOfClient] + " disconnected\n");
                        clientClosed(idxOfClient);
                        sock.Close();
                        // send knowledge to other clients
                        foreach (Socket cl in clientSockets)
                        {
                            send_message(cl, "d" + idxOfClient + namnam); // client disconnected
                        }
                    }
                    else
                    {
                        int num = incomingMessage[0] - '0'; // chosen move
                        if(connectedCount() < 2) // not enough players
                        {
                            richTextBox1.AppendText("" + names[idxOfClient] + " tried to press cell " + num + " but there is not enough players\n");
                            send_message(sock, "p");
                        }
                        else if (idxOfClient != turn() % 2) // not your turn
                        {
                            richTextBox1.AppendText("" + names[idxOfClient] + " tried to press cell " + num + " but its not your turn\n");
                            send_message(sock, "n");
                        }
                        else
                        {
                            if (board[(num - 1) / 3][(num - 1) % 3] != ' ') // cell is full
                            {
                                send_message(sock, "f");
                                richTextBox1.AppendText("" + names[idxOfClient] + " tried to press cell " + num + " but its full\n");
                            }
                            else
                            {
                                foreach (Socket cl in clientSockets)
                                {
                                    send_message(cl, "m" + idxOfClient + num + names[idxOfClient]); // a move has made
                                }
                                board[(num - 1) / 3][(num - 1) % 3] = (idxOfClient % 2 == 0) ? 'X' : 'O'; // updating the game board based on the move and symbol of the player
                                richTextBox1.AppendText(names[idxOfClient] + " played at " + num + "\n");
                                game_over();
                            }
                        }


                    }
                }
                catch
                {
                    richTextBox1.AppendText("an error occured while receiving msg\n");
                }
            }
        }

        public void scoreboardOperations()
        {
            string msg = "" + gamesPlayed + " games played\n";
            List<String> scoreBoardNames = new List<String>();
            foreach(String n in scores.Keys)
            {
                scoreBoardNames.Add(n);
            }
            
            for (int i = 0; i < scoreBoardNames.Count; i++)
            {
                string nam = scoreBoardNames[i];
                msg += nam + ": \nw:" + scores[nam][0] + " d:" + scores[nam][1] + " l:" + scores[nam][2] + "\n";
            }
            //server side
            richTextBox1.AppendText(msg);
            //client side
            foreach( Socket cl in clientSockets)
            {
                Thread.Sleep(200);
                send_message(cl, "*" + msg);
            }
        }
        private void game_over() // checks if game is over
        {
            int count = 0;
            for(int row=0; row<3; row++)
            {
                for(int col=0; col<3; col++)
                {
                    if (board[row][col] != ' ') count++;
                }
            }
            if (count == 9) sendEveryoneDraw();
            else
            {
                //horizontal
                for (int i = 0; i < 3; i++)
                    if (board[i][0] != ' ' && board[i][0] == board[i][1] && board[i][0] == board[i][2]) sendEveryoneWinner(board[i][0]);
                //vertical
                for (int i = 0; i < 3; i++)
                    if (board[0][i] != ' ' && board[0][i] == board[1][i] && board[0][i] == board[2][i]) sendEveryoneWinner(board[0][i]);
                //diagonal
                if (board[0][0] != ' ' && board[0][0] == board[1][1] && board[0][0] == board[2][2]) sendEveryoneWinner(board[0][0]);
                if (board[0][2] != ' ' && board[0][2] == board[1][1] && board[0][2] == board[2][0]) sendEveryoneWinner(board[0][2]);
            }
        }
        private void sendEveryoneDraw()
        {
            foreach (Socket cl in clientSockets)
            {
                send_message(cl, "wd");
            }
            scores[names[0]][1]++; // inc x draw
            scores[names[1]][1]++; // inc o draw
            clearBoard();
            gamesPlayed++;
            scoreboardOperations();
        }
        private void sendEveryoneWinner(char winner)
        {
            foreach (Socket cl in clientSockets)
            {
                send_message(cl, "w" + winner);
            }
            if(winner == 'X')
            {
                scores[names[0]][0]++; // inc x win
                scores[names[1]][2]++; // inc o lose
            }
            else
            {
                scores[names[0]][2]++; // inc x lose
                scores[names[1]][0]++; // inc o win
            }
            clearBoard();
            gamesPlayed++;
            scoreboardOperations();
        }
        public void clearBoard()
        {
            for(int row=0; row<3; row++)
            {
                for(int col=0; col<3; col++)
                {
                    board[row][col] = ' ';
                }
            }
        }

        private void send_message(Socket thisClient, string message) // sending a message to a specific client
        {
            Byte[] buffer = new Byte[5000];
            buffer = Encoding.UTF8.GetBytes(message);
            thisClient.Send(buffer);
            Thread.Sleep(100); // to prevent messages from mixing
        }

        private void close_game() // closing the game and cleaning resources up
        {
            foreach (Socket x in clientSockets)
            {
                x.Close();
            }
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(Socket cl in clientSockets)
            {
                send_message(cl, "q");
            }
            close_game();
            Thread.Sleep(500);
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }
    }

}