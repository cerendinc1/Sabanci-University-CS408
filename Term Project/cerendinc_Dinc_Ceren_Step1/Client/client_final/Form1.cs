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
        bool isX;//turn into a numbered version
        //int button_count = 9;
        int turn = 0;
        //bool b1_moved = false;
        List<int> board = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        bool wait = false;


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

            name = textBox3_name.Text; // entered name
            ip_num = textBox1_ip.Text; // entered ip number

            if (name != "")
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (Int32.TryParse(textBox2_port.Text, out portNum))
                { // if parsing successful
                    try
                    {
                        //richTextBox1.AppendText(ip_num.Replace(".", ",") + ",");
                        IPAddress ip = IPAddress.Parse(ip_num);
                        EndPoint remoteEP = new IPEndPoint(ip, portNum);
                        clientSocket.Connect(remoteEP); // attempts to connect to the server
                        Byte[] buffer = new Byte[64];
                        buffer = Encoding.Default.GetBytes(name);
                        clientSocket.Send(buffer);
                        connected = true;

                        string isConnected = await receive_from_server_async(); // receive data from server
                        if (isConnected != "c")
                        {
                            richTextBox1.AppendText("This name already exists, please choose a new name\n");
                            return;
                        }
                        else
                        {
                            richTextBox1.AppendText("Succesfully connected\n");
                        }

                        string who = await receive_from_server_async(); // receive data from server about the symbol assigned to client

                        if (who == "X")
                        {
                            isX = true;
                            richTextBox1.AppendText("You are " + who + "\n");
                        }
                        else if (who == "O")
                        {
                            isX = false;
                            richTextBox1.AppendText("You are " + who + "\n");
                        }

                        while (!game_over() && turn < 9) //while game is not over
                        {
                            if (turn == 0)
                            {
                                richTextBox1.AppendText("Game started\n");
                            }
                            if ((isX && turn % 2 == 0) || (!isX && turn % 2 != 0)) // checks if its current clients turn to play
                            {
                                button_enable(); // buttons enabled to allow the player to make a move
                                //bool temp = await wait_till_clicked();
                            }

                            //Task completedTask = await Task.WhenAny(receive_from_server_async(), button1_Click());
                            string msg = await receive_from_server_async();
                            //richTextBox1.AppendText(msg + "\n");
                            if (!((isX && turn % 2 == 0) || (!isX && turn % 2 != 0))) // turn of other client
                            {
                                int num;
                                Int32.TryParse(msg, out num);
                                board[num - 1] = 2;
                                richTextBox1.AppendText("Opponent played " + msg + "\n");
                                if (num == 1)
                                {
                                    button1.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 2)
                                {
                                    button2.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 3)
                                {
                                    button3.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 4)
                                {
                                    button4.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 5)
                                {
                                    button5.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 6)
                                {
                                    button6.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 7)
                                {
                                    button7.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 8)
                                {
                                    button8.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                                else if (num == 9)
                                {
                                    button9.Text = (isX) ? "O" : "X";
                                    board[num - 1] = 2;
                                }
                            }
                            turn++;
                        }
                        if (game_over()) // game results
                        {
                            richTextBox1.AppendText("Game Over\n");
                            richTextBox1.AppendText(((turn % 2 == 1) ? "X" : "O") + " Wins");
                        }
                        else richTextBox1.AppendText("Draw\n");
                    }
                    catch
                    {
                        Console.WriteLine("Could not connect to the server!\n");
                    }
                }
            }
        }
        private async Task<bool> wait_till_clicked()
        {
            while (!wait)
            {
                Thread.Sleep(1000);
            }
            return true;
        }
        public bool game_over() // checks the winning conditions
        {
            //horizontal
            for (int i = 0; i < 3; i++)
            {
                if (board[0 + 3 * i] != 0 && board[0 + 3 * i] == board[1 + 3 * i] && board[1 + 3 * i] == board[2 + 3 * i])
                {
                    return true;
                }
            }
            //vertical
            for (int i = 0; i < 3; i++)
            {
                if (board[0 + i] != 0 && board[0 + i] == board[3 + i] && board[0 + i] == board[6 + i])
                {
                    return true;
                }
            }
            //diagonal
            if (board[0] != 0 && board[0] == board[4] && board[0] == board[8]) return true;
            if (board[2] != 0 && board[2] == board[4] && board[2] == board[6]) return true;
            return false;
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
        //private string receive_from_server()
        //{
        //    Byte[] buffer = new Byte[64];
        //    clientSocket.Receive(buffer);

        //    string receivedToken = Encoding.Default.GetString(buffer);
        //    receivedToken = receivedToken.Replace("\0", string.Empty);

        //    return receivedToken;
        //}
        private async Task<string> receive_from_server_async()
        {
            Byte[] buffer = new Byte[64];
            //clientSocket.Receive(buffer);

            //string receivedToken = Encoding.Default.GetString(buffer);
            //receivedToken = receivedToken.Replace("\0", string.Empty);

            int bytesRead = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);

            //return receivedToken;
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (board[0] == 1 || board[0] == 2) // if the button already played by one of the players
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 1\n");
            Byte[] num = new Byte[8];

            if (isX) // updates the button based on the symbol of the player
            {
                button1.Text = "X";
            }
            else
            {
                button1.Text = "O";
            }
            string m = "1";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num); // sends to server
            board[0] = 1;
            button_disable();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (board[1] == 1 || board[1] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 2\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button2.Text = "X";
            }
            else
            {
                button2.Text = "O";
            }
            string m = "2";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[1] = 1;
            button_disable();
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (board[2] == 1 || board[2] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 3\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button3.Text = "X";
            }
            else
            {
                button3.Text = "O";
            }
            string m = "3";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[2] = 1;
            button_disable();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (board[3] == 1 || board[3] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 4\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button4.Text = "X";
            }
            else
            {
                button4.Text = "O";
            }
            string m = "4";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[3] = 1;
            button_disable();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            if (board[4] == 1 || board[4] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 5\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button5.Text = "X";
            }
            else
            {
                button5.Text = "O";
            }
            string m = "5";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[4] = 1;
            button_disable();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (board[5] == 1 || board[5] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 6\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button6.Text = "X";
            }
            else
            {
                button6.Text = "O";
            }
            string m = "6";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[5] = 1;
            button_disable();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (board[6] == 1 || board[6] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 7\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button7.Text = "X";
            }
            else
            {
                button7.Text = "O";
            }
            string m = "7";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[6] = 1;
            button_disable();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (board[7] == 1 || board[7] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 8\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button8.Text = "X";
            }
            else
            {
                button8.Text = "O";
            }
            string m = "8";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[7] = 1;
            button_disable();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (board[8] == 1 || board[8] == 2)
            {
                richTextBox1.AppendText("This cell already played\n");
                return;
            }
            richTextBox1.AppendText("You have played 9\n");
            Byte[] num = new Byte[8];

            if (isX)
            {
                button9.Text = "X";
            }
            else
            {
                button9.Text = "O";
            }
            string m = "9";
            num = Encoding.UTF8.GetBytes(m);
            clientSocket.Send(num);
            board[8] = 1;
            button_disable();
        }
        private void buttonFill_Click(object sender, EventArgs e)
        {
            textBox1_ip.AppendText("127.0.0.1");
            textBox2_port.AppendText("1111");
        }
    }
}