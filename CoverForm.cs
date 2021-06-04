using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoBang
{
    public partial class CoverForm : Form
    {
        public CoverForm()
        {
            InitializeComponent();
        }

        private void CoverForm_Load(object sender, EventArgs e)
        {
            
            uiTitlePanel1.Visible = false;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1(true,null);
            this.Hide();
            form.Show(this);
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            uiTitlePanel1.Visible = true;
            this.BackgroundImage = Properties.Resources.login_background;
        }

      


        private void login_button_Click_1(object sender, EventArgs e)
        {
            if (user_name_text.Text.Length > 0)
            {
                String name = user_name_text.Text;
                PVP login = new PVP(name);
                Form1.Set_pvp(login);
                Form1 form = new Form1(false, name);
                this.Hide();
                form.Show(this);
                uiTitlePanel1.Visible = false;
                this.BackgroundImage = Properties.Resources.gamecove;
            }
        }

        private void uiSymbolButton1_Click(object sender, EventArgs e)
        {
            uiTitlePanel1.Visible = false;
            this.BackgroundImage = Properties.Resources.gamecove;

        }
    }
}