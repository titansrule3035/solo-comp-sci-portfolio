using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool inRange;
    public GameObject playerSprite;
    public SpriteRenderer armorSprite;
    public GameObject visualCue;
    public Rigidbody2D rb;
    public bool isJumping = false;
    public bool canMove = true;
    public bool canJump = true;
    public float maxHealth;
    public float speed;
    public float jumpPower;
    public float defense;
    public float attack;
    public float defaultMaxHealth = 6f;
    public float defaultSpeed = 5f;
    public float defaultJump = 5f;
    public float defaultAttack = 1f;
    public float defaultDefense = 0f;

    public static Player instance;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        instance = this;
        maxHealth = defaultMaxHealth;
        speed = defaultSpeed;
        jumpPower = defaultJump;
        attack = defaultAttack;
        defense = defaultDefense;
    }
    void Update()
    {
        if (inRange)
        {
            visualCue.SetActive(true);
        }
        else
        {
            visualCue.SetActive(false);
        }
        if (canMove && !DebugManager.typing)
        {
            float moveHorizontal = Input.GetAxisRaw("Horizontal");

            rb.velocity = new Vector2(moveHorizontal * speed, rb.velocity.y);
        }
        else
        {
            Vector3 v = rb.velocity;
            v.x = 0f;
            rb.velocity = v;
        }
        if (Input.GetButtonDown("Jump") && !isJumping && canJump && !InkDialogueManager.GetInstance().dialogueIsPlaying && InkDialogueManager.GetInstance().activeChoices == 0 && !DebugManager.typing && !GameManager.paused)
        {
            rb.AddForce(new Vector2(0f, jumpPower), ForceMode2D.Impulse);
            isJumping = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }
    }
    public static Player GetInstance()
    {
        return instance;
    }
}