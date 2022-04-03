using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public struct Gathering
{
    public Tile Tile;
    public HashSet<Ant> Ants;
}
public class AI : MonoBehaviour
{
    public static AI Current;
    
    public Transform cake;
    public Collider2D gatherArea;
    
    private List<Gathering> gatherings = new List<Gathering>();

    private int _maxGatheringAnts = 10;

    private void Awake()
    {
        Current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        var bounds = gatherArea.bounds;
        for (var i = 0; i < 20; i++)
        {
            CreateGathering();
        }
    }

    // Update is called once per frame
    void Update()
    {
        var toBeRemoved = new HashSet<Gathering>();
        
        foreach (var gathering in gatherings.Where(gathering =>
                     gathering.Ants.Count >= _maxGatheringAnts && gathering.Ants.All(ant => ant.inGathering)))
        {
            Pathfinding.RequestPath(gathering.Tile.position, cake.position, gameObject, (path, isSuccess) =>
            {
                if (!isSuccess) return;
                foreach (var ant in gathering.Ants)
                {
                    ant.GiveOrder(Order.CAKE, path);
                }
            });

            toBeRemoved.Add(gathering);
        }

        foreach (var gathering in toBeRemoved)
        {
            gatherings.Remove(gathering);
            if (gatherings.Count < 10) CreateGathering();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var gathering in gatherings)
        {
            Gizmos.DrawWireSphere(gathering.Tile.position, 5f);
        }
    }

    private Gathering CreateGathering()
    {
        var bounds = gatherArea.bounds;
        var gathering = new Gathering
        {
            Tile = Map.Current.GetTile(new Vector2(Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y))),
            Ants = new HashSet<Ant>(),
        };

        gatherings.Add(gathering);

        return gathering;
    }

    public Gathering? FindGathering(Ant ant)
    {
        var gatheringsSortedByDistance =
            gatherings.OrderBy(item => Vector2.Distance(ant.transform.position, item.Tile.position));

        foreach (var gathering in gatheringsSortedByDistance)
        {
            if (gathering.Ants.Count >= _maxGatheringAnts) continue;
            return gathering;
        }

        if (gatherings.Count > 20)
        {
            ant.GiveOrder(Order.CAKE);
            return null;
        }

        return CreateGathering();
    }
}
