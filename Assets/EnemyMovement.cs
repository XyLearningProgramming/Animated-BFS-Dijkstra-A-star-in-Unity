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
        Vector3 offset = Vector3.zero;
        offset.y =  transform.position.y;
        foreach (WayPoint point in path)
        {
            Vector3 originalPos = transform.position;
            Vector3 destPos = point.GetPostionVec();
            transform.position = destPos + offset;

            //Debug.Log("Dest"+destPos);
            //Debug.Log("Ori" + originalPos);

            if (destPos.x - originalPos.x < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else if (destPos.z - originalPos.z < 0)
            {
                //Debug.Log("Trying to rotate");
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else if (destPos.z - originalPos.z > 0)
            {
                //Debug.Log("Trying to rotate");
                transform.eulerAngles = new Vector3(0, -90, 0);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }


            yield return new WaitForSeconds(moveGap_);
        }
    }
}
