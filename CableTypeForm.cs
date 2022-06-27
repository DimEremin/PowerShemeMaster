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
    public partial class CableTypeForm : Form
    {
        public string CableType;
        public string PipeType;
        public List<string> SelectedPanels;
        public List<string> SelectedActions;
        public double LoadCoefficient;
        public int DefaultDiameter;
        public CableTypeForm(List<string> listOfCables, List<string> listOfPipes, List<string> listOfPanels)
        {
            InitializeComponent();

            SelectedPanels = new List<string>();
            SelectedActions = new List<string>();

            foreach (string cable in listOfCables)
            {
                listBox1.Items.Add(cable);
            }
            foreach (string pipe in listOfPipes)
            {
                listBox3.Items.Add(pipe);
            }
            listBox1.SetSelected(0, true);
            listBox3.SetSelected(0, true);

            int i = 0;
            foreach (string panel in listOfPanels)
            {
                listBox2.Items.Add(panel);
                listBox2.SetSelected(i, true);
                i ++;
            }
            comboBox1.SelectedIndex = 1;      

            //OKbutton.Click += OKbutton_Click;
            //Cancelbutton.Click += Cancelbutton_Click;
        }

        private void CableTypeForm_Load(object sender, EventArgs e)
        {

        }



        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {

            foreach (var item in listBox2.SelectedItems)
            {
                if (!SelectedPanels.Contains(item.ToString()))
                {
                    SelectedPanels.Add(item.ToString());
                }                               
            }

            foreach (var item in checkedListBox1.CheckedItems)
            {
                if (!SelectedActions.Contains(item.ToString()))
                {
                    SelectedActions.Add(item.ToString());
                }
               
            }

            CableType = listBox1.SelectedItem.ToString();
            PipeType = listBox3.SelectedItem.ToString();
            try
            {
                LoadCoefficient = Convert.ToDouble(textBox1.Text);
            }
            catch
            {
                MessageBox.Show("Неверный тип данных для поля Запас площади сечения трубы. Принято значение по умолчанию - 40% " );
                LoadCoefficient = 40;
            }


            DefaultDiameter = Convert.ToInt32(comboBox1.SelectedItem);
 
            this.Close();
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            CableType = "Cancel";
            this.Close();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
