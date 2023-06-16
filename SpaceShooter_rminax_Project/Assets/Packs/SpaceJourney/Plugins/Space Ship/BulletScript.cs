using System.Collections;
using _Project.Scripts.Spaceship;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private float _bulletDamage;
    [SerializeField] private float _bulletLifeTime;

    [Tooltip("The particle effect of hitting something.")]
    public GameObject HitEffect;

    public ParticleSystem Trail;
    public Transform ship;

    Transform player;

    //float Damage;

    //float lifetime;

    void Start()
    {
        StartCoroutine(Lifetime());
    }

    void Awake()
    {
        if (SpaceshipController.instance != null)
        {
            player = SpaceshipController.instance.transform;
            ship = SpaceshipController.instance.transform.root;
        }
        else
        {
            player = SpaceshipController2D.instance.transform;
            ship = SpaceshipController2D.instance.transform.root;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.root == ship) return;

        if (col.transform.parent != player) return;

        if (SpaceshipController.instance != null)
            SpaceshipController.instance.Shake();

        if (col.GetComponent<DestructionScript>() != null)
        {
            if (SpaceshipController.instance != null)
                col.GetComponent<DestructionScript>().HP -=
                    SpaceshipController.instance.m_shooting.bulletSettings.BulletDamage;
        }

        if (col.transform.parent != null)
            if (col.transform.parent.GetComponent<SpaceshipController>() != null)
            {
            } //TODO Give Damage to player

        if (col.GetComponent<BasicAI>() != null)
            col.GetComponent<BasicAI>().threat();

        StartCoroutine(DestroySequence());
    }

    private IEnumerator DestroySequence()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Renderer>().enabled = false;

        if (Trail != null)
        {
            Trail.gameObject.SetActive(false);
        }

        if (HitEffect != null)
        {
            var position = transform.GetChild(1).gameObject.transform.position;

            var firework = Instantiate(HitEffect, position, Quaternion.identity);
            firework.transform.parent = gameObject.transform;

            var fireworkPS = GetComponent<ParticleSystem>();
            if (fireworkPS != null) fireworkPS.Play();

            yield return new WaitForSeconds(1f);

            Destroy(firework);
        }

        Destroy(gameObject);
    }

    private IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(_bulletLifeTime);
        Destroy(gameObject);
    }
}