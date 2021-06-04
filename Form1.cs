using GoBang.Properties;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoBang
{
    public partial class Form1 : Form
    {
      
        int wintime=0, defaulttime=0;//胜负次数
        int currentCount=0;//s
        int minutes = 0; //m
        GoBang_AI ChessBoard = new GoBang_AI();
        int[] AI_Piont = new int[2];
        CoverForm f;
        public static bool AI = true;
        private static String User_Nname = null;
        private static PVP pvp = null;
        private static bool is_You = false;
        public static String chat_text = null;
        

        public Form1(bool ai,String name)
        {
            InitializeComponent();
            if (name != null)
            {
                User_Nname = name;
            }
            AI = ai;
        }
       public static void Set_pvp(PVP p)
        {
            pvp = p;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            Control.CheckForIllegalCrossThreadCalls = false;//这一行是关键      
            f = Owner as CoverForm;
            this.Left=f.Left;
            this.Top=f.Top;

            foreach (Control c in this.panel3.Controls)
            {
                // 检查控件是否为PictureBox
                if (c is PictureBox)
                {
                    c.Click += new System.EventHandler(pc_click);
                    c.BackgroundImage = null;
                    c.Enabled = false;
                }
            }
            label2.Text = "You";
            label3.Text = "00:00";
            stay_home.Visible = false;
            win_tip.Visible = false;
            send_chat_button.Visible = false;
            chat_box.Visible = false;
            device_line.Visible = false;
            if (!AI) //设置名字
            set_uiname(uiLabel1,true);
           // rec_opp_chat();
        }
      private void start_game_Click(object sender, EventArgs e)
        {
            Clear_CB();
            defaulttime = 0;
            wintime = 0;
            Clear_CB();
            if (AI)
            {

                this.timer1.Enabled = true;
                this.timer1.Start();
                //开始游戏
                Activating_chess();
            }
            else
            {
               start_game.Text = "匹配中";
               start_game.Enabled = false;
                //匹配开始
                pvp.send_Match_Data();//发送匹配信息
                set_uiname(uiLabel2,false);
                
              
            }
        }

        private async void set_uiname(UILabel uilabel,bool own_nane)
        {
            await Task.Run( async () =>
            {
                if (own_nane)
                {
                    var s = await pvp.RecevieData();
                    uilabel.Text = s.ToString();
                }
                else
                {
                    while (PVP.Oppenonet_name == null) ;
                    uilabel.Text = PVP.Oppenonet_name;
                    start_game.Text = "游戏中";
                    await pvp.SendData("Who is first");
                    var s = await pvp.RecevieData();

                    if(s.ToString().Contains("Not"))
                        {
                             Opponent_turn();
                             set_OPponent_Chess();
                        }
                    else//是否先手
                        {
                            is_You = true;
                             My_turn();
                        }
                }
            });
        }

        private void pc_click(object sender, EventArgs e)//点击棋子的函数
        {
            PictureBox c = sender as PictureBox;
            //如果是未被点击过的空子，就会被变成黑子并加入到chess二维数组棋盘中
            if (c.BackgroundImage == null && AI)
            {
                c.BackgroundImage =Resources.Blue_star;
                c.BackgroundImageLayout = ImageLayout.Stretch;
                ChessBoard.setH_ChessBoard(c.Tag.ToString());
                label2.Text = "Opponent";
                AIchess();//对手落子
            }
            else if(c.BackgroundImage == null && !AI)
            {
                c.BackgroundImage = Resources.Blue_star;
                c.BackgroundImageLayout = ImageLayout.Stretch;
                pvp.set_Chessboard(c.Tag.ToString());
                if (pvp.IsWin(1))
                {
                    wintime++;
                    label6.Text = wintime.ToString();
                    Disabiliting_chess();
                    start_game.Enabled = true;
                    start_game.Text = "开始新游戏";
                    stay_home.Visible = true;
                    win_tip.Text = "恭喜你胜利";
                    win_tip.Visible = true;
                    return;
                }
                Opponent_turn();
                set_OPponent_Chess();
            }
        }
        private async void set_OPponent_Chess()
        {
            await Task.Run(async () =>
            {
                int c = await pvp.set_OPponent_Chess();
                Draw_opp_pic(c/100,c%100);
                if (pvp.IsWin(2))
                {
                    defaulttime++;
                    label7.Text = defaulttime.ToString();
                    Disabiliting_chess();
                    start_game.Enabled = true;
                    start_game.Text = "开始新游戏";
                    stay_home.Visible = true;
                    win_tip.Text = "输了，再接再励吧";
                    win_tip.Visible = true;
                    return;
                }
                My_turn();
            });
        }

        private void Draw_opp_pic(int a,int b)
        {
            foreach (Control c in this.panel3.Controls)
            {
                // 检查控件是否为PictureBox
                if (c is PictureBox)
                {
                    if (c.Tag.Equals(a.ToString() + "," + b.ToString()))
                    {
                        c.BackgroundImage = Resources.Pink_star;
                        c.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }
            }
        }

        private void AIchess()
        {
                if (ChessBoard.IsWin(1))
                {
                    wintime++;
                    label6.Text = wintime.ToString();
                win_tip.Text = "恭喜你胜利";
                win_tip.Visible = true;
                return;
                }
                AI_Piont = ChessBoard.Get_AI();
                ChessBoard.setA_ChessBoard(AI_Piont[0], AI_Piont[1]);
                Draw_AIpic();
            if (ChessBoard.IsWin(2))
            {
                defaulttime++;
                label7.Text = defaulttime.ToString();
                win_tip.Text = "输给了人机";
                win_tip.Visible = true;
                return;
            }
        }
        private void Draw_AIpic()
        {
            foreach (Control c in this.panel3.Controls)
            {
                // 检查控件是否为PictureBox
                if (c is PictureBox)
                {
                    if (c.Tag.Equals(AI_Piont[0].ToString() + "," + AI_Piont[1].ToString()))
                    {
                        c.BackgroundImage = Resources.Pink_star;
                        c.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }
            }
            label2.Text = "You";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (currentCount == 59)
            {
                currentCount = 0;
                if (minutes == 59)
                    minutes = 0;
                minutes++;
            }
            currentCount++;
            if(currentCount<10)
            label3.Text= minutes.ToString()+":0"+currentCount.ToString().Trim();
            else
            label3.Text = minutes.ToString() + ":" + currentCount.ToString().Trim();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            f.Show();
        }

       
      

        private void Clear_CB()//清除棋盘与时间。
        {


            foreach (Control c in this.panel3.Controls)
            {
                // 检查控件是否为PictureBox
                if (c is PictureBox)
                {
                    c.BackgroundImage = null;
                    c.BackColor = Color.Transparent;
                    c.Enabled = false;
                }
            }
            ChessBoard = new GoBang_AI();
            label2.Text = null;
            label3.Text = "00:00";
            timer1.Stop();
            currentCount = 0;
            minutes = 0;
        }

        private void Activating_chess()
        {
            foreach (Control c in this.panel3.Controls)
            {
                // 检查控件是否为PictureBox
                if (c is PictureBox)
                {
                    c.Enabled = true;
                }
            }
        }

        private void stay_home_Click(object sender, EventArgs e)
        {
            Clear_CB();
            pvp.clear_chess();
            if (is_You)
            {
                //开始游戏
                Opponent_turn();
                set_OPponent_Chess();
            }
            else
                My_turn();
        }

        private void Disabiliting_chess()
        {
            foreach (Control c in this.panel3.Controls)
            {
                // 检查控件是否为PictureBox
                if (c is PictureBox)
                {
                    c.Enabled = false;
                }
            }
        }
        private void Opponent_turn()
        {

            Disabiliting_chess();
            label2.Text = uiLabel2.Text;
            uiLabel1.ForeColor = Color.Black;
            uiLabel2.ForeColor = Color.LightGreen;
        }

        private void win_tip_Click(object sender, EventArgs e)
        {
            win_tip.Visible = false;
        }

        private void login_button_Click(object sender, EventArgs e)
        {
            if (chat_box != null)
            {
                pvp.SendData("chat:" + chat_box.Text);
            }
        }
        private async void rec_opp_chat()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                     var s = await pvp.RecevieData();
                    if (s.Contains("chat:"))
                    {
                    }
                }
              
            });
        }

        private void uiSymbolButton1_Click(object sender, EventArgs e)
        {
            send_chat_button.Visible = true;
            chat_box.Visible = true;
            device_line.Visible = true;
            uiSymbolButton1.Symbol = 61778;
        }

        private void My_turn()
        {

            Activating_chess();
            label2.Text = "You";
            uiLabel1.ForeColor = Color.LightGreen;
            uiLabel2.ForeColor = Color.Black;
        }
        /******************************************** AI ******************************************/
    }
}
