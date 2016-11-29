using UnityEngine;

namespace NetworkingAutoMatch
{
    public class Bullet : MonoBehaviour
    {
        public void OnCollisionEnter(Collision collision)
        {
            GameObject hit = collision.gameObject;
            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(10);
            }
            Destroy(gameObject);
        }
    }
}
