﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
//using System.Windows.Shapes;
namespace _1312125
{
    public class Player
    {
        bool turn;
        bool click;
        public Player(bool turn, bool click)
        {
            this.turn = turn;
            this.click = click;
        }
        public void SetClick(bool doClick)
        {
            this.click = doClick;
        }

        public bool GetClick()
        {
            return this.click;
        }

        public void SetTurn(bool turn)
        {
            this.turn = turn;
        }

        public bool GetTurn()
        {
            return this.turn;
        }    

    }
}