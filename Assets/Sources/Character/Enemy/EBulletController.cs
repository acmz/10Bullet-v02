using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EBulletController : MonoBehaviour
{

    //敵弾のオブジェクト
    Rigidbody2D eBulletBody;

    //敵弾の移動スピード
    private const float E_BULLET_MOVE_SPEED = 5.0f;
    private const float E_BULLET_MOVE_SPEED_PLUS = 5.0f;

    //敵弾の発射角度
    private const float E_BULLET_MOVE_ANGLE_X = 1.0f;
    private const float E_BULLET_MOVE_ANGLE_Y = 0.5f;

    //敵弾表示限界
    private const float E_BULLET_DESTROY_POS_LEFT = -10.0f;
    private const float E_BULLET_DESTROY_POS_RIGHT = 10.0f;
    private const float E_BULLET_DESTROY_POS_UP = 6.0f;
    private const float E_BULLET_DESTROY_POS_DOWN = -6.0f;

    //敵弾の発射方向
    public enum EBulletDirection {
        left, up, down
    }

    //敵弾のスピード
    public enum EBulletSpeed {
        low, hight
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //画面外に出たら自分自身を破棄する
        if (this.gameObject.transform.position.x < E_BULLET_DESTROY_POS_LEFT
            || this.gameObject.transform.position.x > E_BULLET_DESTROY_POS_RIGHT
            || this.gameObject.transform.position.y > E_BULLET_DESTROY_POS_UP
            || this.gameObject.transform.position.y < E_BULLET_DESTROY_POS_DOWN) {

            Destroy(this.gameObject);

        }

    }

    //敵弾の発射（直進）
    public void EBulletShoot(Vector2 inBulletPos, EBulletDirection inDirection) {

        //敵弾の発射方向を求める
        Vector2 startPos = inBulletPos;
        Vector2 endPos = inBulletPos;
        Vector2 shootPos;

        endPos.x -= E_BULLET_MOVE_ANGLE_X;
        switch (inDirection) {

            case EBulletDirection.left:
                //正面
                //角度調整不要
                break;

            case EBulletDirection.up:
                //斜め上
                endPos.y += E_BULLET_MOVE_ANGLE_Y; 
                break;

            case EBulletDirection.down:
                //斜め下
                endPos.y -= E_BULLET_MOVE_ANGLE_Y;
                break;

        }
        shootPos = endPos - startPos;

        //敵弾Rigidbody取得
        this.eBulletBody = this.GetComponent<Rigidbody2D>();

        //敵弾の発射角度（敵弾の発射方向.normalized）とスピードを設定
        this.eBulletBody.linearVelocity = shootPos.normalized * E_BULLET_MOVE_SPEED;

        //敵弾に力を加え、発射
        this.eBulletBody.AddForce(shootPos.normalized);

    }

    //敵弾の発射（ホーミング）
    public void EBulletShoot(Vector2 inEBulletPos, Vector2 inPlayerPos, EBulletSpeed inEBulletSpeed) {

        //自機と敵弾の座標から、敵弾の発射方向を求める
        Vector2 shootPos = inPlayerPos - inEBulletPos;

        //敵弾Rigidbody取得
        this.eBulletBody = this.GetComponent<Rigidbody2D>();

        //敵弾の発射角度（敵弾の発射方向.normalized）とスピードを設定
        float eBulletSpeedNum = E_BULLET_MOVE_SPEED;

        switch (inEBulletSpeed) {

            case EBulletSpeed.low:
                //弾速変えない
                break;

            case EBulletSpeed.hight:
                //弾速UP
                eBulletSpeedNum += E_BULLET_MOVE_SPEED_PLUS;
                break;

        }
        this.eBulletBody.linearVelocity = shootPos.normalized * eBulletSpeedNum;

        //敵弾に力を加え、発射
        this.eBulletBody.AddForce(shootPos.normalized);

    }


    //扇状弾の発射
    public void EBulletShoot(Vector2 inDirection) {

        //敵弾Rigidbody取得
        this.eBulletBody = this.GetComponent<Rigidbody2D>();

        //発射方向ベクトルを正規化（長さ1に）し、速度を掛けてRigidbody2Dの速度として設定する
        this.eBulletBody.linearVelocity = inDirection.normalized * E_BULLET_MOVE_SPEED;

        //発射方向へ力を加え、物理的に弾を押し出す
        this.eBulletBody.AddForce(inDirection.normalized);

    }
}
