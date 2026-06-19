using UnityEngine;

public class DestroyOnStateEnter : StateMachineBehaviour
{
    // OnStateExit is called exactly when a transition ends and the state machine finishes evaluating this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Destroy(animator.gameObject);
    }
}