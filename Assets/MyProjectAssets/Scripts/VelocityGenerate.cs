
using System.Collections.Generic;
using UnityEngine;

public class VelocityGenerate : MonoBehaviour
{
    [Header("Flock")]
    public GameObject imagePrefab;
    public Sprite[] images;
    public int startingCount = 12;
    public int maxSpheres = 100;

    [Header("Image Settings")]
    public float imageScale = 0.2f;

    [Header("Random Spawning")]
    public float spawnInterval = 2f;
    public bool randomSpawnPosition = true;
    public float spawnRadius = 3f;

    [Header("Boids Settings")]
    [Range(0.1f, 5f)]
    public float separationMultiplier = 1f;

    private readonly List<ElementFollow> flock =
        new List<ElementFollow>();

    private Vector3 flockTarget;
    private bool hasFlockTarget;
    private float spawnTimer;

    private void Start()
    {
        flockTarget = transform.position;
        hasFlockTarget = true;

        for (int i = 0; i < startingCount; i++)
        {
            SpawnImage();
        }

        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        // Automatically spawn images over time.
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            if (flock.Count < maxSpheres)
            {
                SpawnImage();
            }

            spawnTimer = spawnInterval;
        }
    }

    public void SetFlockTarget(Vector3 target)
    {
        flockTarget = target;
        hasFlockTarget = true;

        foreach (ElementFollow image in flock)
        {
            if (image != null)
                image.SetTarget(flockTarget);
        }
    }

    public void SetSeparationStrength(float strength)
    {
        separationMultiplier = Mathf.Clamp(strength, 0.1f, 5f);

        foreach (ElementFollow image in flock)
        {
            if (image != null)
                image.SetSeparationStrength(separationMultiplier);
        }
    }

    private void SpawnImage()
    {
        if (imagePrefab == null || images == null ||
            images.Length == 0)
            return;

        Vector3 startPosition = hasFlockTarget
            ? flockTarget
            : transform.position;

        // Randomize the initial spawning position.
        if (randomSpawnPosition)
        {
            startPosition += Random.insideUnitSphere * spawnRadius;
        }

        GameObject newImage = Instantiate(
            imagePrefab,
            startPosition,
            Quaternion.identity
        );

        newImage.transform.localScale =
            Vector3.one * imageScale;

        // Choose a random sprite.
        SpriteRenderer spriteRenderer =
            newImage.GetComponent<SpriteRenderer>();

        int randomIndex = Random.Range(0, images.Length);

        if (spriteRenderer != null)
            spriteRenderer.sprite = images[randomIndex];

        // Initialize the Boid.
        ElementFollow follow =
            newImage.GetComponent<ElementFollow>();

        if (follow != null)
        {
            follow.Initialize(startPosition, flock);
            follow.SetTarget(flockTarget);
            follow.SetSeparationStrength(separationMultiplier);

            flock.Add(follow);
        }
        else
        {
            Debug.LogWarning(
                "ImagePrefab is missing ElementFollow!"
            );

            Destroy(newImage);
        }
    }
}
