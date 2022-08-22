using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Animator))]
[ExecuteInEditMode]
public class ViewerAvatar: MonoBehaviour
{
    public Vector3 viewpoint = new Vector3(0, 0.1f, 0);

    Animator animator;

    void OnEnable(){
        animator = GetComponent<Animator>();

        Assert.IsTrue( animator.isHuman );
    }

    void OnDrawGizmosSelected(){
        var scale = transform.localScale;
        scale.x = 1 / scale.x;
        scale.y = 1 / scale.y;
        scale.z = 1 / scale.z;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(Vector3.Scale(viewpoint, scale)), 0.05f);
    }

    public void SetController(RuntimeAnimatorController ac){
        animator.runtimeAnimatorController = ac;
    }
}