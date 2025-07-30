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
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(1000, 600);

            // Menu Strip
            this.menuStrip = new MenuStrip();
            this.fileMenu = new ToolStripMenuItem("&File");
            this.helpMenu = new ToolStripMenuItem("&Help");

            this.menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, helpMenu });
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            // Status Strip
            this.statusStrip = new StatusStrip();
            this.lblStatus = new ToolStripStatusLabel("Sẵn sàng");
            this.lblDatasetCount = new ToolStripStatusLabel("0 datasets");

            this.statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblDatasetCount });
            this.Controls.Add(statusStrip);

            // Main Layout
            this.mainLayout = new TableLayoutPanel();
            this.mainLayout.Dock = DockStyle.Fill;
            this.mainLayout.ColumnCount = 2;
            this.mainLayout.RowCount = 3;
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            this.mainLayout.Padding = new Padding(10);
            this.Controls.Add(mainLayout);

            // Dataset Group
            this.grpDataset = new GroupBox();
            this.grpDataset.Text = "📊 Dataset Management";
            this.grpDataset.Dock = DockStyle.Fill;
            this.grpDataset.Margin = new Padding(5);
            this.grpDataset.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.cmbDataset = new ComboBox();
            this.cmbDataset.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDataset.Location = new System.Drawing.Point(20, 30);
            this.cmbDataset.Size = new System.Drawing.Size(300, 28);
            this.cmbDataset.Font = new System.Drawing.Font("Segoe UI", 9F);

            this.btnGenerateData = new Button();
            this.btnGenerateData.Text = "🎲 Tạo dữ liệu mẫu";
            this.btnGenerateData.Location = new System.Drawing.Point(20, 70);
            this.btnGenerateData.Size = new System.Drawing.Size(140, 35);
            this.btnGenerateData.UseVisualStyleBackColor = true;
            this.btnGenerateData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnGenerateData.Click += btnGenerateData_Click;

            this.btnViewData = new Button();
            this.btnViewData.Text = "👁️ Xem dữ liệu";
            this.btnViewData.Location = new System.Drawing.Point(180, 70);
            this.btnViewData.Size = new System.Drawing.Size(140, 35);
            this.btnViewData.UseVisualStyleBackColor = true;
            this.btnViewData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewData.Click += btnViewData_Click;

            this.grpDataset.Controls.AddRange(new Control[] { cmbDataset, btnGenerateData, btnViewData });
            this.mainLayout.Controls.Add(grpDataset, 0, 0);

            // Training Group
            this.grpTraining = new GroupBox();
            this.grpTraining.Text = "🤖 Model Training";
            this.grpTraining.Dock = DockStyle.Fill;
            this.grpTraining.Margin = new Padding(5);
            this.grpTraining.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.btnStartTraining = new Button();
            this.btnStartTraining.Text = "🚀 Bắt đầu Training";
            this.btnStartTraining.Location = new System.Drawing.Point(20, 30);
            this.btnStartTraining.Size = new System.Drawing.Size(160, 40);
            this.btnStartTraining.UseVisualStyleBackColor = true;
            this.btnStartTraining.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartTraining.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartTraining.Click += btnStartTraining_Click;

            this.progressBar = new ProgressBar();
            this.progressBar.Location = new System.Drawing.Point(20, 80);
            this.progressBar.Size = new System.Drawing.Size(300, 23);
            this.progressBar.Style = ProgressBarStyle.Continuous;

            this.grpTraining.Controls.AddRange(new Control[] { btnStartTraining, progressBar });
            this.mainLayout.Controls.Add(grpTraining, 1, 0);

            // Results Group
            this.grpResults = new GroupBox();
            this.grpResults.Text = "📈 Results & Analysis";
            this.grpResults.Dock = DockStyle.Fill;
            this.grpResults.Margin = new Padding(5);
            this.grpResults.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.btnViewResults = new Button();
            this.btnViewResults.Text = "📊 Xem kết quả";
            this.btnViewResults.Location = new System.Drawing.Point(20, 30);
            this.btnViewResults.Size = new System.Drawing.Size(140, 35);
            this.btnViewResults.UseVisualStyleBackColor = true;
            this.btnViewResults.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnViewResults.Click += btnViewResults_Click;

            this.btnPredictCustomer = new Button();
            this.btnPredictCustomer.Text = "🎯 Dự đoán khách hàng";
            this.btnPredictCustomer.Location = new System.Drawing.Point(180, 30);
            this.btnPredictCustomer.Size = new System.Drawing.Size(140, 35);
            this.btnPredictCustomer.UseVisualStyleBackColor = true;
            this.btnPredictCustomer.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnPredictCustomer.Click += btnPredictCustomer_Click;

            this.grpResults.Controls.AddRange(new Control[] { btnViewResults, btnPredictCustomer });
            this.mainLayout.Controls.Add(grpResults, 0, 1);

            // Settings Group
            this.grpSettings = new GroupBox();
            this.grpSettings.Text = "⚙️ Settings & Help";
            this.grpSettings.Dock = DockStyle.Fill;
            this.grpSettings.Margin = new Padding(5);
            this.grpSettings.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            this.btnSettings = new Button();
            this.btnSettings.Text = "⚙️ Cài đặt";
            this.btnSettings.Location = new System.Drawing.Point(20, 30);
            this.btnSettings.Size = new System.Drawing.Size(140, 35);
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSettings.Click += btnSettings_Click;

            this.btnHelp = new Button();
            this.btnHelp.Text = "❓ Hướng dẫn";
            this.btnHelp.Location = new System.Drawing.Point(180, 30);
            this.btnHelp.Size = new System.Drawing.Size(140, 35);
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnHelp.Click += btnHelp_Click;

            this.grpSettings.Controls.AddRange(new Control[] { btnSettings, btnHelp });
            this.mainLayout.Controls.Add(grpSettings, 1, 1);

            this.Load += MainForm_Load;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}