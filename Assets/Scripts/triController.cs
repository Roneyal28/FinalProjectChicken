using UnityEngine;
using UnityEngine.InputSystem;

public class triController : MonoBehaviour
{
    private float step = 1;
    private float speed = 5;
    void Start()
    {
        
    }

    void Update()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            Move(0,1);
        }
        if (Keyboard.current.aKey.isPressed)
        {
            Move(-1,0);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            Move(1,0);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            Move(0,-1);
        }
    }

    public void Move(int x, int y)
    {
        if (x < 0)
        {
            transform.Translate(-step * speed * Time.deltaTime, 0, 0);
        }
        else if (x > 0)
        {
            transform.Translate(step * speed * Time.deltaTime, 0, 0);
        }
        else if  (y < 0)
        {
            transform.Translate(0,-step * speed * Time.deltaTime, 0);
        }
        else if (y > 0)
        {
            transform.Translate(0, step * speed * Time.deltaTime, 0);
        }
    }
}
