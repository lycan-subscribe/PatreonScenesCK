using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Animator))]
public class ViewerAvatar: MonoBehaviour
{
    Animator animator;

    void OnEnable(){
        animator = GetComponent<Animator>();

        Assert.IsTrue( animator.isHuman );
    }

    public void SetController(RuntimeAnimatorController ac){
        animator.runtimeAnimatorController = ac;
    }
}