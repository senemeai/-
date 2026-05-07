using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Animator ani;
    private Rigidbody2D rBody;
    void Start()
    {
        ani = GetComponent<Animator>();
        rBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (BottomBarDialogue.IsAnyDialogueActive) return;  // 加这一行
        if (OldmanDialogue.IsAnyDialogueActive) return;
        if (YoungManDialogue.IsAnyDialogueActive) return;
        if (ProfessorDialogue.IsAnyDialogueActive) return;
        if (TravelerDialogue.IsAnyDialogueActive) return;
        //获取水平轴和垂直轴-1 0 1
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        //如果水平方向的horizontal不为0，证明按下左或右了
        if (horizontal != 0)
        {
            ani.SetFloat("Horizontal", horizontal);//把horizontal的值传给unity里Horizontal
            ani.SetFloat("Vertical", 0);//左右移动的时候把Vertical设置为0
        }
        if (vertical != 0)
        {
            ani.SetFloat("Vertical", vertical);
            ani.SetFloat("Horizontal", 0);
        }
        //切换跑步状态
        Vector2 dir = new Vector2(horizontal, vertical);
        ani.SetFloat("Speed", dir.magnitude);
        //朝向该方向移动
        rBody.velocity = dir * 1.5f;

    }
}
