using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StackController : MonoBehaviour
{
    [SerializeField] Tilemap stackGUI;
    [SerializeField] TileBase block;
    [SerializeField] TileBase wall;
    [SerializeField] TileBase emptyBlock;

    private TetrominoManager tetroManager;
    private bool holdingTetro = false;
    private const int stackH = 21;
    private const int stackW = 10;

    private int[,] stack = new int[stackH, stackW];

    private Vector2Int startingCOORDs = new Vector2Int(stackH - 3, (stackW - 1) / 2);
    private Vector2Int currentCOORDs = new Vector2Int();
    private Vector2Int queueCOORDs = new Vector2Int(stackH - 7, (stackW + 1));
    private Vector2Int holdCOORDs = new Vector2Int(stackH - 7, -5);

    private tetrominos nextTetro;
    private tetrominos heldTetro;
    private tetrominos currentPiece = tetrominos.i;
    private int[,] currentTetroGrid;

    private void Start()
    {
        tetroManager = GetComponent<TetrominoManager>();
        currentCOORDs = startingCOORDs;
        InitStackGUIPosition();
        InitStacks();
        System.Array values = System.Enum.GetValues(typeof(tetrominos));
        nextTetro = (tetrominos)System.Enum.GetValues(typeof(tetrominos)).GetValue(Random.Range(0, values.Length));
    }

    private void InitStackGUIPosition()
    {
        stackGUI.transform.position -= new Vector3((stackW / 2f) * .3f, (stackH / 2f) * .3f, 0);
    }

    private void InitStacks()
    {
        for (int i = 0; i < stackH - 4; i++)
        {
            for (int j = 0; j < stackW; j++)
            {
                stackGUI.SetTile(ToGUICoords(i, -(j)), emptyBlock);
                stack[i, j] = 0;
            }
        }

        for (int i = 0; i < stackH - 4; i++)
        {
            stackGUI.SetTile(ToGUICoords(i, 1), wall);     //Left Wall
            stackGUI.SetTile(ToGUICoords(i, -stackW), wall); //Right Wall
        }

        for (int i = 0; i < stackW; i++)
        {
            stackGUI.SetTile(ToGUICoords(-1, -i), wall);
        }


        for (int i = 0; i < 6; i++)
        {
            //Draw Queue Walls
            stackGUI.SetTile(ToGUICoords(stackH - 4, -(stackW + i)), wall);
            stackGUI.SetTile(ToGUICoords(stackH - 4 - 5, -(stackW + i)), wall);
            stackGUI.SetTile(ToGUICoords(stackH - 4 - i, -(stackW + 5)), wall);
        }

        for (int i = -1; i > -7; i--)
        {
            //Draw Hold Walls
            stackGUI.SetTile(ToGUICoords(stackH - 4, -(i)), wall);
            stackGUI.SetTile(ToGUICoords(stackH - 4 - 5, -(i)), wall);
            stackGUI.SetTile(ToGUICoords(stackH - 4 + (i + 1), -(-6)), wall);
        }

    }


    private Vector3Int ToGUICoords(int x, int y)
    {
        return new Vector3Int(-y, x, 0);
    }

    public void UpdateTetroStackGUI()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (tetroManager.tetroBox[i, j] == 1)
                {
                    stackGUI.SetTile(ToGUICoords(currentCOORDs.x + i, -(currentCOORDs.y + j)), block);
                }
            }
        }
    }

    public void AddToStack()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (tetroManager.tetroBox[i, j] == 1)
                {
                    stack[currentCOORDs.x + i, currentCOORDs.y + j] = 1;
                }
            }
        }
        CheckAndClear();
    }

    public bool MoveDown()
    {
        if (CanMoveDown())
        {
            ClearOldCOORDs();
            currentCOORDs.x--;
            UpdateTetroStackGUI();
            return true;
        }
        return false;
    }

    private void ClearOldCOORDs()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (tetroManager.tetroBox[i, j] == 1)
                {
                    stack[currentCOORDs.x + i, currentCOORDs.y + j] = -1;
                    if (currentCOORDs.x + i > 16)
                    {
                        stackGUI.SetTile(ToGUICoords(currentCOORDs.x + i, -(currentCOORDs.y + j)), null);
                    }
                    else
                    {
                        stackGUI.SetTile(ToGUICoords(currentCOORDs.x + i, -(currentCOORDs.y + j)), emptyBlock);
                    }

                }
            }
        }
    }

    public bool CheckRotationValid()
    {
        if (currentCOORDs.x >= 18)
            return false;
        if (currentPiece == tetrominos.o)
            return false;
        ClearOldCOORDs();
        int[,] preRotationTetroBox = (int[,])tetroManager.tetroBox.Clone();
        int[,] postRotationGrid = (int[,])currentTetroGrid.Clone();
        tetroManager.RotateTetro(postRotationGrid);
        tetroManager.CreateTetro(postRotationGrid);
        if (!tetroManager.CheckMoveValid(currentCOORDs) || !CheckStackValid(currentCOORDs))
        {
            if (AttemptWallKick())
            {
                if (tetroManager.CheckMoveValid(currentCOORDs) && CheckStackValid(currentCOORDs))
                {
                    ClearOldCOORDs();
                    UpdateTetroStackGUI();
                    return true;
                }
                else
                {
                    tetroManager.tetroBox = preRotationTetroBox;
                    UpdateTetroStackGUI();
                    return false;
                }
            }
            else
            {
                tetroManager.tetroBox = preRotationTetroBox;
                UpdateTetroStackGUI();
                return false;
            }
        }
        currentTetroGrid = postRotationGrid;
        return true;
    }

    private bool AttemptWallKick()
    {
        if (tetroManager.CheckMoveValid(new Vector2(currentCOORDs.x, currentCOORDs.y + 1)) && CheckStackValid(new Vector2Int(currentCOORDs.x, currentCOORDs.y + 1)))
        {
            currentCOORDs.y++;
            return true;
        }
        else
        {
            if (tetroManager.CheckMoveValid(new Vector2(currentCOORDs.x, currentCOORDs.y - 1)) && CheckStackValid(new Vector2Int(currentCOORDs.x, currentCOORDs.y - 1)))
            {
                currentCOORDs.y--;
                return true;
            }
        }
        return false;
    }

    public bool CanMoveDown()
    {
        //Check for floor
        if (tetroManager.CheckMoveValid(new Vector2(currentCOORDs.x - 1, currentCOORDs.y)))
        {
            if (CheckPieceBelow(currentCOORDs))
            {
                return true;
            }
        }
        return false;
    }

    public void MakeNewPiece()
    {
        tetroManager.ClearTetroBox();
        System.Array values = System.Enum.GetValues(typeof(tetrominos));
        currentPiece = nextTetro;
        nextTetro = (tetrominos)System.Enum.GetValues(typeof(tetrominos)).GetValue(Random.Range(0, values.Length));
        currentCOORDs = startingCOORDs;
        currentTetroGrid = tetroManager.GetTetromino(currentPiece);
        tetroManager.CreateTetro(currentTetroGrid);
        UpdateTetroStackGUI();
        DisplayQueue();
    }

    private bool CheckStackValid(Vector2Int newCOORDs)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (tetroManager.tetroBox[i, j] == 1)
                {
                    if (stack[i + newCOORDs.x, j + newCOORDs.y] == 1)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private bool CheckPieceBelow(Vector2Int newCOORDs)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (tetroManager.tetroBox[i, j] == 1)
                {
                    if (stack[i + newCOORDs.x - 1, j + newCOORDs.y] == 1)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public bool CheckLose()
    {
        for (int i = 0; i < stackH; i++)
        {
            for (int j = 0; j < stackW; j++)
            {
                if (stack[i, j] == 1)
                {
                    if (i > stackH - 4)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void CheckAndClear()
    {
        bool rowGood;
        List<int> rowsToBeCleared = new List<int>();
        int moved = 0;
        for (int i = 0; i < stackH; i++)
        {
            rowGood = true;
            for (int j = 0; j < stackW; j++)
            {
                if (stack[i, j] != 1)
                {
                    rowGood = false;
                }
            }
            if (rowGood)
            {
                rowsToBeCleared.Add(i);
            }
        }
        for (int i = 0; i < rowsToBeCleared.Count; i++)
        {
            ClearRow(rowsToBeCleared[i] - moved);
            moved++;
        }
    }

    private void ClearRow(int row)
    {
        for (int j = 0; j < stackW; j++)
        {
            stack[row, j] = 0;
            stackGUI.SetTile(ToGUICoords(row, -j), emptyBlock);
        }
        MoveDown(row);
    }

    private void MoveDown(int row)
    {
        for (int i = 0; i < stackH - 5; i++)
        {
            for (int j = 0; j < stackW; j++)
            {
                if (i >= row)
                {
                    stackGUI.SetTile(ToGUICoords(i, -j), stackGUI.GetTile(ToGUICoords(i + 1, -j)));
                    stackGUI.SetTile(ToGUICoords(i + 1, -j), emptyBlock);

                    stack[i, j] = stack[i + 1, j];
                    stack[i + 1, j] = 0;
                }
            }
        }
    }

    private void DisplayQueue()
    {
        int[,] nextTetroGrid = tetroManager.GetTetromino(nextTetro);
        int[,] nextTetroBox = new int[4, 4];
        for (int i = 0; i < nextTetroGrid.GetLength(0); i++)
        {
            for (int j = 0; j < nextTetroGrid.GetLength(1); j++)
            {
                nextTetroBox[i, j] = nextTetroGrid[i, j];
            }
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                stackGUI.SetTile(ToGUICoords(queueCOORDs.x + i - 1, -(queueCOORDs.y + j)), emptyBlock);
                if (nextTetroBox[i, j] == 1)
                {
                    stackGUI.SetTile(ToGUICoords(queueCOORDs.x + i - 1, -(queueCOORDs.y + j)), block);
                }
            }
        }
    }

    public void DisplayHold()
    {
        int[,] holdTetroBox = new int[4, 4];
        if (holdingTetro)
        {
            int[,] holdTetroGrid = tetroManager.GetTetromino(heldTetro);
            for (int i = 0; i < holdTetroGrid.GetLength(0); i++)
            {
                for (int j = 0; j < holdTetroGrid.GetLength(1); j++)
                {
                    holdTetroBox[i, j] = holdTetroGrid[i, j];
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                stackGUI.SetTile(ToGUICoords(holdCOORDs.x + i - 1, -(holdCOORDs.y + j)), emptyBlock);
                if (holdTetroBox[i, j] == 1)
                {
                    stackGUI.SetTile(ToGUICoords(holdCOORDs.x + i - 1, -(holdCOORDs.y + j)), block);
                }
            }
        }
    }

    public void HoldTetro()
    {
        tetrominos tmp = currentPiece;
        if (holdingTetro)
        {
            currentPiece = heldTetro;
            heldTetro = tmp;
        }
        else
        {
            currentPiece = nextTetro;
            heldTetro = tmp;
            System.Array values = System.Enum.GetValues(typeof(tetrominos));
            nextTetro = (tetrominos)System.Enum.GetValues(typeof(tetrominos)).GetValue(Random.Range(0, values.Length));
        }
        
        holdingTetro = true;
        ClearOldCOORDs();
        tetroManager.ClearTetroBox();
        currentCOORDs = startingCOORDs;
        currentTetroGrid = tetroManager.GetTetromino(currentPiece);
        tetroManager.CreateTetro(currentTetroGrid);
        UpdateTetroStackGUI();
        DisplayQueue();
        DisplayHold();
    }

    public void MoveTetro(Direction dir)
    {
        if (tetroManager.CheckMoveValid(new Vector2(currentCOORDs.x, currentCOORDs.y + ((int)dir))))
        {
            if (CheckStackValid(new Vector2Int(currentCOORDs.x, currentCOORDs.y + ((int)dir))))
            {
                ClearOldCOORDs();
                currentCOORDs.y += ((int)dir);
                UpdateTetroStackGUI();
            }
        }
    }
}
public enum Direction
{
    Right = 1,
    Left = -1,
}
