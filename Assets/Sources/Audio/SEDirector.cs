using UnityEngine;

//ゲーム内のSE（効果音）をまとめて再生する管理役。
//Instantiateで専用オブジェクトを都度生成する方式をやめ、
//あらかじめシーンに置いた自分自身のAudioSourceのPlayOneShotで鳴らす。
//PlayOneShotは同じAudioSource上で複数の音を重ねて再生できるため、
//敵弾がいくつも同時にヒットしても音が途切れない。
[RequireComponent(typeof(AudioSource))]
public class SEDirector : MonoBehaviour
{

    //各SEのAudioClip（Unityエディタ上でアサインする）
    public AudioClip shotClip;
    public AudioClip enemyDamageClip;
    public AudioClip playerDamageClip;

    private AudioSource seAudioSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //自分自身のAudioSourceを取得しておく
        this.seAudioSource = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //弾の発射音を鳴らす
    public void PlayShotSE() {
        this.seAudioSource.PlayOneShot(this.shotClip);
    }

    //敵の被弾音を鳴らす
    public void PlayEnemyDamageSE() {
        this.seAudioSource.PlayOneShot(this.enemyDamageClip);
    }

    //自機の被弾音を鳴らす
    public void PlayPlayerDamageSE() {
        this.seAudioSource.PlayOneShot(this.playerDamageClip);
    }

}
