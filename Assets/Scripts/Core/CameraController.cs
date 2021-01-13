using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform target;
    [SerializeField] Tilemap map;
    [SerializeField] Tilemap bossMap;
    Mover mover;


    [Header("Config")]
    Vector3 bottomLeftLimit;
    Vector3 topRightLimit;
    float halfHeight;
    float halfWidth;


    private void Awake()
    {
        
    }

    void Start()
    {
        FindPlayer();
        SetMapLimits();
    }

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = new Vector3(Mathf.Clamp(target.position.x, bottomLeftLimit.x, topRightLimit.x)
                                            , Mathf.Clamp(target.position.y, bottomLeftLimit.y, topRightLimit.y)
                                            , transform.position.z);
    }
    
    private void FindPlayer()
    {
        mover = GameObject.FindGameObjectWithTag("Player").GetComponent<Mover>();
        if (mover != null)
        {
            target = mover.transform;
        }
        else
        {
            Debug.LogError("MOVER NOT FOUND");
        }
    }

    private void SetMapLimits()
    {
        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;

        bottomLeftLimit = map.localBounds.min + new Vector3(halfWidth, halfHeight, 0);
        topRightLimit = map.localBounds.max + new Vector3(-halfWidth, -halfHeight, 0);

        mover.SetBounds(map.localBounds.min, map.localBounds.max);
    }

    public void SetSpecialArea()
    {
        map = bossMap;
        bottomLeftLimit = map.localBounds.min + new Vector3(halfWidth, halfHeight, 0);
        topRightLimit = map.localBounds.max + new Vector3(-halfWidth, -halfHeight, 0);

        mover.SetBounds(map.localBounds.min, map.localBounds.max);

        SetMapLimits();
    }
}

