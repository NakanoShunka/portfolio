using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IONAreaTrigger : MonoBehaviour
{
    // UIテキストを参照するための変数
    public Text messageText;  // メッセージ用のUI Text
    public float displayTime = 3f;  // メッセージを表示する時間（秒）

    void Start()
    {
        // 初期状態でUIテキストを非表示にする
        messageText.gameObject.SetActive(false);
    }

    // トリガー内にオブジェクトが入ったときに呼ばれる
    void OnTriggerEnter(Collider other)
    {
        // 他のオブジェクトがplayerタグを持っている場合
        if (other.CompareTag("Player"))
        {
            // メッセージを表示
            StartCoroutine(DisplayMessage());
        }
    }

    // トリガー内からオブジェクトが出たときに呼ばれる
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // メッセージを非表示にする（すぐに非表示にしたい場合）
            StopCoroutine(DisplayMessage());
            messageText.gameObject.SetActive(false);
        }
    }

    // メッセージを表示してから消すコルーチン
    IEnumerator DisplayMessage()
    {
        // メッセージを表示
        messageText.gameObject.SetActive(true);

        // 指定した時間だけ待機
        yield return new WaitForSeconds(displayTime);

        // メッセージを非表示にする
        messageText.gameObject.SetActive(false);
    }
}
