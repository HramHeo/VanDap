﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
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

            //MessageBox.Show("Tọa độ: (" + selectedRowIndex + "," + selectedColumnIndex + ")");

        ////MessBox;
        //private void A1_Click(object sender, RoutedEventArgs e, Button A)
        //{
        //    MessageBox.Show("Toạ độ: " + A.Name);
        //}
        //private void A2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A2.Name);
        //}
        //private void A3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A3.Name);
        //}
        //private void A4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A4.Name);
        //}
        //private void A5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A5.Name);
        //}
        //private void A6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A6.Name);
        //}
        //private void A7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A7.Name);
        //}
        //private void A8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A8.Name);
        //}
        //private void A9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A9.Name);
        //}
        //private void A10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A10.Name);
        //}
        //private void A11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A11.Name);
        //}
        //private void A12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + A12.Name);
        //}
        //private void B1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B1.Name);
        //}
        //private void B2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B2.Name);
        //}
        //private void B3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B3.Name);
        //}
        //private void B4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B4.Name);
        //}
        //private void B5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B5.Name);
        //}
        //private void B6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B6.Name);
        //}
        //private void B7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B7.Name);
        //}
        //private void B8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B8.Name);
        //}
        //private void B9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B9.Name);
        //}
        //private void B10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B10.Name);
        //}
        //private void B11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B11.Name);
        //}
        //private void B12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + B12.Name);
        //}
        //private void C1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C1.Name);
        //}
        //private void C2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C2.Name);
        //}
        //private void C3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C3.Name);
        //}
        //private void C4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C4.Name);
        //}
        //private void C5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C5.Name);
        //}
        //private void C6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C6.Name);
        //}
        //private void C7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C7.Name);
        //}
        //private void C8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C8.Name);
        //}
        //private void C9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C9.Name);
        //}
        //private void C10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C10.Name);
        //}
        //private void C11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C11.Name);
        //}
        //private void C12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + C12.Name);
        //}
        //private void D1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D1.Name);
        //}
        //private void D2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D2.Name);
        //}
        //private void D3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D3.Name);
        //}
        //private void D4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D4.Name);
        //}
        //private void D5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D5.Name);
        //}
        //private void D6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D6.Name);
        //}
        //private void D7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D7.Name);
        //}
        //private void D8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D8.Name);
        //}
        //private void D9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D9.Name);
        //}
        //private void D10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D10.Name);
        //}
        //private void D11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D11.Name);
        //}
        //private void D12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + D12.Name);
        //}
        //private void E1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E1.Name);
        //}
        //private void E2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E2.Name);
        //}
        //private void E3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E3.Name);
        //}
        //private void E4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E4.Name);
        //}
        //private void E5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E5.Name);
        //}
        //private void E6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E6.Name);
        //}
        //private void E7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E7.Name);
        //}
        //private void E8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E8.Name);
        //}
        //private void E9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E9.Name);
        //}
        //private void E10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E10.Name);
        //}
        //private void E11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E11.Name);
        //}
        //private void E12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + E12.Name);
        //}
        //private void F1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F1.Name);
        //}
        //private void F2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F2.Name);
        //}
        //private void F3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F3.Name);
        //}
        //private void F4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F4.Name);
        //}
        //private void F5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F5.Name);
        //}
        //private void F6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F6.Name);
        //}
        //private void F7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F7.Name);
        //}
        //private void F8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F8.Name);
        //}
        //private void F9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F9.Name);
        //}
        //private void F10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F10.Name);
        //}
        //private void F11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F11.Name);
        //}
        //private void F12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + F12.Name);
        //}
        //private void G1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G1.Name);
        //}
        //private void G2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G2.Name);
        //}
        //private void G3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G3.Name);
        //}
        //private void G4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G4.Name);
        //}
        //private void G5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G5.Name);
        //}
        //private void G6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G6.Name);
        //}
        //private void G7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G7.Name);
        //}
        //private void G8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G8.Name);
        //}
        //private void G9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G9.Name);
        //}
        //private void G10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G10.Name);
        //}
        //private void G11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G11.Name);
        //}
        //private void G12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + G12.Name);
        //}
        //private void H1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H1.Name);
        //}
        //private void H2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H2.Name);
        //}
        //private void H3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H3.Name);
        //}
        //private void H4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H4.Name);
        //}
        //private void H5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H5.Name);
        //}
        //private void H6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H6.Name);
        //}
        //private void H7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H7.Name);
        //}
        //private void H8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H8.Name);
        //}
        //private void H9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H9.Name);
        //}
        //private void H10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H10.Name);
        //}
        //private void H11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H11.Name);
        //}
        //private void H12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + H12.Name);
        //}
        //private void I1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I1.Name);
        //}
        //private void I2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I2.Name);
        //}
        //private void I3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I3.Name);
        //}
        //private void I4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I4.Name);
        //}
        //private void I5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I5.Name);
        //}
        //private void I6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I6.Name);
        //}
        //private void I7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I7.Name);
        //}
        //private void I8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I8.Name);
        //}
        //private void I9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I9.Name);
        //}
        //private void I10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I10.Name);
        //}
        //private void I11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I11.Name);
        //}
        //private void I12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + I12.Name);
        //}
        //private void J1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J1.Name);
        //}
        //private void J2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J2.Name);
        //}
        //private void J3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J3.Name);
        //}
        //private void J4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J4.Name);
        //}
        //private void J5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J5.Name);
        //}
        //private void J6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J6.Name);
        //}
        //private void J7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J7.Name);
        //}
        //private void J8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J8.Name);
        //}
        //private void J9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J9.Name);
        //}
        //private void J10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J10.Name);
        //}
        //private void J11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J11.Name);
        //}
        //private void J12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + J12.Name);
        //}
        //private void K1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K1.Name);
        //}
        //private void K2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K2.Name);
        //}
        //private void K3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K3.Name);
        //}
        //private void K4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K4.Name);
        //}
        //private void K5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K5.Name);
        //}
        //private void K6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K6.Name);
        //}
        //private void K7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K7.Name);
        //}
        //private void K8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K8.Name);
        //}
        //private void K9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K9.Name);
        //}
        //private void K10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K10.Name);
        //}
        //private void K11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K11.Name);
        //}
        //private void K12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + K12.Name);
        //}
        //private void L1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L1.Name);
        //}
        //private void L2_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L2.Name);
        //}
        //private void L3_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L3.Name);
        //}
        //private void L4_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L4.Name);
        //}
        //private void L5_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L5.Name);
        //}
        //private void L6_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L6.Name);
        //}
        //private void L7_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L7.Name);
        //}
        //private void L8_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L8.Name);
        //}
        //private void L9_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L9.Name);
        //}
        //private void L10_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L10.Name);
        //}
        //private void L11_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L11.Name);
        //}
        //private void L12_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Toạ độ: " + L12.Name);
        //}
    }
}