using Mirror;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Network;

public class AsteroidBehaviour : NetIdentity
{
    [SerializeField] private float _detectionRange;
    [SerializeField] private LayerMask _detectionLayer;
    
    public float spaceshipPushDistance = 125f;
    public float pushSpeedMod = 1.05f;
    private bool running;

    public bool physics;
    
    private readonly List<Collider> _collisions = new();

    private PhysicsScene _physicsScene;

    private void OnColliderEntered(Component coll)
    {
        try
        {
            if (!running && !physics)
            {
                StartCoroutine(Moving(coll.transform));
            }
        }
        catch
        {
            // ignored
        }
    }

    [ServerCallback]
    private void Start()
    {
        _physicsScene = gameObject.scene.GetPhysicsScene();
    }
    
    [ServerCallback]
    private void Update()
    {
        //Check Colliders and do action
        var colls = new Collider[2];
        _physicsScene.OverlapSphere(transform.position, _detectionRange, colls, _detectionLayer,
            QueryTriggerInteraction.UseGlobal);
            
        foreach (var coll in colls)
        {
            if (coll == null) continue;
            
            if(coll.gameObject == gameObject) continue;
            
            if (_collisions.Contains(coll)) continue;

            _collisions.Add(coll);

            OnColliderEntered(coll);
        }

        for (var i = 0; i < _collisions.Count; i++)
        {
            var coll = _collisions[i];

            if (colls.Any(c => c == coll)) continue;

            _collisions.Remove(coll);
        }
    }

    private IEnumerator Moving(Transform other)
    {
        running = true;
        var dir = (transform.position - other.position).normalized;
        
        var dest = transform.position + dir * spaceshipPushDistance;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
}