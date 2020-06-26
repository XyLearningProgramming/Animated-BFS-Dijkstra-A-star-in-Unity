using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEditorInternal;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] GameObject boardForSearch;
    [SerializeField] WayPoint startWayPoint, endWayPoint;
    [SerializeField] Color startPointColor;
    [SerializeField] Color endPointColor;
    [SerializeField] Color exploringFrontColor = Color.gray;
    [SerializeField] Color exploredColor;
    [SerializeField] Color pathColor;

    [Header("TimeScale")]
    [SerializeField] public bool executePerFrame = false;
    [SerializeField] public float stepPerSecond = 0.5f;

    enum Algorithm {BFS, Dijk, Astar }
    [SerializeField] Algorithm algo = Algorithm.BFS;

    [SerializeField] bool executeWaitForKeyDown = false;
    [SerializeField][Tooltip("See Unity Keycode Enum to confirm changes")] string keyEnum = "Space";

    [Header("VFX")]
    [SerializeField] bool turnOnPlasmaEffect = true;
    [SerializeField] float plasmaEffectExistingTime = 2f;
    [SerializeField] GameObject whiteBG;

    public const string endGate = "EndGate"; // call prefab
    public const string startGate = "StartGate";
    public const string resultDisplayObjName = "ResultDisplay";

    // measure time
    [Header("Timing")]
    public bool isTiming = true;
    public float timeElapsed;

    // result display
    TextMeshProUGUI resultDisplay;

    Dictionary<Vector2Int, WayPoint> grid = new Dictionary<Vector2Int, WayPoint>();
    Vector2Int[] directions =
    {
        Vector2Int.up, // in a 2d sense up
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
    };
    Queue<WayPoint> queue = new Queue<WayPoint>();

    public List<WayPoint> path { get; set; } = new List<WayPoint>();

    // Start is called before the first frame update
    void Start()
    {
        // set color for start and end point
        startWayPoint.SetTopColor(startPointColor);
        endWayPoint.SetTopColor(endPointColor);
        // instantiate gates
        Instantiate(Resources.Load(startGate), startWayPoint.transform.position, Quaternion.identity, startWayPoint.transform);
        Instantiate(Resources.Load(endGate), endWayPoint.transform.position, Quaternion.identity, endWayPoint.transform);

        LoadBlocksInDict();

        if (algo==Algorithm.BFS)
            StartCoroutine(BFS());
        else if(algo==Algorithm.Dijk)
            StartCoroutine(Dijkstra());
        else if (algo==Algorithm.Astar)
            StartCoroutine(AStar());

        // cache result display component
        resultDisplay = GameObject.Find(resultDisplayObjName).GetComponent<TextMeshProUGUI>();
        if (!resultDisplay)
            Debug.Log("No where to show result on screen!");

        resultDisplay.text = "";

        if (!whiteBG)
        {
            Debug.Log("Find ResultUI and link the image!");
        }
    }

    IEnumerator BFS()
    {
        WayPoint curWayPoint = startWayPoint;
        queue.Enqueue(curWayPoint);
        // explore neighbors
        while (queue.Count > 0)
        {
            curWayPoint = queue.Dequeue();
            curWayPoint.SetExplored(exploredColor);

            //VFX
            if (turnOnPlasmaEffect)
            {
                CallLighteningEffect(curWayPoint);
            }

            foreach (Vector2Int direction in directions)
            {
                Vector2Int possibleStep = ConvertVec3Position(curWayPoint.GetPostionVec()) + direction;
                if (grid.ContainsKey(possibleStep) && !grid[possibleStep].ReturnIfExplored() && !grid[possibleStep].ifIsWall())
                {
                    WayPoint exploring = grid[possibleStep];

                    // block is accessible then add to queue
                    queue.Enqueue(exploring);
                    // live track of path
                    exploring.lastWayPoint = curWayPoint;

                    // if is the destination then stop doing things
                    if (endWayPoint == exploring)
                    {
                        exploring.SetExplored(exploredColor);
                        AfterFindTheDestination(exploring);

                        yield break;
                    }
                    else 
                    {
                        // aesthetic purpose
                        exploring.SetExploring(exploringFrontColor);
                    }
                }
            }
            // wait for input 
            //Debug.Log(queue.Count);
            if (executePerFrame)
                yield return new WaitForEndOfFrame();
            else if (executeWaitForKeyDown)
            {
                KeyCode tryParse;
                try
                {
                    tryParse = (KeyCode)Enum.Parse(typeof(KeyCode), keyEnum);
                }
                catch
                {
                    Debug.Log("Key Not Found. See Unity Manual. Space is used");
                    tryParse = KeyCode.Space;
                }
                yield return new WaitUntil(() => Input.GetKeyDown(tryParse));
            }
            else
                yield return new WaitForSeconds(stepPerSecond);
        }
        // the destination is inaccessable
        resultDisplay.text += "Not reachable!\n";
        Debug.Log("Not reachable!");
        yield break;
    }

    /// <summary>
    /// distance from src to specific waypoint
    /// </summary>
    Dictionary<WayPoint, int> distanceDict = new Dictionary<WayPoint, int>();

    IEnumerator Dijkstra()
    {
        WayPoint exploring;
        // initialize distance
        DijkInitDistanceAndTracking(startWayPoint);


        // find shortest path for all vertices
        for (int cnt=0;cnt<grid.Count;cnt++)
        {
            //pick a neighbor with closest distance that isn't explored
            exploring = PickClosestNotExplored();

            if (exploring == null)
            {
                // wall is blocking the map
                resultDisplay.text += "Not reachable!\n";
                Debug.Log("Not reachable!");
                yield break;
            }

            // add to explored List
            exploring.SetExplored(exploredColor);

            //VFX
            if (turnOnPlasmaEffect)
            {
                CallLighteningEffect(exploring);
            }

            foreach (Vector2Int direction in directions)
            {
                Vector2Int possibleStep = ConvertVec3Position(exploring.GetPostionVec()) + direction;
                if (grid.ContainsKey(possibleStep)
                    && !grid[possibleStep].ReturnIfExplored()
                    && !grid[possibleStep].ifIsWall())
                {
                    WayPoint checkingWayPoint = grid[possibleStep];

                    if (checkingWayPoint == endWayPoint)
                    {
                        exploring.SetExplored(exploredColor);
                        endWayPoint.lastWayPoint = exploring;
                        AfterFindTheDestination(endWayPoint);

                        yield break;
                    }

                    if (distanceDict[checkingWayPoint] >= distanceDict[exploring] + checkingWayPoint.GetCost()) // need to change if edges have different cost
                    {
                        checkingWayPoint.SetExploring(exploringFrontColor);
                        distanceDict[checkingWayPoint] = distanceDict[exploring] + checkingWayPoint.GetCost();
                        // build link
                        checkingWayPoint.lastWayPoint = exploring;
                    }
                }
            }
            if (executePerFrame)
                yield return new WaitForEndOfFrame();
            else if (executeWaitForKeyDown)
            {
                KeyCode tryParse;
                try
                {
                    tryParse = (KeyCode)Enum.Parse(typeof(KeyCode), keyEnum);
                }
                catch
                {
                    Debug.Log("Key Not Found. See Unity Manual. Space is used");
                    tryParse = KeyCode.Space;
                }
                yield return new WaitUntil(() => Input.GetKeyDown(tryParse));
            }
            else
                yield return new WaitForSeconds(stepPerSecond);
                    
                    
        }
        // the destination is inaccessable
        resultDisplay.text += "Not reachable!\n";
        Debug.Log("Not reachable!");
        yield break;
    }

    private void DijkInitDistanceAndTracking(WayPoint src)
    {
        foreach (KeyValuePair<Vector2Int, WayPoint> entry in grid)
        {
            if (entry.Value == src)
            {
                distanceDict.Add(src, 0);
            }
            else 
            {
                distanceDict.Add(entry.Value, int.MaxValue);
            }
        }
    }

    private WayPoint PickClosestNotExplored()
    {
        int min = int.MaxValue;
        WayPoint nearestWP = null;
        foreach (KeyValuePair<WayPoint, int> entry in distanceDict)
        {
            if (!entry.Key.ReturnIfExplored() && entry.Value < min)
            {
                min = entry.Value;
                nearestWP = entry.Key;
            }
        }
        return nearestWP;
    }

    //HashSet<WayPoint> openList = new HashSet<WayPoint>();
    //HashSet<WayPoint> closeList = new HashSet<WayPoint>();
    IEnumerator AStar()
    { 
        // initialize two lists to save some time with complexity O(1)
        // put the start in openList and start loop

        startWayPoint.f = 0;
        startWayPoint.g = 0;
        startWayPoint.h = 0;
        startWayPoint.SetExploring(exploringFrontColor);

        for(int cnt=0;cnt<grid.Count;cnt++)
        {
            WayPoint origin = FindLeastfInOpenList();

            if (origin == null)
            {
                // wall is blocking the map
                //Debug.Log("Not reachable!" + cnt.ToString()+startWayPoint.f);
                yield break;
            }

            origin.SetExplored(exploredColor); // remove from open list add to explored
                                               //VFX
            if (turnOnPlasmaEffect)
            {
                CallLighteningEffect(origin);
            }

            // explore neighbors
            foreach (Vector2Int direction in directions)
            {
                Vector2Int possibleStep = ConvertVec3Position(origin.GetPostionVec()) + direction;
                if (grid.ContainsKey(possibleStep)
                    && !grid[possibleStep].ifIsWall())
                {
                    WayPoint exploring = grid[possibleStep];
                    if (exploring == endWayPoint)
                    {
                        exploring.lastWayPoint = origin;
                        exploring.SetExplored(exploredColor);
                        AfterFindTheDestination(endWayPoint);
                        yield break;
                    }
                    // if not explored
                    else if (!exploring.ReturnIfExplored())
                    {
                        int gNew = origin.g + exploring.GetCost();
                        int hNew = CalculateHValue(exploring, endWayPoint);
                        int fNew = gNew + hNew;

                        // if not exploring then add it ; if is exploring then check whether to update status
                        if (!exploring.ReturnIfExploring() ||
                            exploring.f > fNew)
                        {
                            exploring.SetExploring(exploringFrontColor);

                            // update all values and make link
                            exploring.f = fNew;
                            exploring.g = gNew;
                            exploring.h = hNew;
                            exploring.lastWayPoint = origin;
                        }
                    }
                }
            }
            if (executePerFrame)
                yield return new WaitForEndOfFrame();
            else if (executeWaitForKeyDown)
            {
                KeyCode tryParse;
                try
                {
                    tryParse = (KeyCode)Enum.Parse(typeof(KeyCode), keyEnum);
                }
                catch
                {
                    Debug.Log("Key Not Found. See Unity Manual. Space is used");
                    tryParse = KeyCode.Space;
                }
                    yield return new WaitUntil(() => Input.GetKeyDown(tryParse));
            }
            else
                yield return new WaitForSeconds(stepPerSecond);
        }

        // no exit in the map
        resultDisplay.text = "Not reachable!\n";
        Debug.Log("Not reachable!");
        yield break;
    }

    private int CalculateHValue(WayPoint exploring, WayPoint endWayPoint)
    {
        // mahattan distance as heuristic
        Vector2Int v1 = ConvertVec3Position(exploring.GetPostionVec());
        Vector2Int v2 = ConvertVec3Position(endWayPoint.GetPostionVec());
        int xDiff = Mathf.RoundToInt(v1.x - v2.x);
        int yDiff = Mathf.RoundToInt(v1.y - v2.y);
        return Mathf.Abs(xDiff) + Mathf.Abs(yDiff);
    }

    WayPoint FindLeastfInOpenList()
    {
        int minf = int.MaxValue;
        WayPoint res = null;
        foreach (KeyValuePair<Vector2Int,WayPoint> entry in grid)
        {
            if (entry.Value.ReturnIfExploring() && entry.Value.f < minf)
            { 
                minf = entry.Value.f;
                res = entry.Value;
            }
        }
        return res;
    }

    public const string plasmaPrefab = "LightingEffect";
    public void CallLighteningEffect(WayPoint dest)
    {
        GameObject obj= ObjectPooler.instance.SpawnFromPool(plasmaPrefab, dest.GetPostionVec(), Quaternion.identity);
        StartCoroutine(RecyclePlasma(obj));
    }

    IEnumerator RecyclePlasma(GameObject obj)
    {
        yield return new WaitForSeconds(plasmaEffectExistingTime);
        ObjectPooler.instance.ReinsertToPool(plasmaPrefab, obj);
    }

    private void AfterFindTheDestination(WayPoint exploring)
    {
        // Found it!
        isTiming = false;

        Debug.Log("Found it!");
        int totalCost = ConstructPathToList(exploring);
        Debug.Log("Cost: " + totalCost.ToString());

        // format output
        string tmp = "";
        tmp = "Algorithm Used: "+ algo.ToString() + "\n";
        tmp += "Cost: " + totalCost.ToString()+"\n";
        tmp+= "Time Elapsed: "+ FormatTime(timeElapsed)+"\n";

        if (whiteBG && !whiteBG.activeSelf)
            whiteBG.SetActive(true);

        resultDisplay.text += tmp;

        DecorateThePath();
        MoveAlongPath();
    }

    public string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time - 60 * minutes;
        int milliseconds = (int)(1000 * (time - minutes * 60 - seconds));
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    private void MoveAlongPath()
    {
        GetComponent<EnemyMovement>().MovingAlongPath(path);
    }

    private void DecorateThePath()
    {
        foreach (WayPoint wpOnPath in path)
        {
            wpOnPath.SetTopColor(pathColor);
            wpOnPath.PutTrail();
        }
    }

    private int ConstructPathToList(WayPoint lastWayPoint)
    {
        int costSum = 0;

        path.Add(lastWayPoint);
        costSum += lastWayPoint.GetCost();

        WayPoint prev = lastWayPoint.lastWayPoint;
        WayPoint tmp = lastWayPoint;

        while (prev != startWayPoint)
        {
            path.Add(prev);
            costSum += prev.GetCost();

            prev.nextWayPoint = tmp;
            tmp = prev;
            prev = prev.lastWayPoint;
        }
        path.Add(prev);
        costSum += prev.GetCost();

        prev.nextWayPoint = tmp;
        path.Reverse();

        return costSum;
    }

    public Vector2Int ConvertVec3Position(Vector3 v)
    {
        return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
    }

    private void LoadBlocksInDict()
    {
        foreach (WayPoint block in boardForSearch.GetComponentsInChildren<WayPoint>())
        {
            // detect overlapping
            Vector2Int gridCoord = ConvertVec3Position(block.GetPostionVec());
            bool isOverLapping = grid.ContainsKey(gridCoord);
            if (!isOverLapping)
            {
                grid.Add(gridCoord, block);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTiming)
            timeElapsed += Time.deltaTime;
    }
}
