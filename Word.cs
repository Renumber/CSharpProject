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
    private bool sysFlag, runFlag, WordFlag;
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

        this.Controls.Add(wordLabel);

        this.Size = new Size(FORM_LENGTH, FORM_HEIGHT);
        this.Opacity = 0.9;
        this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.TopMost = true;
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
    //before showall, do shuffle
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