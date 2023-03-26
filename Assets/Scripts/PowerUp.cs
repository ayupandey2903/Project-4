using UnityEngine;

namespace Assets.Scripts
{
    public enum PowerUpType
    { None, PushBack, Rocket, Smash }

    public class PowerUp : MonoBehaviour
    {
        public PowerUpType powerUpType;
    }
}