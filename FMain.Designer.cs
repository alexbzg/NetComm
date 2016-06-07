namespace NetComm
{
    partial class FMain
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FMain));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.ddbSettings = new System.Windows.Forms.ToolStripDropDownButton();
            this.miControl = new System.Windows.Forms.ToolStripMenuItem();
            this.miWatch = new System.Windows.Forms.ToolStripMenuItem();
            this.tssConnectionsSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.miConnectionsList = new System.Windows.Forms.ToolStripMenuItem();
            this.miRelaySettings = new System.Windows.Forms.ToolStripMenuItem();
            this.miModuleSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.miExpertSync = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.toolStrip.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.toolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ddbSettings});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStrip.Location = new System.Drawing.Point(24, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip.Size = new System.Drawing.Size(42, 25);
            this.toolStrip.TabIndex = 0;
            // 
            // ddbSettings
            // 
            this.ddbSettings.AutoSize = false;
            this.ddbSettings.BackColor = System.Drawing.SystemColors.Control;
            this.ddbSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ddbSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miControl,
            this.miWatch,
            this.tssConnectionsSeparator,
            this.miConnectionsList,
            this.miRelaySettings,
            this.miModuleSettings,
            this.miExpertSync});
            this.ddbSettings.Image = ((System.Drawing.Image)(resources.GetObject("ddbSettings.Image")));
            this.ddbSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ddbSettings.Name = "ddbSettings";
            this.ddbSettings.Size = new System.Drawing.Size(42, 20);
            this.ddbSettings.ToolTipText = "Настройки";
            // 
            // miControl
            // 
            this.miControl.Name = "miControl";
            this.miControl.Size = new System.Drawing.Size(272, 22);
            this.miControl.Text = "Управление";
            // 
            // miWatch
            // 
            this.miWatch.Name = "miWatch";
            this.miWatch.Size = new System.Drawing.Size(272, 22);
            this.miWatch.Text = "Слежение";
            // 
            // tssConnectionsSeparator
            // 
            this.tssConnectionsSeparator.Name = "tssConnectionsSeparator";
            this.tssConnectionsSeparator.Size = new System.Drawing.Size(269, 6);
            // 
            // miConnectionsList
            // 
            this.miConnectionsList.Name = "miConnectionsList";
            this.miConnectionsList.Size = new System.Drawing.Size(272, 22);
            this.miConnectionsList.Text = "Подключения";
            this.miConnectionsList.Click += new System.EventHandler(this.miConnectionsList_Click);
            // 
            // miRelaySettings
            // 
            this.miRelaySettings.Name = "miRelaySettings";
            this.miRelaySettings.Size = new System.Drawing.Size(272, 22);
            this.miRelaySettings.Text = "Настройки реле";
            this.miRelaySettings.Click += new System.EventHandler(this.miRelaySettings_Click);
            // 
            // miModuleSettings
            // 
            this.miModuleSettings.Name = "miModuleSettings";
            this.miModuleSettings.Size = new System.Drawing.Size(272, 22);
            this.miModuleSettings.Text = "Настройки модуля";
            this.miModuleSettings.Click += new System.EventHandler(this.miModuleSettings_Click);
            // 
            // miExpertSync
            // 
            this.miExpertSync.CheckOnClick = true;
            this.miExpertSync.Name = "miExpertSync";
            this.miExpertSync.Size = new System.Drawing.Size(272, 22);
            this.miExpertSync.Text = "Соединение с ExpertSync";
            this.miExpertSync.Click += new System.EventHandler(this.miExpertSync_Click);
            // 
            // FMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(104, 288);
            this.Controls.Add(this.toolStrip);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FMain";
            this.Text = "Ant Comm";
            this.Load += new System.EventHandler(this.FMain_Load);
            this.LocationChanged += new System.EventHandler(this.FMain_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.FMain_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FMain_KeyDown);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_MouseClick);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripDropDownButton ddbSettings;
        private System.Windows.Forms.ToolStripMenuItem miConnectionsList;
        private System.Windows.Forms.ToolStripSeparator tssConnectionsSeparator;
        private System.Windows.Forms.ToolStripMenuItem miModuleSettings;
        private System.Windows.Forms.ToolStripMenuItem miControl;
        private System.Windows.Forms.ToolStripMenuItem miWatch;
        private System.Windows.Forms.ToolStripMenuItem miRelaySettings;
        private System.Windows.Forms.ToolStripMenuItem miExpertSync; 
    }
}

