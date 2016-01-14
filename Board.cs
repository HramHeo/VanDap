﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
//using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace _1312125
{
    public class Board
    {
        //public Broad();

        public void StartGame(Player player1, Player player2)
        {
            player1 = new Player(true,false);
            player2 = new Player(false, false);
            
        }

        //private Point play(Point pos, Player player1, Player player2, Image X, Image O  )
        //{
        //    PaintEventArgs e;
        //    if (player1.GetTurn() == true && player1.GetClick() == true)
        //    {
        //        e.Graphics.DrawImage(X, cell
        //    }
        //    return pos;
        //}
    }
}