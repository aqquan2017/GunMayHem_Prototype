using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visyde
{
    /// <summary>
    /// Movement Controller
    /// - A very simple 2d character controller that uses Rigidbody2D.
    /// - You can use your favorite character controller by replacing this component!
    ///
    ///   To make your custom controller work with this template, the controller must have a "ground" checker, a "jump" system, and 
    ///   an "isMine" boolean to check if this is ours or not.
    /// </summary>

    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementController : MonoBehaviour
    {
        [Header("Settings:")] public float groundCheckerRadius;
        public Vector2 groundCheckerOffset;

        [Space] [Header("References:")] [SerializeField]
        private Rigidbody2D rg;
        private CapsuleCollider2D rgCapsuleCollider2D;

        // The movement speed and jump force doesn't need to be set manually 
        // as they will be overriden by the character data anyway:
        [HideInInspector] public float moveSpeed;
        [HideInInspector] public float jumpForce;
        [HideInInspector] public float jumpForce2;
        [HideInInspector] public float forceTime = 0.5f;

        public const int numberOfExtraJump = 1;
        public int extraJumpCount = 1; //1 is default

        [HideInInspector] public bool isMine;
        public bool allowJump { get; protected set; }

        public bool hasRigidbody
        {
            get { return rg; }
        }

        private Vector2 m; // movement

        public Vector2 movement
        {
            get { return m; }
        }

        public Vector2 velocity
        {
            get
            {
                Vector2 final = rg ? rg.velocity : Vector2.zero;
                return final;
            }
            set
            {
                if (rg) rg.velocity = value;
            }
        }

        public Vector2 position
        {
            get
            {
                Vector2 final = rg ? rg.position : Vector2.zero;
                return final;
            }
            set
            {
                if (rg) rg.position = value;
            }
        }

        public bool isGrounded { get; protected set; }

        // Internal:
        float inputX;

        void Awake()
        {
            if (!rg) rg = GetComponent<Rigidbody2D>();
            if (!rgCapsuleCollider2D) rgCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        }

        void Update()
        {
            // Only enable movement controls if ours:
            if (isMine)
            {
                m.x = isGrounded ? inputX : inputX != 0 ? inputX : movement.x;
                if (!isGrounded)
                {
                    m.x = Mathf.MoveTowards(movement.x, 0, Time.deltaTime);
                }
            }
            // make this immovable if not ours:
            else if (rg)
            {
                rg.mass = 1000;
                rg.gravityScale = 0;
            }

            // Check if grounded:
            allowJump = true;
            isGrounded = false;
            Collider2D[] cols = Physics2D.OverlapCircleAll(
                groundCheckerOffset + new Vector2(transform.position.x, transform.position.y), groundCheckerRadius);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].CompareTag("JumpPad") || extraJumpCount < 1)
                {
                    allowJump = false;
                }

                if (cols[i].gameObject != gameObject && !cols[i].isTrigger && !cols[i].CompareTag("Portal"))
                {
                    if (!isGrounded)
                    {
                        isGrounded = true;
                        extraJumpCount = numberOfExtraJump;
                    }
                }
            }

            forceTime -= Time.deltaTime;
        }

        void FixedUpdate()
        {
            if (isMine && rg)
            {
                if (forceTime > 0f)
                {
                    Vector2 veloc = rg.velocity;
                    veloc.x += movement.x * (moveSpeed / 10) * Time.fixedDeltaTime;
                    veloc.x = Mathf.Clamp(veloc.x, -4f, 4f);
                    rg.velocity = veloc;
                    
                    //clamp speed
                    if (rg.velocity.x > 5f)
                    {
                       
                    }
                }
                else
                {
                    Vector2 veloc = rg.velocity;
                    veloc.x = movement.x * (moveSpeed / 10);
                    rg.velocity = veloc;
                }
                
            }
        }

        // For local movement controlling: 
        public void InputMovement(float x)
        {
            // Movement input:f
            inputX = x;
        }

        public void Jump()
        {
            if (!rg) return;

            Vector2 veloc = rg.velocity;
            veloc.y = jumpForce;
            rg.velocity = veloc;

            // Don't allow jumping right after a jump:
            //NOW ALLOW DOUBLE JUMP
            //allowJump = false;
            extraJumpCount--;
        }

        public void DisableColliderWithPlatform(float time)
        {
            StopCoroutine("DisableCollider");
            StartCoroutine(DisableCollider(time));
        }
        
        IEnumerator DisableCollider(float time)
        {
            rgCapsuleCollider2D.isTrigger = true;
            yield return new WaitForSeconds(time);
            rgCapsuleCollider2D.isTrigger = false;
        }

        public void ApplyForce(Vector2 direction, ForceMode2D forceMode = ForceMode2D.Impulse){
            if (!rg) return;
            forceTime = 0.5f;
            rg.AddForce(direction , forceMode);
        }
    

        public void DestroyRigidbody(){
            Destroy(this);
            Destroy(rg);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + new Vector3(groundCheckerOffset.x, groundCheckerOffset.y), groundCheckerRadius);
        }
    }
}