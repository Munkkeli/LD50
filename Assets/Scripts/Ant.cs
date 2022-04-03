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
    public GameObject headBig;
    public GameObject brood;
    public GameObject fireParticles;
    public GameObject hill;
    
    public AudioClip[] crushSounds;

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

    public bool isBig = false;
    public bool isFromNest = false;
    public bool isCarryingBrood = false;
    public bool inGathering = false;
    public bool isOnFire = false;
    public bool isAtCake = false;
    
    private float legAnimation = 0;
    private float antennaAnimation = 0;
    private float[] antennaAnimationSeed;
    
    private float[] _movementSpeedSeed;

    private Vector2[] _path = Array.Empty<Vector2>();
    private int _pathIndex;

    private Gathering? _gathering;
    private (Vector2?, int) _nest;
    private Vector2? _trap = null;
    
    private Vector3 _movementTarget;
    private Vector2 _lastPosition;

    private const int CheckFireSpreadTimeout = 120;
    private int _checkFireSpreadCooldown;
    private float _onFireTimeout;
    
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

                if (_gathering != null)
                {
                    var position = _gathering.Value.Tile.position + Random.insideUnitCircle * 4f;
                    GoTo(position);
                }
                else
                {
                    GiveOrder(Order.CAKE);
                }
                break;
            case Order.NEST:
                _nest = AI.Current.RandomPointInNestArea();
                if (_nest.Item1 == null)
                {
                    GiveOrder(Order.CAKE);
                    break;
                }
                GoTo((Vector2)_nest.Item1);
                break;
            case Order.TRAP:
                ClearPath();
                Destroy(GetComponent<Collider2D>());
                break;
        }
    }

    public void CatchInTrap(Vector2 trap)
    {
        if (isBig) return;
        _trap = trap;
        GiveOrder(Order.TRAP);
    }

    public void Fire()
    {
        isOnFire = true;
        _onFireTimeout = 4f;
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
        isBig = Random.value * 100f < Mathf.Min(Controller.Current.score / 40f, 50f);
        headBig.SetActive(isBig);
        antennaLeft.gameObject.SetActive(!isBig);
        antennaRight.gameObject.SetActive(!isBig);
        if (isBig) health *= 3;
        
        isCarryingBrood = !isBig && Random.value < 0.1f;
        brood.SetActive(isCarryingBrood);

        if (isCarryingBrood)
        {
            GiveOrder(Order.NEST);
            return;
        }

        if (isFromNest || Random.value < 0.1f)
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
        if (View.Current.state != State.GAME) return;
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
        if (!isBig)
        {
            antennaAnimation += 0.01f + (Mathf.Sin(Time.time * antennaAnimationSpeed) * 0.01f);
            var antennaLeftNoise =
                Mathf.PerlinNoise(antennaAnimationSeed[0], antennaAnimationSeed[1] + antennaAnimation);
            var antennaRightNoise = Mathf.PerlinNoise(antennaAnimationSeed[0] + antennaAnimationCascade,
                antennaAnimationSeed[1] + antennaAnimation);
            antennaLeft.transform.localRotation =
                Quaternion.Euler(0, 0, (antennaLeftNoise * antennaAnimationScale) - 20f);
            antennaRight.transform.localRotation =
                Quaternion.Euler(0, 0, -((antennaRightNoise * antennaAnimationScale) - 20f));
        }

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

        // Nest
        if (isCarryingBrood && _nest.Item1 != null && Vector2.Distance(position, (Vector2)_nest.Item1) < 2f)
        {
            var isPlaceable = AI.Current.CheckIfNestCanBePlaced(_nest.Item2);
            if (!isPlaceable)
            {
                GiveOrder(Order.CAKE);
            }
            else
            {
                AI.Current.InformAboutPlacedNest(_nest.Item2);
                Instantiate(hill, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
        
        // On fire
        fireParticles.SetActive(isOnFire);
        if (isOnFire) _onFireTimeout -= Time.deltaTime;
        if (_onFireTimeout <= 0f) isOnFire = false;
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
            isAtCake = true;
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
        if (View.Current.state != State.GAME) return;
        
        ClearPath();
        _gathering?.Ants.Remove(this);

        if (isAtCake)
        {
            Controller.Current.cakeHealth -= 1;
            return;
        }

        Controller.Current.score += 1;

        var crushSound = crushSounds[Random.Range(0, crushSounds.Length)];
        AudioSource.PlayClipAtPoint(crushSound, transform.position, Random.Range(2f, 6f));

        var scoreParams = new ParticleSystem.EmitParams
        {
            position = transform.position + new Vector3(0, 0, -5),
        };
        
        Map.Current.scoreEffect.Emit(scoreParams, 1);

        if (_trap.HasValue) return;

        var emitParams = new ParticleSystem.EmitParams
        {
            position = transform.position,
            startLifetime = 200,
            rotation = Random.Range(0f, 360f),
            startSize = 1.2f
        };

        if (isOnFire)
        {
            Map.Current.antBurnedEffect.Emit(emitParams, 1);
            return;
        }

        Map.Current.antSplatterEffect.Emit(emitParams, 1);
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
