using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSBackupClient
{
    public partial class PromptUser : Form
    {
        public PromptUser(string sMessage)
        {
            InitializeComponent();
            this.Message.Text = AddLineBreaks(sMessage); 
        }

        private void OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        //Very basic algorithm to add lines to make this dialog more readable. If there are long words passed, may go out of form. 
        private string AddLineBreaks(string sMessage) 
        {
            string[] words = sMessage.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (i == words.Length) break; 
                if ((i+1) % 4 == 0)
                {
                    words[i] = words[i] + "\r\n";
                    continue; 
                }
                words[i] = words[i] + ' '; 
            }

            return string.Join("", words); 
        }
    }
}
