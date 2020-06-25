using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walkable : MonoBehaviour
{
    public bool layerChange = false;
    public List<WalkPath> possiblePaths = new List<WalkPath>();
    [Space]

    public Transform previousBlock;

    [Space]

    [Header("Offsets")]
    public float walkPointOffset = .5f;

    private PlayerController playerController;

    private void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (playerController.walking == false)
        {
            this.previousBlock = null;
        }
    }
    
    public Vector3 GetWalkPoint()
    {
        return transform.position + transform.up * walkPointOffset;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(GetWalkPoint(), .1f);

        if (possiblePaths == null)
            return;

        foreach (WalkPath p in possiblePaths)
        {
            if (p.target == null)
                return;
            Gizmos.color = Color.black;
            Gizmos.DrawLine(GetWalkPoint(), p.target.GetComponent<Walkable>().GetWalkPoint());
        }
    }
}

[System.Serializable]
public class WalkPath
{
    public Transform target;

    public int layerNum = -1;
}
