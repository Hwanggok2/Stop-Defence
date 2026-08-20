using System.Collections;
using UnityEngine;

public class ChainLightningBolt : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    Sprite[] frames;

    [SerializeField]
    float frameInterval = 0.05f;

    [SerializeField]
    float lifeTime = 0.3f;

    public void Play(Vector3 from, Vector3 to)
    {
        transform.position = from;

        Vector3 diff = to - from;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        float distance = diff.magnitude;
        float baseWidth = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.size.x : 1f;
        if (baseWidth <= 0f) baseWidth = 1f;
        transform.localScale = new Vector3(distance / baseWidth, 1f, 1f);

        StopAllCoroutines();
        StartCoroutine(AnimateAndDestroy());
    }

    IEnumerator AnimateAndDestroy()
    {
        float elapsed = 0f;
        int frameIndex = 0;

        while (elapsed < lifeTime)
        {
            if (frames.Length > 0)
            {
                spriteRenderer.sprite = frames[frameIndex % frames.Length];
                frameIndex++;
            }

            yield return new WaitForSeconds(frameInterval);
            elapsed += frameInterval;
        }

        Destroy(gameObject);
    }
}
