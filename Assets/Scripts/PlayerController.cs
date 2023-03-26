using System;
using System.Collections;
using Assets.Scripts;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // public variables
    [Tooltip("Speed at which Player Moves")]
    public float speed = 1.0f;

    [Tooltip("Power up indicator prefab")]
    public GameObject powerUpIndicator;

    [Tooltip("Projectile Prefab")]
    [SerializeField] public GameObject projectilePrefab;

    public float hangTime;
    public float smashSpeed;
    public float explosionForce;
    public float explosionRadius;

    public PowerUpType currentPowerUpType = PowerUpType.None;

    // private variables
    private Rigidbody _playerRb;

    private GameObject _focalPoint;
    private bool _hasPowerUp;
    private const float powerUpStrength = 15.0f;
    private const float yValue = -0.6f;
    private GameObject _tmpRocket;
    private Coroutine _powerUpCoroutine;

    private bool _smashing;
    private float _floorY;

    // Start is called before the first frame update
    private void Start()
    {
        _playerRb = GetComponent<Rigidbody>();
        // Find the GameObject named "CameraFocalPoint" in the scene.
        // This is used as the point of focus for the camera around which the camera is rotated.
        _focalPoint = GameObject.Find("CameraFocalPoint");
    }

    // Update is called once per frame
    private void Update()
    {
        // Get the vertical input and move the player forward with respect to the camera's focal point
        var forwardInput = Input.GetAxis("Vertical");
        _playerRb.AddForce(forwardInput * speed * _focalPoint.transform.forward);

        // If the player has a power-up, set the power-up indicator's position to the player's position.\
        // This ensures that the power-up indicator follows the player around.
        if (_hasPowerUp)
        {
            powerUpIndicator.transform.position = transform.position + new Vector3(0, yValue, 0);
        }

        if (currentPowerUpType == PowerUpType.Rocket && Input.GetKeyDown(KeyCode.Space))
        {
            LaunchRockets();
        }
        if (currentPowerUpType == PowerUpType.Smash && !_smashing && Input.GetKeyDown(KeyCode.Space))
        {
            _smashing = true;
            StartCoroutine(Smash());
        }
    }

    // This method is called when the player collides with a trigger collider (`other`).
    //
    // If the collider has the tag `PowerUp`, the player gains a power-up and the power-up object is destroyed.
    // The `hasPowerUp` variable is set to `true` when the player collides with a power-up.
    // The `PowerUpCountdownRoutine` method is called to start a co-routine which will set `hasPowerUp` to `false` after 7 seconds.
    // The power-up indicator is also activated.
    private void OnTriggerEnter(Component other)
    {
        Destroy(other.gameObject);
        if (!other.CompareTag("PowerUp")) return;
        _hasPowerUp = true;
        currentPowerUpType = other.gameObject.GetComponent<PowerUp>().powerUpType;
        powerUpIndicator.gameObject.SetActive(true);

        if (_powerUpCoroutine != null)
        {
            StopCoroutine(_powerUpCoroutine);
        }
        _powerUpCoroutine = StartCoroutine(PowerUpCountdownRoutine(7));
    }

    // This method is called when the player collides with a non-trigger collider (`collision`).
    //
    // If the collider has the tag `Enemy` and the player has a power-up, the enemy is pushed away from the player.
    private void OnCollisionEnter(Collision collision)
    {
        // If the collision happens with Enemy and the player has a power up,
        // add an Impulse to the Enemy Rigid-body (enemyRB) in the direction away from the player.
        //
        // The direction (awayFromPlayer) is a unit vector which is calculated by subtracting player's
        // 3d coordinates from collision object's 3d coordinates and then normalizing it.
        //
        // We then multiply this vector by the `powerUpStrength` variable (defined in private variables at top in this script)
        // to determine the force to apply.
        //
        // Finally, we apply the force to the enemy's rigid-body (`enemyRB`) as an Impulse.

        if (collision.gameObject.CompareTag("Enemy") && currentPowerUpType == PowerUpType.PushBack)
        {
            var enemyRb = collision.gameObject.GetComponent<Rigidbody>();
            var awayFromPlayer = (collision.gameObject.transform.position - transform.position).normalized;

            var force = awayFromPlayer * powerUpStrength;
            enemyRb.AddForce(force, ForceMode.Impulse);
        }
    }

    private void LaunchRockets()
    {
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            _tmpRocket = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
            _tmpRocket.GetComponent<RocketBehaviour>().Fire(enemy.transform);
        }
    }

    // This co-routine waits for 7 seconds and then disables the power up.
    // It is called when the player picks up a power up
    // and is used to disable the power up after 7 seconds.
    private IEnumerator PowerUpCountdownRoutine(int time)
    {
        yield return new WaitForSeconds(time);
        _hasPowerUp = false;
        currentPowerUpType = PowerUpType.None;
        powerUpIndicator.gameObject.SetActive(false);
    }

    private IEnumerator Smash()
    {
        _floorY = transform.position.y;

        var jumpTime = Time.time + hangTime;

        while (Time.time < jumpTime)
        {
            _playerRb.velocity = Vector3.up * smashSpeed;
            yield return null;
        }

        while (transform.position.y > _floorY)
        {
            _playerRb.velocity = 2 * smashSpeed * Vector3.down;
            yield return null;
        }

        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            enemy.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f,
                ForceMode.Impulse);
        }

        _smashing = false;
    }
}