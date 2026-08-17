using UnityEngine;
using UnityEngine.UI;

public class BorderFitChecker : MonoBehaviour
{
    [Header("Border Check Camera")]
    [SerializeField] private Camera borderCheckCam;

    [Header("Outer Borders")]
    [SerializeField] private GameObject[] topOuter;
    [SerializeField] private GameObject[] rightOuter;
    [SerializeField] private GameObject[] leftOuter;
    [SerializeField] private GameObject[] botOuter;

    [Header("Inner Borders")]
    [SerializeField] private GameObject[] topInner;
    [SerializeField] private GameObject[] rightInner;
    [SerializeField] private GameObject[] leftInner;
    [SerializeField] private GameObject[] botInner;

    [Header("Warning UI")]
    [SerializeField] private GameObject warningBorderPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("Checking")]
    [SerializeField] private bool checkEveryFrame = true;

    private bool previousInvalidState;
    private bool warningShownForCurrentInvalidState;

    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float lastOrthographicSize;
    private float lastAspect;

    private void Awake()
    {
        if (warningBorderPanel != null)
        {
            warningBorderPanel.SetActive(false);
        }
    }

    private void Start()
    {
        SetupButtons();

        if (!ValidateSetup())
        {
            return;
        }

        StoreCameraState();

        CheckBorders(forceWarning: true);
    }

    private void Update()
    {
        if (!ValidateSetup())
        {
            return;
        }

        if (checkEveryFrame)
        {
            CheckBorders(forceWarning: false);
            return;
        }

        if (CameraChanged())
        {
            StoreCameraState();
            CheckBorders(forceWarning: false);
        }
    }

    private void SetupButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinuePressed);
            continueButton.onClick.AddListener(OnContinuePressed);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitPressed);
            quitButton.onClick.AddListener(OnQuitPressed);
        }
    }

    private bool ValidateSetup()
    {
        if (borderCheckCam == null)
        {
            Debug.LogError(
                $"{nameof(BorderFitChecker)}: Border Check Camera is not assigned.",
                this);

            return false;
        }

        if (warningBorderPanel == null)
        {
            Debug.LogError(
                $"{nameof(BorderFitChecker)}: Warning Border Panel is not assigned.",
                this);

            return false;
        }

        return true;
    }

    private void CheckBorders(bool forceWarning)
    {
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(borderCheckCam);

        bool outerBorderDetected =
            IsAnyOuterBorderVisible(cameraPlanes);

        bool innerBordersInvalid =
            !AreAllInnerBordersFullyVisible(cameraPlanes);

        bool invalidZone =
            outerBorderDetected || innerBordersInvalid;

        HandleInvalidState(invalidZone, forceWarning);
    }

    private bool IsAnyOuterBorderVisible(Plane[] cameraPlanes)
    {
        if (IsAnyObjectVisible(topOuter, cameraPlanes))
        {
            return true;
        }

        if (IsAnyObjectVisible(rightOuter, cameraPlanes))
        {
            return true;
        }

        if (IsAnyObjectVisible(leftOuter, cameraPlanes))
        {
            return true;
        }

        if (IsAnyObjectVisible(botOuter, cameraPlanes))
        {
            return true;
        }

        return false;
    }

    private bool AreAllInnerBordersFullyVisible(Plane[] cameraPlanes)
    {
        if (!AreAllObjectsFullyVisible(topInner, cameraPlanes))
        {
            return false;
        }

        if (!AreAllObjectsFullyVisible(rightInner, cameraPlanes))
        {
            return false;
        }

        if (!AreAllObjectsFullyVisible(leftInner, cameraPlanes))
        {
            return false;
        }

        if (!AreAllObjectsFullyVisible(botInner, cameraPlanes))
        {
            return false;
        }

        return true;
    }

    private bool IsAnyObjectVisible(
        GameObject[] objects,
        Plane[] cameraPlanes)
    {
        if (objects == null || objects.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject target = objects[i];

            if (target == null)
            {
                continue;
            }

            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>(true);

            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer renderer = renderers[r];

                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (GeometryUtility.TestPlanesAABB(
                        cameraPlanes,
                        renderer.bounds))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool AreAllObjectsFullyVisible(
        GameObject[] objects,
        Plane[] cameraPlanes)
    {
        if (objects == null || objects.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject target = objects[i];

            if (target == null)
            {
                return false;
            }

            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return false;
            }

            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer renderer = renderers[r];

                if (renderer == null || !renderer.enabled)
                {
                    return false;
                }

                if (!IsRendererFullyInsideCamera(
                        renderer,
                        cameraPlanes))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsRendererFullyInsideCamera(
        Renderer renderer,
        Plane[] cameraPlanes)
    {
        Bounds bounds = renderer.bounds;

        Vector3[] corners = new Vector3[8];

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(max.x, min.y, min.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(max.x, max.y, min.z);

        corners[4] = new Vector3(min.x, min.y, max.z);
        corners[5] = new Vector3(max.x, min.y, max.z);
        corners[6] = new Vector3(min.x, max.y, max.z);
        corners[7] = new Vector3(max.x, max.y, max.z);

        for (int p = 0; p < cameraPlanes.Length; p++)
        {
            Plane plane = cameraPlanes[p];

            for (int c = 0; c < corners.Length; c++)
            {
                if (plane.GetDistanceToPoint(corners[c]) < 0f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void HandleInvalidState(
        bool invalidZone,
        bool forceWarning)
    {
        if (!invalidZone)
        {
            previousInvalidState = false;
            warningShownForCurrentInvalidState = false;

            if (warningBorderPanel != null &&
                warningBorderPanel.activeSelf)
            {
                warningBorderPanel.SetActive(false);
            }

            return;
        }

        bool enteredInvalidState =
            !previousInvalidState;

        previousInvalidState = true;

        if (forceWarning || enteredInvalidState)
        {
            ShowWarningPanel();
        }
    }

    private void ShowWarningPanel()
    {
        if (warningBorderPanel == null)
        {
            return;
        }

        if (warningShownForCurrentInvalidState)
        {
            return;
        }

        warningShownForCurrentInvalidState = true;

        warningBorderPanel.SetActive(true);
    }

    private void OnContinuePressed()
    {
        if (warningBorderPanel != null)
        {
            warningBorderPanel.SetActive(false);
        }
    }

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void StoreCameraState()
    {
        lastCameraPosition = borderCheckCam.transform.position;
        lastCameraRotation = borderCheckCam.transform.rotation;
        lastOrthographicSize = borderCheckCam.orthographicSize;
        lastAspect = borderCheckCam.aspect;
    }

    private bool CameraChanged()
    {
        if (borderCheckCam.transform.position != lastCameraPosition)
        {
            return true;
        }

        if (borderCheckCam.transform.rotation != lastCameraRotation)
        {
            return true;
        }

        if (!Mathf.Approximately(
                borderCheckCam.orthographicSize,
                lastOrthographicSize))
        {
            return true;
        }

        if (!Mathf.Approximately(
                borderCheckCam.aspect,
                lastAspect))
        {
            return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinuePressed);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitPressed);
        }
    }
}