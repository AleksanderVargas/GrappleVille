using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFOV : MonoBehaviour
{
    public float fovSpeed = 4f;
    private Camera playerCamera;
    private float targetFOV;
    private float fov;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        targetFOV = playerCamera.fieldOfView;
        fov = targetFOV;
    }

    // Update is called once per frame
    void Update()
    {
        fov = Mathf.Lerp(fov, targetFOV, Time.deltaTime * fovSpeed);
        playerCamera.fieldOfView = fov;
    }

    public void SetCameraFOV(float targetFOV)
    {
        this.targetFOV = targetFOV;
    }
}
