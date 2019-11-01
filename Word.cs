using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Json;

class Word{
    public static WordDic wordDic;
    static void Main(string[] args){
        if(File.Exists(@"WordDic.db")){
            load();
        }else{
            wordDic = new WordDic();
            wordDic.addDic("word", "mean");
            wordDic.addDic("F1", "add word");
            wordDic.addDic("R", "shuffle");
            save();
        }
        wordDic.shuffle();
        Application.Run(new MainFrame());
    }
    public static void load(){
        DataContractJsonSerializer dataContract = new DataContractJsonSerializer(typeof(WordDic));
        using(FileStream fs = new FileStream(@"WordDic.db", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)){
            wordDic = dataContract.ReadObject(fs) as WordDic;
        }
        Console.WriteLine("load");
    }
    public static void save(){
        wordDic.shuffle();
        DataContractJsonSerializer dataContract = new DataContractJsonSerializer(typeof(WordDic));
        using(FileStream fs = new FileStream(@"WordDic.db", FileMode.Create, FileAccess.ReadWrite, FileShare.None)){
            dataContract.WriteObject(fs, wordDic);
        }
        Console.WriteLine("save");
    }
}

public class MainFrame : Form{
    private const int FORM_HEIGHT = 200;
    private const int FORM_LENGTH = 300;
    private bool sysFlag, runFlag, wordFlag;
    private Label wordLabel, meanLabel;
    private Button preButton, stopButton, nextButton;

    public MainFrame(){
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
        }
    }
    private void btnClick(object sender, EventArgs e){
        string btnText = ((Button)sender).Text;
        string word = "";
        switch(btnText){
            case "이전":
                runFlag = false;
                word = Word.wordDic.getPre();
                wordLabel.Text = word;
                meanLabel.Text = Word.wordDic.getMean(word);
                wordFlag = false;
                break;
            case "정지/시작":
                runFlag = !runFlag;
                Console.WriteLine(runFlag);
                break;
            case "다음":
                runFlag = false;
                word = Word.wordDic.getNext();
                wordLabel.Text = word;
                meanLabel.Text = Word.wordDic.getMean(word);
                wordFlag = false;
                runFlag = true;
                break;
            default :
                break;
        }
    }
    private void formClosing(object sender, CancelEventArgs e){
        Console.WriteLine("끝!");
        sysFlag = false;
    }
    private void wordThread(){
        wordFlag = false;
        string word = "";
        while(sysFlag == true){
            if(runFlag == true && wordFlag == false){
                word = Word.wordDic.getNext();
                wordLabel.Text = word;
                meanLabel.Text = " ";
                wordFlag = true;
                Thread.Sleep(1700);
            }if(runFlag == true && wordFlag == true){
                meanLabel.Text = Word.wordDic.getMean(word);
                wordFlag = false;
                Thread.Sleep(2000);
            }
        }
    }
}
[Serializable]
public class WordDic{
    private Dictionary<string,string> dic;
    private List<string> keys;
    private int index;

    public WordDic(){
        dic = new Dictionary<string, string>();
        index = 0;
    }
    public void addDic(string word, string mean){
        if(dic.ContainsKey(word)){
            Console.WriteLine("이미있");
        }else{
            dic.Add(word, mean);
        }
    }
    public void shuffle(){
        keys = new List<string>(dic.Keys);
        Random rnd = new Random();
        int n = keys.Count;
        while((n--)>1){
            int k = rnd.Next(n+1);
            string temp = keys[k];
            keys[k] = keys[n];
            keys[n] = temp;
        }
    }
    //before showAll, do shuffle
    public void showAll(){
        foreach(string str in keys){
            Console.WriteLine(str + "\t" + dic[str]);
        }
    }
    public string getNext(){
        index = (++index)%(keys.Count);
        return keys[index];
    }
    public string getPre(){
        index = (keys.Count + (--index)) % (keys.Count);
        return keys[index];
    }
    public string getMean(string key){
        return dic[key];
    }
    public List<string> getKey(){
        return new List<string>(dic.Keys);
    }
    public void clear(){
        dic.Clear();
        index = 0;
    }
    public void removeDic(string key){
        dic.Remove(key);
    }
    public bool isEmpty(){
        if(dic.Count == 0){
            return true;
        }else{
            return false;
        }
    }
}