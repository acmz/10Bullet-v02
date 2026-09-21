using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour {

    //自機の弾数表示
    private GameObject pBulletNum;
    private int pBulletStock = 0;
    public int PBulletStock {
        get {
            return pBulletStock;
        }
    }
    private const string P_BULLET_NUM_MSG = "Bullet × ";

    //自機の最大所持弾数
    private const int P_BULLET_MAX = 10;

    //スコア表示
    private GameObject scoreNum;
    private int score = 0;
    private const string SCORE_MSG = "Score ";
    private const string SCORE_FORMAT = "{0:#,0}";

    //敵撃破数
    private int enemyDestroyNum;

    //Wave数
    private const int START_WAVE_NUM = 0;
    private int waveNum = 0;
    public int WaveNum {
        get {
            return waveNum;
        }
    }

    //残り時間
    private GameObject timeLeftUI;
    private const float LIMIT_TIME = 10.0f;
    private const string TIME_LEFT_MSG = "Time : ";
    private const string TIME_LEFT_FORMAT = "F2";
    private float timeLeft = 0f;
    public float TimeLeft {
        get {
            return timeLeft;
        }
    }

    //Wave開始コントロールフラグ
    private bool isWaveInit = false;
    public bool IsWaveInit {
        get {
            return isWaveInit;
        }
    }

    //コルーチン（処理停止）制御フラグ
    private bool isSleeping = false;

    // Use this for initialization
    void Start () {

        //残弾数UI取得、初期化
        this.pBulletNum = GameObject.Find("P_Bullet_Num");
        this.PBulletNumView(this.pBulletStock);

        //残り時間UI取得、初期化
        this.timeLeftUI = GameObject.Find("TimeLeft");
        this.TimeLeftView(this.timeLeft);

        //スコアUI取得、初期化
        this.scoreNum = GameObject.Find("Score");
        this.ScoreReset();

        //Wave数初期化
        this.WaveNumInit();

        //Game開始
        this.isWaveInit = true;

    }

    // Update is called once per frame
    void Update () {

        if (this.isWaveInit) {

            //Wave開始前
            //残弾数と残り時間のリセット、Wave数の設定
            StartCoroutine("InitPlayerStatus", 0.5f);

        } else {

            //Wave開始後
            //残り時間を減らす
            this.TimeLeftMinus();

            //残り時間が0になったら、次のWaveへ。
            if (this.timeLeft <= 0f) {
                this.isWaveInit = true;
            }

        }

    }

    //プレイヤーステータス初期化（コルーチン）
    private IEnumerator InitPlayerStatus(float inTime) {

        //初期化処理中に再度呼ばれたら、何もせずに抜ける
        if (this.isSleeping) {
            yield break;
        }

        this.isSleeping = true;

        //残弾数回復
        while (this.pBulletStock < P_BULLET_MAX) {

            yield return new WaitForSeconds(inTime);
            this.PBulletNumPlus();

        }

        //残り時間初期化
        yield return new WaitForSeconds(inTime);
        this.TimeReset();

        //Wave数設定
        this.NextWave();

        //Wave開始
        this.isWaveInit = false;

        this.isSleeping = false;

    }

    //残弾数増加
    public void PBulletNumPlus() {

        //残弾数を回復
        this.pBulletStock++;

        //表示更新
        this.PBulletNumView(this.pBulletStock);

    }

    //残弾数現象
    public void PBulletNumMinus() {

        //残弾数を減少
        this.pBulletStock -= 1;

        //表示更新
        this.PBulletNumView(this.pBulletStock);

    }

    //残弾数表示
    private void PBulletNumView(int inBulletNum) {

        //残弾数表示を更新
        this.pBulletNum.GetComponent<Text>().text = P_BULLET_NUM_MSG + inBulletNum;

    }

    //敵撃破数初期化
    public void EnemyDestroyNumReset() {

        this.enemyDestroyNum = 0;

    }

    //敵撃破数加算
    public void EnemyDestroyNumPlus() {

        this.enemyDestroyNum++;

    }

    //スコア初期化
    public void ScoreReset() {

        //スコア初期化
        this.score = 0;

        //表示更新
        this.ScoreView(this.score);

    }

    //スコア加算
    public void ScorePlus(int inScore) {

        //スコア加算
        this.score += inScore * this.enemyDestroyNum;

        //表示更新
        this.ScoreView(this.score);

    }

    //スコア表示
    private void ScoreView(int inViewScore) {

        //スコア表示を更新(3桁カンマ区切り)
        this.scoreNum.GetComponent<Text>().text = SCORE_MSG + string.Format(SCORE_FORMAT, inViewScore);

    }

    //残り時間初期化
    public void TimeReset() {

        //残り時間初期化
        this.timeLeft = LIMIT_TIME;

        //表示更新
        this.TimeLeftView(this.timeLeft);

    }

    //残り時間減算
    public void TimeLeftMinus() {

        //残り時間が0の場合、何もしない。
        if(this.timeLeft == 0f) {
            return;
        }

        //残り時間減算
        this.timeLeft -= Time.deltaTime;

        //残り時間が0以下になったら、0固定にする
        if(this.timeLeft <= 0f) {

            this.timeLeft = 0f;

        }

        //表示更新
        this.TimeLeftView(this.timeLeft);

    }

    //残り時間表示
    private void TimeLeftView(float inTimeLeft) {

        //残り時間を表示（ss.ms）
        this.timeLeftUI.GetComponent<Text>().text = TIME_LEFT_MSG + inTimeLeft.ToString(TIME_LEFT_FORMAT);

    }

    //Wave数初期化
    private void WaveNumInit() {

        this.waveNum = START_WAVE_NUM;

    }

    //Wave数更新
    public void NextWave() {

        this.waveNum++;

    }

}
