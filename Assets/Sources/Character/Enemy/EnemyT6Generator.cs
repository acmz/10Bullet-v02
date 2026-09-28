using UnityEngine;

//敵タイプT6（サンプル：出現時点の自機の座標へ直進し、その方向を向く敵）の生成役
//EnemyT1Generator～EnemyT5Generator と同じ形式に合わせている
public class EnemyT6Generator : MonoBehaviour {

    //Unityエディタ上でEnemy_T6_Prefab（EnemyT6Controllerをアタッチしたプレハブ）を
    //アサインするための公開フィールド
    public GameObject enemyT6Prefab;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    //T6敵を指定座標に出現させる（EnemyDirectorから呼び出される想定）
    public void GenerateEnemy(float inXPos, float inYPos) {

        //引数で受け取ったX・Y座標から、出現位置のベクトルを作成する
        Vector2 enemyVector2 = new(inXPos, inYPos);


        //T6敵のプレハブを、指定座標・回転なしで生成する
        //（生成直後、EnemyT6Controller.Start() が自機の座標を取得し、向きと進行方向を上書き設定する）
        _ = Instantiate(this.enemyT6Prefab, enemyVector2, Quaternion.identity);

    }

}
