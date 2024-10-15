using System;
using UnityEngine;

[Serializable]
public enum VncManagerState
{
    TrackedVnc, 
    CreateVnc,
    Waiting
}
public class VncManager : MonoBehaviour
{
    public string JsonName;
    public GameObject vncPrefab;
    public static VncManager instance;
    private Transform trackedVnc;
    private Transform tracker;
    private VncManagerState vncManagerState;

    private void Awake()
    {
        SetVncManagerState(1);
        instance = this;
    }

    public void SetTrackedVnc(Transform _trackedVnc)
    {
        if (_trackedVnc == trackedVnc)
        {
            trackedVnc = null;
            SetVncManagerState(2);
        }
        else
        {
            trackedVnc = _trackedVnc;
            SetVncManagerState(0);
        }
    }
    public void SetVncManagerState(int _vncManagerState)
    {
        switch (_vncManagerState) {
            case 0:
                vncManagerState = VncManagerState.TrackedVnc;
                break;
            case 1 :
                vncManagerState = VncManagerState.CreateVnc;
                break;
            case 2 :
                vncManagerState = VncManagerState.Waiting;
                break;
            default :
                Debug.Assert(false,"vncManagerState must be 0 or 1");
                break;
        }
    }
    public void OnDetectedTracker(Transform trackerTransform)
    {
        if (vncManagerState == VncManagerState.CreateVnc) {
            GameObject vnc = Instantiate(vncPrefab, this.transform, true);
            SetTrackedVnc(vnc.transform);
            tracker = trackerTransform;
            vncManagerState = VncManagerState.TrackedVnc;
            trackedVnc.transform.position = tracker.position;
            Vector3 orientation = Vector3.ProjectOnPlane(tracker.forward, Vector3.up);
            trackedVnc.transform.rotation = Quaternion.LookRotation(orientation);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnDetectedTracker(this.transform);
        }
        
        if (vncManagerState == VncManagerState.TrackedVnc && trackedVnc != null)
        {
            trackedVnc.transform.position = tracker.position;
            Vector3 orientation = Vector3.ProjectOnPlane(tracker.forward, Vector3.up);
            trackedVnc.transform.rotation = Quaternion.LookRotation(orientation);
        }
    }
}
