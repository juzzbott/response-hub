namespace Enivate.ResponseHub.MessageGenerator
{
	partial class frmMain
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
			this.components = new System.ComponentModel.Container();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblMapPagesToolTip = new System.Windows.Forms.Label();
			this.txtMapPages = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.chkNonEmergency = new System.Windows.Forms.CheckBox();
			this.chkEmergency = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnGenerate = new System.Windows.Forms.Button();
			this.prgGenerating = new System.Windows.Forms.ProgressBar();
			this.lblCapcodeToolTip = new System.Windows.Forms.Label();
			this.lblGenerationAmountToolTip = new System.Windows.Forms.Label();
			this.ddlGenerationAmount = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtCapcode = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.label4 = new System.Windows.Forms.Label();
			this.generatorTimer = new System.Windows.Forms.Timer(this.components);
			this.brsMessages = new System.Windows.Forms.WebBrowser();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.lblMapPagesToolTip);
			this.groupBox1.Controls.Add(this.txtMapPages);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.chkNonEmergency);
			this.groupBox1.Controls.Add(this.chkEmergency);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.btnGenerate);
			this.groupBox1.Controls.Add(this.prgGenerating);
			this.groupBox1.Controls.Add(this.lblCapcodeToolTip);
			this.groupBox1.Controls.Add(this.lblGenerationAmountToolTip);
			this.groupBox1.Controls.Add(this.ddlGenerationAmount);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.txtCapcode);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(12, 13);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(940, 220);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Generation options";
			// 
			// lblMapPagesToolTip
			// 
			this.lblMapPagesToolTip.AutoSize = true;
			this.lblMapPagesToolTip.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblMapPagesToolTip.Location = new System.Drawing.Point(87, 89);
			this.lblMapPagesToolTip.Name = "lblMapPagesToolTip";
			this.lblMapPagesToolTip.Size = new System.Drawing.Size(17, 17);
			this.lblMapPagesToolTip.TabIndex = 13;
			this.lblMapPagesToolTip.Text = "?";
			// 
			// txtMapPages
			// 
			this.txtMapPages.Location = new System.Drawing.Point(10, 110);
			this.txtMapPages.Name = "txtMapPages";
			this.txtMapPages.Size = new System.Drawing.Size(317, 21);
			this.txtMapPages.TabIndex = 12;
			this.txtMapPages.Text = "333, 339, 6442, 6421";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(7, 91);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 15);
			this.label5.TabIndex = 11;
			this.label5.Text = "Map page(s):";
			// 
			// chkNonEmergency
			// 
			this.chkNonEmergency.AutoSize = true;
			this.chkNonEmergency.Checked = true;
			this.chkNonEmergency.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkNonEmergency.Location = new System.Drawing.Point(611, 55);
			this.chkNonEmergency.Name = "chkNonEmergency";
			this.chkNonEmergency.Size = new System.Drawing.Size(114, 19);
			this.chkNonEmergency.TabIndex = 10;
			this.chkNonEmergency.Text = "Non-emergency";
			this.chkNonEmergency.UseVisualStyleBackColor = true;
			// 
			// chkEmergency
			// 
			this.chkEmergency.AutoSize = true;
			this.chkEmergency.Checked = true;
			this.chkEmergency.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkEmergency.Location = new System.Drawing.Point(517, 55);
			this.chkEmergency.Name = "chkEmergency";
			this.chkEmergency.Size = new System.Drawing.Size(88, 19);
			this.chkEmergency.TabIndex = 9;
			this.chkEmergency.Text = "Emergency";
			this.chkEmergency.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(514, 36);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(92, 15);
			this.label3.TabIndex = 8;
			this.label3.Text = "Message types:";
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(10, 151);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(75, 23);
			this.btnGenerate.TabIndex = 7;
			this.btnGenerate.Text = "Generate";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// prgGenerating
			// 
			this.prgGenerating.Location = new System.Drawing.Point(10, 180);
			this.prgGenerating.Name = "prgGenerating";
			this.prgGenerating.Size = new System.Drawing.Size(317, 23);
			this.prgGenerating.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.prgGenerating.TabIndex = 6;
			// 
			// lblCapcodeToolTip
			// 
			this.lblCapcodeToolTip.AutoSize = true;
			this.lblCapcodeToolTip.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCapcodeToolTip.Location = new System.Drawing.Point(64, 33);
			this.lblCapcodeToolTip.Name = "lblCapcodeToolTip";
			this.lblCapcodeToolTip.Size = new System.Drawing.Size(17, 17);
			this.lblCapcodeToolTip.TabIndex = 5;
			this.lblCapcodeToolTip.Text = "?";
			// 
			// lblGenerationAmountToolTip
			// 
			this.lblGenerationAmountToolTip.AutoSize = true;
			this.lblGenerationAmountToolTip.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGenerationAmountToolTip.Location = new System.Drawing.Point(468, 34);
			this.lblGenerationAmountToolTip.Name = "lblGenerationAmountToolTip";
			this.lblGenerationAmountToolTip.Size = new System.Drawing.Size(17, 17);
			this.lblGenerationAmountToolTip.TabIndex = 4;
			this.lblGenerationAmountToolTip.Text = "?";
			// 
			// ddlGenerationAmount
			// 
			this.ddlGenerationAmount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ddlGenerationAmount.FormattingEnabled = true;
			this.ddlGenerationAmount.Items.AddRange(new object[] {
            "1-3",
            "2-8",
            "5-15",
            "10-30"});
			this.ddlGenerationAmount.Location = new System.Drawing.Point(357, 54);
			this.ddlGenerationAmount.Name = "ddlGenerationAmount";
			this.ddlGenerationAmount.Size = new System.Drawing.Size(126, 23);
			this.ddlGenerationAmount.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(354, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(116, 15);
			this.label2.TabIndex = 2;
			this.label2.Text = "Generation amount:";
			// 
			// txtCapcode
			// 
			this.txtCapcode.Location = new System.Drawing.Point(10, 54);
			this.txtCapcode.Name = "txtCapcode";
			this.txtCapcode.Size = new System.Drawing.Size(317, 21);
			this.txtCapcode.TabIndex = 1;
			this.txtCapcode.Text = "0241249";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 35);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Capcode:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(12, 241);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(128, 15);
			this.label4.TabIndex = 2;
			this.label4.Text = "Generated messages:";
			// 
			// generatorTimer
			// 
			this.generatorTimer.Interval = 3000;
			this.generatorTimer.Tick += new System.EventHandler(this.generatorTimer_Tick);
			// 
			// brsMessages
			// 
			this.brsMessages.AllowNavigation = false;
			this.brsMessages.AllowWebBrowserDrop = false;
			this.brsMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.brsMessages.IsWebBrowserContextMenuEnabled = false;
			this.brsMessages.Location = new System.Drawing.Point(12, 260);
			this.brsMessages.MinimumSize = new System.Drawing.Size(20, 20);
			this.brsMessages.Name = "brsMessages";
			this.brsMessages.Size = new System.Drawing.Size(940, 256);
			this.brsMessages.TabIndex = 3;
			this.brsMessages.WebBrowserShortcutsEnabled = false;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(964, 528);
			this.Controls.Add(this.brsMessages);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.groupBox1);
			this.Name = "frmMain";
			this.Text = "Message generator";
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox ddlGenerationAmount;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblGenerationAmountToolTip;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.ProgressBar prgGenerating;
		private System.Windows.Forms.Button btnGenerate;
		private System.Windows.Forms.Label lblMapPagesToolTip;
		private System.Windows.Forms.TextBox txtMapPages;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox chkNonEmergency;
		private System.Windows.Forms.CheckBox chkEmergency;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label lblCapcodeToolTip;
		private System.Windows.Forms.TextBox txtCapcode;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Timer generatorTimer;
		private System.Windows.Forms.WebBrowser brsMessages;
	}
}

