using UnityEngine;

public class CarScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public Logic logic;
    public bool alive = true;
    public SpriteRenderer spriteRenderer;
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;
    public float maxSpeed = 100f;
    public float acceleration = 100f;
    public float turnfactor = 3.5f;
    public float driftfactor = 0.9f;
    private float accelerationInput = 0f;
    private float turnInput = 0f;
    public ParticleSystem tireSmokeParticles1;
    public ParticleSystem tireSmokeParticles2;
    public float driftSmokeThreshold = 1.5f;
    public float oversteerFactor = 1.2f;
    public float smokeLingerTime = 0.2f; // How long the smoke continues during a transition
    private float smokeTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        gameObject.name = "George Russell";
        logic = FindAnyObjectByType<Logic>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.name = "George Russell";
        
        // Ensure the particles are hidden when the game starts
    }

    // Update is called once per frame
    void Update()
    {
        accelerationInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        if (!alive) return;
        if (turnInput < 0)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = sprite1;
        }
        else
        if (turnInput > 0)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = sprite2;
        }
        else
        {
            if (spriteRenderer != null) spriteRenderer.sprite = sprite3;
        }
    }
    private void FixedUpdate()
    {
        if (!alive) return;
        ApplyEngineForce();
        ApplyDrift();
        ApplySteering();
        HandleParticleEffects();
    }

    private void ApplyEngineForce()
    {
        float velocityVsUp = Vector2.Dot(transform.up, myRigidbody.linearVelocity);
        if (velocityVsUp > maxSpeed && accelerationInput > 0) return;
        if (velocityVsUp < -maxSpeed && accelerationInput < 0) return;
        Vector2 engineForce = transform.up * accelerationInput * acceleration;
        myRigidbody.AddForce(engineForce, ForceMode2D.Force);
    }
    private void ApplyDrift()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(myRigidbody.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(myRigidbody.linearVelocity, transform.right);
        myRigidbody.linearVelocity = forwardVelocity + rightVelocity * driftfactor;
    }
    private void ApplySteering()
    {
        float minSpeedBeforeAllowTurningFactor = (myRigidbody.linearVelocity.magnitude / 8);
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);
        float directionMultiplier = Vector2.Dot(myRigidbody.linearVelocity, transform.up) >= 0 ? 1.0f : -1.0f;
        float sidewaysVelocity = Vector2.Dot(myRigidbody.linearVelocity, transform.right);
        float spinEffect = sidewaysVelocity * oversteerFactor;
        float rotationangle = (turnInput * turnfactor - spinEffect) * minSpeedBeforeAllowTurningFactor;
    
    myRigidbody.MoveRotation(myRigidbody.rotation - rotationangle * directionMultiplier);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
            logic.gameOver();
            alive = false;
    }
    private void HandleParticleEffects()
    {
        if (tireSmokeParticles1 != null && tireSmokeParticles2 != null)
        {
            float sidewaysSlip = Mathf.Abs(Vector2.Dot(myRigidbody.linearVelocity, transform.right));

            // If we are actively drifting, reset the timer to full
            if (sidewaysSlip > driftSmokeThreshold)
            {
                smokeTimer = smokeLingerTime; 
            }
            else
            {
                // If we are not drifting, count down the timer
                smokeTimer -= Time.fixedDeltaTime; 
            }

            // Emit smoke as long as the timer is above zero
            bool shouldEmit = smokeTimer > 0f;

            var emission1 = tireSmokeParticles1.emission;
            if (emission1.enabled != shouldEmit) emission1.enabled = shouldEmit;

            var emission2 = tireSmokeParticles2.emission;
            if (emission2.enabled != shouldEmit) emission2.enabled = shouldEmit;
        }
    }
}
