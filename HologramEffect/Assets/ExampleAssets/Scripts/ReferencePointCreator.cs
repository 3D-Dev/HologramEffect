using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

//
// This script allows us to create reference points with
// a prefab attached in order to visbly discern where the reference points are created.
// Reference points are a particular point in space that you are asking your device to track.
//

[RequireComponent(typeof(ARReferencePointManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class ReferencePointCreator : MonoBehaviour
{
    // This is the prefab that will appear every time a reference point is created.
    [SerializeField]
    GameObject m_ReferencePointPrefab;

    public GameObject referencePointPrefab
    {
        get => m_ReferencePointPrefab;
        set => m_ReferencePointPrefab = value;
    }

    // Removes all the reference points that have been created.
    public void RemoveAllReferencePoints()
    {
        foreach (var referencePoint in m_ReferencePoints)
        {
            m_ReferencePointManager.RemoveReferencePoint(referencePoint);
        }
        m_ReferencePoints.Clear();
    }

    // On Awake(), we obtains a reference to all the required components.
    // The ARRaycastManager allows us to perform raycasts so that we know where to place an anchor.
    // The ARPlaneManager detects surfaces we can place our objects on.
    // The ARRReferencePointManager handles the processing of all reference points and updates their position and rotation.
    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_ReferencePointManager = GetComponent<ARReferencePointManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();
        m_ReferencePoints = new List<ARReferencePoint>();
    }

    void Update()
    {
        // If there is no tap, then simply do nothing until the next call to Update().
        if (Input.touchCount == 0)
            return;

        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;
            var hitTrackableId = s_Hits[0].trackableId;
            var hitPlane = m_PlaneManager.GetPlane(hitTrackableId);

            // This attaches a reference point to the area on the plane corresponding to the raycast hit,
            // and afterwards instantiates an instance of your chosen prefab at that point.
            // This prefab instance is parented to the reference point to make sure the position of the prefab is consistent
            // with the reference point, since a reference point attached to an ARPlane will be updated automatically by the ARReferencePointManager as the ARPlane's exact position is refined.
            var referencePoint = m_ReferencePointManager.AttachReferencePoint(hitPlane, hitPose);
            Instantiate(m_ReferencePointPrefab, referencePoint.transform);

            if (referencePoint == null)
            {
                Debug.Log("Error creating reference point.");
            }
            else
            {
                // Stores the reference point so that it may be removed later.
                m_ReferencePoints.Add(referencePoint);
            }
        }
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    List<ARReferencePoint> m_ReferencePoints;

    ARRaycastManager m_RaycastManager;

    ARReferencePointManager m_ReferencePointManager;

    ARPlaneManager m_PlaneManager;
}
