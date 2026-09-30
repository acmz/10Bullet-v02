using UnityEngine;
using UnityEngine.EventSystems;

//敵タイプT4：画面上下から出現し、Y軸に沿って直進しながら、直進弾を発射し続ける敵。
//
//T1～T3はX軸方向（画面右から左、自機側）へ移動する敵だったため、
//移動の軸をY軸に変えたバリエーションとして作成した。
public class EnemyT4Controller : MonoBehaviour {

    //撃破時に加算する点数
    private const int ENEMY_SCORE = 200;

    //敵弾発射の間隔（秒）
    private const float SHOOT_INTERVAL = 0.5f;
    private float shootTime;

    //敵のレベル設定
    private const int ENEMY_LEVEL = 1;

    //敵の移動速度
    private const float ENEMY_MOVE_SPEED = 2.5f;

    //進入方向ベクトルを求めるためのY方向の変化量
    //（プラス方向を指定することで、Y軸マイナス側からプラス側＝画面下から上へ進む）
    //private const float ENEMY_MOVE_ANGLE_X = 1.0f;
    //private const float ENEMY_MOVE_ANGLE_Y_UP = 1.0f;
    //private const float ENEMY_MOVE_ANGLE_Y_DOWN = -1.0f;
    private static readonly Vector2 MOVE_DIRECTION_UP = Vector2.up;
    private static readonly Vector2 MOVE_DIRECTION_DOWN = Vector2.down;

    //Y軸の中央座標
    private static float DISPLAY_CENTER_POS = 0.5f;

    //生成から移動を開始するまでの間隔（秒）
    private const float MOVE_INTERVAL = 0.0f;
    private float moveStartTime;

    //移動を開始済みかどうかのフラグ（trueになったら再度速度設定処理を行わない）
    private bool enemyMoveEnabled = true;

    //画面外判定（この範囲を超えたら自分自身を破棄する。T1・T3と同じ範囲を流用）
    private const float ENEMY_DESTROY_POS_LEFT = -10.0f;
    private const float ENEMY_DESTROY_POS_RIGHT = 30.0f;
    private const float ENEMY_DESTROY_POS_UP = 6.0f;
    private const float ENEMY_DESTROY_POS_DOWN = -6.0f;

    //出現時に決定した進行方向。
    private Vector2 moveDirection;

    //敵弾生成オブジェクト
    private GameObject eBulletObj;

    //敵の死亡時のエフェクト
    public GameObject destroyEffect;

    // Use this for initialization
    void Start() {

        this.eBulletObj = GameObject.Find("E_Bullet_Generator");

        //敵の進行方向を設定
        this.moveDirection = this.SetMoveDirection(this.gameObject.transform.position);

    }

    //敵の進行方向を設定する
    private Vector2 SetMoveDirection(Vector2 inStartPosition) {

        //移動先のY軸方向を決める
        //初期位置が中央よりマイナスの場合は上（マイナスからプラスへ）
        //初期位置が中央よりプラスの場合は下（プラスからマイナスへ）
        if(inStartPosition.y < DISPLAY_CENTER_POS) {
            return MOVE_DIRECTION_UP;
        }
        return MOVE_DIRECTION_DOWN;

    }

    // Update is called once per frame
    void Update() {

        //弾発射間隔の経過時間を加算する
        this.shootTime += Time.deltaTime;
        //発射間隔（SHOOT_INTERVAL）を超えたら、直進弾を発射する
        if(this.shootTime >= SHOOT_INTERVAL) {

            this.ShootBullet();

            //発射間隔カウンターをリセットする（繰り返し発射するため）
            this.shootTime = 0f;

        }

        //敵の移動（Y軸マイナス側からプラス側へ、画面下から上へ直進する）
        this.moveStartTime += Time.deltaTime;
        if(this.enemyMoveEnabled && this.moveStartTime >= MOVE_INTERVAL) {

            //敵を移動させる
            this.EnemyMove(this.moveDirection);

            //一度速度を設定したら、以後は再設定しない（Y軸プラス方向へ進み続ける）
            this.enemyMoveEnabled = false;

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

    //直進弾を1発発射する
    private void ShootBullet() {

        //敵の位置を基に、敵弾を発射
        Vector2 enemyPos = this.gameObject.transform.position;
        this.eBulletObj.GetComponent<EBulletGenerator>().EBulletGenerate(
            enemyPos
            , ENEMY_LEVEL
            , EBulletGenerator.EBulletType.straight);

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

            //死亡時のエフェクトを表示
            _ = Instantiate(this.destroyEffect,
                this.transform.position,
                Quaternion.identity);

        }

    }

}
