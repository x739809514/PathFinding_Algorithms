using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerController : MonoBehaviour
{
    public NavMeshAgent agent;
    private Camera mainCamera;
    public ThirdPersonCharacter character;

    private void Start()
    {
        mainCamera=Camera.main;
        agent.updateRotation = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                agent.SetDestination(hit.point);
            }
        }
        
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            character.Move(agent.desiredVelocity ,false,false); 
        }
        else
        {
            character.Move(Vector3.zero,false,false); 
        }
    }
}
