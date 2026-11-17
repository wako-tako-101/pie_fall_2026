using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Leads a VR player through a series of points of interest (empty GameObjects) in order.
/// - Floats toward the current point with smooth movement, hovering at a set height.
/// - If the player falls too far behind (or wanders off), the guide comes back toward them
///   and "nags": it darts back and forth toward the destination, wiggles, and bobs faster
///   while looking at the player, until the player steps toward it and follows.
/// - When it reaches a point and the player is within that point's trigger radius,
///   it fires that region's dialog event, then moves on to the next point.
///
/// Assign the player's HEAD (the XR Origin's Main Camera) as the Player reference,
/// not the rig root, so physical room-scale walking is counted too.
/// </summary>
public class VRGuideCompanion : MonoBehaviour
{
    [Serializable]
    public class PointOfInterest
    {
        [Tooltip("Just for your own organisation, e.g. 'Old Mill'")]
        public string regionName;

        [Tooltip("Empty GameObject placed where the guide should lead the player.")]
        public Transform location;

        [Tooltip("How close the player must be (horizontally, in metres) to the waiting guide to start the dialog.")]
        public float triggerRadius = 3f;

        [Tooltip("Hook your dialog system up here (e.g. DialogueManager.StartDialogue).")]
        public UnityEvent onPlayerArrived;
    }

    private enum State { Guiding, Nagging, WaitingForPlayer, DialogActive, Pausing, Finished }

    [Header("References")]
    [Tooltip("The player's head: the Main Camera under your XR Origin.")]
    [SerializeField] private Transform player;
    [SerializeField] private List<PointOfInterest> pointsOfInterest = new List<PointOfInterest>();

    [Header("Movement")]
    [Tooltip("Lower = snappier, higher = floatier.")]
    [SerializeField] private float smoothTime = 0.7f;
    [Tooltip("Metres per second. Keep this modest in VR to avoid discomfort when the player tracks it.")]
    [SerializeField] private float maxSpeed = 2.5f;
    [Tooltip("How close the guide must get to a point to count as 'arrived'.")]
    [SerializeField] private float arriveDistance = 0.3f;
    [Tooltip("How high above each point's position the guide hovers.")]
    [SerializeField] private float hoverHeight = 1.5f;

    [Header("Guiding Behaviour")]
    [Tooltip("If the player is farther than this (horizontally) from the guide, the guide comes back and nags.")]
    [SerializeField] private float nagStartDistance = 6f;
    [Tooltip("Delay after a dialog is triggered before heading to the next point.")]
    [SerializeField] private float pauseAfterDialog = 1f;
    [Tooltip("If true, the guide stays put until something calls ResumeGuiding() (e.g. when dialog ends).")]
    [SerializeField] private bool waitForDialogToFinish = true;

    [Header("Nagging")]
    [Tooltip("How far from the player the guide hovers while nagging (towards the destination).")]
    [SerializeField] private float nagApproachDistance = 3.5f;
    [Tooltip("Once the player gets this close to the guide, it stops nagging and leads on. Must be smaller than the approach distance.")]
    [SerializeField] private float resumeLeadDistance = 2.5f;
    [Tooltip("If the player moves farther than this from the guide's nag spot, it moves to a new spot near them.")]
    [SerializeField] private float nagRepositionDistance = 6f;
    [Tooltip("How far the guide darts toward the destination with each 'come on!' beckon.")]
    [SerializeField] private float nagAmplitude = 1f;
    [Tooltip("Speed of the beckoning motion (radians per second). Higher = more insistent.")]
    [SerializeField] private float nagFrequency = 4f;
    [Tooltip("Side-to-side wiggle added to the beckon.")]
    [SerializeField] private float nagSideWiggle = 0.25f;
    [Tooltip("Smoothing while nagging. Lower = more twitchy.")]
    [SerializeField] private float nagSmoothTime = 0.3f;
    [Tooltip("Speed multiplier while nagging, so it can catch up and dart around.")]
    [SerializeField] private float nagSpeedMultiplier = 1.5f;
    [Tooltip("How far below the player's eye level the guide hovers while nagging.")]
    [SerializeField] private float nagHeightBelowEyes = 0.2f;
    [Tooltip("How much faster the idle bob gets while nagging.")]
    [SerializeField] private float nagBobMultiplier = 2.5f;

    [Header("Rotation")]
    [SerializeField] private float turnSpeed = 4f;

    [Header("Idle Bob")]
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobSpeed = 1.5f;

    [Header("Events")]
    [Tooltip("Handy for a 'Hey, over here!' voice line or sound.")]
    public UnityEvent onNagStarted;
    public UnityEvent onNagEnded;
    public UnityEvent onAllPointsVisited;

    private State state = State.Guiding;
    private int currentIndex = 0;
    private Vector3 anchor;      // the "true" position; the bob offset is added on top
    private Vector3 velocity;    // used by SmoothDamp
    private Vector3 nagBase;     // the spot near the player the guide hovers around while nagging
    private float bobPhase;

    private PointOfInterest Current => pointsOfInterest[currentIndex];

    private void OnValidate()
    {
        resumeLeadDistance = Mathf.Min(resumeLeadDistance, nagApproachDistance - 0.5f);
    }

    private void Start()
    {
        anchor = transform.position;

        if (pointsOfInterest.Count == 0)
        {
            Debug.LogWarning("VRGuideCompanion: no points of interest assigned.", this);
            state = State.Finished;
        }

        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (player != null && state != State.Finished)
        {
            switch (state)
            {
                case State.Guiding:          UpdateGuiding();  break;
                case State.Nagging:          UpdateNagging();  break;
                case State.WaitingForPlayer: UpdateWaiting();  break;
                // DialogActive and Pausing: hover in place.
            }

            UpdateRotation();
        }

        float speed = state == State.Nagging ? bobSpeed * nagBobMultiplier : bobSpeed;
        bobPhase += speed * Time.deltaTime;
        transform.position = anchor + Vector3.up * (Mathf.Sin(bobPhase) * bobHeight);
    }

    // ---- Leading ----

    private void UpdateGuiding()
    {
        if (Current.location == null)
        {
            Debug.LogWarning($"VRGuideCompanion: point {currentIndex} has no location assigned, skipping.", this);
            AdvanceToNextPoint();
            return;
        }

        if (PlayerTooFar())
        {
            EnterNagging();
            return;
        }

        Vector3 target = PointTarget(Current);
        anchor = Vector3.SmoothDamp(anchor, target, ref velocity, smoothTime, maxSpeed);

        if (Vector3.Distance(anchor, target) <= arriveDistance)
        {
            state = State.WaitingForPlayer;
        }
    }

    // ---- Nagging ----

    private void EnterNagging()
    {
        state = State.Nagging;
        nagBase = ComputeNagBase();
        onNagStarted?.Invoke();
    }

    private void UpdateNagging()
    {
        // The player has stepped toward us: they're following, so lead on.
        if (HorizontalDistance(player.position, anchor) <= resumeLeadDistance)
        {
            state = State.Guiding;
            onNagEnded?.Invoke();
            return;
        }

        // The player drifted away from our nag spot: go find them again.
        if (HorizontalDistance(player.position, nagBase) > nagRepositionDistance)
        {
            nagBase = ComputeNagBase();
        }

        Vector3 dirToPoint = Flat(PointTarget(Current) - nagBase);
        dirToPoint = dirToPoint.sqrMagnitude > 0.001f ? dirToPoint.normalized : Flat(player.forward);
        Vector3 side = Vector3.Cross(Vector3.up, dirToPoint);

        // "Come on!": dart toward the destination and back, with a little wiggle.
        float phase = Time.time * nagFrequency;
        float dart = (Mathf.Sin(phase) * 0.5f + 0.5f) * nagAmplitude;
        float wiggle = Mathf.Sin(phase * 2f) * nagSideWiggle;

        Vector3 target = nagBase + dirToPoint * dart + side * wiggle;
        anchor = Vector3.SmoothDamp(anchor, target, ref velocity, nagSmoothTime, maxSpeed * nagSpeedMultiplier);
    }

    /// <summary>
    /// A spot between the player and the destination, at eye level, where the guide hovers while nagging.
    /// </summary>
    private Vector3 ComputeNagBase()
    {
        Vector3 toPoint = Flat(PointTarget(Current) - player.position);
        float pointDistance = toPoint.magnitude;
        Vector3 dir = pointDistance > 0.01f ? toPoint / pointDistance : Flat(player.forward);

        float distance = Mathf.Min(nagApproachDistance, pointDistance);
        Vector3 spot = player.position + dir * distance;
        spot.y = player.position.y - nagHeightBelowEyes;
        return spot;
    }

    // ---- Waiting at a point / dialog ----

    private void UpdateWaiting()
    {
        // Settle gently into place while we wait.
        anchor = Vector3.SmoothDamp(anchor, PointTarget(Current), ref velocity, smoothTime, maxSpeed);

        if (HorizontalDistance(player.position, anchor) <= Current.triggerRadius)
        {
            TriggerDialog();
        }
        else if (PlayerTooFar())
        {
            // Player isn't coming: go and nag them (they'll be led back here afterwards).
            EnterNagging();
        }
    }

    private void TriggerDialog()
    {
        Current.onPlayerArrived?.Invoke();

        if (waitForDialogToFinish)
        {
            state = State.DialogActive;   // wait for ResumeGuiding()
        }
        else
        {
            StartCoroutine(PauseThenAdvance());
        }
    }

    /// <summary>
    /// Call this from your dialog system when the conversation ends
    /// (needed if 'Wait For Dialog To Finish' is ticked).
    /// </summary>
    public void ResumeGuiding()
    {
        if (state == State.DialogActive)
        {
            StartCoroutine(PauseThenAdvance());
        }
    }

    private IEnumerator PauseThenAdvance()
    {
        state = State.Pausing;
        yield return new WaitForSeconds(pauseAfterDialog);
        AdvanceToNextPoint();
    }

    private void AdvanceToNextPoint()
    {
        currentIndex++;

        if (currentIndex >= pointsOfInterest.Count)
        {
            state = State.Finished;
            onAllPointsVisited?.Invoke();
        }
        else
        {
            state = State.Guiding;
        }
    }

    // ---- Rotation ----

    private void UpdateRotation()
    {
        Vector3 lookDir;

        if (state == State.Guiding && Flat(velocity).sqrMagnitude > 0.05f)
        {
            // Face the way we're travelling.
            lookDir = Flat(velocity);
        }
        else
        {
            // Face the player when nagging, waiting, or in dialog.
            lookDir = Flat(player.position - anchor);
        }

        if (lookDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    // ---- Helpers ----

    private bool PlayerTooFar()
    {
        return HorizontalDistance(anchor, player.position) > nagStartDistance;
    }

    private Vector3 PointTarget(PointOfInterest poi)
    {
        return poi.location.position + Vector3.up * hoverHeight;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private static Vector3 Flat(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    // ---- Editor visualisation ----

    private void OnDrawGizmosSelected()
    {
        // Orange: how far the player can be before the guide starts nagging.
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, nagStartDistance);

        if (pointsOfInterest == null) return;

        for (int i = 0; i < pointsOfInterest.Count; i++)
        {
            var poi = pointsOfInterest[i];
            if (poi.location == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(poi.location.position, poi.triggerRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(poi.location.position + Vector3.up * hoverHeight, 0.15f);

            if (i + 1 < pointsOfInterest.Count && pointsOfInterest[i + 1].location != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(poi.location.position + Vector3.up * hoverHeight,
                                pointsOfInterest[i + 1].location.position + Vector3.up * hoverHeight);
            }
        }
    }
}
