using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Block OccupiedBlock { get; set; }
    public Vector2 Pos => transform.position;
}
