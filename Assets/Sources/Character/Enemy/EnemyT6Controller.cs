using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EBulletGenerator;

//敵タイプT6（サンプル）：出現した瞬間の自機の座標を記憶し、そこへ向けて直進する敵。
//移動中に自機が位置を変えても、狙う座標は出現時点のままで、追尾はしない
//（自機を狙い撃つのではなく、「自機が居た場所」を通過するイメージ）。
//
//スプライト（三角形）は、回転していない状態で頂点がY軸プラス方向（真上）を向いている前提。
//（Enemy_T6_Prefab の PolygonCollider2D の頂点座標から、そのように配置されていることを確認済み）
//このスプライトをZ軸で回転させ、頂点が進行方向＝出現時点の自機の座標の方を向くようにする。
//
//※このスクリプトはサンプルであり、EnemyDirector.cs や EBulletGenerator.cs、
//　GameScene.unity など既存ファイルへの組み込みはまだ行っていない。
public class EnemyT6Controller : MonoBehaviour {

    //撃破時に加算する点数
    private const int ENEMY_SCORE = 150;

    //敵の移動速度
    private const float ENEMY_MOVE_SPEED = 4.0f;

    //自機の座標が取得できなかった場合（既に消えている場合等）に使う、既定の進行方向
    //（画面左＝自機がいるはずの方向へ直進する）
    private static readonly Vector2 DEFAULT_DIRECTION = Vector2.left;

    //スプライトの頂点はY軸プラス方向（Atan2の角度でいう90度の位置）を向いているため、
    //進行方向の角度からこの分だけ引いて補正する
    private const float SPRITE_ANGLE_OFFSET = 90.0f;

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
    //発射する敵弾のプレハブ（Unityエディタ上で、既存の E_Bullet_Prefab をアサインする想定）
    //EBulletGenerator.cs を変更せずに済むよう、このスクリプト単体で弾を生成できるようにしている
    //public GameObject eBulletPrefab;

    //出現時に決定した進行方向。移動だけでなく、弾を発射する方向にもそのまま使う
    private Vector2 moveDirection;

    // Use this for initialization
    void Start () {

        //出現した瞬間の自機の座標を取得するため、Playerオブジェクトを検索する
        GameObject playerObj = GameObject.Find("Player");

        Vector2 direction;
        direction = DEFAULT_DIRECTION;
        if(playerObj != null) {

            //自機が見つからない場合（敵弾に当たって既に消えている等）は、左方向へ直進する
            //自機が存在する場合、出現時点の自機の座標と自分の座標の差分から進行方向を求める
            Vector2 targetPos = playerObj.transform.position;
            direction = targetPos - (Vector2)this.gameObject.transform.position;

        }

        //求めた方向へ、移動とスプライトの向きを設定する
        //this.SetMoveDirection(direction);
        this.moveDirection = direction;
        this.SetMoveDirection(this.moveDirection);

        //敵弾生成オブジェクト取得
        this.eBulletObj = GameObject.Find("E_Bullet_Generator");

    }

    //引数の方向へ移動速度を設定し、スプライトの向き（Z軸回転）も合わせて更新する
    private void SetMoveDirection(Vector2 inDirection) {

        //方向ベクトルの長さが0の場合（自機と全く同じ座標に出現した場合）は正規化できないため、既定方向を使う
        Vector2 direction = (inDirection.sqrMagnitude > 0f) ? inDirection.normalized : DEFAULT_DIRECTION;
        //Vector2 direction = DEFAULT_DIRECTION;
        //敵の移動方向を求める
        //Vector2 startPos = this.gameObject.transform.position;
        //Vector2 endPos = this.gameObject.transform.position;
        //Vector2 movePos;

        //endPos.x -= 1.0f;
        //movePos = endPos - startPos;

        //敵Rigidbody取得
        Rigidbody2D enemyBody = this.GetComponent<Rigidbody2D>();

        //進行方向（direction.normalized）とスピードを設定
        enemyBody.linearVelocity = direction * ENEMY_MOVE_SPEED;
        //enemyBody.linearVelocity = movePos.normalized * ENEMY_MOVE_SPEED;

        //進行方向へ力を加え、物理的に移動を発生させる
        enemyBody.AddForce(direction);
        //enemyBody.AddForce(movePos.normalized);

        //進行方向ベクトルから角度を求める（Atan2はX軸プラス方向を0度とするラジアン角を返す）
        float angleRad = Mathf.Atan2(direction.y, direction.x);

        //ラジアンを度数法に変換し、スプライトの向き（頂点が90度＝真上）とのずれを補正する
        float angleDeg = (angleRad * Mathf.Rad2Deg) - SPRITE_ANGLE_OFFSET;

        //Z軸周りの回転として、スプライトの頂点が進行方向を向くように設定する
        this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        //弾の発射方向にも使うため、進行方向を記憶しておく
        //this.moveDirection = direction;

    }

    // Update is called once per frame
    void Update () {

        //弾発射間隔の経過時間を加算する
        this.shootTime += Time.deltaTime;
        //発射間隔（SHOOT_INTERVAL）を超えたら、敵の進行方向へ弾を発射する
        if(this.shootTime >= SHOOT_INTERVAL && !this.shooted) {

            this.ShootBulletToMoveDirection();

            //発射間隔カウンターをリセットする
            //this.shootTime = 0f;

            //敵弾発射済み
            this.shooted = true;

        }

        //求めた方向へ、移動とスプライトの向きを設定する
        //this.SetMoveDirection(this.moveDirection);

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
    private void ShootBulletToMoveDirection() {

        //弾を生成するプレハブが未設定の場合は何もしない（null参照エラーを避ける）
        //if(this.eBulletPrefab == null) {
        //    return;
        //}

        //発射位置は敵の現在位置とする
        Vector2 shootPos = this.gameObject.transform.position;

        //敵の位置を基に、敵弾を発射
        Vector2 enemyPos = this.gameObject.transform.position;
        this.eBulletObj.GetComponent<EBulletGenerator>().EBulletGenerate(
            enemyPos
            , ENEMY_LEVEL
            , EBulletGenerator.EBulletType.homing);

        //敵弾のプレハブを、発射位置・回転なしで生成する
        //GameObject eBullet = Instantiate(this.eBulletPrefab, shootPos, Quaternion.identity);

        //EBulletController.cs に既にある「任意方向へ発射する」メソッド（扇状弾用に用意されたもの）を
        //そのまま再利用し、敵の進行方向へ弾を撃ち出す。EBulletController.cs 自体は変更していない。
        //EBulletController eBulletController = eBullet.GetComponent<EBulletController>();
        //if(eBulletController != null) {
        //    eBulletController.EBulletShoot(this.moveDirection);
        //}

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
