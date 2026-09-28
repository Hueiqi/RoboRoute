using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RoboRoute
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class RobotMotor : MonoBehaviour
    {
        public ShiftController shift;
        CharacterController controller;
        public float DistanceTravelled { get; private set; }
        void Awake() { controller = GetComponent<CharacterController>(); }

        public static bool InteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.E);
#else
            return false;
#endif
        }

        void Update()
        {
            if (shift == null || shift.Paused || shift.State.Complete) return;
            Vector2 axis = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null)
            {
                axis.x = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0) - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0);
                axis.y = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1 : 0) - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1 : 0);
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            axis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
            Vector3 direction = Vector3.ClampMagnitude(new Vector3(axis.x, 0, axis.y), 1);
            Vector3 before = transform.position;
            float speed = shift.State.carrying ? 3.2f : 4.5f;
            if (shift.State.battery <= 0) direction = Vector3.zero;
            controller.Move((direction * speed + Vector3.down * 4) * Time.deltaTime);
            Vector3 delta = transform.position - before;
            delta.y = 0;
            float distance = delta.magnitude;
            DistanceTravelled += distance;
            shift.State.battery = Mathf.Max(0, shift.State.battery - distance * .65f);
            if (direction.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 12 * Time.deltaTime);
        }

        public void ReturnToDock()
        {
            controller.enabled = false;
            transform.position = new Vector3(-10, .1f, -7);
            controller.enabled = true;
        }
    }
}
