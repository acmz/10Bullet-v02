using UnityEngine;

public class EnemyT2Generator : MonoBehaviour {

    public GameObject enemyT2Prefab;

    // Use this for initialization
    void Start() {
		
	}

    // Update is called once per frame
    void Update() {

    }

    public void GenerateEnemy(float inXPos, float inYPos) {

        //敵の出現位置を設定
        Vector2 enemyVector2 = new(inXPos, inYPos);

        //敵を生成
        _ = Instantiate(this.enemyT2Prefab, enemyVector2, Quaternion.identity);
    
    }

}
