using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int widht = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private float travelTime = 0.2f;
    [SerializeField] private int winCodition = 2048;
    [SerializeField] private GameObject winScreen, loseScreen;
    [SerializeField] List<BlockType> blockTypes;
    
    
    private List<Node> nodes;
    private List<Block> blocks;
    private GameState gameState;
    private int round;
    private BlockType GetBlockTypeByValue(int value) => blockTypes.First(t => t.Value == value);
    private void Start()
    {
        ChangeState(GameState.GENERATELEVEL);
        
    }

    private void Update()
    {
        if (gameState != GameState.WAITINGINPUT) 
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
    }
    private void ChangeState(GameState newGameState)
    {
        gameState = newGameState;
        switch (newGameState)
        {
            case GameState.GENERATELEVEL:
                GenerateGrid();
                break;
            case GameState.SPAWNINGBLOCKS:
                SpawnBlocks(round++ == 0 ?  2 : 1);
                break;
            case GameState.WAITINGINPUT:
                break;
            case GameState.MOVING:
                break;
            case GameState.WIN:
                winScreen.SetActive(true);
                break;
            case GameState.LOSE:
                loseScreen.SetActive(true);
                break;
            default:
                break;
        }
    }
    public void GenerateGrid()
    {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();
        for (int x = 0; x < widht; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                nodes.Add(node);
            }
        }
        Vector2 center = new Vector2((float)widht / 2 - 0.5f, (float)height / 2 - 0.5f);
        SpriteRenderer board = Instantiate(boardPrefab, center, Quaternion.identity);

        board.size = new Vector2(widht, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        ChangeState(GameState.SPAWNINGBLOCKS);
    }

    void SpawnBlocks(int amount)
    {

        var freeNodes = nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => UnityEngine.Random.value).ToList();

        foreach (var node in freeNodes.Take(amount))
        {
            SpawnBlock(node, UnityEngine.Random.value > 0.8f ? 4 : 2);
        }

        if(freeNodes.Count() == 1)
        {
            ChangeState(GameState.LOSE);
            return;
        }

        ChangeState(blocks.Any(b=>b.Value == winCodition) ? GameState.WIN : GameState.WAITINGINPUT);
    }

    void SpawnBlock(Node node, int value)
    {
        Block block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        blocks.Add(block);
    }

    void Shift(Vector2 dir)
    {
        ChangeState(GameState.MOVING);
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y);
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                if(possibleNode != null)
                {
                    if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMarge(block.Value))
                        block.MergeBlock(possibleNode.OccupiedBlock);
                    else if (possibleNode.OccupiedBlock == null) next = possibleNode;

                }
            } while (next != block.Node);
        }

        var sequence = DOTween.Sequence();
        foreach (var block in orderedBlocks)
        {
            var movePoint = block.MerginBlock != null ? block.MerginBlock.Node.Pos : block.Node.Pos;

            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b => b.MerginBlock != null))
            {
                MergeBlocks(block.MerginBlock, block);
            }
            ChangeState(GameState.SPAWNINGBLOCKS);
        });

    }

    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    Node GetNodeAtPosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }
}


[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color; 
}

public enum GameState
{
    GENERATELEVEL,
    SPAWNINGBLOCKS,
    WAITINGINPUT,
    MOVING,
    WIN,
    LOSE,

}