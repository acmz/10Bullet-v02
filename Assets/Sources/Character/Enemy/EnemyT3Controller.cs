using UnityEngine;
using UnityEngine.EventSystems;

//敵タイプT3：画面内の定位置で停止し、扇状弾をばら撒く設置型の敵
//T1（直進弾・低スコア）、T2（ホーミング弾・高スコア）に続く第3の敵タイプ
public class EnemyT3Controller : MonoBehaviour {

    //撃破時に加算する点数
    private const int ENEMY_SCORE = 300;

    //敵弾発射の間隔（秒）
    private const float SHOOT_INTERVAL = 3.0f;
    private float shootTime;

    //敵のレベル設定
    private const int ENEMY_LEVEL = 6;

    //生成から移動開始するまでの間隔（秒）。
    private const float MOVE_INTERVAL = 0.0f;
    private float moveStartTime;

    //敵が画面内へ進入するときの速度
    private const float ENEMY_MOVE_SPEED = 4.0f;

    //敵の移動方向
    private static readonly Vector2 MOVE_DIRECTION = Vector2.left;

    //進入移動を開始済みかどうかのフラグ（trueになったら再度速度設定処理を行わない）
    private bool enemyMoveEnabled = false;

    //生成されてから停止（スウェイ移行）するまでの時間（秒）
    private const float MOVE_TIME = 1.5f;
    private float moveTime;

    //停止後、上下に揺れる動きの振幅（大きいほど揺れ幅が広い）
    private const float SWAY_AMPLITUDE = 1.0f;

    //停止後、上下に揺れる動きの速さ（大きいほど速く揺れる）
    private const float SWAY_SPEED = 2.0f;

    //スウェイ動作の経過時間カウンター（cos関数の入力に使う）
    private float swayTime;

    //画面外判定
    private const float ENEMY_DESTROY_POS_LEFT = -10.0f;
    private const float ENEMY_DESTROY_POS_RIGHT = 30.0f;
    private const float ENEMY_DESTROY_POS_UP = 6.0f;
    private const float ENEMY_DESTROY_POS_DOWN = -6.0f;
    
    //出現時に決定した進行方向。
    private Vector2 moveDirection;

    //扇状弾を生成するオブジェクトへの参照
    private GameObject eBulletObj;

    // Use this for initialization
    void Start() {

        //シーン上の"E_Bullet_Generator"という名前のオブジェクトを検索して取得する
        this.eBulletObj = GameObject.Find("E_Bullet_Generator");

        //敵の進行方向を設定
        this.moveDirection = MOVE_DIRECTION;

    }

    // Update is called once per frame
    void Update() {

        //弾発射間隔の経過時間を加算する
        this.shootTime += Time.deltaTime;
        //発射間隔（SHOOT_INTERVAL）を超えたら弾を発射する
        if(this.shootTime >= SHOOT_INTERVAL) {

            //敵の位置を基に、敵弾を発射
            this.ShootBullet();

            //発射間隔カウンターをリセットする
            this.shootTime = 0f;

        }

        //移動・弾発射で使うため、自分自身のRigidbody2Dコンポーネントを取得する
        Rigidbody2D enemyBody = this.GetComponent<Rigidbody2D>();

        //移動開始
        this.moveStartTime += Time.deltaTime;
        if(!this.enemyMoveEnabled && this.moveStartTime >= MOVE_INTERVAL) {

            //敵を移動させる
            this.EnemyMove(this.moveDirection);

            //移動を開始したことを記録するフラグをonにする
            this.enemyMoveEnabled = true;

            //移動開始までの経過時間カウンターをリセットする
            this.moveStartTime = 0f;

        }

        //停止（スウェイ移行）までの経過時間を加算する
        this.moveTime += Time.deltaTime;
        if(this.moveTime >= MOVE_TIME) {

            //敵を移動させる
            this.EnemySway(this.moveDirection);

            //スウェイ動作の経過時間を加算する
            this.swayTime += Time.deltaTime;

        }

        //現在位置が画面外判定の左端・右端・上端・下端のいずれかを超えていないか判定する
        if(this.gameObject.transform.position.x < ENEMY_DESTROY_POS_LEFT
            || this.gameObject.transform.position.x > ENEMY_DESTROY_POS_RIGHT
            || this.gameObject.transform.position.y > ENEMY_DESTROY_POS_UP
            || this.gameObject.transform.position.y < ENEMY_DESTROY_POS_DOWN) {

            //画面外に出た場合、自分自身のGameObjectを破棄する
            Destroy(this.gameObject);

        }
    }

    //敵弾を発射
    private void ShootBullet() {

        //敵の位置を基に、敵弾を発射
        Vector2 enemyPos = this.gameObject.transform.position;
        this.eBulletObj.GetComponent<EBulletGenerator>().EBulletGenerate(
            enemyPos
            , ENEMY_LEVEL
            , EBulletGenerator.EBulletType.fan);

    }

    //敵の進行方向へ移動する
    private void EnemyMove(Vector2 inDirection) {

        //敵Rigidbody取得
        Rigidbody2D enemyBody = this.GetComponent<Rigidbody2D>();

        //進行方向（direction）とスピードを設定
        enemyBody.linearVelocity = inDirection * ENEMY_MOVE_SPEED;

        //進行方向へ力を加え、物理的に移動を発生させる
        enemyBody.AddForce(inDirection);

    }

    //スウェイ移動する
    private void EnemySway(Vector2 inDirection) {

        //敵Rigidbody取得
        Rigidbody2D enemyBody = this.GetComponent<Rigidbody2D>();

        //スウェイ用の速度ベクトルを初期化する（X方向には移動させない）
        Vector2 swayVelocity = Vector2.zero;

        //cos関数を使い、滑らかに往復するY方向の速度を計算する
        swayVelocity.y = Mathf.Cos(this.swayTime * SWAY_SPEED) * SWAY_AMPLITUDE;

        //計算した速度をRigidbody2Dに設定し、上下に揺れる動きを実現する
        enemyBody.linearVelocity = swayVelocity;

    }

    //自機の弾（トリガー）と衝突した際にUnityから自動的に呼び出されるコールバック
    private void OnTriggerEnter2D(Collider2D collision) {

        //衝突した相手が自弾（P_Bullet_Prefabの複製）かどうかを名前で判定する
        if(collision.gameObject.name == "P_Bullet_Prefab(Clone)") {

            //スコアや撃破数を管理しているGameDirectorオブジェクトを検索して取得する
            GameObject gameDirectorObj = GameObject.Find("GameDirector");

            //倒されたこと、加算する点数をDirectorに伝える
            gameDirectorObj.GetComponent<GameDirector>().EnemyDestroyNumPlus();
            gameDirectorObj.GetComponent<GameDirector>().ScorePlus(ENEMY_SCORE);

            //自分自身（敵）のGameObjectを破棄する
            Destroy(this.gameObject);

        }

    }

}
