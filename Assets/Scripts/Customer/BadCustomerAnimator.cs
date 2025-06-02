using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadCustomerAnimator : MonoBehaviour
{
    Animator anim;
    BadCustomer badCustMove;
    SpriteRenderer spriteRen;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        badCustMove = GetComponent<BadCustomer>();
        spriteRen = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(badCustMove.moveDir.x !=0 || badCustMove.moveDir.y !=0)
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
        if(badCustMove.lastHorizontalVec > 0)
        {
            spriteRen.flipX = true;
        }
        else
        {
            spriteRen.flipX = false;
        }
    }
}
