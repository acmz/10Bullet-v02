using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EBulletGenerator;

//敵タイプT4（サンプル）：画面下側（Y座標がマイナス）から出現し、
//Y軸のマイナス方向からプラス方向（画面下から上）へ直進しながら、
//ランダムな角度で弾を発射し続ける敵。
//
//T1～T3はX軸方向（画面右から左、自機側）へ移動する敵だったため、
//移動の軸をY軸に変えたバリエーションのサンプルとして作成した。
//
//※このスクリプトはサンプルであり、EnemyDirector.cs や EBulletGenerator.cs、
//　GameScene.unity など既存ファイルへの組み込みはまだ行っていない。
//　実際に登場させる場合は、以下の対応が別途必要になる。
//　　・このスクリプトをアタッチした敵プレハブ（Enemy_T4_Prefab 等）の作成
//　　・EnemyT4Generator を保持するオブジェクトをシーンに配置し、
//　　　enemyT4Prefab に上記プレハブを、eBulletPrefab に
//　　　既存の E_Bullet_Prefab をそれぞれアサインする
//　　・EnemyDirector.cs の敵種類（EnemyType）や出現パターンCSVへの追加
public class EnemyT4Controller : MonoBehaviour {

    //撃破時に加算する点数
    private const int ENEMY_SCORE = 200;

    //敵弾発射の間隔（秒）
    private const float SHOOT_INTERVAL = 1.5f;
    private float shootTime;

    //敵弾の移動スピード
    private const float E_BULLET_MOVE_SPEED = 5.0f;

    //敵のレベル設定
    private const int ENEMY_LEVEL = 6;

    //敵の移動速度
    private const float ENEMY_MOVE_SPEED = 2.5f;

    //進入方向ベクトルを求めるためのY方向の変化量
    //（プラス方向を指定することで、Y軸マイナス側からプラス側＝画面下から上へ進む）
    //private const float ENEMY_MOVE_ANGLE_X = 1.0f;
    private const float ENEMY_MOVE_ANGLE_Y = 1.0f;

    //生成から移動を開始するまでの間隔（秒）
    private const float MOVE_INTERVAL = 0.0f;
    private float moveStartTime;

    //移動を開始済みかどうかのフラグ（trueになったら再度速度設定処理を行わない）
    private bool enemyMoveEnabled = true;

    //画面外判定（この範囲を超えたら自分自身を破棄する。T1・T3と同じ範囲を流用）
    private const float ENEMY_DESTROY_POS_LEFT = -10.0f;
    private const float ENEMY_DESTROY_POS_RIGHT = 30.0f;
    private const float ENEMY_DESTROY_POS_UP = 15.0f;
    private const float ENEMY_DESTROY_POS_DOWN = -15.0f;

    //発射する敵弾のプレハブ（Unityエディタ上で、既存の E_Bullet_Prefab をアサインする想定）
    //EBulletGenerator.cs を変更せずに済むよう、このスクリプト単体で弾を生成できるようにしている
    //public GameObject eBulletPrefab;

    //敵弾生成オブジェクト
    GameObject eBulletObj;

    // Use this for initialization
    void Start () {

        this.eBulletObj = GameObject.Find("E_Bullet_Generator");
    }

    // Update is called once per frame
    void Update () {

        //移動・弾発射で使うため、自分自身のRigidbody2Dコンポーネントを取得する
        Rigidbody2D enemyBody = this.GetComponent<Rigidbody2D>();

        //敵の移動（Y軸マイナス側からプラス側へ、画面下から上へ直進する）
        this.moveStartTime += Time.deltaTime;
        if(this.enemyMoveEnabled && this.moveStartTime >= MOVE_INTERVAL) {

            //現在位置を移動開始地点として取得する
            Vector2 startPos = this.gameObject.transform.position;
            Vector2 endPos = this.gameObject.transform.position;
            Vector2 movePos;

            //移動先のY座標をプラス方向へずらし、Y軸プラス方向への移動ベクトルを作る
            endPos.y += ENEMY_MOVE_ANGLE_Y;
            movePos = endPos - startPos;

            //移動方向ベクトルを正規化し、移動速度を掛けてRigidbody2Dの速度に設定する
            enemyBody.linearVelocity = movePos.normalized * ENEMY_MOVE_SPEED;

            //移動方向へ力を加え、物理的に移動を発生させる
            enemyBody.AddForce(movePos.normalized);

            //一度速度を設定したら、以後は再設定しない（Y軸プラス方向へ進み続ける）
            this.enemyMoveEnabled = false;

            //移動開始までの経過時間カウンターをリセットする
            this.moveStartTime = 0f;

        }

        //弾発射間隔の経過時間を加算する
        this.shootTime += Time.deltaTime;
        //発射間隔（SHOOT_INTERVAL）を超えたら、ランダムな角度で弾を発射する
        if(this.shootTime >= SHOOT_INTERVAL) {

            this.ShootRandomAngleBullet();

            //発射間隔カウンターをリセットする
            this.shootTime = 0f;

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

    //ランダムな角度で弾を1発発射する
    private void ShootRandomAngleBullet() {

        //敵の位置を基に、敵弾を発射
        Vector2 enemyPos = this.gameObject.transform.position;
        this.eBulletObj.GetComponent<EBulletGenerator>().EBulletGenerate(
            enemyPos
            , ENEMY_LEVEL
            , EBulletGenerator.EBulletType.random);

        //発射間隔をリセット
        this.shootTime = 0f;

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
