using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProceduralFishAnimation : MonoBehaviour
{
    public enum AnimationMode { Idle, Flee, Track }

    private struct ModeSettings
    {
        public float amplitude;
        public float frequency;
    }

    [Header("Bone References")]
    [SerializeField] private SkinnedMeshRenderer skinnedMesh;
    [SerializeField] private bool autoFindBones = true;
    [SerializeField] private Transform[] bones = new Transform[0];


    [SerializeField] private Transform target;
    [SerializeField] private float turnSpeed = 5f;

    private List<Transform> animatedBones = new List<Transform>();
    private Vector3[] boneBindPoses;
    private Quaternion[] boneBindRotations;

    [SerializeField] private Dictionary<AnimationMode, ModeSettings> modeSettings = new Dictionary<AnimationMode, ModeSettings>
    {
        { AnimationMode.Idle, new ModeSettings { amplitude = 10f, frequency = 1f } },
        { AnimationMode.Flee, new ModeSettings { amplitude = 15f, frequency = 10f } },
        { AnimationMode.Track, new ModeSettings { amplitude = 10f, frequency = 9f } }
    };


    private float swimAmplitude = 20f;
    private float swimFrequency = 2f;
    private AnimationMode currentMode = AnimationMode.Idle;
    
    // Animation constants
    private float phaseOffset = 0.5f;
    private float followSpeed = 10f;
    private int pivotBoneIndex = 0;
    private bool useCurves = true;
    private bool isTracking = false;

    [SerializeField] private AnimationCurve amplitudeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve phaseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);


    private Canvas uiCanvas;
    private VerticalLayoutGroup buttonContainer;

    void Start()
    {
        if (autoFindBones && skinnedMesh != null)
        {
            GetBonesFromSkinnedMesh();
        }
        else if (bones.Length > 0)
        {
            animatedBones = new List<Transform>(bones);
        }

        // Store initial bind poses for resetting
        if (animatedBones.Count > 0)
        {
            boneBindPoses = new Vector3[animatedBones.Count];
            boneBindRotations = new Quaternion[animatedBones.Count];
            for (int i = 0; i < animatedBones.Count; i++)
            {
                boneBindPoses[i] = animatedBones[i].localPosition;
                boneBindRotations[i] = animatedBones[i].localRotation;
            }
        }


        CreateAnimationModeUI();
        SetAnimationMode(AnimationMode.Idle);
    }


    // Update is called once per frame
    void Update()
    {
        if (isTracking && target != null) 
            TrackTarget(target);
        
        if (useCurves)
            AnimateFishBonesWithCurves();
        else
            AnimateFishBones();
    }
    
    void AnimateFishBones()
    {
        if (animatedBones == null || animatedBones.Count == 0) return;

        float time = Time.time;
        int boneCountActual = animatedBones.Count;
        int pivot = Mathf.Clamp(pivotBoneIndex, 0, boneCountActual - 1);
        float maxDist = Mathf.Max(pivot, boneCountActual - 1 - pivot);

        for (int i = 0; i < boneCountActual; i++)
        {
            Transform curr = animatedBones[i];
            
            // Calculate amplitude factor based on distance from pivot
            float distFromPivot = Mathf.Abs(i - pivot);
            float amplitudeFactor = maxDist > 0 ? distFromPivot / maxDist : 0f;
            
            // Calculate phase based on position along body
            float distAlongBody = (i - pivot);
            float phase = -distAlongBody * phaseOffset;
            
            // Calculate wave offset
            float offset = Mathf.Sin(time * swimFrequency + phase) * swimAmplitude * amplitudeFactor;
            
            // Apply rotation to bone - rotate around local Z axis for side-to-side movement
            Quaternion baseRot = boneBindRotations[i];
            Quaternion waveRotation = Quaternion.AngleAxis(offset, Vector3.forward);
            curr.localRotation = baseRot * waveRotation;
        }
    }


    // Curve-driven wave animation: amplitude and phase are sampled from AnimationCurves
    void AnimateFishBonesWithCurves()
    {
        if (animatedBones == null || animatedBones.Count == 0) return;

        int count = animatedBones.Count;
        float time = Time.time;

        for (int i = 0; i < count; i++)
        {
            Transform curr = animatedBones[i];

            // Normalized position along the body (0 = head, 1 = tail)
            float t = count > 1 ? (float)i / (count - 1) : 0f;

            // Sample curves
            float ampFactor = amplitudeCurve.Evaluate(t);
            float phaseFactor = phaseCurve.Evaluate(t);

            float phase = -phaseFactor * phaseOffset * count;
            float offset = Mathf.Sin(time * swimFrequency + phase) * swimAmplitude * ampFactor;
            
            // Apply rotation to bone - rotate around local Z axis
            Quaternion baseRot = boneBindRotations[i];
            Quaternion waveRotation = Quaternion.AngleAxis(offset, Vector3.forward);
            curr.localRotation = baseRot * waveRotation;
        }
    }

    void TrackTarget(Transform target)
    {
        if (target == null) return;

        Vector3 directionToTarget = (target.position - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime * 60f);

        transform.position += transform.forward * Time.deltaTime * 2f;
    }

    public void SetAnimationMode(AnimationMode mode)
    {
        currentMode = mode;
        ModeSettings settings = modeSettings[mode];
        swimAmplitude = settings.amplitude;
        swimFrequency = settings.frequency;
        
        // Enable tracking only for Track mode
        isTracking = (mode == AnimationMode.Track);

        Debug.Log($"Animation Mode: {mode} | Amplitude: {swimAmplitude} | Frequency: {swimFrequency}");
    }

    private void CreateAnimationModeUI()
    {
        // Create EventSystem if it doesn't exist
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObj = new GameObject("AnimationUI");
        canvasObj.name = "AnimationModeUI";
        uiCanvas = canvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject containerObj = new GameObject("ButtonContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);
        containerRect.sizeDelta = new Vector2(200, 350);

        buttonContainer = containerObj.AddComponent<VerticalLayoutGroup>();
        buttonContainer.spacing = 10;
        buttonContainer.childForceExpandHeight = false;
        buttonContainer.childForceExpandWidth = true;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(containerObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.text = "Animation Mode";
        titleText.font.material.name = "Arial";
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(200, 30);
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 30;
        titleLayout.preferredWidth = 200;

        foreach (AnimationMode mode in System.Enum.GetValues(typeof(AnimationMode)))
        {
            CreateModeButton(containerObj, mode);
        }
    }

    private void CreateModeButton(GameObject parent, AnimationMode mode)
    {
        GameObject buttonObj = new GameObject(mode.ToString() + "Button");
        buttonObj.transform.SetParent(parent.transform, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 40);

        LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
        buttonLayout.preferredHeight = 40;
        buttonLayout.preferredWidth = 200;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.text = mode.ToString().ToUpper();
        buttonText.font.material.name = "Arial";
        buttonText.fontSize = 16;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        button.onClick.AddListener(() => SetAnimationMode(mode));
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.1f, 0.6f, 0.2f, 1f);
        button.colors = colors;
    }


    private void GetBonesFromSkinnedMesh()
    {
        if (skinnedMesh == null)
        {
            Debug.LogWarning("SkinnedMeshRenderer not assigned!");
            return;
        }

        Transform[] meshBones = skinnedMesh.bones;
        if (meshBones == null || meshBones.Length == 0)
        {
            Debug.LogWarning("SkinnedMeshRenderer has no bones!");
            return;
        }

        animatedBones = new List<Transform>(meshBones);
        Debug.Log($"Found {animatedBones.Count} bones in SkinnedMeshRenderer");
    }


    public void SetBones(Transform[] newBones)
    {
        bones = newBones;
        animatedBones = new List<Transform>(newBones);
        
        if (animatedBones.Count > 0)
        {
            boneBindPoses = new Vector3[animatedBones.Count];
            boneBindRotations = new Quaternion[animatedBones.Count];
            for (int i = 0; i < animatedBones.Count; i++)
            {
                boneBindPoses[i] = animatedBones[i].localPosition;
                boneBindRotations[i] = animatedBones[i].localRotation;
            }
        }
    }

    [ContextMenu("Reset Bones")]
    public void ResetBones()
    {
        if (boneBindPoses == null || boneBindRotations == null)
            return;

        for (int i = 0; i < animatedBones.Count; i++)
        {
            animatedBones[i].localPosition = boneBindPoses[i];
            animatedBones[i].localRotation = boneBindRotations[i];
        }
    }
}
