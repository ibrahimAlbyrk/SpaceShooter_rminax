using _Project.Scripts.Spaceship;
using Mirror;
using UnityEngine;

public class BulletScript : NetworkBehaviour
{
    [SerializeField] private float _bulletDamage;
    [SerializeField] private float _bulletLifeTime;
    [SerializeField] private float _bulletSpeed = 300f;

    [Tooltip("The particle effect of hitting something.")]
    public GameObject HitEffect;

    public ParticleSystem Trail;
    public Transform ship;

    Transform player;

    //float Damage;

    //float lifetime;
    
    private void Start()
    {
        Invoke(nameof(CMD_Lifetime), _bulletLifeTime);
        
        if (SpaceshipController.instance == null) return;
        player = SpaceshipController.instance.transform;
        ship = SpaceshipController.instance.transform.root;
    }
    
    private void Update()
    {
        transform.position += transform.forward * (_bulletSpeed * Time.fixedDeltaTime);
    }
    
    private void CMD_DestroySequence() => RPC_DestroySequence();

    private void RPC_DestroySequence() => DestroySequence();
    
    private void DestroySequence()
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

            Destroy(firework);
        }

        Destroy(gameObject);
    }
    
    private void CMD_Lifetime() => Destroy(gameObject);
    
    //private void OnTriggerEnter(Collider col)
    //{
    //    if (col.transform.root == ship) return;
    //
    //    if (col.transform.parent != player) return;
    //
    //    if (SpaceshipController.instance != null)
    //        SpaceshipController.instance.Shake();
    //
    //    if (col.GetComponent<DestructionScript>() != null)
    //    {
    //        if (SpaceshipController.instance != null)
    //            col.GetComponent<DestructionScript>().HP -=
    //                SpaceshipController.instance.m_shooting.bulletSettings.BulletDamage;
    //    }
    //
    //    if (col.transform.parent != null)
    //        if (col.transform.parent.GetComponent<SpaceshipController>() != null)
    //        {
    //        } //TODO Give Damage to player
    //
    //    if (col.GetComponent<BasicAI>() != null)
    //        col.GetComponent<BasicAI>().threat();
    //
    //    CMD_DestroySequence();
    //}
}