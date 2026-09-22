using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

        //キーボードの状態取得
        Keyboard keyboard = Keyboard.current;

        //キーボードが接続されていない場合、何もしない
        if(keyboard == null) {
            return;
        }

        //スペースキー押下でゲームスタート
        if(keyboard.spaceKey.wasPressedThisFrame) {
            SceneManager.LoadScene("GameScene");
        }

    }
}
