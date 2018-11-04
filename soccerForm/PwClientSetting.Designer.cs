namespace soccerForm
{
    partial class PwClientSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PwClientSetting));
            this.panel1 = new System.Windows.Forms.Panel();
            this.Confirm_txtBox = new System.Windows.Forms.TextBox();
            this.PW_txtBox = new System.Windows.Forms.TextBox();
            this.Logo_Join = new System.Windows.Forms.PictureBox();
            this.ID_txtBox = new System.Windows.Forms.TextBox();
            this.register_btn = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Logo_Join)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.register_btn)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.Confirm_txtBox);
            this.panel1.Controls.Add(this.PW_txtBox);
            this.panel1.Controls.Add(this.Logo_Join);
            this.panel1.Controls.Add(this.ID_txtBox);
            this.panel1.Controls.Add(this.register_btn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(435, 369);
            this.panel1.TabIndex = 3;
            // 
            // Confirm_txtBox
            // 
            this.Confirm_txtBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Confirm_txtBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Confirm_txtBox.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.Confirm_txtBox.Location = new System.Drawing.Point(66, 225);
            this.Confirm_txtBox.Margin = new System.Windows.Forms.Padding(2);
            this.Confirm_txtBox.MaxLength = 20;
            this.Confirm_txtBox.Name = "Confirm_txtBox";
            this.Confirm_txtBox.Size = new System.Drawing.Size(296, 32);
            this.Confirm_txtBox.TabIndex = 10;
            this.Confirm_txtBox.TabStop = false;
            this.Confirm_txtBox.Text = "Confirm New Password";
            this.Confirm_txtBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Confirm_txtBox.Click += new System.EventHandler(this.txt_Click);
            // 
            // PW_txtBox
            // 
            this.PW_txtBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.PW_txtBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PW_txtBox.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.PW_txtBox.Location = new System.Drawing.Point(66, 174);
            this.PW_txtBox.Margin = new System.Windows.Forms.Padding(2);
            this.PW_txtBox.MaxLength = 20;
            this.PW_txtBox.Name = "PW_txtBox";
            this.PW_txtBox.Size = new System.Drawing.Size(296, 32);
            this.PW_txtBox.TabIndex = 7;
            this.PW_txtBox.TabStop = false;
            this.PW_txtBox.Text = "New Password";
            this.PW_txtBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PW_txtBox.Click += new System.EventHandler(this.txt_Click);
            // 
            // Logo_Join
            // 
            this.Logo_Join.Image = ((System.Drawing.Image)(resources.GetObject("Logo_Join.Image")));
            this.Logo_Join.Location = new System.Drawing.Point(123, 14);
            this.Logo_Join.Name = "Logo_Join";
            this.Logo_Join.Size = new System.Drawing.Size(173, 71);
            this.Logo_Join.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Logo_Join.TabIndex = 2;
            this.Logo_Join.TabStop = false;
            // 
            // ID_txtBox
            // 
            this.ID_txtBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ID_txtBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ID_txtBox.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.ID_txtBox.Location = new System.Drawing.Point(66, 124);
            this.ID_txtBox.Margin = new System.Windows.Forms.Padding(2);
            this.ID_txtBox.MaxLength = 20;
            this.ID_txtBox.Name = "ID_txtBox";
            this.ID_txtBox.Size = new System.Drawing.Size(296, 32);
            this.ID_txtBox.TabIndex = 8;
            this.ID_txtBox.TabStop = false;
            this.ID_txtBox.Text = "Previous Password";
            this.ID_txtBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ID_txtBox.Click += new System.EventHandler(this.txt_Click);
            // 
            // register_btn
            // 
            this.register_btn.Image = ((System.Drawing.Image)(resources.GetObject("register_btn.Image")));
            this.register_btn.Location = new System.Drawing.Point(123, 282);
            this.register_btn.Name = "register_btn";
            this.register_btn.Size = new System.Drawing.Size(165, 51);
            this.register_btn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.register_btn.TabIndex = 1;
            this.register_btn.TabStop = false;
            this.register_btn.Click += new System.EventHandler(this.register_btn_Click);
            // 
            // PwClientSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 369);
            this.Controls.Add(this.panel1);
            this.Name = "PwClientSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PwClientSetting";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Logo_Join)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.register_btn)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox Confirm_txtBox;
        private System.Windows.Forms.TextBox PW_txtBox;
        private System.Windows.Forms.PictureBox Logo_Join;
        private System.Windows.Forms.TextBox ID_txtBox;
        private System.Windows.Forms.PictureBox register_btn;
    }
}