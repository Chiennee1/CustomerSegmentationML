using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class PredictionForm : Form
    {
        public PredictionForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeComponent()
        {
            this.Text = "D? ?o�n kh�ch h�ng m?i";
            this.Size = new System.Drawing.Size(600, 400);
        }

        private void InitializeUI()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            // TODO: Th�m c�c control nh?p th�ng tin kh�ch h�ng, n�t d? ?o�n, hi?n th? k?t qu?
        }
    }
}
