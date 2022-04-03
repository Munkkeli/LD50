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

    public int Difficulty
    {
        get
        {
            if (Controller.Current.score < 80) return 1;
            if (Controller.Current.score < 300) return 2;
            return 3;
        }
    }

    public Transform cake;
    public Collider2D gatherArea;

    public Collider2D[] nestAreasEasy;
    public Collider2D[] nestAreasMedium;
    public Collider2D[] nestAreasHard;

    private Collider2D[][] _nestAreasByDifficulty;
    private Dictionary<int, int> _nestsInAreaByDifficulty = new Dictionary<int, int>() { { 1, 0 }, { 2, 0 }, { 3, 0 } };

    private List<Gathering> gatherings = new List<Gathering>();

    private int _maxGatheringAnts = 10;

    private void Awake()
    {
        Current = this;

        _nestAreasByDifficulty = new[]
        {
            nestAreasEasy,
            nestAreasMedium,
            nestAreasHard,
        };
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

    public (Vector2?, int) RandomPointInNestArea()
    {
        var difficulties = new int[] { 1, 2, 3 };
        var difficulty = difficulties[Random.Range(0, Difficulty - 1)];

        while (_nestsInAreaByDifficulty[difficulty] > 10 - ((difficulty - 1) * 2))
        {
            if (difficulty >= 3) return (null, difficulty);
            if (difficulty >= Difficulty) return (null, difficulty);
            difficulty++;
        }
        
        Debug.Log($"Found nest spot in area {difficulty}");
        
        var nestAreas = _nestAreasByDifficulty[difficulty - 1];
        var area = nestAreas[Random.Range(0, nestAreas.Length)];
        var bounds = area.bounds;
        var position = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));
        return (position, difficulty);
    }
    
    public bool CheckIfNestCanBePlaced(int difficulty)
    {
        var isAllowed = _nestsInAreaByDifficulty[difficulty] <= 10;
        Debug.Log($"Is allowed to place nest in {difficulty}? {isAllowed}");
        return isAllowed;
    }

    public void InformAboutPlacedNest(int difficulty)
    {
        _nestsInAreaByDifficulty[difficulty]++;
    }
    
    public void InformAboutDestroyedNest(int difficulty)
    {
        _nestsInAreaByDifficulty[difficulty]--;
    }
}
