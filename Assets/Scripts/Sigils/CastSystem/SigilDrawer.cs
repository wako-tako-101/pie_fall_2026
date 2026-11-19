using System.Collections;
using System.Collections.Generic;
using SigilClassifier;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SigilCasting
{
    public class SigilDrawer : MonoBehaviour
    {
        //system fields
        [SerializeField] Transform head;
        [SerializeField] Transform drawPoint;
        [SerializeField] SigilRecognizer recognizer;
        [SerializeField] LineRenderer stroke;
        [SerializeField] int minPoints = 10;
        [Tooltip("How far in front of the draw point the sigil is drawn, in meters. 0 draws right at the draw point.")]
        [SerializeField, Min(0f)] float drawDistance = 0f;

        [Header("Stroke Fade")]
        [Tooltip("Seconds an unrecognized stroke stays fully visible before it starts fading. A recognized sigil stays until it is saved or discarded.")]
        [SerializeField, Min(0f)] float persistTime = 3f;
        [Tooltip("How fast the stroke fades, in alpha per second. 1 = fades out over 1 second, 2 = over half a second.")]
        [SerializeField, Min(0.01f)] float fadeSpeed = 1f;

        //fade fields
        Gradient baseGradient;
        Coroutine fadeRoutine;
        SigilCast cast;

        //drawing feilds
        bool drawingActive = false;
        //true while the trigger is held and a stroke is being drawn
        public bool IsDrawing => drawingActive;
        //stroke in world space
        readonly List<Vector3> strokePoints = new List<Vector3>();
        //page variables for the flattening process and orientation
        Vector3 pageOrigin, pageRight, pageUp;

        //methods
        void Awake()
        {
            stroke.useWorldSpace = true;
            stroke.positionCount = 0;
            //remember the line's original colors so the fade can be undone
            baseGradient = stroke.colorGradient;
            cast = GetComponent<SigilCast>();
        }
        void Update()
        {
            //drawing controls
            if (drawingActive) AddPoint();
        }
        void ResetStroke()
        {
            //cancel any fade in progress and restore full opacity
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
            SetStrokeAlpha(1f);

            strokePoints.Clear();
            stroke.positionCount = 0;
        }
        //scales the line's alpha, 1 = original look, 0 = invisible
        void SetStrokeAlpha(float multiplier)
        {
            GradientAlphaKey[] alphaKeys = baseGradient.alphaKeys;
            for (int i = 0; i < alphaKeys.Length; i++)
                alphaKeys[i].alpha *= multiplier;

            Gradient faded = new Gradient();
            faded.SetKeys(baseGradient.colorKeys, alphaKeys);
            stroke.colorGradient = faded;
        }
        //fades the stroke away right now, used when a sigil is saved or discarded
        public void FadeOutStroke()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeStroke(0f));
        }
        IEnumerator FadeStroke(float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= fadeSpeed * Time.deltaTime;
                SetStrokeAlpha(Mathf.Clamp01(alpha));
                yield return null;
            }

            fadeRoutine = null;
            ResetStroke();
        }
        void StartStroke()
        {
            ResetStroke();
            //drawing again throws away a sigil that was waiting to be saved
            if (cast != null) cast.ClearPending();

            //places the page infront of the player
            //"page" is held in place even is player moves
            pageOrigin = head.position;
            pageRight = head.right;
            pageUp = head.up;
        }
        void AddPoint()
        {
            //since script is placed on hand, the transform
            //is inside hand
            //change this later so stroke appears infront of hand
            Vector3 point = drawPoint.position + drawPoint.forward * drawDistance;
            strokePoints.Add(point);

            stroke.positionCount = strokePoints.Count;
            stroke.SetPosition(strokePoints.Count - 1, point);
        }
        void FinishStroke()
        {
            if (strokePoints.Count < minPoints)
            {
                Debug.Log("Stroke is too small");
                fadeRoutine = StartCoroutine(FadeStroke(persistTime));
                return;
            }

            //recognition process
            FlattenStroke(out var pagePoints);

            //recognize
            Classifier.Result result = recognizer.Recognize(pagePoints);

            //print results
            Debug.Log("Top result: " + result.Name);

            //hold the result until the player saves or discards it
            //the stroke stays on screen while it waits, SigilCast fades it after
            if (result.IsConfident(recognizer.confidenceThreshold))
            {
                if (cast != null && cast.Propose(result.Name)) return;
            }
            else Debug.Log("Result did not pass the confidence threashold " + result.Name + " score: " + result.Score + " threashold " + recognizer.confidenceThreshold);

            //nothing to confirm, so the stroke lingers and then fades by itself
            fadeRoutine = StartCoroutine(FadeStroke(persistTime));
        }
        void FlattenStroke(out List<Vector2> flattenStroke)
        {
            flattenStroke = new List<Vector2>(strokePoints.Count);
            foreach (Vector3 point in strokePoints)
            {
                Vector3 fromHead = point - pageOrigin;
                float x = Vector3.Dot(fromHead, pageRight);
                float y = Vector3.Dot(fromHead, pageUp);
                flattenStroke.Add(new Vector2(x, y));
            }
        }
        void OnDisable()
        {
            //a stroke in progress is dropped if drawing gets turned off, like when a menu opens
            if (!drawingActive) return;
            drawingActive = false;
            ResetStroke();
        }
        public void OnDraw(InputAction.CallbackContext context)
        {
            //input events still arrive while disabled, so ignore them here
            if (!isActiveAndEnabled) return;

            if (context.started)
            {
                StartStroke();
            }
            if (context.performed)
            {
                drawingActive = true;
            }
            if (context.canceled)
            {
                drawingActive = false;
                FinishStroke();
            }
        }
    }
}
