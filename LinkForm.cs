using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerShemeMaster
{
    public partial class LinkForm : Form
    {
        public string Link;
        public LinkForm(List<string> linkNames)
        {
            InitializeComponent();
           
            foreach (string linkName in linkNames)
            {
                listBox1.Items.Add(linkName);
            }
            listBox1.SetSelected(0, true);

            OKbutton.Click += OKbutton_Click;
            Cancelbutton.Click += Cancelbutton_Click;
        }

        private void LinkForm_Load(object sender, EventArgs e)
        {

        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            Link = listBox1.SelectedItem.ToString();
            this.Close();
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            Link = "";
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
