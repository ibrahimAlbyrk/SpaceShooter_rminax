using System.Collections;
using UnityEngine;

public class DestructionScript : MonoBehaviour
{
    public GameObject Explosion;
    public float HP;
    public Transform particleParent;
    public float ExplosionScale = 10f;

    private bool dead;

    public bool Asteroid;
    public bool Metal;

    private void Update()
    {
        if (!dead && HP <= 0f)
        {
            StartCoroutine(Death());
            dead = true;
        }
    }

    //TODO: Run If HP is Zero
    private IEnumerator Death()
    {
        if (GetComponent<BasicAI>() != null)
        {
            if (GetComponent<BasicAI>().shoot != null)
            {
                GetComponent<BasicAI>().StopCoroutine(GetComponent<BasicAI>().shoot);
            }
        }

        GetComponent<Renderer>().enabled = false;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>() != null)
            {
                child.GetComponent<Renderer>().enabled = false;
            }
        }

        if (particleParent != null)
        {
            foreach (Transform child in particleParent)
            {
                child.gameObject.SetActive(false);
            }
        }

        GetComponent<Collider>().enabled = false;
        if (Explosion != null)
        {
            var firework = Instantiate(Explosion, transform.position, transform.rotation);
            firework.transform.localScale *= ExplosionScale;
            
            firework.GetComponent<ParticleSystem>().Play();
            
            Destroy(gameObject);
            
            yield return new WaitForSeconds(2f);
            
            Destroy(firework);
        }
        else
            Destroy(gameObject);
            
    }
}