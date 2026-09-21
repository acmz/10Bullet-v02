using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//外部ファイル読み込み用のクラスライブラリ読み込み
using System.IO;    //System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System.Text;  //Encoding
using System;       //Exception


//敵出現パターン(csv)
//フォーマット
//出現wave,出現タイミング(s.ms),敵の種類,出現座標(x),出現座標(y)
//例)
//wave1開始から1.5秒後に、T1を座標(2.0,3.0)に出現させる
//1,1.5,1,2.0,3.0
//出現wave:1
//出現タイミング(s.ms):1.5
//敵の種類:1
//出現座標(x):2.0
//出現座標(y):3.0

public class FileReader : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //敵の出現パターンファイルを読み込む
    public List<string> ReadEnemyGeneratePattern() {

        //csvファイルの内容を格納するList
        List<string> csvRecord = new List<string>();

        //enemyGeneratePattern.csvファイルを読み込む
        TextAsset fi = Resources.Load("enemyGeneratePattern") as TextAsset;

        try {

            //CSVファイル読み込み
            string[] sr = fi.text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach(string lineStr in sr) {

                if (lineStr.IndexOf("#") >= 0) {
                    //コメント行は無視する
                    continue;
                }

                //読み込んだ行をListに格納
                csvRecord.Add(lineStr);

            }

        }
        catch (Exception e) {
            Debug.Log(e.Message);
        }

        return csvRecord;

    }
}
