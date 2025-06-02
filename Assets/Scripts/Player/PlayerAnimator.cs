using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    Animator anim;
    PlayerMovement playerMove;
    SpriteRenderer spriteRen;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        playerMove = GetComponent<PlayerMovement>();
        spriteRen = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerMove.moveDir.x !=0 || playerMove.moveDir.y !=0)
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
        if(playerMove.lastHorizontalVec > 0)
        {
            spriteRen.flipX = true;
        }
        else
        {
            spriteRen.flipX = false;
        }
    }
}
