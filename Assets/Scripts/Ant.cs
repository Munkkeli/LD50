using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Order
{
    CAKE = 1,
    GATHER = 2,
    NEST = 3,
    TRAP = 4,
}

public class Ant : MonoBehaviour
{
    public Transform[] LegsLeft;
    public Transform[] LegsRight;
    public Transform antennaLeft;
    public Transform antennaRight;
    public GameObject brood;
    public GameObject fireParticles;
    public GameObject hill;

    public Order order;
    public float health = 1f;
    
    public float movementSpeed = 10f;
    public float rotationSpeed = 10f;
    
    public float legAnimationSpeed = 10f;
    public float legAnimationScale = 5f;
    public float legAnimationCascade = 1f;

    public float antennaAnimationSpeed = 10f;
    public float antennaAnimationScale = 5f;
    public float antennaAnimationCascade = 1f;

    public bool isCarryingBrood = false;
    public bool inGathering = false;
    public bool isOnFire = false;
    
    private float legAnimation = 0;
    private float antennaAnimation = 0;
    private float[] antennaAnimationSeed;
    
    private float[] _movementSpeedSeed;

    private Vector2[] _path = Array.Empty<Vector2>();
    private int _pathIndex;

    private Gathering? _gathering;
    private Vector2? _trap = null;
    
    private Vector3 _movementTarget;
    private Vector2 _lastPosition;

    private const int CheckFireSpreadTimeout = 120;
    private int _checkFireSpreadCooldown;
    
    public void GiveOrder(Order order, Vector2[] path = null)
    {
        if (this.order == Order.TRAP) return;
        
        this.order = order;

        switch (order)
        {
            case Order.CAKE:
                if (path == null)
                {
                    GoTo(AI.Current.cake.position);
                }
                else
                {
                    ClearPath();
                    _path = path;
                    _pathIndex = 0;
                }
                break;
            case Order.GATHER:
                _gathering = AI.Current.FindGathering(this);
                _gathering?.Ants.Add(this);
                
                // TODO: Remove ant from gathering
                
                if (_gathering != null) GoTo(_gathering.Value.Tile.position);
                break;
            case Order.NEST:
                _gathering = AI.Current.FindGathering(this);
                if (_gathering != null) GoTo(_gathering.Value.Tile.position);
                break;
            case Order.TRAP:
                ClearPath();
                break;
        }
    }

    public void CatchInTrap(Vector2 trap)
    {
        _trap = trap;
        GiveOrder(Order.TRAP);
    }

    private void GoTo(Vector2 position)
    {
        ClearPath();
        
        Pathfinding.RequestPath(transform.position, position, gameObject, (path, isSuccess) =>
        {
            if (!isSuccess) return;
            _path = path;
            _pathIndex = 0;

            foreach (var node in path)
            {
                Map.Current.GetTile(node).isInUse = true;
            }
        });
    }

    private void ClearPath()
    {
        // Clear inUse from all tiles on this ants path
        for (var i = _pathIndex; i < _path.Length; i++)
        {
            Map.Current.GetTile(_path[i]).isInUse = false;
        }

        _path = Array.Empty<Vector2>();
        _pathIndex = 0;
    }

    private void Awake()
    {
        antennaAnimationSeed = new[] {Random.value, Random.value};
        _movementSpeedSeed = new[] {Random.value, Random.value};
    }

    private void Start()
    {
        isCarryingBrood = Random.value < 0.05f;
        brood.SetActive(isCarryingBrood);

        if (isCarryingBrood)
        {
            GiveOrder(Order.NEST);
            return;
        }

        if (Random.value < 0.1f)
        {
            GiveOrder(Order.CAKE);
        }
        else
        {
            GiveOrder(Order.GATHER);
        }
    }

    private void Update()
    {
        if (health <= 0) Destroy(gameObject);
        
        var position = transform.position;

        var legAnimationSpeedWithVelocity = (_lastPosition - (Vector2)position).magnitude * legAnimationSpeed * 1000f;
        legAnimation += Time.deltaTime * legAnimationSpeedWithVelocity;

        // Animate left legs
        var index = 0f;
        foreach (var leg in LegsLeft)
        {
            leg.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(legAnimation + (index * legAnimationCascade)) * legAnimationScale);
            index++;
        }

        // Animate right legs
        index = 0.5f;
        foreach (var leg in LegsRight)
        {
            leg.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(legAnimation + (index * legAnimationCascade)) * legAnimationScale);
            index++;
        }

        // Animate antenna
        antennaAnimation += 0.01f + (Mathf.Sin(Time.time * antennaAnimationSpeed) * 0.01f);
        var antennaLeftNoise = Mathf.PerlinNoise(antennaAnimationSeed[0], antennaAnimationSeed[1] + antennaAnimation);
        var antennaRightNoise = Mathf.PerlinNoise(antennaAnimationSeed[0] + antennaAnimationCascade, antennaAnimationSeed[1] + antennaAnimation);
        antennaLeft.transform.localRotation = Quaternion.Euler(0, 0, (antennaLeftNoise * antennaAnimationScale) - 20f);
        antennaRight.transform.localRotation = Quaternion.Euler(0, 0, -((antennaRightNoise * antennaAnimationScale) - 20f));
        
        _lastPosition = position;
        
        // Move ant
        if (_path.Length > 0 && _pathIndex < _path.Length)
        {
            var node = _path[_pathIndex];
            var distance = Vector2.Distance(position, node);
            if (distance < 0.1f)
            {
                Map.Current.GetTile(node).isInUse = false;
                _pathIndex++;
            }
            else
            {
                // var speed = Mathf.Max(0.1f, movementSpeed * Math.Min(1f, distance));
                
                var target = new Vector3(node.x, node.y, position.z);
                var facing = Quaternion.LookRotation(Vector3.forward, target - position);
                
                var rotationSpeedWithDistance = rotationSpeed + ((rotationSpeed * 2f) - Mathf.Min((rotationSpeed * 2f), distance * rotationSpeed));
                var rotation = Quaternion.RotateTowards(transform.rotation, facing, rotationSpeedWithDistance * Time.deltaTime);

                var sidewaysMovement = 15f + (45f - Mathf.Min(45f, distance * 50f));
                _movementTarget = position + Quaternion.RotateTowards(transform.rotation, facing, sidewaysMovement) * new Vector3(0, 1, 0);

                transform.rotation = rotation;
                
                var movementSpeedStuttering = Mathf.PerlinNoise(_movementSpeedSeed[0] + position.x, _movementSpeedSeed[1] + position.y);
                var movementSpeedSin = Mathf.Abs((Mathf.Sin(_movementSpeedSeed[0] * 10f + Time.time * 2f)));
                var movementSpeedStutteringSin = Mathf.Min(1f, movementSpeedSin + movementSpeedStuttering);
                var isStuttering = movementSpeedStutteringSin < 0.8f;
                var movementSpeedWithStuttering = isStuttering ? 0 : movementSpeed;
                transform.position = Vector3.MoveTowards(position, _movementTarget, movementSpeedWithStuttering * Time.deltaTime);
            }
        }
        
        // Check gathering
        inGathering = _gathering != null && Vector2.Distance(position, _gathering.Value.Tile.position) < 5f;
        
        // Check trap
        if (_trap != null)
        {
            var distanceToTrap = Vector2.Distance(transform.position, (Vector2) _trap);

            transform.position = Vector3.MoveTowards(position, (Vector3)_trap, movementSpeed * Time.deltaTime);

            if (distanceToTrap < 0.5f)
            {
                Destroy(gameObject);
            }
        }

        // Hill
        if (isCarryingBrood && inGathering)
        {
            Instantiate(hill, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        
        // On fire
        fireParticles.SetActive(isOnFire);
        if (isOnFire)
        {
            _checkFireSpreadCooldown--;
            if (_checkFireSpreadCooldown <= 0)
            {
                _checkFireSpreadCooldown = CheckFireSpreadTimeout;

                if (isOnFire && Random.value < 0.001f) SpreadFire();
            }
        }
        
        if (isOnFire) health -= Time.deltaTime * 0.2f;
        
        // Cake
        if (order == Order.CAKE && _pathIndex == _path.Length - 1 &&
            Vector2.Distance(position, AI.Current.cake.position) < 3f)
        {
            Destroy(gameObject);
        }
    }

    private void SpreadFire()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Ant ant))
            {
                if (ant == this) continue;
                ant.isOnFire = true;
                return;
            }
        }
    }

    private void OnDestroy()
    {
        ClearPath();
        _gathering?.Ants.Remove(this);
    }

    private void OnDrawGizmos()
    {
        if (_path == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, _movementTarget);
        
        Gizmos.color = Color.green;

        Vector2 previousNode = transform.position;
        for (var i = _pathIndex; i < _path.Length; i++)
        {
            var node = _path[i];
            Gizmos.DrawWireSphere(node, Map.Current.tileSize / 2f);
            Gizmos.DrawLine(node, (Vector2)previousNode);
            previousNode = node;
        }
    }
}