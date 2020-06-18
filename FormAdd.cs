using System;
using System.Windows.Forms;

namespace ParetoFrontCursach
{
    public partial class FormAdd : Form
    {
        Model model;
        Controller controller;

        public FormAdd(Model model)
        {
            InitializeComponent();

            this.model = model;
            controller = new Controller(this, model);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (controller.AddPoint(textBoxX.Text, textBoxY.Text))
            {
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректные данные!");
            }

        }
    }
}
