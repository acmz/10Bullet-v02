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

    // Use this for initialization
    void Start() {

        this.eBulletObj = GameObject.Find("E_Bullet_Generator");

        //敵の進行方向を設定
        this.moveDirection = MOVE_DIRECTION;

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

        }

    }

}

