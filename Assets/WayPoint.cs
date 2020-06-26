using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    Vector3 positionVec;

    [SerializeField] Color defColor;
    [SerializeField] public WayPoint lastWayPoint { get; set; } = null;
    [SerializeField] public WayPoint nextWayPoint { get; set; } = null;

    [Header("Algorithm")]
    [SerializeField] bool isCostRandom = false;
    [SerializeField] [Range(1f, 5f)] float cost = 1f;
    [SerializeField] public bool isWall = false;
    [SerializeField] public int h { get; set; } = int.MaxValue;
    [SerializeField] public int f { get; set; } = int.MaxValue;
    //[SerializeField] public int g { get { return Mathf.FloorToInt(cost); } set { cost = value; } } // property
    [SerializeField] public int g { get; set; } = int.MaxValue;
    enum State {Uncharted,Exploring,Explored};
    State state = State.Uncharted;
    GameObject currPrefabOnTop = null;
    // to use pool
    Dictionary<State, string> stateToStringDict = new Dictionary<State, string>() 
    { 
         [State.Uncharted] = "UnexPrefab",
         [State.Explored]= "ExploredPrefab",
         [State.Exploring] = "ExploreFrontPrefab"
    };

    // load prefab from resources 
    public const string unExPrefab = "UnexPrefab";
    public const string exploredPrefab = "ExploredPrefab";
    public const string exploringPrefab = "ExploreFrontPrefab";
    public const string trailLeftRight = "TrailRight";
    public const string trailUpDown = "TrailUp";
    public const string trailLeftDown = "TrailLeftDown";
    public const string trailLeftUp = "TrailLeftUp";
    public const string trailUpRight = "TrailUpRight";
    public const string trailDownRight = "TrailDownRight";

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        // get position
        Vector3 tarPos = transform.position;
        tarPos.x = Mathf.RoundToInt(tarPos.x);
        tarPos.z = Mathf.RoundToInt(tarPos.z);
        if (isCostRandom)
        {
            cost = UnityEngine.Random.Range(1, 5 + 1); // randomly generate cost
        }
        // set y based on cost
        if (!isWall)
        {
            tarPos.y = (cost - 1f) * 0.2f;
            Vector3 tmpscale = Vector3.one;
            tmpscale.y = 1f + tarPos.y;
            transform.localScale = tmpscale;
            positionVec = tarPos;
            transform.position = positionVec;
            SetTopColor(defColor);
            currPrefabOnTop = ObjectPooler.instance.SpawnFromPool(stateToStringDict[state], positionVec, Quaternion.identity);
        }
        else
        {
            // wall
            tarPos.y = 1f;
            Vector3 tmpscale = Vector3.one;
            tmpscale.y = 2f;
            transform.localScale = tmpscale;
            positionVec = tarPos;
            transform.position = positionVec;
            SetTopColor(Color.gray);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetCost()
    {
        return Mathf.FloorToInt(cost);
    }

    public bool ifIsWall()
    {
        return isWall;
    }

    public Vector3 GetPostionVec()
    {
        return positionVec;
    }

    public void SetTopColor(Color tar)
    {
        transform.Find("Up").GetComponent<MeshRenderer>().material.color = tar;
    }

    public void SetExplored(Color color)
    {
        if (currPrefabOnTop != null) ObjectPooler.instance.ReinsertToPool(stateToStringDict[state], currPrefabOnTop);
        state = State.Explored;
        SetTopColor(color);
        currPrefabOnTop = ObjectPooler.instance.SpawnFromPool(stateToStringDict[state], positionVec, Quaternion.identity);
    }

    public void SetExploring(Color color)
    {
        if (state != State.Exploring)
        {
            if (currPrefabOnTop != null) ObjectPooler.instance.ReinsertToPool(stateToStringDict[state], currPrefabOnTop);
            state = State.Exploring;
            SetTopColor(color);
            currPrefabOnTop = ObjectPooler.instance.SpawnFromPool(stateToStringDict[state], positionVec, Quaternion.identity);
        }
    }

    public bool ReturnIfExplored()
    {
        return (state==State.Explored);
    }

    public bool ReturnIfExploring()
    {
        return (state == State.Exploring);
    }

    public void PutTrail()
    {
        Vector2Int nextVec;
        if (nextWayPoint == null)
        {
            nextVec = Vector2Int.right; // last trail facing right
        }
        else
        { 
            nextVec = ConvertVec3Position( nextWayPoint.positionVec - positionVec);
        }
        Vector2Int beforeVec;

        if (lastWayPoint == null)
        {
            beforeVec = Vector2Int.right; // first trail facing left
        }
        else
        { 
            beforeVec = ConvertVec3Position(positionVec - lastWayPoint.positionVec);
        }
        //Debug.Log("NextVec"+ nextVec.x.ToString()+nextVec.y.ToString());
        //Debug.Log("beforeVec" + beforeVec.x.ToString() + beforeVec.y.ToString());
        if (nextVec.x == 0 && beforeVec.x == 0)
        {
            // right straight
            if (currPrefabOnTop != null) Destroy(currPrefabOnTop);
            currPrefabOnTop = (GameObject)Instantiate(Resources.Load(trailUpDown), positionVec, Quaternion.identity, transform);
        }
        else if (nextVec.y == 0 && beforeVec.y == 0)
        {
            if (currPrefabOnTop != null) Destroy(currPrefabOnTop);
            currPrefabOnTop = (GameObject)Instantiate(Resources.Load(trailLeftRight), positionVec, Quaternion.identity, transform);
        }
        else if ((beforeVec == Vector2Int.up && nextVec == Vector2Int.left) 
            || (beforeVec == Vector2Int.right && nextVec == Vector2Int.down))
        {
            if (currPrefabOnTop != null) Destroy(currPrefabOnTop);
            currPrefabOnTop = (GameObject)Instantiate(Resources.Load(trailUpRight), positionVec, Quaternion.identity, transform);
        }
        else if ((beforeVec == Vector2Int.right && nextVec == Vector2Int.up)
            || (beforeVec == Vector2Int.left && nextVec == Vector2Int.down))
        {
            if (currPrefabOnTop != null) Destroy(currPrefabOnTop);
            currPrefabOnTop = (GameObject)Instantiate(Resources.Load(trailLeftUp), positionVec, Quaternion.identity, transform);
        }
        else if ((beforeVec == Vector2Int.up && nextVec == Vector2Int.right)
            || (beforeVec == Vector2Int.down && nextVec == Vector2Int.left))
        {
            if (currPrefabOnTop != null) Destroy(currPrefabOnTop);
            currPrefabOnTop = (GameObject)Instantiate(Resources.Load(trailDownRight), positionVec, Quaternion.identity, transform);
        }
        else 
        {
            if (currPrefabOnTop != null) Destroy(currPrefabOnTop);
            currPrefabOnTop = (GameObject)Instantiate(Resources.Load(trailLeftDown), positionVec, Quaternion.identity, transform);
        }
    }

    public Vector2Int ConvertVec3Position(Vector3 v)
    {
        return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
    }

}
