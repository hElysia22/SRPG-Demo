using UnityEngine;
public class TestRootMotion : MonoBehaviour
{
    public Animator anim;
    public Rigidbody rb;

    void Update()
    {
        // 强制播放走路动画
        anim.SetFloat("Speed", 3f);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void OnAnimatorMove()
    {
        // 纯根运动，不调速，直接移动
        Vector3 move = anim.deltaPosition;
        Debug.Log(move);
        move.y = 0;
        transform.position += move;
    }
}