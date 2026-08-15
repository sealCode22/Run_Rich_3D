using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ReferencePlayerController : MonoBehaviour
{
    private Animator animator;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    [Header("References")]
    [SerializeField]
    private ReferencePathway pathway;

    [SerializeField]
    private ReferenceSwipeTutorial swipeTutorial;

    [SerializeField]
    private ReferencePlayerStatus playerStatus;

    [Header("Movement")]
    [SerializeField]
    private float speed = 5f;

    [Header("Swipe")]
    [SerializeField]
    private float sensitivity = 0.012f;

    [SerializeField]
    private float horizontalSmooth = 12f;

    [SerializeField]
    private float trackWidth = 3f;

    [Header("Turn")]
    [SerializeField]
    private float maxRotationSpeed = 120f;

    [Header("Start")]
    [Tooltip(
        "Минимальное движение мыши/пальца " +
        "для начала игры."
    )]
    [SerializeField]
    private float startSwipeThreshold = 1f;

    private Transform origin;

    private float rotation;
    private bool pointerHeld;
    private float lastPointerX;
    private float targetLocalX;
    private bool gameStarted;
    private bool finished;

    private void Awake()
    {
        origin = transform.parent;

        animator =
            GetComponentInChildren<Animator>(true);

        if (origin == null)
        {
            Debug.LogError(
                "ReferencePlayerController: " +
                "Player должен быть дочерним " +
                "Player_Container."
            );
        }

        if (pathway == null)
        {
            Debug.LogError(
                "ReferencePlayerController: " +
                "ReferencePathway не назначен."
            );
        }

        if (swipeTutorial == null)
        {
            Debug.LogWarning(
                "ReferencePlayerController: " +
                "ReferenceSwipeTutorial не назначен."
            );
        }

        if (playerStatus == null)
        {
            Debug.LogWarning(
                "ReferencePlayerController: " +
                "ReferencePlayerStatus не назначен."
            );
        }
    }

    private void Start()
    {
        if (pathway == null)
        {
            Debug.LogError(
                "ReferencePlayerController: " +
                "Игра не может стартовать без " +
                "ReferencePathway."
            );

            enabled = false;
            return;
        }

        ResetPlayer();

        gameStarted = false;
        finished = false;

        SetMovingAnimation(false);

        if (playerStatus != null)
            playerStatus.Hide();

        if (swipeTutorial != null)
            swipeTutorial.Show();
    }

    private void Update()
    {
        if (finished)
            return;

        HandlePointer();
    }

    private void FixedUpdate()
    {
        if (origin == null ||
            pathway == null ||
            !gameStarted ||
            finished)
        {
            return;
        }

        UpdateRotation();
        MoveForward();
    }

    private void HandlePointer()
    {
        if (finished)
            return;

        if (Touchscreen.current != null)
        {
            HandleTouch();
            return;
        }

        if (Mouse.current != null)
            HandleMouse();
    }

    private void HandleTouch()
    {
        var touch =
            Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
        {
            pointerHeld = true;

            lastPointerX =
                touch.position.ReadValue().x;

            targetLocalX =
                transform.localPosition.x;
        }

        if (pointerHeld &&
            touch.press.isPressed)
        {
            float currentX =
                touch.position.ReadValue().x;

            float delta =
                currentX - lastPointerX;

            TryStartGame(delta);

            if (gameStarted &&
                !finished)
            {
                MoveSide(delta);
            }

            lastPointerX = currentX;
        }

        if (touch.press.wasReleasedThisFrame)
            pointerHeld = false;
    }

    private void HandleMouse()
    {
        if (Mouse.current.leftButton
            .wasPressedThisFrame)
        {
            pointerHeld = true;

            lastPointerX =
                Mouse.current.position.ReadValue().x;

            targetLocalX =
                transform.localPosition.x;
        }

        if (pointerHeld &&
            Mouse.current.leftButton.isPressed)
        {
            float currentX =
                Mouse.current.position.ReadValue().x;

            float delta =
                currentX - lastPointerX;

            TryStartGame(delta);

            if (gameStarted &&
                !finished)
            {
                MoveSide(delta);
            }

            lastPointerX = currentX;
        }

        if (Mouse.current.leftButton
            .wasReleasedThisFrame)
        {
            pointerHeld = false;
        }
    }

    private void TryStartGame(float delta)
    {
        if (gameStarted ||
            finished)
        {
            return;
        }

        if (Mathf.Abs(delta) <
            startSwipeThreshold)
        {
            return;
        }

        gameStarted = true;

        SetMovingAnimation(true);

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();

        if (swipeTutorial != null &&
            swipeTutorial.IsVisible())
        {
            swipeTutorial.Hide(
                ShowStatusAfterTutorial
            );
        }
        else
        {
            ShowStatusAfterTutorial();
        }
    }

    private void ShowStatusAfterTutorial()
    {
        if (finished ||
            playerStatus == null)
        {
            return;
        }

        playerStatus.Show();
    }

    private void MoveSide(float delta)
    {
        if (finished)
            return;

        targetLocalX +=
            delta * sensitivity;

        float halfWidth =
            trackWidth * 0.5f;

        targetLocalX =
            Mathf.Clamp(
                targetLocalX,
                -halfWidth,
                halfWidth
            );

        float currentX =
            transform.localPosition.x;

        float newX =
            Mathf.Lerp(
                currentX,
                targetLocalX,
                horizontalSmooth *
                Time.deltaTime
            );

        transform.localPosition =
            new Vector3(
                newX,
                transform.localPosition.y,
                transform.localPosition.z
            );
    }

    private void UpdateRotation()
    {
        if (finished ||
            pathway == null ||
            origin == null)
        {
            return;
        }

        float targetRotation =
            pathway.GetTargetRotation(
                origin.position
            );

        float delta =
            Mathf.DeltaAngle(
                rotation,
                targetRotation
            );

        float maxStep =
            maxRotationSpeed *
            Time.fixedDeltaTime;

        if (Mathf.Abs(delta) <= maxStep)
        {
            rotation = targetRotation;
        }
        else
        {
            rotation +=
                Mathf.Sign(delta) *
                maxStep;
        }

        origin.rotation =
            Quaternion.AngleAxis(
                rotation,
                Vector3.up
            );
    }

    private void MoveForward()
    {
        if (finished ||
            origin == null)
        {
            return;
        }

        origin.Translate(
            Vector3.forward *
            speed *
            Time.fixedDeltaTime,
            Space.Self
        );
    }

    public void StopAtFinish()
    {
        if (finished)
            return;

        finished = true;
        gameStarted = false;
        pointerHeld = false;

        targetLocalX =
            transform.localPosition.x;

        SetMovingAnimation(false);
    }

    public void Lose()
    {
        if (finished)
            return;

        finished = true;
        gameStarted = false;
        pointerHeld = false;

        targetLocalX =
            transform.localPosition.x;

        SetMovingAnimation(false);
    }

    private void SetMovingAnimation(bool moving)
    {
        if (animator == null)
            return;

        animator.SetBool(
            IsMovingHash,
            moving
        );
    }

    public void ResetPlayer()
    {
        if (origin == null)
            return;

        origin.rotation =
            Quaternion.identity;

        rotation = 0f;

        transform.localPosition =
            Vector3.zero;

        targetLocalX = 0f;

        pointerHeld = false;
        gameStarted = false;
        finished = false;

        SetMovingAnimation(false);

        if (pathway != null)
            pathway.ResetPath();
    }

    public bool IsGameStarted
    {
        get
        {
            return gameStarted;
        }
    }

    public bool IsFinished
    {
        get
        {
            return finished;
        }
    }
}