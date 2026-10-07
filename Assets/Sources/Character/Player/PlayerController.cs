using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    //自機のオブジェクト
    Rigidbody2D playerBody;

    //自機の移動スピード
    private const float PLAYER_MOVE_SPEED = 5.0f;

    //自機の移動可能範囲
    private const float WINDOW_LIMIT_LEFT = -8.6f;
    private const float WINDOW_LIMIT_RIGHT = 8.3f;
    private const float WINDOW_LIMIT_TOP = 4.6f;
    private const float WINDOW_LIMIT_BOTTOM = -3.6f;

    //自機の死亡時のエフェクト
    public GameObject destroyEffect;

    //自機の死亡時のSE
    private GameObject destroySE;

    // Use this for initialization
    void Start() {

        playerBody = GetComponent<Rigidbody2D>();

        //死亡時のSEオブジェクト取得
        this.destroySE = GameObject.Find("SEDirector");

	}

    // Update is called once per frame
    void Update() {

        //自機の移動
        int keyLfRi = 0;
        int keyUpDw = 0;

        //キーボードの状態取得
        Keyboard keyboard = Keyboard.current;

        //キーボードが接続されていない場合、何もしない
        if(keyboard == null) {
            return;
        }

        //左右移動
        if((keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
            && this.transform.position.x > WINDOW_LIMIT_LEFT) {
            keyLfRi = -1;
        }

        if((keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
            && this.transform.position.x < WINDOW_LIMIT_RIGHT) {
            keyLfRi = 1;
        }

        //上下移動
        if((keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed)
            && this.transform.position.y < WINDOW_LIMIT_TOP) {
            keyUpDw = 1;
        }

        if((keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed)
            && this.transform.position.y > WINDOW_LIMIT_BOTTOM) {
            keyUpDw = -1;
        }

        //自機の移動制御
        Vector2 playerVector2;
        playerVector2.x = keyLfRi * PLAYER_MOVE_SPEED;
        playerVector2.y = keyUpDw * PLAYER_MOVE_SPEED;

        this.playerBody.linearVelocity = playerVector2;

    }

    //敵弾に当たったら、自分自身を消す
    private void OnTriggerEnter2D(Collider2D collision) {

        //敵弾に当たったら消滅
        if(collision.gameObject.name == "E_Bullet_Prefab(Clone)") {

            //自分自身を消す
            Destroy(this.gameObject);

            //死亡時のエフェクトを表示
            _ = Instantiate(this.destroyEffect,
                this.transform.position,
                Quaternion.identity);

            //死亡時のSEを鳴らす
            this.destroySE.GetComponent<SEDirector>().PlayPlayerDamageSE();

        }

    }


}
