using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoManager : MonoBehaviour
{
    public int[,] tetroBox = new int[4, 4];

    private int[,] iTetr = { {0,0,0,0},
                     {1,1,1,1},
                     {0,0,0,0},
                     {0,0,0,0}, };

    private int[,] oTetr = { {0,1,1,0},
                     {0,1,1,0},
                     {0,0,0,0},
                     {0,0,0,0},};

    private int[,] jTetr = { {1,0,0},
                     {1,1,1},
                     {0,0,0}, };

    private int[,] lTetr = { {0,0,1},
                     {1,1,1},
                     {0,0,0}, };

    private int[,] tTetr = { {0,1,0},
                     {1,1,1},
                     {0,0,0}, };

    private int[,] sTetr = { {0,1,1},
                     {1,1,0},
                     {0,0,0}, };

    private int[,] zTetr = { {1,1,0},
                     {0,1,1},
                     {0,0,0}, };

    public int[,] GetTetromino(tetrominos tetro)
    {
        int[,] tetrominoGrid = null;
        switch (tetro)
        {
            case tetrominos.i:
                tetrominoGrid = iTetr;
                break;
            case tetrominos.o:
                tetrominoGrid = oTetr;
                break;
            case tetrominos.j:
                tetrominoGrid = jTetr;
                break;
            case tetrominos.l:
                tetrominoGrid = lTetr;
                break;
            case tetrominos.t:
                tetrominoGrid = tTetr;
                break;
            case tetrominos.s:
                tetrominoGrid = sTetr;
                break;
            case tetrominos.z:
                tetrominoGrid = zTetr;
                break;
        }

        return (int[,])tetrominoGrid.Clone();
    }

    public void RotateTetro(int[,] rotatedTetroBox)
    {
        int N = rotatedTetroBox.GetLength(0);
        for (int x = 0; x < N / 2; x++)
        {
            // Consider elements
            // in group of 4 in
            // current square
            for (int y = x; y < N - x - 1; y++)
            {
                // store current cell
                // in temp variable
                int temp = rotatedTetroBox[x, y];

                // move values from
                // right to top
                rotatedTetroBox[x, y] = rotatedTetroBox[y, N - 1 - x];

                // move values from
                // bottom to right
                rotatedTetroBox[y, N - 1 - x] = rotatedTetroBox[N - 1 - x,
                                        N - 1 - y];

                // move values from
                // left to bottom
                rotatedTetroBox[N - 1 - x,
                    N - 1 - y]
                    = rotatedTetroBox[N - 1 - y, x];

                // assign temp to left
                rotatedTetroBox[N - 1 - y, x] = temp;
            }
        }
    }

    public void CreateTetro(int[,] tetroGrid)
    {
        for (int i = 0; i < tetroGrid.GetLength(0); i++)
        {
            for (int j = 0; j < tetroGrid.GetLength(0); j++)
            {
                tetroBox[i, j] = tetroGrid[i, j];
            }
        }
    }

    public bool CheckMoveValid(Vector2 newCOORDs)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (tetroBox[i, j] == 1)
                {
                    if (j + newCOORDs.y < 0 || newCOORDs.y + j >= 10)
                    {
                        return false;
                    }
                    if (i + newCOORDs.x < 0)
                    {
                        return false;
                    }
                    if (j + newCOORDs.y >= 16)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void ClearTetroBox()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tetroBox[i, j] = 0;
            }
        }
    }

}
public enum tetrominos { i, o, j, l, t, s, z };