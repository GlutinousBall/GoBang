using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBang
{
    class GoBang_AI
    {
        int[,] chess = new int[9, 12];//一个二维数组棋盘
        int[] Ai_Point = new int[2];


        public GoBang_AI()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 12; j++)
                {
                    chess[i, j] = 0;
                }
        }

        public void setH_ChessBoard(String tag)
        {
            String str, str1;
            int a, b;
            str = tag.Substring(0, 1);
            str1 = tag.Substring(2);
            a = int.Parse(str);
            b = int.Parse(str1);
            chess[a, b] = 1;
        }
        public void setA_ChessBoard(int x,int y)
        {
            chess[x, y] = 2;
        }
            /*************************************************************************
           * 判断是否获得胜利
           * value：1是人，2是电脑
           **************************************************************************/
        public bool IsWin(int value)
        {
            if (Hor_win(value) || Ver_win(value) || Bev_Win(value))
                return true;
            else
                return false;
        }
        private bool Hor_win(int value)
        {
            int k = 0;
            for (int i = 0; i < 9; i++) //横着的五子
            {
                for (int j = 0; j < 12; j++)
                {
                    if (chess[i, j] == value)
                    {
                        k++;
                        if (k == 5)
                           return true;
                    }
                    else
                        k = 0;
                }
                k = 0;

            }
            return false;
        }
        private bool Ver_win(int value)
        {
            int k = 0;
            for (int i = 0; i < 12; i++) //竖着的五子
            {
                for (int j = 0; j < 9; j++)
                {
                    if (chess[j, i] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                }
                k = 0;
            }
            return false;
        }
        private bool Bev_Win(int value)
        {
            int k = 0;
            int c = 0;
            for (int i = 0; i < 5; i++)//斜着五子
            {
                c = i;
                for (int j = 0; j < 9 - i; j++)
                {
                    if (chess[c, j] == value)
                    {
                        k++;
                        if (k == 5)
                           return true;
                    }
                    else
                        k = 0;
                    c++;
                }
                k = 0;
            }
            for (int i = 0; i < 5; i++)//斜着五子
            {
                c = i;
                for (int j = 11; j > (2 + i); j--)
                {
                    if (chess[c, j] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                    c++;
                }
                k = 0;
            }
            for (int i = 10; i > 3; i--)//斜着五子
            {
                int x = i;
                if (i >= 8)
                    c = 0;
                else
                    c++;
                for (int j = 0; j < 9 - c; j++)
                {
                    if (chess[j, x] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                    x--;
                }
                k = 0;
            }
            for (int i = 1; i < 8; i++)//斜着五子
            {
                int x = i;
                if (i <= 3)
                    c = 0;
                else
                    c++;
                for (int j = 0; j < 9 - c; j++)
                {
                    if (chess[j, x] == value)
                    {
                        k++;
                        if (k == 5)
                            return true;
                    }
                    else
                        k = 0;
                    x++;
                }
                k = 0;
            }
            return false;
        }

        /*************************************************************************
       * 获取AI落子点
       * 返回值：AI落子的坐标
       **************************************************************************/
        public int[] Get_AI()
        {
            int k, max = 0;

            int i = 0, j = 0;

            for (i = 0; i < 9; i++)
                for (j = 0; j < 12; j++)
                    if (chess[i, j] == 0)
                    {      // 历遍棋盘，遇到空点则计算价值，取最大价值点下子。 
                        k = calculatevalue(i, j);//分别计算八个方位价值
                        if (k >= max)
                        {
                            Ai_Point[0] = i;
                            Ai_Point[1] = j;
                            max = k;
                        }

                    }
            return Ai_Point;
        }
        /*************************************************************************
        * 此函数获取可下点的权值
        * pare p ；x
        * pare q ；y
        **************************************************************************/
        private int calculatevalue(int p, int q)
        {
            int k = 0;
            k += checkpos(p, q, 0, -1);//左
            k += checkpos(p, q, 0, 1);//右
            k += checkpos(p, q, -1, 0);//上
            k += checkpos(p, q, 1, 0);//下
            k += checkpos(p, q, -1, -1);//左上
            k += checkpos(p, q, 1, -1);//左下
            k += checkpos(p, q, -1, 1);//右上
            k += checkpos(p, q, 1, 1);//右下
            return k;
        }
        /*************************************************************************
       * 此函数获取特定方向的权值
       * pare p ；x
       * pare q ；y
       * pos_x:x的增量
       * pos_y:y的增量
       **************************************************************************/
        private int checkpos(int p, int q, int pos_x, int pos_y)
        {
            int k = 0;
            int check;
            if (checkaround(p, q, pos_x, pos_y) == 1)//判断每一个空位选定的方向有没有相邻的子，若有
            {
                check = ai_judga_line(12, p + pos_x, q + pos_y, pos_x, pos_y, 1);//判断连成线的1个子是不是一样的
                k += checkline(1, check, p, q, pos_x, pos_y);//1

                check = ai_judga_line(12, p + pos_x, q + pos_y, pos_x, pos_y, 2);//判断连成线的2个子是不是一样的
                k += checkline(2, check, p, q, pos_x, pos_y);//2

                check = ai_judga_line(12, p + pos_x, q + pos_y, pos_x, pos_y, 3);//判断连成线的3个子是不是一样的
                k += checkline(3, check, p, q, pos_x, pos_y);//3

                check = ai_judga_line(12, p + pos_x, q + pos_y, pos_x, pos_y, 4);//判断连成线的4个子是不是一样的
                k += checkline(4, check, p, q, pos_x, pos_y);//4

            }
            return k;
        }
        private int checkaround(int p, int q, int pos_x, int pos_y)
        {
            if (p + pos_x < 0 || p + pos_x > 8 || q + pos_y < 0 || q + pos_y > 11)
                return 0;
            if (chess[p + pos_x, q + pos_y] != 0)
            {
                return 1;
            }
            return 0;
        }

        /************************************************************************
         * 此函数用获某方向上的权值
         * 判断某方向上是否存在num子
         * 是不是连续的
         * 是己方还是敌方
         * pare ln 棋盘大小
         * pare XS  某方向的相邻点x坐标
         * pare YS  某方向的相邻点y坐标
         * pare dx  增量
         * pare dy
         * pare num
         *************************************************************************/
        private int ai_judga_line(int ln, int XS, int YS, int dx, int dy, int num)
        {
            if ((XS < ln - 2) && (YS < ln) && (XS >= 0) && (YS >= 0) && (dx != 0 || dy != 0))        //起点坐标在棋盘内//坐标增量不为同时0
            {

                if (((XS + dx * (num - 1)) >= ln - 3) || ((XS + dx * (num - 1)) < 0) || ((YS + dy * (num - 1)) >= ln) || ((YS + dy * (num - 1)) < 0) || (0 == chess[XS, YS]))//判断终点坐标//在棋盘外
                {
                    return 0;  //不在棋盘内，或者起点是没下子
                }
                else
                {
                    int i = 0;
                    for (i = 1; i < num; ++i)
                    {
                        if (chess[XS, YS] != chess[XS + (dx * i), YS + (dy * i)])
                        {
                            return 0;  //如果不是连续num个一样的
                        }//end if3
                    }//end for1

                    if (chess[XS, YS] == 1)
                    {
                        return 1;  //num个都是对手的棋子，且都在棋盘内
                    }
                    if (chess[XS, YS] == 2)
                    {
                        return 2;  //num个都是自己的棋子，且都在棋盘内
                    }

                }//end if 2
            }
            return 0;  //其他情况
        }
        /*************************************************************************
       * 特殊情况的权值
       * linum：几子连成线
       **************************************************************************/
        private int checkline(int linenum, int check, int p, int q, int des_p, int des_q)
        {
            int num = linenum + 1;
            int k = 0;
            int value = 1;
            for (; linenum > 0; linenum--)//加权重
            {
                value = value * 10;
            }

            if (check > 0)
            {
                k += value;
                if (check == 1)//对手的棋子
                {
                    k += 20;
                }
                if (check == 2)//自己的棋子
                {
                    k += 10;
                }
                //活三 连四判断
                if (des_p == 0 && des_q == 1)//向右
                {
                    if (q + num < 12)//这几个相连的棋子的下一个位置还在棋盘内
                    {
                        if (chess[p, q + num] == 0)//下一个位置的空的
                        {
                            k += 10;
                            if (linenum == 3)//活三
                            {
                                k += 10000000;
                            }
                        }
                        if (chess[p, q + num] == 2)//下一个位置是自己的棋子
                        {
                            if (linenum == 4)//连四
                            {
                                k += 100000000;
                            }

                        }
                        if (chess[p, q + num] == 1)//下一个位置是对方的棋子
                        {
                            k += 20;
                        }
                    }
                }
                if (des_p == 1 && des_q == 0)//向下
                {
                    if (p + num < 9)//这几个相连的棋子的下一个位置还在棋盘内
                    {
                        if (chess[p + num, q] == 0)//下一个位置的空的
                        {
                            k += 10;
                            if (linenum == 3)//活三
                            {
                                k += 10000000;
                            }
                        }
                        if (chess[p + num, q] == 2)//下一个位置是自己的棋子
                        {
                            if (linenum == 4)//连四
                            {
                                k += 100000000;
                            }

                        }
                        if (chess[p + num, q] == 1)//下一个位置是对方的棋子
                        {
                            k += 20;
                        }
                    }
                }
                if (des_p == 1 && des_q == 1)//斜右
                {
                    if (q + num < 12 && p + num < 9)//这几个相连的棋子的下一个位置还在棋盘内
                    {
                        if (chess[p + num, q + num] == 0)//下一个位置的空的
                        {
                            k += 10;
                            if (linenum == 3)//活三
                            {
                                k += 10000000;
                            }
                        }
                        if (chess[p + num, q + num] == 2)//下一个位置是自己的棋子
                        {
                            if (linenum == 4)//连四
                            {
                                k += 100000000;
                            }

                        }
                        if (chess[p + num, q + num] == 1)//下一个位置是对方的棋子
                        {
                            k += 20;
                        }
                    }
                }
            }
            return k;
        }

    }
}
