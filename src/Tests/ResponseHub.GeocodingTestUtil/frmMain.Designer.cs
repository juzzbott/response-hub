namespace ResponseHub.GeocodingTestUtil
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
            this.btnGenerate = new System.Windows.Forms.Button();
            this.prgGenerating = new System.Windows.Forms.ProgressBar();
            this.lblLocationToolTip = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtResponseBody = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtResponseHeaders = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblResonseStatus = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.browserFormattedJson = new System.Windows.Forms.WebBrowser();
            this.rdoFormattedJson = new System.Windows.Forms.RadioButton();
            this.rdoRawResponse = new System.Windows.Forms.RadioButton();
            this.lblResponseTime = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnGenerate);
            this.groupBox1.Controls.Add(this.prgGenerating);
            this.groupBox1.Controls.Add(this.lblLocationToolTip);
            this.groupBox1.Controls.Add(this.txtAddress);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1032, 101);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Location";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(11, 66);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 10;
            this.btnGenerate.Text = "Submit";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // prgGenerating
            // 
            this.prgGenerating.Location = new System.Drawing.Point(142, 66);
            this.prgGenerating.Name = "prgGenerating";
            this.prgGenerating.Size = new System.Drawing.Size(340, 23);
            this.prgGenerating.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgGenerating.TabIndex = 9;
            // 
            // lblLocationToolTip
            // 
            this.lblLocationToolTip.AutoSize = true;
            this.lblLocationToolTip.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLocationToolTip.Location = new System.Drawing.Point(97, 19);
            this.lblLocationToolTip.Name = "lblLocationToolTip";
            this.lblLocationToolTip.Size = new System.Drawing.Size(17, 17);
            this.lblLocationToolTip.TabIndex = 8;
            this.lblLocationToolTip.Text = "?";
            // 
            // txtAddress
            // 
            this.txtAddress.Location = new System.Drawing.Point(11, 40);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.Size = new System.Drawing.Size(471, 20);
            this.txtAddress.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Address/location:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.lblResponseTime);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.rdoRawResponse);
            this.groupBox2.Controls.Add(this.rdoFormattedJson);
            this.groupBox2.Controls.Add(this.browserFormattedJson);
            this.groupBox2.Controls.Add(this.txtResponseBody);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtResponseHeaders);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.lblResonseStatus);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(13, 133);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1032, 558);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Response details";
            // 
            // txtResponseBody
            // 
            this.txtResponseBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResponseBody.Location = new System.Drawing.Point(11, 190);
            this.txtResponseBody.Multiline = true;
            this.txtResponseBody.Name = "txtResponseBody";
            this.txtResponseBody.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResponseBody.Size = new System.Drawing.Size(1015, 362);
            this.txtResponseBody.TabIndex = 16;
            this.txtResponseBody.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Response body:";
            // 
            // txtResponseHeaders
            // 
            this.txtResponseHeaders.Location = new System.Drawing.Point(11, 61);
            this.txtResponseHeaders.Multiline = true;
            this.txtResponseHeaders.Name = "txtResponseHeaders";
            this.txtResponseHeaders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResponseHeaders.Size = new System.Drawing.Size(471, 89);
            this.txtResponseHeaders.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Response headers:";
            // 
            // lblResonseStatus
            // 
            this.lblResonseStatus.AutoSize = true;
            this.lblResonseStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResonseStatus.Location = new System.Drawing.Point(97, 20);
            this.lblResonseStatus.Name = "lblResonseStatus";
            this.lblResonseStatus.Size = new System.Drawing.Size(0, 13);
            this.lblResonseStatus.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Response Status:";
            // 
            // browserFormattedJson
            // 
            this.browserFormattedJson.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.browserFormattedJson.Location = new System.Drawing.Point(11, 190);
            this.browserFormattedJson.MinimumSize = new System.Drawing.Size(20, 20);
            this.browserFormattedJson.Name = "browserFormattedJson";
            this.browserFormattedJson.Size = new System.Drawing.Size(1015, 362);
            this.browserFormattedJson.TabIndex = 17;
            // 
            // rdoFormattedJson
            // 
            this.rdoFormattedJson.AutoSize = true;
            this.rdoFormattedJson.Checked = true;
            this.rdoFormattedJson.Location = new System.Drawing.Point(118, 168);
            this.rdoFormattedJson.Name = "rdoFormattedJson";
            this.rdoFormattedJson.Size = new System.Drawing.Size(103, 17);
            this.rdoFormattedJson.TabIndex = 18;
            this.rdoFormattedJson.Text = "Formatted JSON";
            this.rdoFormattedJson.UseVisualStyleBackColor = true;
            this.rdoFormattedJson.CheckedChanged += new System.EventHandler(this.rdoFormattedJson_CheckedChanged);
            // 
            // rdoRawResponse
            // 
            this.rdoRawResponse.AutoSize = true;
            this.rdoRawResponse.Location = new System.Drawing.Point(227, 168);
            this.rdoRawResponse.Name = "rdoRawResponse";
            this.rdoRawResponse.Size = new System.Drawing.Size(98, 17);
            this.rdoRawResponse.TabIndex = 19;
            this.rdoRawResponse.Text = "Raw Response";
            this.rdoRawResponse.UseVisualStyleBackColor = true;
            this.rdoRawResponse.CheckedChanged += new System.EventHandler(this.rdoRawResponse_CheckedChanged);
            // 
            // lblResponseTime
            // 
            this.lblResponseTime.AutoSize = true;
            this.lblResponseTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResponseTime.Location = new System.Drawing.Point(439, 20);
            this.lblResponseTime.Name = "lblResponseTime";
            this.lblResponseTime.Size = new System.Drawing.Size(0, 13);
            this.lblResponseTime.TabIndex = 21;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(350, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "Response Time:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1057, 703);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmMain";
            this.Text = "Geocode Test Util";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblLocationToolTip;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ProgressBar prgGenerating;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblResonseStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtResponseBody;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtResponseHeaders;
        private System.Windows.Forms.RadioButton rdoRawResponse;
        private System.Windows.Forms.RadioButton rdoFormattedJson;
        private System.Windows.Forms.WebBrowser browserFormattedJson;
        private System.Windows.Forms.Label lblResponseTime;
        private System.Windows.Forms.Label label6;
    }
}

