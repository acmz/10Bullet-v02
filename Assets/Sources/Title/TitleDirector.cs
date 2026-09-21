using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleDirector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //スペースキー押下でゲームスタート
        if (Input.GetKeyDown(KeyCode.Space)) {
            SceneManager.LoadScene("GameScene");
        }

    }
}
