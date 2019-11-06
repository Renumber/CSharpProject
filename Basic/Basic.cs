using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Json;

class Basic{
    public static int TOP_HEIGHT = 50;
    public static int FORM_HEIGHT = 700 + TOP_HEIGHT;
    public static int FORM_LENGTH = 700;
    public static int INTERVAL_NUM = 10;
    private static string FILE_NAME = "Basic" + @"database.db";
        
    public static Rank rank;
    public static List<Bullet> bullets;
    
    static void Main(string[] args){
        if(File.Exists(FILE_NAME)){
            load();
        }else{
            rank = new Rank();
            save();
        }
        Application.Run(new MainForm());
    }
    public static void load(){
        DataContractJsonSerializer dataContract = new DataContractJsonSerializer(typeof(Rank));
        using(FileStream fs = new FileStream(FILE_NAME, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)){
            rank = dataContract.ReadObject(fs) as Rank;
        }
        Console.WriteLine("load");
    }
    public static void save(){
        DataContractJsonSerializer dataContract = new DataContractJsonSerializer(typeof(Rank));
        using(FileStream fs = new FileStream(FILE_NAME, FileMode.Create, FileAccess.ReadWrite, FileShare.None)){
            dataContract.WriteObject(fs, rank);
        }
        Console.WriteLine("save");
    }
}
[Serializable]
public class Rank{
    public int length = 9;
    public int[] scores;
    public Rank(){
        scores = new int[length];
    }
    public void addScore(int score){
        for(int i = 0; i < length; i++){
            if(scores[i] < score){
                changeRank(i);
                scores[i] = score;
                break;
            }
        }
    }
    private void changeRank(int index){
        for(int i = length-2; i>= index; i--){
            scores[i+1] = scores[i];
        }
    }
}

public class RankForm : Form{
    private const int FORM_HEIGHT = 200;
    private const int FORM_LENGTH = 150;
    private ListBox rankBox;

    public RankForm(){
        rankBox = new ListBox();
        initializeComponent();
    }
    private void initializeComponent(){
        rankBox.Size = new Size(FORM_LENGTH, FORM_HEIGHT);
        rankBox.Location = new Point(0,0);
        rankBox.Font = new Font("Serif", 10);
        rankBox.KeyDown += new KeyEventHandler(this.keyDown);

        for(int i = 0; i < Basic.rank.length; i++){
            rankBox.Items.Add((i+1) + "등 : " + Basic.rank.scores[i] + "점");
        }
        this.Controls.Add(rankBox);
        this.Text = "당신의 점수는?";
        this.Size = new Size(FORM_LENGTH, FORM_HEIGHT); 
        this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
    }
    private void keyDown(object sender, KeyEventArgs e){
        if(e.KeyCode == Keys.Escape){
            this.Close();
        }
    }
}

public class MainForm : Form{
    bool topMouseDownFlag, gameFlag;
    bool upFlag, downFlag, rightFlag, leftFlag;
    int mouseX, mouseY, score;
    Label topLabel;
    Brush borderBrush;
    Random rnd;

    public MainForm(){
        score = 0;
        topMouseDownFlag = false;
        gameFlag = false;
        topLabel = new Label();
        borderBrush = new SolidBrush(Color.LightGreen);
        rnd = new Random();
        int x , y, r, angle, vel;
        Brush bulletBrush;
        for(int i = 0; i < 10; i++){
            x = 30;
            y = 30;
            r = rnd.Next(1, 31);
            angle = rnd.Next(0, 360);
            vel = rnd.Next(3, 7);
            bulletBrush = new SolidBrush(Color.FromArgb(rnd.Next(0,256), rnd.Next(0,256), rnd.Next(0,256)));
            Basic.bullets.Add(new Bullet(x, y, r, angle, vel, bulletBrush));
        }
        initializeComponent();
    }
    private void initializeComponent(){
        topLabel.Text = "Hello?";
        topLabel.Size = new Size(Basic.FORM_LENGTH, Basic.FORM_HEIGHT);
        topLabel.Location = new Point(0,0);
        topLabel.BackColor = Color.LightGreen;
        topLabel.Font = new Font("Serif", 16, FontStyle.Bold);
        topLabel.TextAlign = ContentAlignment.MiddleCenter;
//        topLabel.MouseDown += new MouseEventHandler(this.topDown);
//        topLabel.MouseMove += new MouseEventHandler(this.topMove);
//        topLabel.MouseUp += new MouseEventHandler(this.topUp);

        this.Controls.Add(topLabel);

        this.Size = new Size(Basic.FORM_LENGTH, Basic.FORM_HEIGHT);
        this.Opacity = 0.9;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.KeyDown += new KeyEventHandler(this.keyDown);
        this.KeyUp += new KeyEventHandler(this.keyUp);
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        this.SetStyle(ControlStyles.UserPaint, true);
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        this.UpdateStyles();

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        timer.Interval = Basic.INTERVAL_NUM;
        timer.Tick += new EventHandler(update);
        timer.Start();

        gameFlag = true;
    }
    protected override void OnPaintBackground(PaintEventArgs e){
        base.OnPaint(e);
        e.Graphics.Clear(Color.White);
        e.Graphics.FillRectangle(borderBrush, 0, 0, 2, Basic.FORM_HEIGHT);
        e.Graphics.FillRectangle(borderBrush, 0, Basic.FORM_HEIGHT - 2, Basic.FORM_LENGTH, 2);
        e.Graphics.FillRectangle(borderBrush, Basic.FORM_LENGTH - 2, 0, 2, Basic.FORM_HEIGHT);
        foreach (Bullet bullet in Basic.bullets){
            e.Graphics.FillEllipse(bullet.brush, bullet.x, bullet.y, bullet.r * 2, bullet.r * 2);
        }
    }
    private void update(object sender, EventArgs e){
        if(gameFlag){
            foreach (Bullet bullet in Basic.bullets){
                bullet.update();
            }
        }
        Invalidate();
    }
    private void gameOver(){
        gameFlag = false;
        MessageBox.Show("Game Over! \n" + score + " 점\n" + "R 키로 재시작");
        Basic.rank.addScore(score);
        Basic.save();
        //todo
        RankForm rankForm = new RankForm();
        rankForm.ShowDialog();
    }
    private void restart(){
        //todo
    }
    private void topDown(object sender, MouseEventArgs e){
        topMouseDownFlag = true;
        mouseX = e.X;
        mouseY = e.Y;
    }
    private void topMove(object sender, EventArgs e){
        if(topMouseDownFlag){
            this.SetDesktopLocation(Cursor.Position.X - mouseX, Cursor.Position.Y - mouseY);
        }
    }
    private void topUp(object sender, MouseEventArgs e){
        topMouseDownFlag = false;
    }
    private void keyDown(object sender, KeyEventArgs e){
        if(e.KeyCode == Keys.Up || upFlag){
            upFlag = true;
        }if(e.KeyCode == Keys.Down || downFlag){
            downFlag = true;
        }if(e.KeyCode == Keys.Right || rightFlag){
            rightFlag = true;
        }if(e.KeyCode == Keys.Left || leftFlag){
            leftFlag = true;
        }if(e.KeyCode == Keys.R){
            restart();
        }if(e.KeyCode == Keys.Escape){
            gameFlag = false;
            this.Close();
        }if(e.KeyCode == Keys.F1){
            gameOver();
        }
    }
    private void keyUp(object sender, KeyEventArgs e){
        if(e.KeyCode == Keys.Up){
            upFlag = false;
        }if(e.KeyCode == Keys.Down){
            downFlag = false;
        }if(e.KeyCode == Keys.Right){
            rightFlag = false;
        }if(e.KeyCode == Keys.Left){
            leftFlag = false;
        }
    }
}
public class Bullet{
    public float x, y, r, vel;
    public Brush brush;
    public Vector vector;
    public Bullet(float x, float y, float r, float angle, float vel, Brush brush){
        this.r = r;
        this.x = x - r;
        this.y = y;
        this.vector = new Vector(angle);
        this.vel = vel;
        this.brush = brush;
    }
    public void update(){
        x += vector.x * vel;
        y += -(vector.y) * vel;
        if(x < -r * 2 || x > Basic.FORM_LENGTH + r * 2){ //when bullet meet left or right side
            vector.turnX();
        }if(y < -r * 2 || y > Basic.FORM_HEIGHT + r * 2){ //when bullet meet top or bottom side
            vector.turnY();
        }
    } 
    /* 
    public bool isCol(){//return true if bullet collide with target
        float dis = (float)Math.Pow(x + r - tarX - tarR, 2.0f) + (float)Math.Pow(y + r -tarY - tarR, 2.0f);
        if(Math.Pow(r + tarR - 2, 2.0f) > dis){
            brush = new SolidBrush(Color.Red);
            return true;
        }
        return false;
    }
    */
}
public class Vector{
    public float x, y, anglePI;
    public Vector(float angle){
        this.anglePI = (float)Math.PI * angle / 180.0f;
        this.x = (float)Math.Cos(anglePI);
        this.y = (float)Math.Sin(anglePI);
    }
    public void turnX(){
        anglePI = ((3.0f * (float)Math.PI) - anglePI) % (2.0f * (float)Math.PI);
        this.x = (float)Math.Cos(anglePI);
        this.y = (float)Math.Sin(anglePI);
    }
    public void turnY(){
        anglePI = (2.0f * (float)Math.PI) - anglePI;
        this.x = (float)Math.Cos(anglePI);
        this.y = (float)Math.Sin(anglePI);
    }
}