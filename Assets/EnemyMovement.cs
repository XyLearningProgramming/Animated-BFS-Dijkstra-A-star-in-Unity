using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] List<SnapToUnitScript> path;
    [SerializeField] float moveGap = 1f;
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(FollowPresetPath());
    }

    IEnumerator FollowPresetPath()
    {
        Vector3 offset = transform.position - Vector3.zero;
        foreach (SnapToUnitScript point in path)
        {
            transform.position = point.transform.position + offset;
            yield return new WaitForSeconds(moveGap);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MovingAlongPath(List<WayPoint> path)
    {
        StartCoroutine(FollowPath(path, moveGap));
    }
    public void MovingAlongPath(List<WayPoint> path, float moveGap_)
    {
        StartCoroutine(FollowPath(path, moveGap_));
    }

    IEnumerator FollowPath(List<WayPoint> path, float moveGap_)
    {
        Vector3 offset = transform.position - Vector3.zero;
        foreach (WayPoint point in path)
        {
            transform.position = point.GetPostionVec() + offset;
            yield return new WaitForSeconds(moveGap_);
        }
    }
}
