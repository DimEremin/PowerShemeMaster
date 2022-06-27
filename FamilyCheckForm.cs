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
    public partial class FamilyCheckForm : Form
    {
        public string FamilyDirectory;
        public FamilyCheckForm(List<string> missingFamilyNames)
        {
            InitializeComponent();

            foreach (string familyName in missingFamilyNames)
            {
                listBox1.Items.Add(familyName);
            }

            //DirectoryButton.Click += DirectoryButton_Click;
            //CancelButton.Click += CancelButton_Click;
        }

        private void FamilyCheckForm_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            FamilyDirectory = "Отмена";
            this.Close();
        }

        private void DirectoryButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                FamilyDirectory = FBD.SelectedPath;
            }
            else
            {
                FamilyDirectory = "Отмена";
            }
            this.Close();
        }
    }
}
