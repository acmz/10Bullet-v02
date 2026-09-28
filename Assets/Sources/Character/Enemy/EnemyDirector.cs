using System.Collections.Generic;
using UnityEngine;

public class EnemyDirector : MonoBehaviour {

    
    //GameDirectorオブジェクト
    private GameObject gameDirectorObj;


    //出現させる敵の情報のList
    //各waveの敵出現パターン
    private List<List<string[]>> generateEnemyEachWaveList = new List<List<string[]>>();
    private List<string[]> thisWaveEnemyPattern;
    string[] generateParameter;
    private int thisWaveEnemyPatternIndex = 0;
    private float generateTime = 0f;

    //現在のWave数
    private int currentWaveNum = 0;

    //敵出現パターンの要素
    private enum GenerateEnemyProperty : int {
          time = 0
        , type = 1
        , xPos = 2
        , yPos = 3
    }

    //敵出現パターンの要素(csv)
    private enum GeneratePatternProperty : int {
        wave = 0
        , time = 1
        , type = 2
        , xPos = 3
        , yPos = 4
    }

    //敵の種類
    private enum EnemyType : int {
        T1 = 1
      , T2 = 2
      , T3 = 3
      , T4 = 4
      , T5 = 5
      , T6 = 6
    }

    //敵タイプ別生成オブジェクト
    private GameObject enemyT1GeneratorObj;
    private GameObject enemyT2GeneratorObj;
    private GameObject enemyT3GeneratorObj;
    private GameObject enemyT4GeneratorObj;
    private GameObject enemyT5GeneratorObj;
    private GameObject enemyT6GeneratorObj;

    // Start is called before the first frame update
    void Start()
    {

        //敵の出現パターン取得
        GameObject fileReader = GameObject.Find("FileReader");
        List<string> enemyPattern;
        enemyPattern = fileReader.GetComponent<FileReader>().ReadEnemyGeneratePattern();
        fileReader = null;
        this.InitEnemyGenerate(enemyPattern);

        //生成する敵のGeneratorオブジェクトを取得する。
        this.enemyT1GeneratorObj = GameObject.Find("Enemy_T1_Generator");
        this.enemyT2GeneratorObj = GameObject.Find("Enemy_T2_Generator");
        this.enemyT3GeneratorObj = GameObject.Find("Enemy_T3_Generator");
        this.enemyT4GeneratorObj = GameObject.Find("Enemy_T4_Generator");
        this.enemyT5GeneratorObj = GameObject.Find("Enemy_T5_Generator");
        this.enemyT6GeneratorObj = GameObject.Find("Enemy_T6_Generator");

        //GameDirectorオブジェクトを取得する。
        this.gameDirectorObj = GameObject.Find("GameDirector");

    }

    // Update is called once per frame
    void Update()
    {

        if(this.generateEnemyEachWaveList == null) {
            return;
        }

        if(this.gameDirectorObj == null) {
            return;
        }

        if(this.gameDirectorObj.GetComponent<GameDirector>().WaveNum == 0) {
            return;
        }

        if(this.gameDirectorObj.GetComponent<GameDirector>().IsWaveInit) {
            return;
        }

        //GameDirectorから現在のWave数を取得
        //Debug.Log("currentWaveNum : " + this.currentWaveNum);
        //Debug.Log("WaveNum : " + this.gameDirectorObj.GetComponent<GameDirector>().WaveNum);
        if(this.currentWaveNum < this.gameDirectorObj.GetComponent<GameDirector>().WaveNum) {

            //現在のWaveの敵生成パターンリストを取得する。
            this.currentWaveNum = this.gameDirectorObj.GetComponent<GameDirector>().WaveNum;
            this.thisWaveEnemyPattern = this.GetGenerateEnemyPattern(this.currentWaveNum - 1);
            this.thisWaveEnemyPatternIndex = 0;
            this.generateTime = 99.0f;

        }

        //全てのパターンを出し尽くしたら、次のWaveまで何もしない。
        if (this.thisWaveEnemyPatternIndex < thisWaveEnemyPattern.Count) {

            //生成パターン取得。
            this.generateParameter = this.thisWaveEnemyPattern[this.thisWaveEnemyPatternIndex];

            if(this.generateParameter == null) {
                return;
            }

            //現在の時間が敵の出現タイムを下回ったら、敵を出現させる。
            this.generateTime = float.Parse(this.generateParameter[(int)GenerateEnemyProperty.time]);
            if(this.generateTime >= this.gameDirectorObj.GetComponent<GameDirector>().TimeLeft) {

                //敵を生成する。
                int generateType = int.Parse(this.generateParameter[(int)GenerateEnemyProperty.type]);
                float generateXPos = float.Parse(this.generateParameter[(int)GenerateEnemyProperty.xPos]);
                float generateYPos = float.Parse(this.generateParameter[(int)GenerateEnemyProperty.yPos]);

                this.GenerateEnemy(generateType, generateXPos, generateYPos);
                this.thisWaveEnemyPatternIndex++;

            }

        }

    }

    //敵の出現パターンをListに格納する。
    public void InitEnemyGenerate(List<string> inEnemyPatternList) {

        //同一wave内での敵出現パターン
        List<string[]> generateEnemyPattern = new();

        //wave数（コントロールブレイク用）
        int waveNum = 1;

        foreach(string record in inEnemyPatternList) {

            if(record.Length <= 0) {
                continue;
            }

            string[] splitRecord = record.Split(',');

            string[] enemyPatternArray = new string[4];

            //出現wave判定(enemyListのIndex設定）
            int recordWaveNum = int.Parse(splitRecord[(int)GeneratePatternProperty.wave]);

            if(waveNum != recordWaveNum) {

                //各waveの出現パターンリストを格納。
                this.generateEnemyEachWaveList.Add(generateEnemyPattern);
                generateEnemyPattern = new List<string[]>();
                waveNum = recordWaveNum;

            }

            //敵の出現パターンを格納（出現タイミング(s.ms),敵の種類,出現座標(x),出現座標(y)）
            enemyPatternArray[(int)GenerateEnemyProperty.time] = splitRecord[(int)GeneratePatternProperty.time];
            enemyPatternArray[(int)GenerateEnemyProperty.type] = splitRecord[(int)GeneratePatternProperty.type];
            enemyPatternArray[(int)GenerateEnemyProperty.xPos] = splitRecord[(int)GeneratePatternProperty.xPos];
            enemyPatternArray[(int)GenerateEnemyProperty.yPos] = splitRecord[(int)GeneratePatternProperty.yPos];

            //同wave内での出現パターンを格納。
            generateEnemyPattern.Add(enemyPatternArray);

        }

        //最終waveの情報が格納されていないので、ここで格納する
        if(inEnemyPatternList.Count > 0) {
            generateEnemyEachWaveList.Add(generateEnemyPattern);
        }

    }

    //敵の出現タイミングを知る(Wave数、時間)ために、GameDirectorオブジェクトを受け取る。
    public void SetGameDirectorObj(GameObject inGameDirector) {

        this.gameDirectorObj = inGameDirector;

    }

    //各Waveの敵出現パターンを返す。
    private List<string[]> GetGenerateEnemyPattern(int inWaveNum) {

        return this.generateEnemyEachWaveList[inWaveNum];

    }

    //敵を出現させる。
    private void GenerateEnemy(int inEnemyType, float inXPos, float inYPos) {

        //敵の種類を判断して、生成する。
        switch(inEnemyType) {
            case (int)EnemyType.T1:
                this.enemyT1GeneratorObj.GetComponent<EnemyT1Generator>().GenerateEnemy(inXPos, inYPos);
                break;

            case (int)EnemyType.T2:
                this.enemyT2GeneratorObj.GetComponent<EnemyT2Generator>().GenerateEnemy(inXPos, inYPos);
                break;

            case (int)EnemyType.T3:
                this.enemyT3GeneratorObj.GetComponent<EnemyT3Generator>().GenerateEnemy(inXPos, inYPos);
                break;

            case (int)EnemyType.T4:
                this.enemyT4GeneratorObj.GetComponent<EnemyT4Generator>().GenerateEnemy(inXPos, inYPos);
                break;

            case (int)EnemyType.T5:
                this.enemyT5GeneratorObj.GetComponent<EnemyT5Generator>().GenerateEnemy(inXPos, inYPos);
                break;

            case (int)EnemyType.T6:
                this.enemyT6GeneratorObj.GetComponent<EnemyT6Generator>().GenerateEnemy(inXPos, inYPos);
                break;

            default:
                break;
        }

    }

}
