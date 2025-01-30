using UnityEngine;
using System.Collections;

public class IntroSequenceManager : MonoBehaviour
{
    public Camera playerCamera;  // プレイヤーのカメラ（視点）
    public GameObject[] targetObjects;  // 物体（最初は非表示、複数設定）
    public AudioSource audioSource;  // セリフの音声
    public AudioClip firstDialogue;  // 1つ目のセリフ（ここはどこだ？）
    public AudioClip secondDialogue; // 2つ目のセリフ（化学知識を使って解決しよう）
    public AudioClip thirdDialogue;  // 3つ目のセリフ（物体に関連するセリフ）
    public AudioClip fourthDialogue; // 4つ目のセリフ（重要な情報）

    // 見回す際の設定
    //public float lookAroundDistance = 30f; // 見回す最大の角度（左右）
    //public float lookAroundSpeed = 1f;    // 見回す速さ

    // 演出のための変数
    public Canvas fadeCanvas;  // 暗転用Canvas
    public Canvas uiTextCanvas;  // UIテキスト用Canvas
    public UnityEngine.UI.Text uiText;  // UIテキストコンポーネント
    public Canvas newUITextCanvas;  // 新しいUIテキスト用Canvas
    public UnityEngine.UI.Text newUIText;  // 新しいUIテキストコンポーネント
    public float fadeDuration = 2f;  // 暗転から明るくなる時間
    public float blinkDuration = 0.5f;  // 瞬きの長さ
    public float blinkWaitTime = 1f;  // 瞬き後の待機時間
    public float textDisplaySpeed = 0.1f;  // 文字表示の速さ（秒）

    private bool isSequenceActive = true;
    private bool isAudioPlaying = false;  // 音声再生中かどうか
    private ChemistryReaction chemistryReactionScript;  // ChemistryReactionスクリプトへの参照

    void Start()
    {
        // ChemistryReactionスクリプトへの参照を取得
        chemistryReactionScript = FindObjectOfType<ChemistryReaction>();

        // 最初に物体を非表示にする
        if (targetObjects != null)
        {
            foreach (var targetObject in targetObjects)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
                }
            }
        }

        // プレイヤーの操作を無効にする
        DisablePlayerControl();

        // UIテキストCanvasを非表示にする
        uiTextCanvas.gameObject.SetActive(false);
        newUITextCanvas.gameObject.SetActive(false);  // 新しいUIテキストCanvasも非表示にする

        // シーケンス開始
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // 1. 暗転から始める
        yield return StartCoroutine(FadeIn());

        // 2. 瞬きする
        yield return StartCoroutine(Blink());

        // 3. 起き上がる
        yield return StartCoroutine(WakeUpSequence());

        // 4. セリフ1（ここはどこだ？）と辺りを見回す
        //yield return StartCoroutine(LookAroundWhilePlayingDialogue(firstDialogue));
        yield return StartCoroutine(PlayDialogueAndWait(firstDialogue));

        // 5. セリフ2（化学知識を使って解決しよう）
        yield return StartCoroutine(PlayDialogueAndWait(secondDialogue));

        // 6. 物体が非表示から表示
        yield return StartCoroutine(ShowTargetObjects());
        yield return new WaitForSeconds(1f);

        // 7. セリフ3（物体が表示された後）
        yield return StartCoroutine(PlayDialogueAndWait(thirdDialogue));

        // 8. セリフ4開始時にUIテキストを表示
        uiTextCanvas.gameObject.SetActive(true);
        string textForUI = "化学者メモ：NAOH+HCL->NACL+H2O 水酸化ナトリウム＋塩酸→塩化ナトリウム＋水";
        yield return StartCoroutine(DisplayTextWithTypingEffect(uiText, textForUI));
        yield return StartCoroutine(PlayDialogueAndWait(fourthDialogue));

        // セリフ4終了後、UIテキストを非表示にする
        yield return new WaitForSeconds(0.5f);
        uiTextCanvas.gameObject.SetActive(false);

        // 9. プレイヤー操作を有効にする
        EnablePlayerControl();

        // 10. 新しいUIテキストを表示
        newUITextCanvas.gameObject.SetActive(true);
        string newUITextMessage = "どちらかの手に水素イオン１個（水色）と塩素イオン１個（黄色）を両方乗せてみよう！";
        yield return StartCoroutine(DisplayTextWithTypingEffect(newUIText, newUITextMessage));
    }

    // 音声再生と待機
    IEnumerator PlayDialogueAndWait(AudioClip dialogueClip)
    {
        if (audioSource != null && dialogueClip != null)
        {
            isAudioPlaying = true; // 再生中フラグをセット
            audioSource.PlayOneShot(dialogueClip);
            yield return new WaitForSeconds(dialogueClip.length);
            isAudioPlaying = false; // 再生終了後にフラグをリセット
        }
    }

    // フェードイン
    IEnumerator FadeIn()
    {
        float time = 0f;
        CanvasGroup canvasGroup = fadeCanvas.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;  // 完全に暗い状態

        while (time < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);  // 徐々に明るくなる
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;  // 最終的に完全に透明
    }

    // 瞬きの処理
    IEnumerator Blink()
    {
        float blinkTime = 0f;
        CanvasGroup canvasGroup = fadeCanvas.GetComponent<CanvasGroup>();

        // 目を閉じる（暗くする）
        while (blinkTime < blinkDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, blinkTime / blinkDuration);
            blinkTime += Time.deltaTime;
            yield return null;
        }

        // 目を開ける（明るくする）
        yield return new WaitForSeconds(blinkWaitTime); // 瞬き後の待機時間
        blinkTime = 0f;
        while (blinkTime < blinkDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, blinkTime / blinkDuration);
            blinkTime += Time.deltaTime;
            yield return null;
        }
    }

    // 物体を順番に表示
    IEnumerator ShowTargetObjects()
    {
        if (targetObjects != null)
        {
            foreach (var targetObject in targetObjects)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(true);
                    yield return new WaitForSeconds(1f);  // 各物体の表示の間に少し待機
                }
            }
        }
    }

    // 起き上がるアニメーション
    IEnumerator WakeUpSequence()
    {
        OVRPlayerController playerController = playerCamera.GetComponentInParent<OVRPlayerController>();
        Vector3 startPos = playerCamera.transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y + 1.5f, startPos.z); // 起き上がった位置
        Quaternion startRotation = playerController.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0f, 0f, 0f);  // (0, 0, 0) に回転を変更
        float time = 0f;
        float duration = 3f;  // 起き上がりのアニメーション時間

        while (time < duration)
        {
            // カメラの位置を変化させる
            playerCamera.transform.position = Vector3.Lerp(startPos, endPos, time / duration);

            // OVRPlayerControllerの回転を (0, 0, 0) に変化させる
            playerController.transform.rotation = Quaternion.Slerp(startRotation, endRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.position = endPos;  // 最終位置
        playerController.transform.rotation = endRotation;  // 最終回転
    }

    // UIテキストにタイピング効果を適用
    IEnumerator DisplayTextWithTypingEffect(UnityEngine.UI.Text targetText, string message)
    {
        targetText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            targetText.text += letter;
            yield return new WaitForSeconds(textDisplaySpeed);
        }
    }

    // プレイヤーの操作を無効化
    void DisablePlayerControl()
    {
        OVRPlayerController playerController = playerCamera.GetComponentInParent<OVRPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false; // プレイヤーの操作を無効化
        }
    }

    // プレイヤーの操作を有効化
    void EnablePlayerControl()
    {
        OVRPlayerController playerController = playerCamera.GetComponentInParent<OVRPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true; // プレイヤーの操作を有効化
        }
    }

    // 視点を左右に振る（左 → 右 → そのまま終了）
    /*IEnumerator LookAroundWhilePlayingDialogue(AudioClip dialogueClip)
    {
        // セリフを再生しながら視点を動かす
        float rotationTime = 1.5f;  // 左右に振る時間
        Vector3 startRotation = playerCamera.transform.eulerAngles;

        // 左向きに回転
        Vector3 leftRotation = startRotation + new Vector3(0, -lookAroundDistance, 0);
        float t = 0;
        while (t < rotationTime)
        {
            playerCamera.transform.eulerAngles = Vector3.Lerp(startRotation, leftRotation, t / rotationTime);
            t += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.eulerAngles = leftRotation;

        // 右向きに回転
        Vector3 rightRotation = startRotation + new Vector3(0, lookAroundDistance, 0);
        t = 0;
        while (t < rotationTime)
        {
            playerCamera.transform.eulerAngles = Vector3.Lerp(leftRotation, rightRotation, t / rotationTime);
            t += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.eulerAngles = rightRotation;
    }*/
    

}
