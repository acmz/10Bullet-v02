using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敵タイプT4（サンプル：Y軸を下から上へ進み、ランダムな角度で弾を撃つ敵）の生成役
//EnemyT1Generator / EnemyT2Generator / EnemyT3Generator と同じ形式に合わせている
public class EnemyT4Generator : MonoBehaviour {

    //Unityエディタ上でEnemy_T4_Prefab（EnemyT4Controllerをアタッチしたプレハブ）を
    //アサインするための公開フィールド
    public GameObject enemyT4Prefab;

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    //T4敵を指定座標に出現させる（EnemyDirectorから呼び出される想定）
    public void GenerateEnemy(float inXPos, float inYPos) {

        //引数で受け取ったX・Y座標から、出現位置のベクトルを作成する
        //Y軸下から上へ進む敵のため、呼び出し側では inYPos に画面下側（マイナス側）の座標を渡す想定
        Vector2 enemyVector2 = new Vector2(inXPos, inYPos);

        //T4敵のプレハブを、指定座標・回転なしで生成する
        Instantiate(enemyT4Prefab, enemyVector2, Quaternion.identity);

    }

}
