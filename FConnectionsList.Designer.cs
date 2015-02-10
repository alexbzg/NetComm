namespace Jerome
{
    partial class FConnectionsList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FConnectionsList));
            this.lbConnections = new System.Windows.Forms.ListBox();
            this.bEdit = new System.Windows.Forms.Button();
            this.bDelete = new System.Windows.Forms.Button();
            this.bOK = new System.Windows.Forms.Button();
            this.bNew = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbConnections
            // 
            this.lbConnections.FormattingEnabled = true;
            this.lbConnections.ItemHeight = 20;
            this.lbConnections.Location = new System.Drawing.Point(0, 0);
            this.lbConnections.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbConnections.Name = "lbConnections";
            this.lbConnections.Size = new System.Drawing.Size(439, 444);
            this.lbConnections.TabIndex = 0;
            // 
            // bEdit
            // 
            this.bEdit.Enabled = false;
            this.bEdit.Location = new System.Drawing.Point(12, 452);
            this.bEdit.Name = "bEdit";
            this.bEdit.Size = new System.Drawing.Size(108, 31);
            this.bEdit.TabIndex = 1;
            this.bEdit.Text = "Изменить";
            this.bEdit.UseVisualStyleBackColor = true;
            this.bEdit.Click += new System.EventHandler(this.bEdit_Click);
            // 
            // bDelete
            // 
            this.bDelete.Enabled = false;
            this.bDelete.Location = new System.Drawing.Point(126, 452);
            this.bDelete.Name = "bDelete";
            this.bDelete.Size = new System.Drawing.Size(108, 31);
            this.bDelete.TabIndex = 2;
            this.bDelete.Text = "Удалить";
            this.bDelete.UseVisualStyleBackColor = true;
            this.bDelete.Click += new System.EventHandler(this.bDelete_Click);
            // 
            // bOK
            // 
            this.bOK.Location = new System.Drawing.Point(351, 452);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(75, 31);
            this.bOK.TabIndex = 3;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // bNew
            // 
            this.bNew.Location = new System.Drawing.Point(240, 452);
            this.bNew.Name = "bNew";
            this.bNew.Size = new System.Drawing.Size(105, 31);
            this.bNew.TabIndex = 4;
            this.bNew.Text = "Новое";
            this.bNew.UseVisualStyleBackColor = true;
            this.bNew.Click += new System.EventHandler(this.bNew_Click);
            // 
            // FConnectionsList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 505);
            this.Controls.Add(this.bNew);
            this.Controls.Add(this.bOK);
            this.Controls.Add(this.bDelete);
            this.Controls.Add(this.bEdit);
            this.Controls.Add(this.lbConnections);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FConnectionsList";
            this.Text = "Соединения";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbConnections;
        private System.Windows.Forms.Button bEdit;
        private System.Windows.Forms.Button bDelete;
        private System.Windows.Forms.Button bOK;
        private System.Windows.Forms.Button bNew;
    }
}