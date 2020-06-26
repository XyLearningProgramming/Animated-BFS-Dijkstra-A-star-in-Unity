using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool useProportion = false;
        public float proportion;
    }
    public List<Pool> pools;
    public Dictionary<string,Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    #region Singleton
    public static ObjectPooler instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // find how many waypoints in current scene
        int totalNumCell = FindObjectsOfType<WayPoint>().Length;

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            if (pool.useProportion)
            {
                pool.size = Mathf.RoundToInt(totalNumCell * pool.proportion);
            }
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab,transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDict.Add(pool.tag,objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(tag)) return null;
        GameObject obj = poolDict[tag].Peek();

        if (obj!=null && !obj.activeSelf)
        {
            obj = poolDict[tag].Dequeue();
            obj.SetActive(true);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        else 
        {
            // not enough in pool then 
            obj = Instantiate(poolDict[tag].Peek(),position,rotation, transform);
        }

        return obj;
    }

    public void ReinsertToPool(string tag, GameObject obj)
    {
        if (!poolDict.ContainsKey(tag)) return;
        obj.SetActive(false);
        poolDict[tag].Enqueue(obj);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
