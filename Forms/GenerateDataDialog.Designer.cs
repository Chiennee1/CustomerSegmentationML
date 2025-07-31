namespace CustomerSegmentationML.Forms
{
    partial class GenerateDataDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblCustomerCount;
        private System.Windows.Forms.NumericUpDown numCustomerCount;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpSettings;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblCustomerCount = new System.Windows.Forms.Label();
            this.numCustomerCount = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.numCustomerCount)).BeginInit();
            this.grpSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCustomerCount
            // 
            this.lblCustomerCount.AutoSize = true;
            this.lblCustomerCount.Location = new System.Drawing.Point(15, 30);
            this.lblCustomerCount.Name = "lblCustomerCount";
            this.lblCustomerCount.Size = new System.Drawing.Size(150, 17);
            this.lblCustomerCount.TabIndex = 0;
            this.lblCustomerCount.Text = "Số lượng khách hàng:";
            // 
            // numCustomerCount
            // 
            this.numCustomerCount.Location = new System.Drawing.Point(18, 55);
            this.numCustomerCount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numCustomerCount.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numCustomerCount.Name = "numCustomerCount";
            this.numCustomerCount.Size = new System.Drawing.Size(200, 22);
            this.numCustomerCount.TabIndex = 1;
            this.numCustomerCount.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.lblCustomerCount);
            this.grpSettings.Controls.Add(this.numCustomerCount);
            this.grpSettings.Location = new System.Drawing.Point(12, 12);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(260, 100);
            this.grpSettings.TabIndex = 2;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "Cài đặt";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(30, 130);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 30);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "Tạo dữ liệu";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(150, 130);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Hủy";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // GenerateDataDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 181);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.grpSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateDataDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tạo dữ liệu mẫu";
            ((System.ComponentModel.ISupportInitialize)(this.numCustomerCount)).EndInit();
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}