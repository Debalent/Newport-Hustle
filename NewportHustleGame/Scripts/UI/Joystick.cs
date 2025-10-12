using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NewportHustle.UI
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick Settings")]
        public float handleRange = 50f;
        public float deadZone = 0.1f;
        public bool snapToCenter = true;
        public float returnSpeed = 5f;
        
        [Header("Visual Feedback")]
        public bool showDebugInfo = false;
        public Color activeColor = Color.white;
        public Color inactiveColor = Color.gray;
        
        // Components
        private RectTransform backgroundRect;
        private RectTransform handleRect;
        private Image backgroundImage;
        private Image handleImage;
        
        // Input values
        private Vector2 inputVector;
        private bool isDragging = false;
        private Vector2 centerPosition;
        
        // Properties
        public float Horizontal => inputVector.x;
        public float Vertical => inputVector.y;
        public Vector2 Direction => inputVector;
        public float Magnitude => inputVector.magnitude;
        
        void Start()
        {
            InitializeJoystick();
        }
        
        void Update()
        {
            if (!isDragging && snapToCenter)
            {
                ReturnToCenter();
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Joystick - X: {Horizontal:F2}, Y: {Vertical:F2}, Magnitude: {Magnitude:F2}");
            }
        }
        
        private void InitializeJoystick()
        {
            // Get components
            backgroundRect = GetComponent<RectTransform>();
            backgroundImage = GetComponent<Image>();
            
            // Find handle (first child with Image component)
            foreach (Transform child in transform)
            {
                Image childImage = child.GetComponent<Image>();
                if (childImage != null)
                {
                    handleRect = child.GetComponent<RectTransform>();
                    handleImage = childImage;
                    break;
                }
            }
            
            if (handleRect == null)
            {
                Debug.LogError("Joystick handle not found! Please ensure the joystick has a child object with an Image component.");
                return;
            }
            
            // Set initial position
            centerPosition = Vector2.zero;
            SetHandlePosition(centerPosition);
            
            // Set initial colors
            SetJoystickState(false);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            SetJoystickState(true);
            OnDrag(eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            // Convert screen point to local point
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backgroundRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                // Clamp to joystick range
                localPoint = Vector2.ClampMagnitude(localPoint, handleRange);
                
                // Set handle position
                SetHandlePosition(localPoint);
                
                // Calculate input vector
                inputVector = localPoint / handleRange;
                
                // Apply dead zone
                if (inputVector.magnitude < deadZone)
                {
                    inputVector = Vector2.zero;
                }
                else
                {
                    // Normalize to remove dead zone effect
                    inputVector = inputVector.normalized * ((inputVector.magnitude - deadZone) / (1 - deadZone));
                }
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            SetJoystickState(false);
            
            if (snapToCenter)
            {
                inputVector = Vector2.zero;
            }
        }
        
        private void SetHandlePosition(Vector2 position)
        {
            if (handleRect != null)
            {
                handleRect.anchoredPosition = position;
            }
        }
        
        private void ReturnToCenter()
        {
            if (handleRect != null && handleRect.anchoredPosition != centerPosition)
            {
                Vector2 currentPos = handleRect.anchoredPosition;
                Vector2 newPos = Vector2.Lerp(currentPos, centerPosition, returnSpeed * Time.deltaTime);
                
                if (Vector2.Distance(newPos, centerPosition) < 1f)
                {
                    newPos = centerPosition;
                    inputVector = Vector2.zero;
                }
                
                SetHandlePosition(newPos);
            }
        }
        
        private void SetJoystickState(bool active)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = active ? activeColor : inactiveColor;
            }
            
            if (handleImage != null)
            {
                handleImage.color = active ? activeColor : inactiveColor;
            }
        }
        
        // Public methods for external control
        public void SetJoystickValue(Vector2 value)
        {
            inputVector = Vector2.ClampMagnitude(value, 1f);
            Vector2 handlePos = inputVector * handleRange;
            SetHandlePosition(handlePos);
        }
        
        public void ResetJoystick()
        {
            inputVector = Vector2.zero;
            SetHandlePosition(centerPosition);
            isDragging = false;
            SetJoystickState(false);
        }
        
        public void SetDeadZone(float newDeadZone)
        {
            deadZone = Mathf.Clamp01(newDeadZone);
        }
        
        public void SetHandleRange(float newRange)
        {
            handleRange = Mathf.Max(10f, newRange);
        }
        
        // Newport-specific joystick configurations
        public void ConfigureForMovement()
        {
            handleRange = 50f;
            deadZone = 0.1f;
            snapToCenter = true;
            returnSpeed = 8f;
            activeColor = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Green for movement
        }
        
        public void ConfigureForCamera()
        {
            handleRange = 60f;
            deadZone = 0.05f;
            snapToCenter = true;
            returnSpeed = 10f;
            activeColor = new Color(0.2f, 0.2f, 0.8f, 0.8f); // Blue for camera
        }
        
        public void ConfigureForVehicle()
        {
            handleRange = 45f;
            deadZone = 0.15f;
            snapToCenter = true;
            returnSpeed = 6f;
            activeColor = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Red for vehicle
        }
    }
}