using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public bool walking = false;

    [Space]

    //public Transform currentCube;
    //public Transform currentSide;
    public List<Transform> currentCubes = new List<Transform>();
    public List<Transform> currentSides = new List<Transform>();
    public Transform clickedCube;

    [Space]

    public List<FinalPath> finalPathList = new List<FinalPath>();

    void Start()
    {
        RayCastDown();
    }

    void Update()
    {

        //当前位置射线检测
        if(walking==false)
            RayCastDown();

        //点击方块射线检测

        if (Input.GetMouseButtonDown(0) && walking == false)
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); RaycastHit mouseHit;
            bool move = false;
            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                Debug.Log(mouseHit.transform.name);
                clickedCube = mouseHit.transform;
                Transform[] father = clickedCube.GetComponentsInChildren<Transform>();

                foreach (Transform t in currentSides)
                {
                    Debug.Log("GG");
                    if (move == true)
                    {
                        //break;
                    }
                    foreach (var child in father)
                    {
                        if (child.name == t.name)
                        {
                            clickedCube = child;
                            DOTween.Kill(gameObject.transform);
                            finalPathList.Clear();
                            move = FindPath(t);
                        }
                    }
                }
            }
        }
    }

    bool FindPath(Transform currentside)
    {
        foreach (WalkPath path in currentside.GetComponent<Walkable>().possiblePaths)
        {
            List<Transform> nextCubes = new List<Transform>();
            List<Transform> pastCubes = new List<Transform>();
            finalPathList.Add(new FinalPath());
            nextCubes.Add(path.target);
            path.target.GetComponent<Walkable>().previousBlock = currentside;
            pastCubes.Add(currentside);
            ExploreCube(nextCubes, pastCubes);
            BuildPath(currentside);
        }
        return (FollowPath());
    }

    void ExploreCube(List<Transform> nextCubes, List<Transform> visitedCubes)
    {
        Transform current = nextCubes.First();
        nextCubes.Remove(current);

        if (current == clickedCube)
        {

            return;
        }

        foreach (WalkPath path in current.GetComponent<Walkable>().possiblePaths)
        {
            if (!visitedCubes.Contains(path.target))
            {
                nextCubes.Add(path.target);
                path.target.GetComponent<Walkable>().previousBlock = current;
            }
        }

        visitedCubes.Add(current);

        if (nextCubes.Any())
        {
            ExploreCube(nextCubes, visitedCubes);
        }
    }

    void BuildPath(Transform currentSide)
    {
        Transform cube = clickedCube;
        while (cube != currentSide)
        {
            finalPathList[finalPathList.Count - 1].finalPath.Add(cube);
            if (cube.GetComponent<Walkable>().previousBlock != null)
                cube = cube.GetComponent<Walkable>().previousBlock;
            else
                return;
        }
    }

    bool FollowPath()
    {
        List<Transform> finalPathx = new List<Transform>();
        List<FinalPath> trueFinalPath = new List<FinalPath>();

        foreach (FinalPath f in finalPathList)//去除错误路径
        {
            if (f.finalPath.Count == 1)
            {
                Debug.Log(f.finalPath[0].position);
                Debug.Log(this.transform.position);
                Debug.Log((f.finalPath[0].GetComponent<Walkable>().GetWalkPoint() - this.transform.position).sqrMagnitude);

                if (Vector3.Distance(f.finalPath[0].GetComponent<Walkable>().GetWalkPoint(), this.transform.position) <= 1)
                {
                    finalPathx = f.finalPath;
                    trueFinalPath.Add(f);
                }
            }
            else
            {
                finalPathx = f.finalPath;
                trueFinalPath.Add(f);
            }
        }
        foreach (FinalPath f in trueFinalPath)//找最短路径
        {
            if (finalPathx.Count >= f.finalPath.Count)
            {
                foreach (Transform x in f.finalPath)
                {
                    if (x.GetComponent<Walkable>().possiblePaths.Count == 0)
                    {

                    }
                    else
                    {
                        finalPathx = f.finalPath;
                    }
                }
            }
        }
        foreach (Transform x in finalPathx)
        {
            if (x.GetComponent<Walkable>().possiblePaths.Count == 0)
            {
                finalPathx.Clear();
                return (false);
            }
            Debug.Log(x.transform.parent.name + "  " + x.name + "  " + "finalx");
        }
        finalPathx.Insert(0, clickedCube);
        Sequence s = DOTween.Sequence();
        for (int i = finalPathx.Count - 1; i > 0; i--)
        {
            float time = 1;
            walking = true;
            s.Append(transform.DOMove(finalPathx[i].GetComponent<Walkable>().GetWalkPoint(), .2f * time).SetEase(Ease.Linear));
            currentCubes.Clear();
            currentSides.Clear();
        }

        s.AppendCallback(() => Clear());
        return (walking);
    }

    void Clear()
    {
        finalPathList.Clear();
        RayCastDown();
        currentCubes.Clear();
        currentSides.Clear();
        walking = false;
    }

    public void RayCastDown()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 dir = new Vector3(0, 0, 0);
            if (i < 4)
            {
                dir = new Vector3(Mathf.Sin(i * 90 * Mathf.Deg2Rad), Mathf.Cos(i * 90 * Mathf.Deg2Rad), 0);
            }
            else
            {
                dir = new Vector3(0, 0, Mathf.Sin((180 * i - 630) * Mathf.Deg2Rad));
            }
            Ray playerRay = new Ray(this.transform.position, dir.normalized);
            RaycastHit playerHit;
            if (Physics.Raycast(playerRay, out playerHit, 1f))
            {
                Debug.Log(playerHit.transform.name);
                if (!currentCubes.Contains(playerHit.transform))
                {
                        currentCubes.Add(playerHit.transform);

                    Transform[] father = playerHit.transform.GetComponentsInChildren<Transform>();
                    if (playerHit.normal != new Vector3(0, 0, 0))
                    {
                        foreach (var child in father)
                        {
                            if (child.name == playerHit.normal.x.ToString() + playerHit.normal.y.ToString() + playerHit.normal.z.ToString())
                            {
                                currentSides.Add(child);

                                if (child.GetComponent<Walkable>().layerChange == true)
                                {
                                    foreach (WalkPath walkPath in child.GetComponent<Walkable>().possiblePaths)
                                    {
                                        if (walkPath.target != child.GetComponent<Walkable>().previousBlock)
                                        {
                                            StartCoroutine(ChangeLayer(0.1f, walkPath.layerNum));
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < 6; i++)
        {
            if (i < 4)
            {
                Vector3 dir = new Vector3(Mathf.Sin(i * 90 * Mathf.Deg2Rad), Mathf.Cos(i * 90 * Mathf.Deg2Rad), 0);
                Ray ray = new Ray(this.transform.position, dir.normalized);
                Gizmos.DrawRay(ray);
            }
            else
            {
                Vector3 dir = new Vector3(0, 0, Mathf.Sin((180 * i - 630) * Mathf.Deg2Rad));
                Ray ray = new Ray(this.transform.position, dir.normalized);
                Gizmos.DrawRay(ray);
            }
        }
    }

    IEnumerator ChangeLayer(float t, int layerNum)
    {
        yield return new WaitForSeconds(t);//运行到这，暂停t秒

        Transform[] c = GetComponentsInChildren<Transform>();
            this.gameObject.layer = layerNum;
    }
}

[System.Serializable]
public class FinalPath
{
    public List<Transform> finalPath = new List<Transform>();
}