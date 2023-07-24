using System.Collections;
using _Project.Scripts.Spaceship;
using UnityEngine;

public class AsteroidBehaviour : MonoBehaviour
{
    public float spaceshipPushDistance = 125f;
    public float projectilePushDistance = 35f;
    public float pushSpeedMod = 1.05f;
    private bool running;

    public bool physics;

    private void OnTriggerEnter(Collider other)
    {
        try
        {
            if ((!running && !physics))
            {
                StartCoroutine(Moving(other.transform));
            }
        }
        catch
        {
            // ignored
        }
    }

    private IEnumerator Moving(Transform other)
    {
        running = true;
        var dir = (transform.position - other.position).normalized;
        Vector3 dest;

        try
        {
            if (other.transform == SpaceshipController.instance.transform)
            {
                dest = transform.position + ((dir) * spaceshipPushDistance);
            }
            else
            {
                dest = transform.position + ((dir) * projectilePushDistance);
            }
        }
        catch
        {
            dest = transform.position + ((dir) * projectilePushDistance);
        }

        while (Vector3.Distance(transform.position, dest) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, dest, Time.deltaTime * pushSpeedMod);
            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(-dir, transform.up),
                    Time.deltaTime / 3.5f);
            yield return null;
        }


        running = false;
    }
}