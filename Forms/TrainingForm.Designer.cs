using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    partial class TrainingForm
    {
        private System.ComponentModel.IContainer components = null;
        private GroupBox grpAlgorithm;
        private ComboBox cmbAlgorithm;
        private Label lblAlgorithm;

        private GroupBox grpProgress;
        private ProgressBar progressOverall;
        private ProgressBar progressAlgorithm;
        private Label lblOverallProgress;
        private Label lblAlgorithmProgress;
        private Label lblCurrentAlgorithm;
        private Label lblProgressMessage;

        private GroupBox grpLog;
        private RichTextBox rtbLog;

        private GroupBox grpResults;
        private RichTextBox rtbResults;

        private Button btnStartTraining;
        private Button btnCancel;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;

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
            this.SuspendLayout();

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Status Strip
            this.statusStrip = new StatusStrip();
            this.lblStatus = new ToolStripStatusLabel("Sẵn sàng");
            this.statusStrip.Items.Add(lblStatus);
            this.Controls.Add(statusStrip);

            // Algorithm Group
            this.grpAlgorithm = new GroupBox();
            this.grpAlgorithm.Text = "🤖 Chọn thuật toán";
            this.grpAlgorithm.Location = new System.Drawing.Point(12, 12);
            this.grpAlgorithm.Size = new System.Drawing.Size(300, 100);
            this.grpAlgorithm.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);

            this.lblAlgorithm = new Label();
            this.lblAlgorithm.Text = "Thuật toán:";
            this.lblAlgorithm.Location = new System.Drawing.Point(15, 25);
            this.lblAlgorithm.Size = new System.Drawing.Size(80, 23);

            this.cmbAlgorithm = new ComboBox();
            this.cmbAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbAlgorithm.Location = new System.Drawing.Point(15, 50);
            this.cmbAlgorithm.Size = new System.Drawing.Size(270, 21);

            this.grpAlgorithm.Controls.AddRange(new Control[] { lblAlgorithm, cmbAlgorithm });
            this.Controls.Add(grpAlgorithm);

            // Progress Group
            this.grpProgress = new GroupBox();
            this.grpProgress.Text = "📊 Tiến trình";
            this.grpProgress.Location = new System.Drawing.Point(330, 12);
            this.grpProgress.Size = new System.Drawing.Size(400, 180);
            this.grpProgress.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);

            this.lblOverallProgress = new Label();
            this.lblOverallProgress.Text = "Tiến trình tổng thể:";
            this.lblOverallProgress.Location = new System.Drawing.Point(15, 25);
            this.lblOverallProgress.Size = new System.Drawing.Size(120, 23);

            this.progressOverall = new ProgressBar();
            this.progressOverall.Location = new System.Drawing.Point(15, 50);
            this.progressOverall.Size = new System.Drawing.Size(370, 23);

            this.lblAlgorithmProgress = new Label();
            this.lblAlgorithmProgress.Text = "Tiến trình thuật toán:";
            this.lblAlgorithmProgress.Location = new System.Drawing.Point(15, 85);
            this.lblAlgorithmProgress.Size = new System.Drawing.Size(130, 23);

            this.progressAlgorithm = new ProgressBar();
            this.progressAlgorithm.Location = new System.Drawing.Point(15, 110);
            this.progressAlgorithm.Size = new System.Drawing.Size(370, 23);

            this.lblCurrentAlgorithm = new Label();
            this.lblCurrentAlgorithm.Text = "Thuật toán: Chưa bắt đầu";
            this.lblCurrentAlgorithm.Location = new System.Drawing.Point(15, 140);
            this.lblCurrentAlgorithm.Size = new System.Drawing.Size(200, 23);

            this.lblProgressMessage = new Label();
            this.lblProgressMessage.Text = "Sẵn sàng...";
            this.lblProgressMessage.Location = new System.Drawing.Point(220, 140);
            this.lblProgressMessage.Size = new System.Drawing.Size(165, 23);

            this.grpProgress.Controls.AddRange(new Control[] {
                lblOverallProgress, progressOverall, lblAlgorithmProgress,
                progressAlgorithm, lblCurrentAlgorithm, lblProgressMessage
            });
            this.Controls.Add(grpProgress);

            // Buttons
            this.btnStartTraining = new Button();
            this.btnStartTraining.Text = "🚀 Bắt đầu Training";
            this.btnStartTraining.Location = new System.Drawing.Point(12, 130);
            this.btnStartTraining.Size = new System.Drawing.Size(150, 35);
            this.btnStartTraining.UseVisualStyleBackColor = true;
            this.btnStartTraining.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStartTraining.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartTraining.Click += btnStartTraining_Click;

            this.btnCancel = new Button();
            this.btnCancel.Text = "❌ Hủy";
            this.btnCancel.Location = new System.Drawing.Point(175, 130);
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCancel.Click += btnCancel_Click;

            this.Controls.AddRange(new Control[] { btnStartTraining, btnCancel });

            // Log Group
            this.grpLog = new GroupBox();
            this.grpLog.Text = "📋 Log";
            this.grpLog.Location = new System.Drawing.Point(12, 200);
            this.grpLog.Size = new System.Drawing.Size(580, 250);
            this.grpLog.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);

            this.rtbLog = new RichTextBox();
            this.rtbLog.Location = new System.Drawing.Point(15, 25);
            this.rtbLog.Size = new System.Drawing.Size(550, 210);
            this.rtbLog.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.rtbLog.ReadOnly = true;
            this.rtbLog.BackColor = System.Drawing.Color.Black;
            this.rtbLog.ForeColor = System.Drawing.Color.LightGreen;

            this.grpLog.Controls.Add(rtbLog);
            this.Controls.Add(grpLog);

            // Results Group
            this.grpResults = new GroupBox();
            this.grpResults.Text = "📈 Kết quả";
            this.grpResults.Location = new System.Drawing.Point(610, 200);
            this.grpResults.Size = new System.Drawing.Size(570, 250);
            this.grpResults.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);

            this.rtbResults = new RichTextBox();
            this.rtbResults.Location = new System.Drawing.Point(15, 25);
            this.rtbResults.Size = new System.Drawing.Size(540, 210);
            this.rtbResults.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.rtbResults.ReadOnly = true;

            this.grpResults.Controls.Add(rtbResults);
            this.Controls.Add(grpResults);

            this.FormClosing += TrainingForm_FormClosing;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}