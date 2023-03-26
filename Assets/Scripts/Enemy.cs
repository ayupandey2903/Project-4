using UnityEngine;

namespace Assets.Scripts
{
    public class Enemy : MonoBehaviour
    {
        // public variables
        [Tooltip("Speed at which Enemy Moves")]
        public float speed = 1.0f;

        public bool isBoss = false;

        public float spawnInterval;
        public float nextSpawn;

        public int miniEnemyCount;

        // private variables
        private Rigidbody _enemyRb;

        private GameObject _player;

        private SpawnManager spawnManager;

        // Start is called before the first frame update
        private void Start()
        {
            _enemyRb = GetComponent<Rigidbody>();
            _player = GameObject.Find("Player");

            if (!isBoss) return;
            spawnManager = FindObjectOfType<SpawnManager>();
        }

        // Update is called once per frame
        private void Update()
        {
            // Move the enemy towards the player by adding a force to its rigid body.
            // We calculate the direction from the enemy's position to the player's position using a normalized vector.
            // This gives us a unit vector pointing towards the player.
            // We then multiply this vector by the `speed` variable (defined in the editor) to determine the force to apply.
            // Finally, we apply the force to the enemy's rigid body (`enemyRB`).
            var lookDirection = (_player.transform.position - transform.position).normalized;
            var forceMagnitude = speed * Time.deltaTime;
            var force = lookDirection * forceMagnitude;
            _enemyRb.AddForce(force, ForceMode.Impulse);

            // If the enemy falls below the bottom of the screen, destroy it.
            if (transform.position.y < -10)
            {
                Destroy(gameObject);
            }

            if (!isBoss) return;
            if (Time.time > nextSpawn)
            {
                nextSpawn = Time.time + spawnInterval;
                spawnManager.SpawnMiniEnemy(miniEnemyCount);
            }
        }
    }
}