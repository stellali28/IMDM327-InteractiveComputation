
using System.Collections.Generic;
using UnityEngine;

public class ElementFollow : MonoBehaviour
{
    public float maxSpeed = 3f;
    public float maxForce = 5f;

    public float neighborRadius = 2.5f;
    public float separationRadius = 1.0f;

    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float attractionWeight = 1.0f;

    public float randomSpread = 1.2f;

    private Vector3 velocity;
    private Vector3 targetPosition;
    private List<ElementFollow> flock;
    private float separationMultiplier = 1f;

    public void Initialize(
        Vector3 startPosition,
        List<ElementFollow> allAgents
    )
    {
        transform.position =
            startPosition + Random.insideUnitSphere * randomSpread;

        flock = allAgents;

        velocity = Random.insideUnitSphere * maxSpeed * 0.5f;
        targetPosition = startPosition;
    }

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }

    public void SetSeparationStrength(float strength)
    {
        separationMultiplier = strength;
    }

    private void Update()
    {
        if (flock == null) return;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        int neighborCount = 0;
        int separationCount = 0;

        foreach (ElementFollow other in flock)
        {
            if (other == null || other == this)
                continue;

            Vector3 difference =
                transform.position - other.transform.position;

            float distance = difference.magnitude;

            if (distance < neighborRadius && distance > 0.001f)
            {
                alignment += other.velocity;
                cohesion += other.transform.position;
                neighborCount++;
            }

            if (distance < separationRadius && distance > 0.001f)
            {
                separation += difference.normalized / distance;
                separationCount++;
            }
        }

        // Separation: move away from nearby agents.
        if (separationCount > 0)
            separation /= separationCount;

        separation = Steer(separation);

        // Alignment: match nearby agents' direction.
        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            alignment = Steer(alignment);
        }

        // Cohesion: move toward the center of nearby agents.
        if (neighborCount > 0)
        {
            cohesion /= neighborCount;
            cohesion = Steer(cohesion - transform.position);
        }

        // Attraction: move toward the left fingertip.
        Vector3 attraction =
            Steer(targetPosition - transform.position);

        // Combine all four behaviors.
        Vector3 acceleration =
            separation * separationWeight * separationMultiplier +
            alignment * alignmentWeight +
            cohesion * cohesionWeight +
            attraction * attractionWeight;

        velocity += acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
    }

    private Vector3 Steer(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 desiredVelocity = direction.normalized * maxSpeed;

        return Vector3.ClampMagnitude(
            desiredVelocity - velocity,
            maxForce
        );
    }
}
