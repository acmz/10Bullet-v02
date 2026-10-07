using UnityEngine;

//敵タイプT6：出現した瞬間の自機の座標を記憶し、そこへ向けて直進する敵。
//移動中に自機が位置を変えても、狙う座標は出現時点のままで、追尾はしない
//（自機を狙い撃つのではなく、「自機が居た場所」を通過するイメージ）。
//
//スプライト（三角形）は、回転していない状態で頂点がY軸プラス方向（真上）を向いている前提。
//（Enemy_T6_Prefab の PolygonCollider2D の頂点座標から、そのように配置されていることを確認済み）
//このスプライトをZ軸で回転させ、頂点が進行方向＝出現時点の自機の座標の方を向くようにする。
public class EnemyT6Controller : MonoBehaviour {

    //撃破時に加算する点数
    private const int ENEMY_SCORE = 150;

    //生成から移動開始するまでの間隔
    private const float MOVE_INTERVAL = 0.5f;
    private float moveStartTime;

    //敵の移動速度
    private const float ENEMY_MOVE_SPEED = 4.0f;

    //自機の座標が取得できなかった場合（既に消えている場合等）に使う、既定の進行方向
    //（画面左＝自機がいるはずの方向へ直進する）
    private static readonly Vector2 DEFAULT_DIRECTION = Vector2.left;

    //スプライトの頂点はY軸プラス方向（Atan2の角度でいう90度の位置）を向いているため、
    //進行方向の角度からこの分だけ引いて補正する
    private const float SPRITE_ANGLE_OFFSET = 90.0f;

    //敵移動済み判定フラグ
    private bool enemyMoveEnabled = true;

    //画面外判定（他の敵タイプと同じ範囲を流用）
    private const float ENEMY_DESTROY_POS_LEFT = -10.0f;
    private const float ENEMY_DESTROY_POS_RIGHT = 30.0f;
    private const float ENEMY_DESTROY_POS_UP = 6.0f;
    private const float ENEMY_DESTROY_POS_DOWN = -6.0f;

    //敵弾発射の間隔（秒）
    private const float SHOOT_INTERVAL = 0.0f;
    private float shootTime;

    //敵弾発射済み判定フラグ
    private bool shooted = false;

    //敵のレベル設定
    private const int ENEMY_LEVEL = 1;

    //敵弾生成オブジェクト
    private GameObject eBulletObj;

    //出現時に決定した進行方向。移動だけでなく、弾を発射する方向にもそのまま使う
    private Vector2 moveDirection;

    //敵の死亡時のエフェクト
    public GameObject destroyEffect;

    //敵の死亡時のSE
    private GameObject destroySE;

    // Use this for initialization
    void Start() {

        //出現した瞬間の自機の座標を取得するため、Playerオブジェクトを検索する
        GameObject playerObj = GameObject.Find("Player");

        Vector2 direction = DEFAULT_DIRECTION;
        if(playerObj != null) {

            //自機が見つからない場合（敵弾に当たって既に消えている等）は、左方向へ直進する
            //自機が存在する場合、出現時点の自機の座標と自分の座標の差分から進行方向を求める
            Vector2 targetPos = playerObj.transform.position;
            direction = targetPos - (Vector2)this.gameObject.transform.position;

        }

        //求めた方向へスプライトの向きを設定する
        this.moveDirection = direction;
        this.SetSpriteDirection(this.moveDirection);

        //敵弾生成オブジェクト取得
        this.eBulletObj = GameObject.Find("E_Bullet_Generator");

        //敵死亡時のSEオブジェクト取得
        this.destroySE = GameObject.Find("SEDirector");

    }

    //引数の方向へスプライトの向き（Z軸回転）を更新する
    private void SetSpriteDirection(Vector2 inDirection) {

        //方向ベクトルの長さが0の場合（自機と全く同じ座標に出現した場合）は正規化できないため、既定方向を使う
        Vector2 direction = (inDirection.sqrMagnitude > 0f) ? inDirection.normalized : DEFAULT_DIRECTION;

        //進行方向ベクトルから角度を求める（Atan2はX軸プラス方向を0度とするラジアン角を返す）
        float angleRad = Mathf.Atan2(direction.y, direction.x);

        //ラジアンを度数法に変換し、スプライトの向き（頂点が90度＝真上）とのずれを補正する
        float angleDeg = (angleRad * Mathf.Rad2Deg) - SPRITE_ANGLE_OFFSET;

        //Z軸周りの回転として、スプライトの頂点が進行方向を向くように設定する
        this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

    }

    // Update is called once per frame
    void Update() {

        //弾発射間隔の経過時間を加算する
        this.shootTime += Time.deltaTime;
        //発射間隔（SHOOT_INTERVAL）を超えたら、敵の進行方向へ弾を発射する
        if(this.shootTime >= SHOOT_INTERVAL && !this.shooted) {

            this.ShootBullet();

            //敵弾発射済み
            this.shooted = true;

        }

        //敵移動
        this.moveStartTime += Time.deltaTime;
        if(this.enemyMoveEnabled && this.moveStartTime >= MOVE_INTERVAL) {

            this.EnemyMove(this.moveDirection);
            this.enemyMoveEnabled = false;

        }

        //画面外に出たら自分自身を破棄する
        if(this.gameObject.transform.position.x < ENEMY_DESTROY_POS_LEFT
            || this.gameObject.transform.position.x > ENEMY_DESTROY_POS_RIGHT
            || this.gameObject.transform.position.y > ENEMY_DESTROY_POS_UP
            || this.gameObject.transform.position.y < ENEMY_DESTROY_POS_DOWN) {

            //画面外に出た場合、自分自身のGameObjectを破棄する
            Destroy(this.gameObject);

        }

    }

    //敵の進行方向（moveDirection）へ弾を1発発射する
    private void ShootBullet() {

        //敵の位置を基に、敵弾を発射
        Vector2 enemyPos = this.gameObject.transform.position;
        this.eBulletObj.GetComponent<EBulletGenerator>().EBulletGenerate(
            enemyPos
            , ENEMY_LEVEL
            , EBulletGenerator.EBulletType.homing);

    }

    //引数の方向へスプライトの向き（Z軸回転）を更新する
    private void EnemyMove(Vector2 inDirection) {

        //方向ベクトルの長さが0の場合（自機と全く同じ座標に出現した場合）は正規化できないため、既定方向を使う
        Vector2 direction = (inDirection.sqrMagnitude > 0f) ? inDirection.normalized : DEFAULT_DIRECTION;

        //敵Rigidbody取得
        Rigidbody2D enemyBody = this.GetComponent<Rigidbody2D>();

        //進行方向（direction）とスピードを設定
        enemyBody.linearVelocity = direction * ENEMY_MOVE_SPEED;

        //進行方向へ力を加え、物理的に移動を発生させる
        enemyBody.AddForce(direction);

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

            //死亡時のSEを再生
            this.destroySE.GetComponent<SEDirector>().PlayEnemyDamageSE();

        }

    }

}
