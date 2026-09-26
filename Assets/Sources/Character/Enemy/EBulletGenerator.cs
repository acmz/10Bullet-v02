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
    private Color E_BULLET_FAN_COLOR = new Color(1.0f, 0.7f, 0.4f, 1.0f);

    //レベル上昇のボーダーライン
    public enum EnemyLevel:int {
        lv1 = 1
        , lv2 = 5
        , lv3 = 7
    }

    //敵弾の種類
    public enum EBulletType {
        straight, 
        homing, 
        fan,
        random
    }

    //扇状弾の開き角度（度）。画面左方向（自機側）を中心に左右均等へ広げる
    private float SPREAD_ANGLE = 45.0f;

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

            case EBulletType.fan:
                //扇弾
                this.EBulletFan(eBulletVector2, inEnemyLevel);
                break;

            case EBulletType.random:
                //ランダム弾
                this.EBulletFan(eBulletVector2, inEnemyLevel);
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

    //扇状弾を生成する
    public void EBulletFan(Vector2 inBulletPos, int inEnemyLevel) {

        //扇の中心となる基準角度（180度＝Unityの角度系でX軸負方向＝画面左＝自機側）
        const float baseAngle = 180.0f;

        //敵レベルを基に、扇状弾の球数を決める
        int enemyLevelBorder = (int)EnemyLevel.lv2;
        int bulletCount = 3;
        if(inEnemyLevel >= enemyLevelBorder) {
            //敵レベルが上がっていたら球数を増やす
            bulletCount = 5;
        } 

        //弾と弾の間の角度間隔を計算する（弾が1発のときは0除算を避けるため0にする）
        float angleStep = (bulletCount > 1) ? SPREAD_ANGLE / (bulletCount - 1) : 0f;

        //扇の一番端（開始角度）を、基準角度から開き角度の半分を引いて求める
        float startAngle = baseAngle - (SPREAD_ANGLE / 2.0f);

        //指定された弾数の分だけループし、1発ずつ生成する
        for(int i = 0;i < bulletCount;i++) {

            //Debug.Log("T3 Shoot");

            //この弾の角度を「開始角度＋（角度間隔×インデックス）」で求める
            float angle = startAngle + (angleStep * i);

            //度数法の角度をラジアンに変換する（Mathf.Cos/Sinはラジアンを使うため）
            float radian = angle * Mathf.Deg2Rad;

            //角度から発射方向ベクトル（X, Y）を計算する
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));

            //扇状弾のプレハブを、敵の座標・回転なしで生成する
            GameObject eFanBullet = Instantiate(eBulletPrefab, inBulletPos, Quaternion.identity) as GameObject;
            eFanBullet.GetComponent<Renderer>().material.color = E_BULLET_FAN_COLOR;
            eFanBullet.GetComponent<EBulletController>().EBulletShoot(direction);

        }

    }

    //ランダム弾を生成する
    public void EBulletRandom(Vector2 inBulletPos, int inEnemyLevel) {

        //発射位置は敵の現在位置とする
        Vector2 shootPos = this.gameObject.transform.position;

        //0～360度の範囲でランダムな角度（度数法）を決定する
        float randomAngleDeg = UnityEngine.Random.Range(0f, 360f);

        //度数法の角度をラジアンに変換する（Mathf.Cos/Sinはラジアンを使うため）
        float randomAngleRad = randomAngleDeg * Mathf.Deg2Rad;

        //ランダムな角度から発射方向ベクトル（X, Y）を求める
        Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngleRad), Mathf.Sin(randomAngleRad));

        //敵弾のプレハブを、発射位置・回転なしで生成する
        GameObject eBullet = Instantiate(this.eBulletPrefab, shootPos, Quaternion.identity);

        //EBulletController.cs に既にある「任意方向へ発射する」メソッド（扇状弾用に用意されたもの）を
        //そのまま再利用し、ランダムな方向へ弾を撃ち出す。EBulletController.cs 自体は変更していない。
        EBulletController eBulletController = eBullet.GetComponent<EBulletController>();
        if(eBulletController != null) {
            eBulletController.EBulletShoot(randomDirection);
        }

    }

}
