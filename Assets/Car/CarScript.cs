using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class CarScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public Logic logic;
    public bool alive = true;
    public SpriteRenderer spriteRenderer;
    public CarDefinition definition;
    public TireSmokeProfile smokeProfile;
    public ParticleSystem tireSmokeParticles1;
    public ParticleSystem tireSmokeParticles2;
    public float maxSpeed;
    public float acceleration;
    public float turnfactor;
    public float driftfactor;
    public float oversteerFactor = 0.5f;
    float accelerationInput;
    float turnInput;
    float smokeTimer;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        logic = FindAnyObjectByType<Logic>();
        ApplyDefinition(CarSelection.Current);
    }
    public void ApplyDefinition(CarDefinition car)
    {
        definition = car;
        myRigidbody.linearDamping = 0.5f;
        gameObject.name = car.displayName;
        transform.localScale = Vector3.one;
        spriteRenderer.sprite = car.straight;
        spriteRenderer.sortingOrder = 10;
        maxSpeed = car.maxSpeed;
        acceleration = car.acceleration;
        turnfactor = car.steering;
        driftfactor = car.driftFactor;
        oversteerFactor = car.oversteerFactor;
        var collider = GetComponent<BoxCollider2D>();
        collider.size = car.colliderSize;
        collider.offset = car.colliderOffset;
        SetupSmoke(tireSmokeParticles1, car.leftTire);
        SetupSmoke(tireSmokeParticles2, car.rightTire);
    }
    void SetupSmoke(ParticleSystem particles, Vector2 position)
    {
        if (particles == null || smokeProfile == null) return;
        particles.transform.localPosition = new Vector3(position.x, position.y, 0.5f);
        particles.transform.localScale = Vector3.one;
        particles.transform.localRotation = Quaternion.identity;
        smokeProfile.Apply(particles);
    }
    void Update()
    {
        if (!alive) return;
        accelerationInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        spriteRenderer.sprite = turnInput < -0.05f ? definition.left : turnInput > 0.05f ? definition.right : definition.straight;
    }
    void FixedUpdate()
    {
        if (!alive) return;
        float forwardSpeed = Vector2.Dot(transform.up, myRigidbody.linearVelocity);
        if (!((forwardSpeed > maxSpeed && accelerationInput > 0) || (forwardSpeed < -maxSpeed * 0.5f && accelerationInput < 0)))
            myRigidbody.AddForce(transform.up * accelerationInput * acceleration);
        Vector2 forward = transform.up * Vector2.Dot(myRigidbody.linearVelocity, transform.up);
        Vector2 sideways = transform.right * Vector2.Dot(myRigidbody.linearVelocity, transform.right);
        myRigidbody.linearVelocity = forward + sideways * driftfactor;
        float turning = Mathf.Clamp01(myRigidbody.linearVelocity.magnitude / 8);
        float direction = Vector2.Dot(myRigidbody.linearVelocity, transform.up) >= 0 ? 1 : -1;
        float slip = Vector2.Dot(myRigidbody.linearVelocity, transform.right);
        myRigidbody.MoveRotation(myRigidbody.rotation - (turnInput * turnfactor - slip * oversteerFactor) * turning * direction);
        UpdateSmoke(Mathf.Abs(slip));
    }
    public void UpdateSmoke(float slip)
    {
        if (smokeProfile == null) return;
        smokeTimer = alive && slip > smokeProfile.slipThreshold ? smokeProfile.linger : Mathf.Max(0, smokeTimer - Time.fixedDeltaTime);
        SetEmission(tireSmokeParticles1, alive && smokeTimer > 0);
        SetEmission(tireSmokeParticles2, alive && smokeTimer > 0);
    }
    static void SetEmission(ParticleSystem particles, bool enabled)
    {
        if (particles == null) return;
        var emission = particles.emission;
        emission.enabled = enabled;
    }
    public void StopCar()
    {
        alive = false;
        accelerationInput = turnInput = smokeTimer = 0;
        myRigidbody.linearVelocity = Vector2.zero;
        myRigidbody.angularVelocity = 0;
        SetEmission(tireSmokeParticles1, false);
        SetEmission(tireSmokeParticles2, false);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (alive && logic != null) logic.gameOver();
    }
}
