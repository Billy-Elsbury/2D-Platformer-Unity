using System.Collections;
using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine(LevelExit());
        }
    }

    IEnumerator LevelExit()
    {
        yield return new WaitForSeconds(0.1f);

        GameManager.instance.LevelComplete();
    }
}
