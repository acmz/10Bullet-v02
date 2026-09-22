using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyT1Controller : MonoBehaviour {

    //点数
    private const int ENEMY_SCORE = 10;

    //敵弾発射の間隔
    private const float SHOOT_INTERVAL = 2.0f;
    private float shootTime;

    //敵のレベル設定
    private const int ENEMY_LEVEL = 5;

    //生成から移動開始するまでの間隔
    private const float MOVE_INTERVAL = 0.0f;
    private float moveStartTime;

    //敵の移動速度
    private const float ENEMY_MOVE_SPEED = 3.0f;

    //敵の移動方向
    private const float ENEMY_MOVE_ANGLE_X = 1.0f;
    //private const float ENEMY_MOVE_ANGLE_Y = 0.5f;

    //敵移動済み判定フラグ
    private Boolean enemyMoveEnabled = true;

    //敵表示限界
    private const float ENEMY_DESTROY_POS_LEFT = -10.0f;
    private const float ENEMY_DESTROY_POS_RIGHT = 10.0f;
    private const float ENEMY_DESTROY_POS_UP = 6.0f;
    private const float ENEMY_DESTROY_POS_DOWN = -6.0f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //敵弾発射
        this.shootTime += Time.deltaTime;
        if(this.shootTime >= SHOOT_INTERVAL) {

            //敵の位置を基に、敵弾を発射
            Vector2 enemyPos = this.gameObject.transform.position;
            GameObject eBulletObj = GameObject.Find("E_Bullet_Generator");
            eBulletObj.GetComponent<EBulletGenerator>().EBulletGenerate(
                enemyPos
                ,ENEMY_LEVEL
                ,EBulletGenerator.EBulletType.straight);

            //発射間隔をリセット
            this.shootTime = 0f;

        }

        //敵移動
        this.moveStartTime += Time.deltaTime;
        if(this.enemyMoveEnabled && this.moveStartTime >= MOVE_INTERVAL) {

            //Debug.Log("collision = " + this.gameObject.name);

            //敵を移動させる
            //敵の移動方向を求める
            Vector2 startPos = this.gameObject.transform.position;
            Vector2 endPos = this.gameObject.transform.position;
            Vector2 movePos;

            endPos.x -= ENEMY_MOVE_ANGLE_X;
            movePos = endPos - startPos;

            //敵Rigidbody取得
            Rigidbody2D enemyBody;
            enemyBody = this.GetComponent<Rigidbody2D>();

            //敵の移動方向（敵の移動方向.normalized）とスピードを設定
            enemyBody.linearVelocity = movePos.normalized * ENEMY_MOVE_SPEED;

            //敵に力を加え、移動
            enemyBody.AddForce(movePos.normalized);

            //移動判定フラグをoffにする
            this.enemyMoveEnabled = false;

            //移動開始時間を初期化
            this.moveStartTime = 0f;

        }

        //画面外に出たら自分自身を破棄する
        if(this.gameObject.transform.position.x < ENEMY_DESTROY_POS_LEFT
            || this.gameObject.transform.position.x > ENEMY_DESTROY_POS_RIGHT
            || this.gameObject.transform.position.y > ENEMY_DESTROY_POS_UP
            || this.gameObject.transform.position.y < ENEMY_DESTROY_POS_DOWN) {

            Destroy(this.gameObject);

        }
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

        }

    }

}
