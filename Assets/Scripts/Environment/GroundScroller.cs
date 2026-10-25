using UnityEngine;

namespace ChromeDino.Environment
{
    // Continuous ground scrolling and looping
    public class GroundScroller : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 6f;
        [SerializeField] private float resetThresholdX = -20f;
        [SerializeField] private float repositionOffsetX = 40f;

        private void Update()
        {
            // Move ground tile left
            transform.Translate(Vector3.left * (scrollSpeed * Time.deltaTime));

            // Reposition tile forward to create infinite loop
            if (transform.position.x <= resetThresholdX)
            {
                transform.position += new Vector3(repositionOffsetX, 0f, 0f);
            }
        }
    }
}
