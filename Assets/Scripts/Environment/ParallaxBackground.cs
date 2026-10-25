using UnityEngine;

namespace ChromeDino.Environment
{
    // Parallax background layer scrolling and looping
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 2f;
        [SerializeField] private float spriteWidth = 20f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;

            // Auto calculate sprite width if needed
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && spriteWidth <= 0.1f)
            {
                spriteWidth = sr.bounds.size.x;
            }
        }

        private void Update()
        {
            // Move background layer left
            transform.Translate(Vector3.left * (scrollSpeed * Time.deltaTime));

            // Reset position to create seamless loop
            if (transform.position.x <= startPosition.x - spriteWidth)
            {
                transform.position += new Vector3(spriteWidth, 0f, 0f);
            }
        }
    }
}
