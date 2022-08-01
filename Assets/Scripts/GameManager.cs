using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    private StackController stack;
    private bool hitSomething = false;
    private bool canHold = true;
    private bool heldDown = false;

    private void Awake()
    {
        stack = GetComponent<StackController>();
    }
    private void Start()
    {
        stack.MakeNewPiece();
        InvokeRepeating("MoveDown", 1f, 1f);
        stack.DisplayHold();
    }

    private void Update()
    {
        if (hitSomething)
        {
            stack.AddToStack();
            if (stack.CheckLose())
            {
                RestartGame();
            }
            stack.MakeNewPiece();
            canHold = true;
            hitSomething = false;
            heldDown = false;

        } else
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (hitSomething)
        {
            if (heldDown)
            {
                heldDown = false;
                CancelInvoke("HeldMove");
            }
            return;
        }
        else if (Input.GetKey("down"))
        {
            if (!heldDown)
            {
                HeldMove();
                heldDown = true;
                InvokeRepeating("HeldMove", .25f, .25f);
            }
        }
        else
        {
            heldDown = false;
            CancelInvoke("HeldMove");
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (stack.CheckRotationValid())
                {
                    stack.UpdateTetroStackGUI();
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                stack.MoveTetro(Direction.Right);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                stack.MoveTetro(Direction.Left);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                while(stack.CanMoveDown() && !hitSomething)
                {
                    MoveDown();
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (canHold)
                {
                    stack.HoldTetro();
                    canHold = false;
                }
            }
        }
    }

    private void HeldMove()
    {
        MoveDown();
    }

    private void MoveDown()
    {
        bool stat = stack.MoveDown();
        if (!stat)
        {
            hitSomething = true;
            if (stack.CheckLose())
            {
                RestartGame();
            }
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Level1");
    }
}
