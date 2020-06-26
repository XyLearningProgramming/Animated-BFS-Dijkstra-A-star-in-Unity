using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
public class SnapToUnitScript : MonoBehaviour
{
    TextMesh textMesh;
    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponentInChildren<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tarPos = transform.position;
        tarPos.x = Mathf.RoundToInt(tarPos.x);
        tarPos.z = Mathf.RoundToInt(tarPos.z);
        tarPos.y = transform.position.y;
        transform.position = tarPos;

        // mark position
        string cubeLabel = tarPos.x.ToString() + ',' + tarPos.z.ToString();
        textMesh.text = cubeLabel;
        gameObject.name = cubeLabel;
    }
}
