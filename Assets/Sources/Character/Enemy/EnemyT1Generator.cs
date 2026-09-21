using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyT1Generator : MonoBehaviour {

    public GameObject enemyT1Prefab;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void GenerateEnemy(float inXPos, float inYPos) {

        //敵の出現位置を設定
        Vector2 enemyVector2 = new Vector2(inXPos, inYPos);

        //敵を生成
        Instantiate(enemyT1Prefab, enemyVector2, Quaternion.identity);

    }

}
