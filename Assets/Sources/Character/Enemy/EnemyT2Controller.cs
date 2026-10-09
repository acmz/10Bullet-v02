using UnityEngine;

public class EnemyT2Controller : MonoBehaviour {

    //点数
    private const int ENEMY_SCORE = 1000;

    //敵弾発射の間隔
    private const float SHOOT_INTERVAL = 2.5f;
    private float shootTime;

    //敵のレベル設定
    private const int ENEMY_LEVEL = 6;

    //生成から移動開始するまでの間隔
    private const float MOVE_INTERVAL = 0.0f;
    private float moveStartTime;

    //敵の移動速度
    private const float ENEMY_MOVE_SPEED = 1.0f;

    //敵の移動方向
    private static readonly Vector2 MOVE_DIRECTION = Vector2.left;

    //スプライトの頂点はY軸プラス方向（Atan2の角度でいう90度の位置）を向いているため、
    //進行方向の角度からこの分だけ引いて補正する
    private const float SPRITE_ANGLE_OFFSET = 90.0f;

    //敵移動済み判定フラグ
    private bool enemyMoveEnabled = false;

    //敵を生成してから移動停止するまでの時間
    private const float MOVE_TIME = 2.0f;
    private float moveTime;

    //敵表示限界
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

    //敵の死亡時のSE
    private GameObject destroySE;

    // Use this for initialization
    void Start() {

        this.eBulletObj = GameObject.Find("E_Bullet_Generator");

        //敵死亡時のSEオブジェクト取得
        this.destroySE = GameObject.Find("SEDirector");

        //敵の進行方向を設定
        this.moveDirection = MOVE_DIRECTION;

        //進行方向へスプライトの向きを設定する
        this.SetSpriteDirection(this.moveDirection);

    }

    //引数の方向へスプライトの向き（Z軸回転）を更新する
    private void SetSpriteDirection(Vector2 inDirection) {

        //方向ベクトルの長さが0の場合（自機と全く同じ座標に出現した場合）は正規化できないため、既定方向を使う
        Vector2 direction = (inDirection.sqrMagnitude > 0f) ? inDirection.normalized : MOVE_DIRECTION;

        //進行方向ベクトルから角度を求める（Atan2はX軸プラス方向を0度とするラジアン角を返す）
        float angleRad = Mathf.Atan2(direction.y, direction.x);

        //ラジアンを度数法に変換し、スプライトの向き（頂点が90度＝真上）とのずれを補正する
        float angleDeg = (angleRad * Mathf.Rad2Deg) - SPRITE_ANGLE_OFFSET;

        //Z軸周りの回転として、スプライトの頂点が進行方向を向くように設定する
        this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

    }

    // Update is called once per frame
    void Update() {

        //敵弾発射
        this.shootTime += Time.deltaTime;
        if(this.shootTime >= SHOOT_INTERVAL) {

            //敵の位置を基に、敵弾を発射
            this.ShootBullet();

            //発射間隔をリセット
            this.shootTime = 0f;

        }

        //敵移動
        this.moveStartTime += Time.deltaTime;
        if(!this.enemyMoveEnabled && this.moveStartTime >= MOVE_INTERVAL) {

            //Debug.Log("collision = " + this.gameObject.name);

            //敵を移動させる
            this.EnemyMove(this.moveDirection);

            //移動判定フラグをoffにする
            this.enemyMoveEnabled = true;

            //移動開始時間を初期化
            this.moveStartTime = 0f;

        }

        //敵移動停止
        this.moveTime += Time.deltaTime;
        if(this.moveTime >= MOVE_TIME
            && this.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 0f) {

            //Debug.Log("enemy stoped");

            //移動を停止させる
            this.EnemyStop();

        }

        //画面外に出たら自分自身を破棄する
        if(this.gameObject.transform.position.x < ENEMY_DESTROY_POS_LEFT
            || this.gameObject.transform.position.x > ENEMY_DESTROY_POS_RIGHT
            || this.gameObject.transform.position.y > ENEMY_DESTROY_POS_UP
            || this.gameObject.transform.position.y < ENEMY_DESTROY_POS_DOWN) {

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
            , EBulletGenerator.EBulletType.homing);

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

    //移動停止
    private void EnemyStop() {

        //移動を停止させる
        //敵Rigidbody取得
        Rigidbody2D enemyBody = this.GetComponent<Rigidbody2D>();

        //敵の移動停止
        enemyBody.linearVelocity = Vector2.zero;

    }

    //自弾に当たったら、自分自身を消す
    private void OnTriggerEnter2D(Collider2D collision) {

        //自弾に当たったら消滅
        if(collision.gameObject.name == "P_Bullet_Prefab(Clone)") {

            //Directorと連携
            GameObject gameDirectorObj = GameObject.Find("GameDirector");

            //倒されたこと、加算する点数をDirectorに伝える
            gameDirectorObj.GetComponent<GameDirector>().EnemyDestroyNumPlus();
            gameDirectorObj.GetComponent<GameDirector>().ScorePlus(ENEMY_SCORE);

            //自分自身を消す
            Destroy(gameObject);

            //死亡時のエフェクトを表示
            _ = Instantiate(this.destroyEffect,
                this.transform.position,
                Quaternion.identity);

            //死亡時のSEを再生
            this.destroySE.GetComponent<SEDirector>().PlayEnemyDamageSE();

        }

    }

}

