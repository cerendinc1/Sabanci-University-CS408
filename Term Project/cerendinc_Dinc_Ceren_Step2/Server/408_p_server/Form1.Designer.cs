namespace _408_p_server
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBox1_port = new TextBox();
            button_listen = new Button();
            richTextBox1 = new RichTextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 11);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(32, 15);
            label1.TabIndex = 0;
            label1.Text = "Port:";
            // 
            // textBox1_port
            // 
            textBox1_port.Location = new Point(62, 11);
            textBox1_port.Margin = new Padding(2, 1, 2, 1);
            textBox1_port.Name = "textBox1_port";
            textBox1_port.Size = new Size(110, 23);
            textBox1_port.TabIndex = 1;
            // 
            // button_listen
            // 
            button_listen.Location = new Point(196, 12);
            button_listen.Margin = new Padding(2, 1, 2, 1);
            button_listen.Name = "button_listen";
            button_listen.Size = new Size(81, 22);
            button_listen.TabIndex = 2;
            button_listen.Text = "Listen";
            button_listen.UseVisualStyleBackColor = true;
            button_listen.Click += button_listen_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(16, 48);
            richTextBox1.Margin = new Padding(2, 1, 2, 1);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(409, 280);
            richTextBox1.TabIndex = 3;
            richTextBox1.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(436, 348);
            Controls.Add(richTextBox1);
            Controls.Add(button_listen);
            Controls.Add(textBox1_port);
            Controls.Add(label1);
            Margin = new Padding(2, 1, 2, 1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1_port;
        private Button button_listen;
        private RichTextBox richTextBox1;
    }
}