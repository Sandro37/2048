using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    public Vector2 Pos => transform.position;
    public Node Node;
    public Block MerginBlock;
    public bool Merging;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private TextMeshPro text;
   public void Init(BlockType blockType)
    {
        Value = blockType.Value;
        renderer.color = blockType.Color;
        text.text = blockType.Value.ToString();
    }

    public void SetBlock(Node node)
    {
        if (this.Node != null) this.Node.OccupiedBlock = null;

        this.Node = node;
        Node.OccupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith)
    {
        MerginBlock = blockToMergeWith;

        Node.OccupiedBlock = null;

        blockToMergeWith.Merging = true;
    }

    public  bool  CanMarge(int value) => Value == value && !Merging &&  MerginBlock ==null;
}
