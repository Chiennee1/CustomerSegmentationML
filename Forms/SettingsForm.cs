using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.Text = "C�i ??t h? th?ng";
            this.Size = new System.Drawing.Size(500, 350);
        }

        private void InitializeUI()
        {
            this.Text = "C�i ??t h? th?ng - Settings";
            this.Size = new System.Drawing.Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            // TODO: Th�m c�c control ch?n file d? li?u, thu?t to�n, c?u h�nh h? th?ng
        }
    }
}
