using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private MouseClickController mouseInput;

    private void Awake()
    {
        mouseInput = FindFirstObjectByType<MouseClickController>();
    }
    
    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void GoToDestination(Vector3 destination)
    {
        navMeshAgent.SetDestination(destination);
    }

    private void OnEnable()
    {
        mouseInput.OnMouseClick.AddListener(GoToDestination);
    }

    private void OnDisable()
    {
        mouseInput.OnMouseClick.RemoveListener(GoToDestination);
    }
}
