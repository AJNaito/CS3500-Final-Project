
namespace ClientView
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ServerAddress = new System.Windows.Forms.TextBox();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.EnterPlayName = new System.Windows.Forms.Label();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.PlayerName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ServerAddress
            // 
            this.ServerAddress.Location = new System.Drawing.Point(268, 28);
            this.ServerAddress.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.ServerAddress.Name = "ServerAddress";
            this.ServerAddress.Size = new System.Drawing.Size(232, 44);
            this.ServerAddress.TabIndex = 0;
            this.ServerAddress.Text = "localhost";
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Location = new System.Drawing.Point(28, 35);
            this.ServerLabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(213, 37);
            this.ServerLabel.TabIndex = 1;
            this.ServerLabel.Text = "Server Name:";
            // 
            // EnterPlayName
            // 
            this.EnterPlayName.AutoSize = true;
            this.EnterPlayName.Location = new System.Drawing.Point(565, 35);
            this.EnterPlayName.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.EnterPlayName.Name = "EnterPlayName";
            this.EnterPlayName.Size = new System.Drawing.Size(197, 37);
            this.EnterPlayName.TabIndex = 2;
            this.EnterPlayName.Text = "Enter Name:";
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(1078, 25);
            this.ConnectButton.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(226, 60);
            this.ConnectButton.TabIndex = 4;
            this.ConnectButton.TabStop = false;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // PlayerName
            // 
            this.PlayerName.Location = new System.Drawing.Point(786, 30);
            this.PlayerName.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.PlayerName.Name = "PlayerName";
            this.PlayerName.Size = new System.Drawing.Size(232, 44);
            this.PlayerName.TabIndex = 3;
            this.PlayerName.Text = "player";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(19F, 37F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 1054);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.PlayerName);
            this.Controls.Add(this.EnterPlayName);
            this.Controls.Add(this.ServerLabel);
            this.Controls.Add(this.ServerAddress);
            this.Margin = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.Name = "Form1";
            this.Text = "TankWars";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ServerAddress;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.Label EnterPlayName;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.TextBox PlayerName;
    }
}

