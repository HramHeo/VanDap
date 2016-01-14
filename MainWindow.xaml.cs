﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
using System.Configuration;
namespace _1312125
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard(br);   
        }

        Rectangle[,] cell;
        
        const int MCOL = 12;
        const int MROW = 12;
        int [,] check;
        bool tf = false;
        string name = "Guest";
        Player player1; 
        Player player2;
        bool s = false;
        string message;
        Board br;
        int state;
        ImageBrush X = new ImageBrush();
        ImageBrush O = new ImageBrush();

        List <Point> ds = new List<Point>();
        Point p = new Point();

        public void InitializeBoard(Board br)
        {            
            int row, col;            
            cell = new Rectangle[MCOL, MROW];
            check = new int[MCOL, MROW];
            for (row = 0; row < MROW; row++)
                for (col = 0; col < MCOL; col++)
                {
                    check[row, col] = 0;
                    cell[row, col] = new Rectangle();
                    if ((row % 2 == 1 && col % 2 == 0) || (row % 2 == 0 && col % 2 == 1))
                    {
                        cell[row, col].Fill = System.Windows.Media.Brushes.Blue;
                        //cell[row, col].StrokeThickness = 5;
                        Grid.SetColumn(cell[row, col], col);
                        Grid.SetRow(cell[row, col], row);
                    }
                    else //if ((row % 2 == 0 && col % 2 == 1) || (row % 2 == 1 && col % 2 == 0))
                    {
                        cell[row, col].Fill = System.Windows.Media.Brushes.White;
                        //cell[row, col].StrokeThickness = 5;
                        Grid.SetColumn(cell[row, col], col);
                        Grid.SetRow(cell[row, col], row);
                    }
                    GridBC.Children.Add(cell[row, col]);
                }
           
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            name = NameBox.Text;
        }
        string mess;

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            
            if (ChatBox.Text.Length != 0)
            {
                mess = ChatBox.Text;
                ChatBox.Text = "";
                ListChat.Text += "\n";
                ListChat.Text += name;
                ListChat.Text += "\t\t\t";
                ListChat.Text += DateTime.Now;
                ListChat.Text += "\n";
                ListChat.Text += mess;              
                ListChat.Text += "\n..........................................................................................\n\n";
                try
                {
                    socket.Emit("ChatMessage", mess);
                }
                catch (Exception) { }
            }
        }

        private void GridBC_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (tf == false) //chua ai win
            {
                int row, col;
                Point a = LayToaDo(sender, e);
                row = Convert.ToInt32(a.Y);
                col = Convert.ToInt32(a.X);
                if (check[row, col] == 0) //o chua danh // X = 1 // O = 2
                {
                    try
                    {
                        if (player2.GetClick() == true)
                        {
                            player2.SetClick(false);
                            player1.SetClick(true);
                        }
                        else
                        {
                            player2.SetClick(true);
                            player1.SetClick(false);
                        }
                        switch (state)
                        {
                            case 1:
                                {
                                    playPvP(a, row, col, player1, player2);
                                    break;
                                }
                            case 2:
                                {
                                    playPvB(a, row, col, player1, player2);
                                    break;
                                }
                        }
                        
                    }
                    catch (Exception) { }
                }
                //check here
                tf = isWin(row, col);
            }
        }
        
        private Point playPvP(Point pos, int row, int col, Player p1, Player p2)
        {
            bool tf = false; //tf la danh dung luat = true, sai luat || chua danh = false
            while (tf == false)
            {
                    if (p1.GetTurn() == true && p1.GetClick() == true)
                    {
                        cell[row, col].Fill = X;                    
                        p.X = row;
                        p.Y = col;                        
                        tf = true;
                        player1.SetTurn(false);
                        player2.SetTurn(true);
                        check[row, col] = 1;
                    }

                    if (p2.GetTurn() == true && p2.GetClick() == true)
                    {
                        cell[row, col].Fill = O;
                        tf = true;
                        player2.SetTurn(false);
                        player1.SetTurn(true);
                        check[row, col] = 2;
                    }
            }
            return pos;
        }

        int Rrow, Rcol;

        private Point playPvB(Point pos, int row, int col, Player p1, Player p2)
        {
            bool tf = false; //tf la danh dung luat = true, sai luat || chua danh = false
            while (tf == false)
            {
                    cell[row, col].Fill = X;
                    p.X = row;
                    p.Y = col;
                    tf = true;
                    check[row, col] = 1;                
                    do
                    {      
                        Random ran = new Random();
                        Rcol = ran.Next(0, 11);
                        Rrow = ran.Next(0, 11);
                    }
                    while (check[Rrow, Rcol] != 0);
                    cell[Rrow, Rcol].Fill = O;
                    tf = true;
                    check[Rrow, Rcol] = 2;
                
            }
            return pos;
        }
        
        public Point LayToaDo(object sender, MouseButtonEventArgs e)
        {
            int selectedColumnIndex = -1, selectedRowIndex = -1;
            var grid = sender as Grid;
            if (grid != null)
            {
                var pos = e.GetPosition(grid);
                var temp = pos.X;
                for (var i = 0; i < grid.ColumnDefinitions.Count; i++)
                {
                    var colDef = grid.ColumnDefinitions[i];
                    temp -= colDef.ActualWidth;
                    if (temp <= -1)
                    {
                        selectedColumnIndex = i;
                        break;
                    }
                }

                temp = pos.Y;
                for (var i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    var rowDef = grid.RowDefinitions[i];
                    temp -= rowDef.ActualHeight;
                    if (temp <= -1)
                    {
                        selectedRowIndex = i;
                        break;
                    }
                }
            }
            Point a = new Point();
            a.X = selectedColumnIndex;
            a.Y = selectedRowIndex;
            return a;
        }
             
        bool isWin(int row, int col)
        {
            int Just = check[row, col];            
            int r;
            int c;
            int tong = 0;
            if (Just == 1)
            {
                r = row;
                c = col;
                while ((0 <= r && r <= 11) && tong < 5) //qua phai
                {
                    r++;
                    if (r > 11)
                        break;
                    if (check[r, c] == 1)
                        tong++;                       
                    else break;
                }

                r = row;
                c = col;
                while ((0 <= r && r <= 11) && tong < 5) //qua trai
                {
                    r--;
                    if (r < 0)
                        break;
                    if (check[r, c] == 1)
                        tong++;
                    else break;
                }
                if (tong >= 4)
                {
                    System.Windows.MessageBox.Show(name + " win!"); //rename thanh ten nguoi choi
                    return true;
                }
                else
                    tong = 0;

//----------------------------------------------------------------------------------------------------- asdqqqqqqqqqqqqqqq lolz
//-----------------------------------------------------------------------------------------------------

                r = row;
                c = col;
                while ((0 <= c && c <= 11) && tong < 5) //di len
                {
                    c++;
                    if (c > 11)
                        break;
                    if (check[r, c] == 1)
                        tong++;
                    else break;
                }

                r = row;
                c = col;
                while ((0 <= c && c <= 11) && tong < 5) //di xuong
                {
                    c--;
                    if (c < 0)
                        break;
                    if (check[r, c] == 1)
                        tong++;
                    else break;
                }
                if (tong >= 4)
                {
                    System.Windows.MessageBox.Show(name + " win!"); //rename thanh ten nguoi choi
                    return true;
                }
                else
                    tong = 0;

//-----------------------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------------------

                r = row;
                c = col;
                while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //D-B
                {
                    r++;
                    c++;
                    if (r > 11 || c > 11)
                        break;
                    if (check[r, c] == 1)
                        tong++;
                    else break;
                }

                r = row;
                c = col;
                while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //T-N
                {
                    r--;
                    c--;
                    if (r < 0 || c < 0)
                        break;
                    if (check[r, c] == 1)
                        tong++;
                    else break;
                }
                if (tong >= 4)
                {
                    System.Windows.MessageBox.Show(name + " win!"); //rename thanh ten nguoi choi
                    return true;
                }
                else
                    tong = 0;
                //-----------------------------------------------------------------------------------------------------
                //-----------------------------------------------------------------------------------------------------

                r = row;
                c = col;
                while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //D-N
                {
                    r++;
                    c--;
                    if (r > 11 || c < 0)
                        break;
                    if (check[r, c] == 1)
                        tong++;
                    else break;
                }

                r = row;
                c = col;
                while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //T-B
                {
                    r--;
                    c++;
                    if (r < 0 || c > 11)
                        break;
                    if (check[r, c] == 1)
                        tong++;
                    else break;
                }
                if (tong >= 4)
                {
                    System.Windows.MessageBox.Show(name + " win!"); //rename thanh ten nguoi choi
                    return true;
                }
                else
                    tong = 0;
            }
            else
 //####################################################################################################
 //####################################################################################################
                if (Just == 2)
                {
                    r = row;
                    c = col;
                    while ((0 <= r && r <= 11) && tong < 5) //qua phai
                    {
                        r++;
                        if (r > 11)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }

                    r = row;
                    c = col;
                    while ((0 <= r && r <= 11) && tong < 5) //qua trai
                    {
                        r--;
                        if (r < 0)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }
                    if (tong >= 4)
                    {
                        System.Windows.MessageBox.Show(name + " win!z"); //rename thanh ten nguoi choi
                        return true;
                    }
                    else
                        tong = 0;

                    //-----------------------------------------------------------------------------------------------------
                    //-----------------------------------------------------------------------------------------------------

                    r = row;
                    c = col;
                    while ((0 <= c && c <= 11) && tong < 5) //di len
                    {
                        c++;
                        if (c > 11)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }

                    r = row;
                    c = col;
                    while ((0 <= c && c <= 11) && tong < 5) //di xuong
                    {
                        c--;
                        if (c < 0)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }
                    if (tong >= 4)
                    {
                        System.Windows.MessageBox.Show(name + " win!z"); //rename thanh ten nguoi choi
                        return true;
                    }
                    else
                        tong = 0;

                    //-----------------------------------------------------------------------------------------------------
                    //-----------------------------------------------------------------------------------------------------

                    r = row;
                    c = col;
                    while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //D-B
                    {
                        r++;
                        c++;
                        if (r > 11 || c > 11)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }

                    r = row;
                    c = col;
                    while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //T-N
                    {
                        r--;
                        c--;
                        if (r < 0 || c < 0)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }
                    if (tong >= 4)
                    {
                        System.Windows.MessageBox.Show(name + " win!z"); //rename thanh ten nguoi choi
                        return true;
                    }
                    else
                        tong = 0;
                    //-----------------------------------------------------------------------------------------------------
                    //-----------------------------------------------------------------------------------------------------

                    r = row;
                    c = col;
                    while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //D-N
                    {
                        r++;
                        c--;
                        if (r > 11 || c < 0)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }

                    r = row;
                    c = col;
                    while ((0 <= r && r <= 11) && (0 <= c && c <= 11) && tong < 5) //T-B
                    {
                        r--;
                        c++;
                        if (r < 0 || c > 11)
                            break;
                        if (check[r, c] == 2)
                            tong++;
                        else break;
                    }
                    if (tong >= 4)
                    {
                        System.Windows.MessageBox.Show(name + " win!z"); //rename thanh ten nguoi choi
                        return true;
                    }
                    else
                        tong = 0;
                }
            return false;
        }

        private void PvP_btn_Click(object sender, RoutedEventArgs e)
        {
            Board br = new Board();
            player1 = new Player(true, false);
            player2 = new Player(false, true);
            Uri uX = new Uri(@"../../Image/X.png", UriKind.Relative);
            Uri uO = new Uri(@"../../Image/O.png", UriKind.Relative);
            X.ImageSource = new BitmapImage(uX);
            O.ImageSource = new BitmapImage(uO);
            state = 1;
        }

        private void PvB_btn_Click(object sender, RoutedEventArgs e)
        {
            Board br = new Board();
            player1 = new Player(true, false);
            player2 = new Player(false, true);
            Uri uX = new Uri(@"../../Image/X.png", UriKind.Relative);
            Uri uO = new Uri(@"../../Image/O.png", UriKind.Relative);
            X.ImageSource = new BitmapImage(uX);
            O.ImageSource = new BitmapImage(uO);
            state = 2;
        }
        
        Quobject.SocketIoClientDotNet.Client.Socket socket;
        private void Play_online_Click(object sender, RoutedEventArgs e)
        {
            //var socket = IO.Socket("ws://gomoku-lajosveres.rhcloud.com:8000");
            string connect = ConfigurationManager.ConnectionStrings["LINK_GOMOKU"].ConnectionString;
            socket = IO.Socket(connect);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    message += "\n";
                    message += "connected";
                    ListChat.Text += message;
                    message += "\n";
                }));
            });

            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    mess += data.ToString();
                    ListChat.Text = mess;
                    mess += "\n";
                }));
            });

            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    mess += data.ToString();
                    ListChat.Text = mess;
                    mess += "\n";
                }));
            });

            socket.On("ChatMessage", (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {                   
                    message = ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString();
                    if (message == "Welcome!")
                    {
                        socket.Emit("MyNameIs", name);
                        socket.Emit("ConnectToOtherPlayer");
                        ListChat.Text += "\nServer";
                        ListChat.Text += "\t\t\t";
                        ListChat.Text += DateTime.Now;
                        ListChat.Text += "\n";
                        ListChat.Text += message;
                        ListChat.Text += "\n..........................................................................................\n\n";
                        
                        //mess = "\nServer";
                        //mess = "\t\t\t";
                        //mess = DateTime.Now;
                        //mess = "\n";
                        //mess = message;
                        //mess = "\n..........................................................................................\n\n";
                    }
                    else
                    {
                        if (((Newtonsoft.Json.Linq.JObject)data)["from"] == null)
                        {
                            ListChat.Text += "Server";
                            ListChat.Text += "\t\t\t";
                            ListChat.Text += DateTime.Now;
                            ListChat.Text += "\n";
                            message = XuLy(ref message);
                            ListChat.Text += message;
                            ListChat.Text += "\n.................ab......................................................................\n\n";
                            message += "\n";
                        }
                        else
                            if (((Newtonsoft.Json.Linq.JObject)data)["from"].ToString() != name)
                            {
                                ListChat.Text += ((Newtonsoft.Json.Linq.JObject)data)["from"].ToString();
                                ListChat.Text += "\t\t\t";
                                ListChat.Text += DateTime.Now;
                                ListChat.Text += "\n";
                                //message = XuLy(ref message);
                                ListChat.Text += message;
                                ListChat.Text += "\n.................abc......................................................................\n\n";
                                message += "\n";
                            }
                    }
                }));
                //s = true;
            });
            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    message += data.ToString();
                    ListChat.Text += message;
                    message += "\n";
                }));
            });
        }

        string XuLy(ref string t)
        {
            string s = "<br />";
            if (t.Contains(s))
                t = t.Replace(s, "\n");

            //string vd = "con chim non";
            //vd.Replace("non", "gia");
            return t;
        }
    }
}