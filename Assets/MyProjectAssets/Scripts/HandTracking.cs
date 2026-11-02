
using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public class HandTracking : MonoBehaviour
{
    public HandLandmarkerRunner handRunner;
    public VelocityGenerate flock;

    public Camera targetCamera;
    public float distanceFromCamera = 5f;

    public bool mirrorX = false;
    public bool swapHandLabels = false;

    public float trackingSmoothness = 15f;

    private Vector3 smoothedPosition;
    private bool hasSmoothedPosition;

    private void Update()
    {
        if (handRunner == null || flock == null)
            return;

        HandLandmarkerResult result = handRunner.LatestResult;

        if (result.handLandmarks == null ||
            result.handedness == null)
        {
            return;
        }

        bool foundLeft = false;
        bool foundRight = false;

        Vector3 leftPosition = Vector3.zero;
        Vector3 rightPosition = Vector3.zero;

        int handCount = Mathf.Min(
            result.handLandmarks.Count,
            result.handedness.Count
        );

        for (int i = 0; i < handCount; i++)
        {
            if (result.handLandmarks[i].landmarks == null ||
                result.handLandmarks[i].landmarks.Count <= 8 ||
                result.handedness[i].categories == null ||
                result.handedness[i].categories.Count == 0)
            {
                continue;
            }

            string label =
                result.handedness[i].categories[0].categoryName;

            if (swapHandLabels)
            {
                label = label == "Left" ? "Right" :
                        label == "Right" ? "Left" : label;
            }

            // Landmark 8 is the index fingertip.
            var tip = result.handLandmarks[i].landmarks[8];

            Vector3 worldPosition = LandmarkToWorld(
                tip.x,
                tip.y
            );

            if (label == "Left")
            {
                leftPosition = worldPosition;
                foundLeft = true;
            }
            else if (label == "Right")
            {
                rightPosition = worldPosition;
                foundRight = true;
            }
        }

        // Left index finger controls the flock target.
        if (foundLeft)
        {
            if (!hasSmoothedPosition)
            {
                smoothedPosition = leftPosition;
                hasSmoothedPosition = true;
            }

            smoothedPosition = Vector3.Lerp(
                smoothedPosition,
                leftPosition,
                trackingSmoothness * Time.deltaTime
            );

            flock.SetFlockTarget(smoothedPosition);
        }
        else
        {
            hasSmoothedPosition = false;
        }

        // Distance between hands controls flock separation.
        if (foundLeft && foundRight)
        {
            float handDistance = Vector3.Distance(
                leftPosition,
                rightPosition
            );

            float separation = Mathf.Lerp(
                0.5f,
                4f,
                Mathf.InverseLerp(0.5f, 4f, handDistance)
            );

            flock.SetSeparationStrength(separation);
        }
    }

    private Vector3 LandmarkToWorld(float x, float y)
    {
        if (!mirrorX)
        {
            x = 1f - x;
        }

        Vector3 screenPosition = new Vector3(
            x * Screen.width,
            (1f - y) * Screen.height,
            distanceFromCamera
        );

        Camera cam = targetCamera != null
            ? targetCamera
            : Camera.main;

        if (cam == null)
        {
            return Vector3.zero;
        }

        return cam.ScreenToWorldPoint(screenPosition);
    }
}
