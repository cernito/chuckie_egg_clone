namespace ChuckieEgg
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
            components = new System.ComponentModel.Container();
            LPlayer = new Label();
            LLevel = new Label();
            LBonus = new Label();
            LScore = new Label();
            LScoreValue = new Label();
            LTime = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            PGamePanel = new GamePanel();
            LMessageOverlay = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            tableLayoutPanel1.SuspendLayout();
            PGamePanel.SuspendLayout();
            SuspendLayout();
            // 
            // LPlayer
            // 
            LPlayer.AutoSize = true;
            LPlayer.BackColor = SystemColors.Control;
            LPlayer.Dock = DockStyle.Fill;
            LPlayer.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            LPlayer.ForeColor = SystemColors.ControlText;
            LPlayer.Location = new Point(72, 63);
            LPlayer.Name = "LPlayer";
            LPlayer.Size = new Size(225, 44);
            LPlayer.TabIndex = 0;
            LPlayer.Text = "PLAYER 1";
            LPlayer.TextAlign = ContentAlignment.MiddleCenter;
            LPlayer.Click += label1_Click;
            // 
            // LLevel
            // 
            LLevel.AutoSize = true;
            LLevel.Dock = DockStyle.Fill;
            LLevel.Location = new Point(303, 63);
            LLevel.Name = "LLevel";
            LLevel.Size = new Size(225, 44);
            LLevel.TabIndex = 1;
            LLevel.Text = "LEVEL   00";
            LLevel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LBonus
            // 
            LBonus.AutoSize = true;
            LBonus.Dock = DockStyle.Fill;
            LBonus.Location = new Point(534, 63);
            LBonus.Name = "LBonus";
            LBonus.Size = new Size(225, 44);
            LBonus.TabIndex = 2;
            LBonus.Text = "BONUS 0000";
            LBonus.TextAlign = ContentAlignment.MiddleCenter;
            LBonus.Click += LBonus_Click;
            // 
            // LScore
            // 
            LScore.AutoSize = true;
            LScore.Dock = DockStyle.Fill;
            LScore.Location = new Point(72, 21);
            LScore.Name = "LScore";
            LScore.Size = new Size(225, 42);
            LScore.TabIndex = 3;
            LScore.Text = "SCORE";
            LScore.TextAlign = ContentAlignment.BottomCenter;
            // 
            // LScoreValue
            // 
            LScoreValue.AutoSize = true;
            LScoreValue.Dock = DockStyle.Fill;
            LScoreValue.Location = new Point(303, 21);
            LScoreValue.Name = "LScoreValue";
            LScoreValue.Size = new Size(225, 42);
            LScoreValue.TabIndex = 4;
            LScoreValue.Text = "000000";
            LScoreValue.TextAlign = ContentAlignment.BottomLeft;
            // 
            // LTime
            // 
            LTime.AutoSize = true;
            LTime.Dock = DockStyle.Fill;
            LTime.Location = new Point(765, 63);
            LTime.Name = "LTime";
            LTime.Size = new Size(225, 44);
            LTime.TabIndex = 5;
            LTime.Text = "TIME 000";
            LTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 6;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6.52173948F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.7391319F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.7391319F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.7391319F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 21.7391319F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 6.52173948F));
            tableLayoutPanel1.Controls.Add(LScore, 1, 1);
            tableLayoutPanel1.Controls.Add(LTime, 4, 2);
            tableLayoutPanel1.Controls.Add(LPlayer, 1, 2);
            tableLayoutPanel1.Controls.Add(LBonus, 3, 2);
            tableLayoutPanel1.Controls.Add(LScoreValue, 2, 1);
            tableLayoutPanel1.Controls.Add(LLevel, 2, 2);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tableLayoutPanel1.Size = new Size(1064, 107);
            tableLayoutPanel1.TabIndex = 6;
            tableLayoutPanel1.Paint += tableLayoutPanel1_Paint;
            // 
            // PGamePanel
            // 
            PGamePanel.BackColor = SystemColors.ControlText;
            PGamePanel.Controls.Add(LMessageOverlay);
            PGamePanel.Dock = DockStyle.Fill;
            PGamePanel.Location = new Point(0, 107);
            PGamePanel.Name = "PGamePanel";
            PGamePanel.Size = new Size(1064, 574);
            PGamePanel.TabIndex = 7;
            PGamePanel.Paint += P_GamePanel_Paint;
            // 
            // LMessageOverlay
            // 
            LMessageOverlay.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LMessageOverlay.AutoSize = true;
            LMessageOverlay.BackColor = Color.Black;
            LMessageOverlay.Font = new Font("Segoe UI", 24F, FontStyle.Regular, GraphicsUnit.Point, 238);
            LMessageOverlay.ForeColor = Color.Yellow;
            LMessageOverlay.Location = new Point(353, 252);
            LMessageOverlay.Name = "LMessageOverlay";
            LMessageOverlay.Size = new Size(0, 45);
            LMessageOverlay.TabIndex = 0;
            LMessageOverlay.TextAlign = ContentAlignment.MiddleCenter;
            LMessageOverlay.Visible = false;
            // 
            // timer1
            // 
            timer1.Interval = 33;
            timer1.Tick += timer1_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1064, 681);
            Controls.Add(PGamePanel);
            Controls.Add(tableLayoutPanel1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            Text = "Chuckie Egg";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            Paint += Form1_Paint;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            Resize += Form1_Resize_1;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            PGamePanel.ResumeLayout(false);
            PGamePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label LPlayer;
        private Label LLevel;
        private Label LBonus;
        private Label LScore;
        private Label LScoreValue;
        private Label LTime;
        private TableLayoutPanel tableLayoutPanel1;
        private GamePanel PGamePanel;
        private System.Windows.Forms.Timer timer1;
        private Label LMessageOverlay;
    }
}
