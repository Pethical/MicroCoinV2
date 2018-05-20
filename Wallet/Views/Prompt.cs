using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wallet.Views
{
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, bool password = true)
        {
            XtraForm prompt = new XtraForm()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true
            };
            LabelControl textLabel = new LabelControl() { Left = 50, Top = 20, Text = text };
            TextEdit textBox = new TextEdit() { Left = 50, Top = 50, Width = 400 };
            textBox.Properties.UseSystemPasswordChar = password;
            SimpleButton confirmation = new SimpleButton() { Text = "Ok", Left = 200, Width = 100, Top = 80, DialogResult = DialogResult.OK };            
            confirmation.Click += (sender, e) => {
                prompt.Close();
            };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
