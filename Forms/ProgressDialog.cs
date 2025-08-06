using System;
using System.Windows.Forms;

namespace CustomerSegmentationML.Forms
{
    public partial class ProgressDialog : Form
    {
        public ProgressDialog(string message)
        {
            InitializeComponent();
            lblMessage.Text = message;
        }

        // Thêm phương thức để cập nhật message nếu cần
        public void UpdateMessage(string message)
        {
            if (lblMessage.InvokeRequired)
            {
                lblMessage.Invoke(new Action<string>(UpdateMessage), message);
                return;
            }
            
            lblMessage.Text = message;
        }
    }
}
