using UnityEngine;
using UnityEngine.AI;

public class NPCAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(SpeedHash, speed);
    }
}