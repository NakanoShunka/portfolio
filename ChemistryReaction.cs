using UnityEngine;
using UnityEngine.UI; // UI テキストを使用するために必要
using UnityEngine.SceneManagement; // 画面遷移のために必要
using System.Collections; // IEnumeratorを使うために必要

public class ChemistryReaction : MonoBehaviour
{
    public Transform leftHandAnchor; // 左手のアンカー
    public Transform rightHandAnchor; // 右手のアンカー
    public ParticleSystem reactionEffectHClNaOH; // HClとNaOHの反応エフェクト
    public ParticleSystem reactionEffectNH3HCl; // NH3とHClの反応エフェクト
    public GameObject generatedObjectPrefab; // 生成物のPrefab
    public int ReactionCount => reactionCount; // 外部から反応回数を参照できるようにする

    private bool firstReactionDone = false; // 最初の反応が済んだか
    private bool reactionInProgress = false; // 反応中かどうか
    private float reactionDuration = 5f; // 反応エフェクトの持続時間
    private float reactionCooldown = 2f; // 反応後の待機時間（クールダウン）
    private float nextReactionTime = 0f; // 次の反応が可能になる時間

    private int leftHandIons = 0; // 左手に吸収されたイオンの数
    private int rightHandIons = 0; // 右手に吸収されたイオンの数
    private const int maxIonsPerHand = 4; // 一つの手に吸収できる最大イオン数

    public AudioClip reactionSound; // 反応開始時から終了までの音声
    public AudioClip handIonSound; // 片手に2個以上のイオンがある場合の音声
    public AudioClip destroySound; // イオンが破壊された時の音声
    public AudioSource audioSource; // 音声を再生するためのAudioSource

    private OVRPlayerController playerController; // プレイヤーのコントローラ
    private bool isAudioPlaying = false; // 音声再生中かどうか

    public Text uiText; // UIテキストを変更するための Text コンポーネント
    private int reactionCount = 0; // 反応の回数
    public string afterUIText = "どちらかの手に水素イオン１個（水色）と塩素イオン１個（黄色）を両方乗せてみよう！"; // イオン破壊後に表示されるUIテキスト

    // インスペクターでシーン名を設定するための変数
    public string nextSceneName = "NextSceneName"; // シーン名（デフォルト値を設定）

    void Start()
    {
        // AudioSourceの設定がインスペクターでされていない場合に取得する
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>(); // AudioSourceがアタッチされていない場合に取得
        }

        playerController = FindObjectOfType<OVRPlayerController>();  // プレイヤーのコントローラを探す
        InvokeRepeating("CheckIonsAndHands", 0f, 1f); // イオンと手の状態を定期的にチェック
    }

    // 両手にイオンがあるかどうかを判定
    bool AreIonsOnBothHands()
    {
        IonController leftIonController = leftHandAnchor.GetComponentInChildren<IonController>();
        IonController rightIonController = rightHandAnchor.GetComponentInChildren<IonController>();

        bool leftHasIon = leftIonController != null && leftIonController.IsAbsorbed() && !leftIonController.IsDeleted();
        bool rightHasIon = rightIonController != null && rightIonController.IsAbsorbed() && !rightIonController.IsDeleted();

        return leftHasIon && rightHasIon;
    }

    // 両手が接触しているかどうかを判定
    bool AreHandsTouching()
    {
        if (leftHandAnchor == null || rightHandAnchor == null)
        {
            Debug.LogError("Either leftHandAnchor or rightHandAnchor is null!");
            return false;
        }

        float distance = Vector3.Distance(leftHandAnchor.position, rightHandAnchor.position);
        return distance < 0.5f;
    }

    // 反応を開始する
    void TriggerReaction()
    {
        if (reactionInProgress || Time.time < nextReactionTime)
        {
            Debug.Log("Reaction is either in progress or on cooldown. Skipping...");
            return;
        }

        reactionInProgress = true;
        Debug.Log("Reaction started.");
        reactionCount++;  // ここで反応回数をインクリメント
        Debug.Log("Reaction count incremented. Current count: " + reactionCount);  // デバッグ用ログ


        // 反応音を再生（反応開始時から反応終了まで）
        if (reactionSound != null && !audioSource.isPlaying) // 反応音が設定されていて、音声が再生されていない場合
        {
            audioSource.clip = reactionSound;
            audioSource.Play();
            isAudioPlaying = true; // 音声再生中フラグを立てる
            Debug.Log("Reaction sound played");
        }

        Vector3 reactionPosition = (leftHandAnchor.position + rightHandAnchor.position) / 2f;
        ParticleSystem effect = null;

        if (!firstReactionDone)
        {
            effect = Instantiate(reactionEffectHClNaOH, reactionPosition, Quaternion.identity);
            firstReactionDone = true;
            Debug.Log("First reaction effect triggered.");

            // 手と手の間に生成物を生成
            Instantiate(generatedObjectPrefab, reactionPosition, Quaternion.identity);
            Debug.Log("Generated object created.");
        }
        else
        {
            effect = Instantiate(reactionEffectNH3HCl, reactionPosition, Quaternion.identity);
            Debug.Log("Second reaction effect triggered.");
        }

        Destroy(effect.gameObject, reactionDuration);

        nextReactionTime = Time.time + reactionCooldown;
        Invoke(nameof(EndReaction), reactionDuration);
    }

    // 反応終了処理
    void EndReaction()
    {
        Debug.Log("Reaction ended.");

        // 反応音が鳴っている場合、音声を停止する
        if (audioSource.isPlaying && audioSource.clip == reactionSound)
        {
            audioSource.Stop();  // 反応音を停止
            isAudioPlaying = false;  // 音声再生中フラグをリセット
            Debug.Log("Reaction sound stopped");
        }

        RemoveIonsFromHands();
        reactionInProgress = false;
        Debug.Log("reactionInProgress reset to false.");
        nextReactionTime = Time.time + reactionCooldown;

        // 反応回数に応じて処理を分ける
        if (reactionCount == 1)
        {
            // 1回目の反応後に音声終了後にUIテキストを変更する
            StartCoroutine(ChangeUITextAfterAudio()); // IEnumeratorを使って音声終了を待ってからUI変更
        }
        else if (reactionCount >= 2)
        {
            // 2回目の反応後にシーン遷移
            ChangeScene();  // 反応が終了した後にシーン遷移を行う
        }




    }

    // 両手のイオンを削除する
    void RemoveIonsFromHands()
    {
        IonController[] ionControllers = GetComponentsInChildren<IonController>();

        foreach (IonController ionController in ionControllers)
        {
            if (ionController != null && ionController.IsAbsorbed())
            {
                // イオン破壊音を再生
                if (destroySound != null && !audioSource.isPlaying)
                {
                    audioSource.clip = destroySound;
                    audioSource.Play();
                    Debug.Log("Destroy sound played");
                }

                ionController.DestroyIon();
                Debug.Log("Ion destroyed.");
            }
        }

        leftHandIons = 0;
        rightHandIons = 0;
    }

    // 定期的にイオンと手の状態をチェックする
    void CheckIonsAndHands()
    {
        if (reactionInProgress || Time.time < nextReactionTime)
        {
            Debug.Log("Check skipped because reaction is in progress or on cooldown.");
            return;
        }

        if (AreIonsOnBothHands() && AreHandsTouching())
        {
            Debug.Log("Ions detected on both hands and hands are touching. Triggering reaction...");
            TriggerReaction();
        }

        // 片手に2個以上のイオンがある場合に音声を鳴らす
        if ((leftHandIons >= 2 || rightHandIons >= 2) && !isAudioPlaying && handIonSound != null)
        {
            audioSource.clip = handIonSound;
            audioSource.Play();
            Debug.Log("Hand Ion sound played");
        }
    }

    // 音声終了後にUIテキストを変更
    IEnumerator ChangeUITextAfterAudio()
    {
        // 音声が再生されていれば待機
        while (audioSource.isPlaying)
        {
            yield return null; // 音声が終了するまで待機
        }

        // 音声が終了した後にUIテキストを変更
        if (uiText != null)
        {
            uiText.text = afterUIText; // destroySound後に表示するUIテキスト
            Debug.Log("After UI Text: " + afterUIText);
        }


        // yield return new WaitForSeconds(1f); // ヒント表示前の時間待機（削除）
        if (uiText != null)
        {
            uiText.text = afterUIText; // UIテキストをそのまま表示し続ける
        }


        // OVRPlayerControllerを再度有効化して動きを再開
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    // シーン遷移
    void ChangeScene()
    {
        SceneManager.LoadScene(nextSceneName); // インスペクターで設定したシーン名に遷移
    }


}
