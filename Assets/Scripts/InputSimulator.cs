using UnityEngine;

public class InputSimulator : MonoBehaviour
{
    public PlayerCustomController playerCustomController;

    void Start()
    {
        // Simulate a sequence of inputs
        StartCoroutine(SimulateInputs());
    }

    System.Collections.IEnumerator SimulateInputs()
    {
        // Move right for 2 seconds
        playerCustomController.MoveRight();
        yield return new WaitForSeconds(2f);

        // Stop moving
        playerCustomController.StopMoving();
        yield return new WaitForSeconds(0.5f);

        // Jump
        playerCustomController.Jump();
        yield return new WaitForSeconds(1f);

        // Move left for 2 seconds
        playerCustomController.MoveLeft();
        yield return new WaitForSeconds(2f);

        // Stop moving
        playerCustomController.StopMoving();
    }
}