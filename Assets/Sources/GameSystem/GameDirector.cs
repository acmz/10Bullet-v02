using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using unityroom.Api;

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
    private float score = 0f;
    private const string SCORE_MSG = "Score ";
    private const string SCORE_FORMAT = "{0:#,0}";

    //敵撃破数
    private int enemyDestroyNum;

    //残り時間
    private GameObject timeLeftUI;
    private const float LIMIT_TIME = 10.0f;
    private const string TIME_LEFT_MSG = "Time : ";
    private const string TIME_LEFT_FORMAT = "F2";
    private float timeLeft = 0f;
    public float TimeLeft {
        get {
            return this.timeLeft;
        }
    }

    //ゲームオーバー
    private GameObject gameOverUI;

    //タイトル画面へ戻る
    private GameObject returnTitleUI;

    //ゲームオーバーコントロールフラグ
    private bool isGameOver = false;

    //Wave数表示
    private GameObject waveUI;
    private const int START_WAVE_NUM = 0;
    private int waveNum = 0;
    public int WaveNum {
        get {
            return this.waveNum;
        }
    }
    private const string WAVE_NUM_MSG = "Wave ";

    //Ready
    private GameObject readyUI;
    
    //Wave開始コントロールフラグ
    private bool isWaveInit = false;
    public bool IsWaveInit {
        get {
            return this.isWaveInit;
        }
    }

    //Wave終了時のウェイトタイム
    private const float WAVE_END_WAIT_TIME = 3.0f;

    //コルーチン（処理停止）制御フラグ
    private bool isSleeping = false;

    //ゲームオーバー表示時間
    //private const float GAMEOVER_WAIT_TIME = 5.0f;
    //private float gameOverWaitTime = 0f;

    // Use this for initialization
    void Start() {

        //ゲームオーバー、タイトルへ戻る非表示
        this.gameOverUI = GameObject.Find("GameOver");
        this.gameOverUI.SetActive(false);
        this.returnTitleUI = GameObject.Find("ReturnTitle");
        this.returnTitleUI.SetActive(false);

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

        //Wave数、ReadyUIオブジェクト取得
        this.waveUI = GameObject.Find("Wave");
        this.readyUI = GameObject.Find("Ready");

        //Game開始
        this.isWaveInit = true;

    }

    // Update is called once per frame
    void Update() {


        //キーボードの状態取得
        Keyboard keyboard = Keyboard.current;

        //キーボードが接続されていない場合、何もしない
        if(keyboard == null) {
            return;
        }

        //ゲームオーバー中にESCキーが押されたら、シーンをリセットする
        if(this.isGameOver && keyboard.escapeKey.wasPressedThisFrame) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        //ゲームオーバー中なら何もしない
        if(this.isGameOver) {
            return;
        }

        //自機が消えたらゲームオーバー
        if(GameObject.Find("Player") == null && !this.isGameOver) {

            //Debug.Log("Game Over");
            this.gameOverUI.SetActive(true);
            this.returnTitleUI.SetActive(this);

            //スコア表示
            UnityroomApiClient.Instance.SendScore(1, this.score, ScoreboardWriteMode.HighScoreDesc);

            this.isGameOver = true;
            return;
        
        }

        //Wave開始前
        //残弾数と残り時間のリセット、Wave数の設定
        if(this.isWaveInit) {

            _ = this.StartCoroutine("InitPlayerStatus", 0.5f);
            return;

        }

        //Wave開始後
        //残り時間を減らす
        this.TimeLeftMinus();

        //残り時間が0になったら、全ての敵と敵弾を削除し、次のWaveへ。
        if(this.timeLeft <= 0f) {
            _ = this.StartCoroutine("WaveEnd", WAVE_END_WAIT_TIME);
        }

    }

    //プレイヤーステータス初期化（コルーチン）
    private IEnumerator InitPlayerStatus(float inTime) {

        //初期化処理中に再度呼ばれたら、何もせずに抜ける
        if(this.isSleeping) {
            yield break;
        }

        this.isSleeping = true;

        //Wave数設定
        this.NextWave();

        //Wave数、Ready表示
        this.waveUI.SetActive(true);
        this.waveView(this.waveNum);
        this.readyUI.SetActive(true);

        //残弾数回復
        while(this.pBulletStock < P_BULLET_MAX) {

            yield return new WaitForSeconds(inTime);
            this.PBulletNumPlus();

        }

        //残り時間初期化
        yield return new WaitForSeconds(inTime);
        this.TimeReset();

        //Wave開始
        this.waveUI.SetActive(false);
        this.readyUI.SetActive(false);
        this.isWaveInit = false;
        this.isSleeping = false;

    }

    //Wave終了
    private IEnumerator WaveEnd(float inTime) {

        this.DestroyEnemyAll();
        yield return new WaitForSeconds(inTime);
        this.isWaveInit = true;

    }

    //敵と敵弾の全消去
    private void DestroyEnemyAll() {
        //Debug.Log("destroy object ");

        //画面上に残っている敵を全て削除（タグ：Enemyに属するオブジェクト）
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
            Destroy(enemy);
        }

        //画面上に残っている敵弾を全て削除（タグ：EnemyBulletに属するオブジェクト）
        foreach(GameObject eBullet in GameObject.FindGameObjectsWithTag("EnemyBullet")) {
            Destroy(eBullet);
        }

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
        this.score = 0f;

        //表示更新
        this.ScoreView(this.score);

    }

    //スコア加算
    public void ScorePlus(float inScore) {

        //スコア加算
        this.score += inScore * this.enemyDestroyNum;

        //表示更新
        this.ScoreView(this.score);

    }

    //スコア表示
    private void ScoreView(float inViewScore) {

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

    //Wave数表示
    private void waveView(int inWaveNum) {

        //残弾数表示を更新
        this.waveUI.GetComponent<Text>().text = WAVE_NUM_MSG + inWaveNum;

    }
}
