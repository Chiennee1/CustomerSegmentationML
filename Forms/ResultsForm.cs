using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class ResultsForm : Form
    {
        public ResultsForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.Text = "K?t qu? ph�n t�ch";
            this.Size = new System.Drawing.Size(900, 600);
        }

        private void InitializeUI()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            // TODO: Th�m c�c control hi?n th? k?t qu?, bi?u ??, b?ng ph�n t�ch segment
        }
    }
}
