using UnityEngine;

//敵タイプT5：挟撃する敵。
//x軸のマイナスからプラスへ、左から右へ移動する。
public class EnemyT5Controller : MonoBehaviour {

    //点数
    private const int ENEMY_SCORE = 10;

    //敵弾発射の間隔
    private const float SHOOT_INTERVAL = 0.5f;
    private float shootTime;

    //弾発射済み判定フラグ
    private bool shooted = false;

    //敵のレベル設定
    private const int ENEMY_LEVEL = 1;

    //生成から移動開始するまでの間隔
    private const float MOVE_INTERVAL = 0.0f;
    private float moveStartTime;

    //敵の移動速度
    private const float ENEMY_MOVE_SPEED = 3.0f;

    //敵の移動方向
    private static readonly Vector2 MOVE_DIRECTION = Vector2.right;

    //敵移動済み判定フラグ
    private bool enemyMoveEnabled = true;

    //敵表示限界
    private const float ENEMY_DESTROY_POS_LEFT = -30.0f;
    private const float ENEMY_DESTROY_POS_RIGHT = 10.0f;
    private const float ENEMY_DESTROY_POS_UP = 6.0f;
    private const float ENEMY_DESTROY_POS_DOWN = -6.0f;

    //出現時に決定した進行方向。
    private Vector2 moveDirection;

    //敵の死亡時のエフェクト
    public GameObject destroyEffect;

    //敵弾生成オブジェクト
    private GameObject eBulletObj;

    //敵の死亡時のSE
    private GameObject destroySE;

    // Use this for initialization
    void Start() {

        //敵弾生成オブジェクト取得
        this.eBulletObj = GameObject.Find("E_Bullet_Generator");

        //敵死亡時のSEオブジェクト取得
        this.destroySE = GameObject.Find("SEDirector");

        //敵の進行方向を設定
        this.moveDirection = MOVE_DIRECTION;

    }

    // Update is called once per frame
    void Update() {

        //敵弾発射
        this.shootTime += Time.deltaTime;
        if(this.shootTime >= SHOOT_INTERVAL && !this.shooted) {

            //敵の位置を基に、敵弾を発射
            this.ShootBullet();
            //弾発射済み
            this.shooted = true;

        }

        //敵移動
        this.moveStartTime += Time.deltaTime;
        if(this.enemyMoveEnabled && this.moveStartTime >= MOVE_INTERVAL) {

            //Debug.Log("collision = " + this.gameObject.name);

            //敵を移動させる
            this.EnemyMove(this.moveDirection);

            //移動判定フラグをoffにする
            this.enemyMoveEnabled = false;

        }

        //画面外に出たら自分自身を破棄する
        if(this.gameObject.transform.position.x < ENEMY_DESTROY_POS_LEFT
            || this.gameObject.transform.position.x > ENEMY_DESTROY_POS_RIGHT
            || this.gameObject.transform.position.y > ENEMY_DESTROY_POS_UP
            || this.gameObject.transform.position.y < ENEMY_DESTROY_POS_DOWN) {

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
            , EBulletGenerator.EBulletType.straightOpposite);

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

    //自弾に当たったら、点数を加算して自分自身を消す
    private void OnTriggerEnter2D(Collider2D collision) {

        //Debug.Log("collision = " + collision.gameObject.name);

        //自弾に当たったら消滅
        if(collision.gameObject.name == "P_Bullet_Prefab(Clone)") {

            //Directorと連携
            GameObject gameDirectorObj = GameObject.Find("GameDirector");

            //倒されたこと、加算する点数をDirectorに伝える
            gameDirectorObj.GetComponent<GameDirector>().EnemyDestroyNumPlus();
            gameDirectorObj.GetComponent<GameDirector>().ScorePlus(ENEMY_SCORE);

            //自分自身を消す
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
