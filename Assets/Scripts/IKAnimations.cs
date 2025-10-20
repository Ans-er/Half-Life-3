using UnityEngine;

public class IKAnimations : MonoBehaviour
{
  private Animator animator;

  public bool isActive = true;

  public Transform target;

  public float targetWeight;

  public Transform lookAtTarget;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    animator = GetComponent<Animator>();
  }

  // Update is called once per frame
  void Update()
  {

  }

  private void OnAnimatorIK()
  {
    if(isActive)
    {
      animator.SetIKPosition(AvatarIKGoal.RightHand, target.position);
      animator.SetIKPositionWeight(AvatarIKGoal.RightHand, targetWeight);

      animator.SetLookAtPosition(lookAtTarget.position);
      animator.SetLookAtWeight(1.0f);
    }
  }
}
