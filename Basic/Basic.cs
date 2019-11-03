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
    public static const int TOP_HEIGHT = 50;
    public static const int FORM_HEIGHT = 700 + TOP_HEIGHT;
    public static const int FORM_LENGTH = 700;
    public static const int INTERVAL_NUM = 10;
    private static const string FILE_NAME = "Basic" + @"database.db";
        
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
[serializable]
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

public class MainForm : Form{
    private const int FORM_HEIGHT = 200;
    private const int FORM_LENGTH = 300;
    private bool sysFlag, runFlag, wordFlag;
    private Label wordLabel, meanLabel;
    private Button preButton, stopButton, nextButton;

    public MainForm(){
        wordLabel = new Label();
        meanLabel = new Label();
        preButton = new Button();
        nextButton = new Button();
        stopButton = new Button();
        sysFlag = true;
        runFlag = true;
        initializeComponent();
    }
    private void initializeComponent(){
        int margin = 15;
        int buttonSize = (int)((FORM_LENGTH-margin*4)/3);

        wordLabel.Text = "word";
        wordLabel.Size = new Size(FORM_LENGTH-24, 40);
        wordLabel.Location = new Point(10,10);
        wordLabel.BackColor = Color.LightGreen;
        wordLabel.Font = new Font("Serif", 18, FontStyle.Bold);
        wordLabel.TextAlign = ContentAlignment.MiddleCenter;

        meanLabel.Text = "mean";
        meanLabel.Size = new Size(FORM_LENGTH-24, 40);
        meanLabel.Location = new Point(10,60);
        meanLabel.BackColor = Color.LightBlue;
        meanLabel.Font = new Font("Serif", 18, FontStyle.Bold);
        meanLabel.TextAlign = ContentAlignment.MiddleCenter;

        preButton.Text = "이전";
        preButton.Size = new Size((buttonSize), 30);
        preButton.Location = new Point(margin, 130);
        preButton.Font = new Font("Serif", 10, FontStyle.Bold);
        preButton.TextAlign = ContentAlignment.MiddleCenter;
        preButton.KeyDown += new KeyEventHandler(this.keyDown);
        preButton.Click += new EventHandler(this.btnClick);

        stopButton.Text = "정지/시작";
        stopButton.Size = new Size((buttonSize), 30);
        stopButton.Location = new Point(margin * 2 + buttonSize, 130);
        stopButton.Font = new Font("Serif", 10, FontStyle.Bold);
        stopButton.TextAlign = ContentAlignment.MiddleCenter;
        stopButton.KeyDown += new KeyEventHandler(this.keyDown);
        stopButton.Click += new EventHandler(this.btnClick);

        nextButton.Text = "다음";
        nextButton.Size = new Size((buttonSize), 30);
        nextButton.Location = new Point(margin * 3 + buttonSize * 2, 130);
        nextButton.Font = new Font("Serif", 10, FontStyle.Bold);
        nextButton.TextAlign = ContentAlignment.MiddleCenter;
        nextButton.KeyDown += new KeyEventHandler(this.keyDown);
        nextButton.Click += new EventHandler(this.btnClick);

        this.Controls.Add(wordLabel);
        this.Controls.Add(meanLabel);
        this.Controls.Add(preButton);
        this.Controls.Add(stopButton);
        this.Controls.Add(nextButton);

        this.Size = new Size(FORM_LENGTH, FORM_HEIGHT);
        this.KeyDown += new KeyEventHandler(this.keyDown);
        this.Opacity = 0.9;
        this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.TopMost = true;

        this.Closing += new CancelEventHandler(this.formClosing);

        Thread thread = new Thread(wordThread);
        thread.Start();

    }
    private void keyDown(object sender, KeyEventArgs e){
        if(e.KeyCode == Keys.Escape){
            sysFlag = false;
            this.Close();
        }else if(e.KeyCode == Keys.R){
            Word.wordDic.shuffle();
            Word.wordDic.showAll();
        }else if(e.KeyCode == Keys.F1){
            runFlag = false;
            wordFlag = false;
            SetFrame SetFrame = new SetFrame();
            SetFrame.ShowDialog();
        }
    }
    private void btnClick(object sender, EventArgs e){
        string btnText = ((Button)sender).Text;
        string word = "";
        switch(btnText){
        }
    }

}
public class Bullet{
    public float x, y, r, vel;
    public Brush brush;
    public Vector vector;
    public Bullet(){
        this(100, 100, 10, 80, 5, new SolidBrush(Color.FromArgb(100, 100, 255)));
    }
    public Bullet(float x, float y, float r, float angle, float vel, Brush brush){
        this.r = r;
        this.x = x - r;
        this.y = y;
        this.vector = new Vector(angle);
        this.vel = vel;
        this.brush = brush;
    }
    public void upate(){
        x += vector.x * vel;
        y += -(vector.y) * vel;
        if(x < -r * 2 || x > Basic.FORM_LENGTH + r * 2){ //when bullet meet left or right side
            vector.turnX;
        }if(y < -r * 2 || y > Basic.FORM_HEIGHT + r * 2){ //when bullet meet top or bottom side
            vector.turnY;
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
    public void turnX(){
        anglePI = (2.0f * (float)Math.PI) - anglePI;
        this.x = (float)Math.Cos(anglePI);
        this.y = (float)Math.Sin(anglePI);
    }
}