using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerAnimator : MonoBehaviour
{
    Animator anim;
    NormalCustomer normalCustMove;
    SpriteRenderer spriteRen;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        normalCustMove = GetComponent<NormalCustomer>();
        spriteRen = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(normalCustMove.moveDir.x !=0 || normalCustMove.moveDir.y !=0)
        {
            anim.SetBool("Move", true);

            SpriteDirection(); 
        }
        else
        {
            anim.SetBool("Move", false);
        }
    }

    void SpriteDirection()
    {
        if(normalCustMove.lastHorizontalVec > 0)
        {
            spriteRen.flipX = true;
        }
        else
        {
            spriteRen.flipX = false;
        }
    }
}
