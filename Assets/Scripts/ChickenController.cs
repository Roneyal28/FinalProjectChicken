using UnityEngine;
using UnityEngine.InputSystem;

public class ChickenController : MonoBehaviour
{
    [Header("Chicken Components")]
    private SpriteRenderer chickenSR;
    private Rigidbody2D chickenRB;
    [Header("Movement")]
   [SerializeField] int Step = 1;
   [SerializeField] int speed = 5;
   [SerializeField] LayerMask floorLayer;
   private bool isOnFloor;
   [Header("Jump")]
   [SerializeField] private float jumpForce = 7f;
   [SerializeField] Transform groundCheck;
   [SerializeField] float groundCheckRadius =0.2f;
   [SerializeField] LayerMask groundLayer;
   private bool isGrounded;
   
    void Start()
    {
        chickenSR = GetComponent<SpriteRenderer>();
        chickenRB = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ChickenMovement();
        isOnFloor = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, floorLayer);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && Keyboard.current.spaceKey.isPressed)
        {
            chickenRB.linearVelocity = new Vector2(chickenRB.linearVelocity.x, jumpForce);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(groundCheck != null)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
    

    private void ChickenMovement()
    {
        if(Keyboard.current.wKey.isPressed)
        {
            Move(0, 1);
        } 
        if (Keyboard.current.aKey.isPressed)
        {
            Move(-1, 0);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            Move(0, -1);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            Move(1, 0);
        }
    }

    public void Move(int x, int y)
    {
        if (x == 0 && y == 0) 
        {
            transform.Translate(0, 0, 0); 
        }
        if (x == 0 && y != 0 && isOnFloor) 
        {
            transform.Translate(0, y * Step * speed * Time.deltaTime , 0); 
        }
        if (x != 0 && y == 0) 
        {
            FlipChicken(x);
            transform.Translate(x * Step * speed * Time.deltaTime, 0 , 0);
        }
    }

    public void FlipChicken(int x)
    {
        if (x < 0)
        {
            chickenSR.flipX = true;
        }
        else 
        {
            chickenSR.flipX = false;
        }
    }
    public void CanWalkUp()
    {
        if (isOnFloor)
        {
            chickenRB.gravityScale = 0;
        }
        else
        {
            chickenRB.gravityScale = 1;
        }
    }
}
