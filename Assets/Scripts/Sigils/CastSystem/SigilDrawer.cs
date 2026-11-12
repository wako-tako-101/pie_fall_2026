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

        //drawing feilds
        bool drawingActive = false;
        //stroke in world space
        readonly List<Vector3> strokePoints = new List<Vector3>();
        //page variables for the flattening process and orientation
        Vector3 pageOrigin, pageRight, pageUp;

        //methods
        void Awake()
        {
            stroke.useWorldSpace = true;
            stroke.positionCount = 0;
        }
        void Update()
        {
            //drawing controls
            if (drawingActive) AddPoint();
        }
        void ResetStroke()
        {
            strokePoints.Clear();
            stroke.positionCount = 0;
        }
        void StartStroke()
        {
            ResetStroke();

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
            strokePoints.Add(drawPoint.position);

            stroke.positionCount = strokePoints.Count;
            stroke.SetPosition(strokePoints.Count - 1, drawPoint.position);
        }
        void FinishStroke()
        {
            if (strokePoints.Count < minPoints)
            {
                Debug.Log("Stroke is too small");
                return;
            }

            //recognition process
            FlattenStroke(out var pagePoints);

            //recognize
            Classifier.Result result = recognizer.Recognize(pagePoints);

            //print results
            Debug.Log("Top result: " + result.Name);

            //load result
            if (result.IsConfident(recognizer.confidenceThreshold))
            {
                var cast = GetComponent<SigilCast>();
                cast.Load(result.Name);
            }
            else Debug.Log("Result did not pass the confidence threashold " + result.Name + " score: " + result.Score + " threashold " + recognizer.confidenceThreshold);
            Invoke(nameof(strokePoints.Clear), 3f);
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
        public void OnDraw(InputAction.CallbackContext context)
        {
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
