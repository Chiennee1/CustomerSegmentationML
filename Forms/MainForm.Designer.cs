using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem helpMenu;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblDatasetCount;

        private GroupBox grpDataset;
        private ComboBox cmbDataset;
        private Button btnGenerateData;
        private Button btnViewData;

        private GroupBox grpTraining;
        private Button btnStartTraining;
        private ProgressBar progressBar;

        private GroupBox grpResults;
        private Button btnViewResults;
        private Button btnPredictCustomer;

        private GroupBox grpSettings;
        private Button btnSettings;
        private Button btnHelp;

        private TableLayoutPanel mainLayout;

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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDatasetCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpDataset = new System.Windows.Forms.GroupBox();
            this.cmbDataset = new System.Windows.Forms.ComboBox();
            this.btnGenerateData = new System.Windows.Forms.Button();
            this.btnViewData = new System.Windows.Forms.Button();
            this.grpTraining = new System.Windows.Forms.GroupBox();
            this.btnStartTraining = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.grpResults = new System.Windows.Forms.GroupBox();
            this.btnViewResults = new System.Windows.Forms.Button();
            this.btnPredictCustomer = new System.Windows.Forms.Button();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.mainLayout.SuspendLayout();
            this.grpDataset.SuspendLayout();
            this.grpTraining.SuspendLayout();
            this.grpResults.SuspendLayout();
            this.grpSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.helpMenu});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1350, 33);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip_ItemClicked);
            // 
            // fileMenu
            // 
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(54, 29);
            this.fileMenu.Text = "&File";
            // 
            // helpMenu
            // 
            this.helpMenu.Name = "helpMenu";
            this.helpMenu.Size = new System.Drawing.Size(65, 29);
            this.helpMenu.Text = "&Help";
            // 
            // statusStrip
            // 
            this.statusStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblDatasetCount});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip.Size = new System.Drawing.Size(196, 32);
            this.statusStrip.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(84, 25);
            this.lblStatus.Text = "Sẵn sàng";
            // 
            // lblDatasetCount
            // 
            this.lblDatasetCount.Name = "lblDatasetCount";
            this.lblDatasetCount.Size = new System.Drawing.Size(93, 25);
            this.lblDatasetCount.Text = "0 datasets";
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 2;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.9247F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0753F));
            this.mainLayout.Controls.Add(this.grpDataset, 0, 0);
            this.mainLayout.Controls.Add(this.grpTraining, 1, 0);
            this.mainLayout.Controls.Add(this.grpResults, 0, 1);
            this.mainLayout.Controls.Add(this.grpSettings, 1, 1);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(11, 12, 11, 12);
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.mainLayout.Size = new System.Drawing.Size(1350, 1000);
            this.mainLayout.TabIndex = 2;
            // 
            // grpDataset
            // 
            this.grpDataset.Controls.Add(this.cmbDataset);
            this.grpDataset.Controls.Add(this.btnGenerateData);
            this.grpDataset.Controls.Add(this.btnViewData);
            this.grpDataset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDataset.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpDataset.Location = new System.Drawing.Point(17, 18);
            this.grpDataset.Margin = new System.Windows.Forms.Padding(6);
            this.grpDataset.Name = "grpDataset";
            this.grpDataset.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpDataset.Size = new System.Drawing.Size(651, 313);
            this.grpDataset.TabIndex = 0;
            this.grpDataset.TabStop = false;
            this.grpDataset.Text = "📊 Dataset Management";
            this.grpDataset.Enter += new System.EventHandler(this.grpDataset_Enter);
            // 
            // cmbDataset
            // 
            this.cmbDataset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataset.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbDataset.Location = new System.Drawing.Point(22, 38);
            this.cmbDataset.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbDataset.Name = "cmbDataset";
            this.cmbDataset.Size = new System.Drawing.Size(337, 33);
            this.cmbDataset.TabIndex = 0;
            // 
            // btnGenerateData
            // 
            this.btnGenerateData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGenerateData.Location = new System.Drawing.Point(22, 88);
            this.btnGenerateData.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnGenerateData.Name = "btnGenerateData";
            this.btnGenerateData.Size = new System.Drawing.Size(158, 44);
            this.btnGenerateData.TabIndex = 1;
            this.btnGenerateData.Text = "🎲 Tạo dữ liệu mẫu";
            this.btnGenerateData.UseVisualStyleBackColor = true;
            this.btnGenerateData.Click += new System.EventHandler(this.btnGenerateData_Click);
            // 
            // btnViewData
            // 
            this.btnViewData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewData.Location = new System.Drawing.Point(202, 88);
            this.btnViewData.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnViewData.Name = "btnViewData";
            this.btnViewData.Size = new System.Drawing.Size(158, 44);
            this.btnViewData.TabIndex = 2;
            this.btnViewData.Text = "👁️ Xem dữ liệu";
            this.btnViewData.UseVisualStyleBackColor = true;
            this.btnViewData.Click += new System.EventHandler(this.btnViewData_Click);
            // 
            // grpTraining
            // 
            this.grpTraining.Controls.Add(this.btnStartTraining);
            this.grpTraining.Controls.Add(this.progressBar);
            this.grpTraining.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTraining.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpTraining.Location = new System.Drawing.Point(680, 18);
            this.grpTraining.Margin = new System.Windows.Forms.Padding(6);
            this.grpTraining.Name = "grpTraining";
            this.grpTraining.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpTraining.Size = new System.Drawing.Size(653, 313);
            this.grpTraining.TabIndex = 1;
            this.grpTraining.TabStop = false;
            this.grpTraining.Text = "🤖 Model Training";
            // 
            // btnStartTraining
            // 
            this.btnStartTraining.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartTraining.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartTraining.Location = new System.Drawing.Point(22, 38);
            this.btnStartTraining.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnStartTraining.Name = "btnStartTraining";
            this.btnStartTraining.Size = new System.Drawing.Size(180, 50);
            this.btnStartTraining.TabIndex = 0;
            this.btnStartTraining.Text = "🚀 Bắt đầu Training";
            this.btnStartTraining.UseVisualStyleBackColor = false;
            this.btnStartTraining.Click += new System.EventHandler(this.btnStartTraining_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(22, 100);
            this.progressBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(338, 29);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 1;
            // 
            // grpResults
            // 
            this.grpResults.Controls.Add(this.btnViewResults);
            this.grpResults.Controls.Add(this.btnPredictCustomer);
            this.grpResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpResults.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpResults.Location = new System.Drawing.Point(17, 343);
            this.grpResults.Margin = new System.Windows.Forms.Padding(6);
            this.grpResults.Name = "grpResults";
            this.grpResults.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpResults.Size = new System.Drawing.Size(651, 313);
            this.grpResults.TabIndex = 2;
            this.grpResults.TabStop = false;
            this.grpResults.Text = "📈 Results & Analysis";
            // 
            // btnViewResults
            // 
            this.btnViewResults.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewResults.Location = new System.Drawing.Point(22, 38);
            this.btnViewResults.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnViewResults.Name = "btnViewResults";
            this.btnViewResults.Size = new System.Drawing.Size(158, 44);
            this.btnViewResults.TabIndex = 0;
            this.btnViewResults.Text = "📊 Xem kết quả";
            this.btnViewResults.UseVisualStyleBackColor = true;
            this.btnViewResults.Click += new System.EventHandler(this.btnViewResults_Click);
            // 
            // btnPredictCustomer
            // 
            this.btnPredictCustomer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPredictCustomer.Location = new System.Drawing.Point(202, 38);
            this.btnPredictCustomer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPredictCustomer.Name = "btnPredictCustomer";
            this.btnPredictCustomer.Size = new System.Drawing.Size(158, 44);
            this.btnPredictCustomer.TabIndex = 1;
            this.btnPredictCustomer.Text = "🎯 Dự đoán khách hàng";
            this.btnPredictCustomer.UseVisualStyleBackColor = true;
            this.btnPredictCustomer.Click += new System.EventHandler(this.btnPredictCustomer_Click);
            // 
            // grpSettings
            // 
            this.grpSettings.Controls.Add(this.btnSettings);
            this.grpSettings.Controls.Add(this.btnHelp);
            this.grpSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.grpSettings.Location = new System.Drawing.Point(680, 343);
            this.grpSettings.Margin = new System.Windows.Forms.Padding(6);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpSettings.Size = new System.Drawing.Size(653, 313);
            this.grpSettings.TabIndex = 3;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "⚙️ Settings & Help";
            // 
            // btnSettings
            // 
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSettings.Location = new System.Drawing.Point(22, 38);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(158, 44);
            this.btnSettings.TabIndex = 0;
            this.btnSettings.Text = "⚙️ Cài đặt";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnHelp.Location = new System.Drawing.Point(202, 38);
            this.btnHelp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(158, 44);
            this.btnHelp.TabIndex = 1;
            this.btnHelp.Text = "❓ Hướng dẫn";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 1000);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mainLayout);
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(1122, 736);
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load_1);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.mainLayout.ResumeLayout(false);
            this.grpDataset.ResumeLayout(false);
            this.grpTraining.ResumeLayout(false);
            this.grpResults.ResumeLayout(false);
            this.grpSettings.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}