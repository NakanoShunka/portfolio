using UnityEngine;

public class IonController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float changeDirectionInterval = 2f;
    public Transform[] movementAreas;  // この変数はもう使われません
    private bool isAbsorbed = false;
    private bool isDeleted = false;
    private Animator animator;

    private Transform absorbedHand;

    public ParticleSystem absorptionEffect; // 吸収時のエフェクト

    void Start()
    {
        // movementAreasが使われなくなるため、関連するコードは削除します。
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
    }

    void Update()
    {
        // 吸収されていなければ移動
        if (!isAbsorbed)
        {
            // MoveTowardsTarget メソッドは削除されたので何もしません
        }
        else
        {
            // 吸収された場合、手の位置に追従
            if (absorbedHand != null)
            {
                transform.position = absorbedHand.position;
            }
        }
    }

    // SetNewTargetPositionは不要になりました。
    // 代わりに吸収されたときのみ位置が固定されます。

    public void AbsorbToHand(Transform hand)
    {
        isAbsorbed = true;
        absorbedHand = hand;
        transform.SetParent(hand); // オブジェクトを手の子オブジェクトにする
        transform.localPosition = Vector3.zero; // 手の中心に位置を合わせる
        Debug.Log($"Ion absorbed by {hand.name}");

        // 手に吸収されたときのエフェクト発生
        if (absorptionEffect != null)
        {
            ParticleSystem effect = Instantiate(absorptionEffect, hand.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);  // エフェクトを2秒後に削除
        }
    }

    public bool IsDeleted()
    {
        return isDeleted;
    }

    public void MarkAsDeleted()
    {
        isDeleted = true;
    }

    public bool IsAbsorbed()
    {
        return isAbsorbed;
    }

    public void DestroyIon()
    {
        Destroy(gameObject);
        Debug.Log("Ion destroyed from the hand.");
    }
}
