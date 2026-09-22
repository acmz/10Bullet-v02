using UnityEngine;

public class GameController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //画面上から自弾がすべて消えたら、撃破数を初期化する
        if(GameObject.Find("P_Bullet_Prefab(Clone)") == null) {

            //Debug.Log("enemy destroy num reset");
            GameObject gameDirectorObj = GameObject.Find("GameDirector");
            gameDirectorObj.GetComponent<GameDirector>().EnemyDestroyNumReset();

        }

    }
}
