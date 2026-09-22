using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PBulletGenerator : MonoBehaviour {

    public GameObject pBulletPrefab;
    private const float P_BULLET_POS_SET = 1.1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //キーボードの状態取得
        Keyboard keyboard = Keyboard.current;

        //キーボードが接続されていない場合、何もしない
        if(keyboard == null) {
            return;
        }

        //スペースキーで弾発射
        if(keyboard.spaceKey.wasPressedThisFrame) {

            //GameDirectorと連携
            GameObject gameDirectorObj = GameObject.Find("GameDirector");

            //Wave開始処理中は弾発射不可
            if(gameDirectorObj.GetComponent<GameDirector>().IsWaveInit) {
                return;
            }

            //残弾数を取得
            int pBulletStock = gameDirectorObj.GetComponent<GameDirector>().PBulletStock;

            //仕様変更 時間内に弾を打ち切れないので、連射可能にする
            ////弾が画面上に残っていたら発射不可
            //GameObject oldPBullet = GameObject.Find("P_Bullet_Prefab(Clone)");
            //if(oldPBullet != null) {
            //    return;
            //}

            if(pBulletStock > 0) {

                //弾が残ってるので、発射可能

                //自機の位置を基に、自弾の発射位置を設定
                Vector2 playerPos = GameObject.Find("Player").transform.position;
                Vector2 pBulletVector2 = new Vector2(playerPos.x + P_BULLET_POS_SET, playerPos.y);

                //自弾を生成し、発射
                GameObject pBullet = Instantiate(this.pBulletPrefab,
                                                 pBulletVector2,
                                                 Quaternion.identity);
                pBullet.GetComponent<PBulletController>().PBulletShoot(playerPos);

                //弾を発射したことをDirectorに伝え、撃破数を初期化する
                //仕様変更 連射可能にしたので、撃破数初期化は画面から全ての弾が消えたタイミングで行う
                gameDirectorObj.GetComponent<GameDirector>().PBulletNumMinus();
                //gameDirectorObj.GetComponent<GameDirector>().EnemyDestroyNumReset();

            }

        }
	}
}
