using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EBulletGenerator : MonoBehaviour
{

    public GameObject eBulletPrefab;
    private const float E_BULLET_POS_SET = 0f;

    //弾の色
    private Color E_BULLET_STRIGHT_COLOR = new Color(1.0f, 1.0f, 0.5f, 1.0f);
    private Color E_BULLET_HOMING_COLOR = new Color(1.0f, 0.5f, 0.5f, 1.0f);

    //レベル上昇のボーダーライン
    public enum EnemyLevel:int {
        lv1 = 1
        , lv2 = 5
        , lv3 = 7
    }

    //敵弾の種類
    public enum EBulletType {
        straight, homing
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    //敵弾生成
    public void EBulletGenerate(
        Vector2 inEnemyPos
        , int inEnemyLevel
        , EBulletType inEBulletType) {

        //敵の位置（inEnemyPos）を基に、敵弾の発射位置を設定
        Vector2 eBulletVector2 = new Vector2(inEnemyPos.x + E_BULLET_POS_SET, inEnemyPos.y);

        //敵弾を生成し、発射
        switch (inEBulletType) {

            case EBulletType.straight:
                //直進弾
                this.EBulletStraight(eBulletVector2, inEnemyLevel);
                break;

            case EBulletType.homing:
                //ホーミング弾
                this.EBulletHoming(eBulletVector2, inEnemyLevel);
                break;

        }

    }

    //敵弾（直進）
    private void EBulletStraight(Vector2 inBulletPos, int inEnemyLevel) {

        //正面
        GameObject eBullet1 = Instantiate(eBulletPrefab, inBulletPos, Quaternion.identity) as GameObject;
        eBullet1.GetComponent<Renderer>().material.color = E_BULLET_STRIGHT_COLOR;
        eBullet1.GetComponent<EBulletController>().EBulletShoot(
            inBulletPos,
            EBulletController.EBulletDirection.left);

        //レベル判定
        int enemyLevelBorder = (int)EnemyLevel.lv2;
        if(inEnemyLevel >= enemyLevelBorder) {

            //上下
            GameObject eBullet2 = Instantiate(eBulletPrefab, inBulletPos, Quaternion.identity) as GameObject;
            eBullet2.GetComponent<Renderer>().material.color = E_BULLET_STRIGHT_COLOR;
            eBullet2.GetComponent<EBulletController>().EBulletShoot(
                inBulletPos,
                EBulletController.EBulletDirection.up);

            GameObject eBullet3 = Instantiate(eBulletPrefab, inBulletPos, Quaternion.identity) as GameObject;
            eBullet3.GetComponent<Renderer>().material.color = E_BULLET_STRIGHT_COLOR;
            eBullet3.GetComponent<EBulletController>().EBulletShoot(
                inBulletPos,
                EBulletController.EBulletDirection.down);

        }

    }

    //敵弾（ホーミング）
    private void EBulletHoming(Vector2 inBulletPos, int inEnemyLevel) {

        //自機の位置を取得し、打ち出す方角を決定
        if (GameObject.Find("Player") == null) {
            return;
        }
        Vector2 playerPos = GameObject.Find("Player").transform.position;

        //レベル判定
        int enemyLevelBorder = (int)EnemyLevel.lv3;
        int eBulletSpeed = (int)EBulletController.EBulletSpeed.low;
        if (inEnemyLevel >= enemyLevelBorder) {

            //弾速アップ
            eBulletSpeed = (int)EBulletController.EBulletSpeed.hight;

        }

        GameObject eBullet1 = Instantiate(eBulletPrefab, inBulletPos, Quaternion.identity) as GameObject;
        eBullet1.GetComponent<Renderer>().material.color = E_BULLET_HOMING_COLOR;
        eBullet1.GetComponent<EBulletController>().EBulletShoot(
            inBulletPos,
            playerPos,
            (EBulletController.EBulletSpeed)eBulletSpeed);

    }

}
