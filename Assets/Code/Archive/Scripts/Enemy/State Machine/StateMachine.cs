using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private IState currentState;

    public void SetState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
