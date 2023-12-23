using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Microsoft.VisualBasic.Logging;
using System.Net;

namespace client_final
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;

        Socket clientSocket;
        string name;
        string ip_num;
        int portNum;
        int playerNumber;
        int connectedPlayerCount;

        List<List<char>> board = new List<List<char>>(){ // board for the game created
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
                    if (board[row][col] == 0) { count++; }
                }
            }
            return count;
        }

        public Form1()
        {
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void button_connect_Click(object sender, EventArgs e)
        {
            terminating = false;

            name = textBox3_name.Text;
            ip_num = textBox1_ip.Text;

            if (name != "")
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (Int32.TryParse(textBox2_port.Text, out portNum))
                { // if parsing successful
                    try
                    {
                        // connect to server
                        IPAddress ip = IPAddress.Parse(ip_num);
                        EndPoint remoteEP = new IPEndPoint(ip, portNum);
                        clientSocket.Connect(remoteEP); // attempts to connect to the server
                        Byte[] buffer = new Byte[64];
                        buffer = Encoding.Default.GetBytes(name);
                        clientSocket.Send(buffer);
                        string msg = await receive_from_server_async();
                        if(msg == "-")
                        {
                            richTextBox1.AppendText("This name already exists, please choose a new name\n");
                            return;
                        }
                        richTextBox1.AppendText("Succesfully connected\n");
                        connected = true;
                        button_connect.Enabled = false;

                        playerNumber = msg[0] - '0';
                        connectedPlayerCount = playerNumber + 1;

                        string msg_board = await receive_from_server_async(); // initial state of the board
                        for(int i=1;i<=9;i++)
                        {
                            string str = "" + msg_board[2 * i - 1];
                            if(str != " ")
                            {
                                board[(i - 1) / 3][(i - 1) % 3] = msg_board[2 * i - 1];
                                if (i == 1) button1.Text = str;
                                else if (i == 2) button2.Text = str;
                                else if (i == 3) button3.Text = str;
                                else if (i == 4) button4.Text = str;
                                else if (i == 5) button5.Text = str;
                                else if (i == 6) button6.Text = str;
                                else if (i == 7) button7.Text = str;
                                else if (i == 8) button8.Text = str;
                                else if (i == 9) button9.Text = str;
                            }
                        }
                        button_enable();
                        // start thread
                        Thread receiveThread = new Thread(() => receiveFromServerThread()); // thread responsible for listening server messages
                        receiveThread.Start();

                    }
                    catch
                    {
                        richTextBox1.AppendText("Could not connect to the server!\n");
                    }
                }
                else
                {
                    richTextBox1.AppendText("Check the port number. Port number is wrong.\n");
                }

            }
            else
            {
                richTextBox1.AppendText("You should enter a name.\n");
            }
        }
        private void button_enable()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
        }
        private void button_disable()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
        }
        private async Task<string> receive_from_server_async()
        {
            Byte[] buffer = new Byte[64];

            int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private void receiveFromServerThread() {
            while (connected)
            {
                Byte[] buffer = new Byte[256];
                clientSocket.Receive(buffer);
                string incomingMessage = Encoding.Default.GetString(buffer);
                incomingMessage = incomingMessage.Replace("\0", string.Empty);
                if(incomingMessage == "q") // server disconnected
                {
                    richTextBox1.AppendText("Server disconnected.\n");
                    button_disable();
                    clientSocket.Close();
                    connected = false;
                    button_connect.Enabled = true;
                }
                else if (incomingMessage[0] == 'd') // client disconnected
                {
                    int idx = incomingMessage[1] - '0';
                    string namnam = incomingMessage.Substring(2);
                    richTextBox1.AppendText("Client named \"" + namnam + "\" disconnected.\n");
                    connectedPlayerCount--;
                    if(playerNumber > idx && playerNumber > 1)
                    {
                        if(playerNumber == 2 && idx == 0) {
                            playerNumber--;
                        }
                        playerNumber--;
                        if(playerNumber == 0)
                        {
                            richTextBox1.AppendText("you are now X\n");
                        } else if (playerNumber == 1)
                        {
                            richTextBox1.AppendText("you are now O\n");
                        } else
                        {
                            richTextBox1.AppendText("you are now " + ("" + (playerNumber-1)) + "th player waiting on the queue\n");
                        }
                    }
                }
                else if ( incomingMessage == "s" ) // game started
                {
                    richTextBox1.AppendText("Game started\n");
                    //button_enable();
                }
                else if (incomingMessage[0] == 'm') // a move has been made to the board
                {
                    int num = incomingMessage[2] - '0';
                    char who = (incomingMessage[1] == '0') ? 'X' : 'O';
                    string namnam = incomingMessage.Substring(3);
                    board[(num - 1) / 3][(num - 1) % 3] = who;
                    richTextBox1.AppendText("Player " + who + " (" + namnam + ") played at cell: " + num + "\n");
                    if (num == 1) button1.Text = "" + who;
                    else if (num == 2) button2.Text = "" + who;
                    else if (num == 3) button3.Text = "" + who;
                    else if (num == 4) button4.Text = "" + who;
                    else if (num == 5) button5.Text = "" + who;
                    else if (num == 6) button6.Text = "" + who;
                    else if (num == 7) button7.Text = "" + who;
                    else if (num == 8) button8.Text = "" + who;
                    else if (num == 9) button9.Text = "" + who;
                }
                else if (incomingMessage == "n") // not your turn
                {
                    richTextBox1.AppendText("Not your turn\n");
                }
                else if (incomingMessage[0] == 'w') // game finished
                {
                    if (incomingMessage[1] == 'd') // draw
                    {
                        richTextBox1.AppendText("Draw\n");
                        Thread.Sleep(2000);
                        clearBoard();
                    }
                    else // a player won the game
                    {
                        richTextBox1.AppendText("Player " + incomingMessage[1] + " wins\n");
                        Thread.Sleep(2000);
                        clearBoard();
                    }
                }
                else if (incomingMessage[0] == '*') // game finished, write scoreboard
                {
                    richTextBox1.AppendText(incomingMessage.Substring(1) + "\n");
                }
                else if (incomingMessage == "p") // not enough players
                {
                    richTextBox1.AppendText("not enough players to play game, wait till someone joins\n");
                }
                else if (incomingMessage[0] == 'g') // new client joined
                {
                    richTextBox1.AppendText(incomingMessage.Substring(1) + " has joined\n");
                }
                else if (incomingMessage[0] == 'f') // cell is already full
                {
                    richTextBox1.AppendText("This cell is already played, click somewhere else.\n");
                }
            }
        }
        public void clearBoard()
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    board[row][col] = ' ';
                }
            }
            button1.Text = "1";
            button2.Text = "2";
            button3.Text = "3";
            button4.Text = "4";
            button5.Text = "5";
            button6.Text = "6";
            button7.Text = "7";
            button8.Text = "8";
            button9.Text = "9";
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            send_message("1");
        }
        private void button2_Click(object sender, EventArgs e)
        {
            send_message("2");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            send_message("3");
        }
        private void button4_Click(object sender, EventArgs e)
        {
            send_message("4");
        }
        private void button5_Click(object sender, EventArgs e)
        {
            send_message("5");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            send_message("6");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            send_message("7");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            send_message("8");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            send_message("9");
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            send_message("q");
            Thread.Sleep(500);
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }
        private void buttonFill_Click(object sender, EventArgs e)
        {
            textBox1_ip.AppendText("127.0.0.1");
            textBox2_port.AppendText("1111");
        }
        private void send_message(string message) // sending a message to a specific client
        {
            if(!connected) { return; }
            Byte[] buffer = new Byte[1000];
            buffer = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(buffer);
            Thread.Sleep(500);
        }
    }
}