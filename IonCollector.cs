using UnityEngine;
using UnityEngine.UI;  // UIに必要
using TMPro;  // TextMeshProを使う場合
using System.Collections;
using System.Collections.Generic;

public class IonCollector : MonoBehaviour
{
    public Transform[] handAnchors; // 左手と右手
    public ParticleSystem collectEffect;
    public AudioSource audioSource;  // 音声を再生するためのAudioSource
    public AudioClip collectSound;  // 収集時の音
    public Text uiText;  // UI Text (TextMeshProでもOK)
    public float reactionCountInterval = 2f;  // リアクションカウント表示間隔


    private Dictionary<Transform, List<GameObject>> handIons = new Dictionary<Transform, List<GameObject>>();
    private int maxIonsPerHand = 4; // 1つの手に収集できるイオンの最大数
    private ChemistryReaction chemistryReaction; // ChemistryReactionスクリプトを参照
    private bool hasReactionCountReachedOne = false; // 反応回数が1になったことを記録

    private void Start()
    {
        chemistryReaction = FindObjectOfType<ChemistryReaction>();  // ChemistryReactionスクリプトのインスタンスを取得

        // 手ごとのイオンリストを初期化
        foreach (Transform hand in handAnchors)
        {
            handIons[hand] = new List<GameObject>();
        }

        // UIテキストを初期化
        if (uiText != null)
        {
            uiText.text = ""; // 初期状態では何も表示しない
        }


    }
    void Update()
    {
        // ChemistryReactionの反応回数を取得
        int currentReactionCount = chemistryReaction.ReactionCount;

        // 反応回数が1になった場合に手のイオンリストをリセット
        if (currentReactionCount == 1 && !hasReactionCountReachedOne)
        {
            // 反応回数が1に達したことを記録
            hasReactionCountReachedOne = true;

            // 反応回数が1になったら手のイオンリストをリセット
            foreach (var hand in handAnchors)
            {
                handIons[hand].Clear();
            }

            Debug.Log("handIons have been cleared because reaction count is 1.");
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ion"))
        {
            IonController ion = other.GetComponent<IonController>();
            if (ion != null && !ion.IsAbsorbed())
            {
                CollectIon(ion.gameObject);
            }
        }
    }

    void CollectIon(GameObject ion)
    {
        Transform targetHand = GetNextAvailableHand();
        if (targetHand == null) return; // 手がいっぱいなら収集できない

        IonController ionController = ion.GetComponent<IonController>();
        if (ionController != null)
        {
            ionController.AbsorbToHand(targetHand);
        }

        // イオンを手に追加
        handIons[targetHand].Add(ion);

        // 収集エフェクトを表示
        if (collectEffect != null)
        {
            Instantiate(collectEffect, targetHand.position, Quaternion.identity);
        }

        // ChemistryReactionの反応回数を取得
        int currentReactionCount = chemistryReaction.ReactionCount;

        // 手にイオンが2個乗った場合に、反応回数に応じたUIテキストを表示
        if (handIons[targetHand].Count == 2)
        {
            PlayCollectSound();

            // 反応回数に応じたUIテキストを表示
            if (currentReactionCount == 0)
            {
                ShowUIText("もう片方の手にナトリウムイオン１個（赤色）と水酸化物イオン１個（水色と紫色）を両方乗せてみよう！");
            }
            else if (currentReactionCount == 1)
            {
                ShowUIText("もう片方の手に窒素イオン１個（黄緑色）と水素イオン３個（水色）を両方乗せてみよう！");
            }
            else
            {
                Debug.Log("currentReactionCount is not 1, currentReactionCount: " + currentReactionCount);
            }
        }
    }

    // 空いている手を探して返す
    Transform GetNextAvailableHand()
    {
        foreach (Transform hand in handAnchors)
        {
            // その手にまだ収集できる余地があればその手を返す
            if (handIons[hand].Count < maxIonsPerHand)
            {
                return hand;
            }
        }
        return null; // すべての手が満杯ならnullを返す
    }

    // 音声を再生
    private void PlayCollectSound()
    {
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
    }

    private void ShowUIText(string message)
    {
        // すべてのUIテキストを非表示にする（前回のメッセージを消す）
        HideAllUIText();

        // uiText.text を最初にクリア
        if (uiText != null)
        {
            uiText.text = "";  // 既存のテキストをクリア
            Debug.Log("UI Text cleared");  // クリアされたことを確認
        }

        // 新しいメッセージを表示
        if (uiText != null)
        {
            uiText.text = message;
            Debug.Log("UI Text updated to: " + message);  // 新しいテキストが設定されたことを確認
        }

        
    }

    


    // 同じCanvasを親に持つ全てのTextを非表示にするメソッド
    private void HideAllUIText()
    {
        Canvas canvas = uiText != null ? uiText.GetComponentInParent<Canvas>() : null;
        if (canvas != null)
        {
            // TextMeshProを使っている場合、TextMeshProUGUIを使用
            Text[] allText = canvas.GetComponentsInChildren<Text>();
            foreach (Text text in allText)
            {
                // 自身のUIテキスト以外は非表示にする
                if (text != uiText) // uiText以外のTextコンポーネントを非表示
                {
                    text.gameObject.SetActive(false);
                }
            }

            // TextMeshProを使用している場合、こちらも対応
            TextMeshProUGUI[] allTextMeshPro = canvas.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI textMeshPro in allTextMeshPro)
            {
                // 自身のUIテキスト以外は非表示にする
                if (textMeshPro != uiText) // uiText以外のTextMeshProUGUIを非表示
                {
                    textMeshPro.gameObject.SetActive(false);
                }
            }
        }
    }

    

    










}
