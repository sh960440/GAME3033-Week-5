using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

namespace Character
{
    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] private float WalkSpeed;
        [SerializeField] private float RunSpeed;
        [SerializeField] private float JumpForce;

        [SerializeField] private LayerMask JumpLayerMask;
        [SerializeField] private float JumpThreshold = 0.1f;
        [SerializeField] private float JumpLandingCheckDelay = 0.1f;

        //Components
        private PlayerController PlayerController;
        private Animator PlayerAnimator;
        private Rigidbody PlayerRigidbody;
        private NavMeshAgent PlayerNavMeshAgent;
        
        //References
        private Transform PlayerTransform;

        private Vector2 InputVector = Vector2.zero;
        private Vector3 MoveDirection = Vector3.zero;
        
        //Animator Hashes
        private readonly int MovementXHash = Animator.StringToHash("MovementX");
        private readonly int MovementYHash = Animator.StringToHash("MovementY");
        private readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private readonly int IsRunningHash = Animator.StringToHash("IsRunning");

        private void Awake()
        {
            PlayerTransform = transform;
            PlayerController = GetComponent<PlayerController>();
            PlayerAnimator = GetComponent<Animator>();
            PlayerRigidbody = GetComponent<Rigidbody>();
            PlayerNavMeshAgent = GetComponent<NavMeshAgent>();
        }


        /// <summary>
        /// Get's notified when the player moves, called by the PlayerInput Component.
        /// </summary>
        /// <param name="value"></param>
        public void OnMovement(InputValue value)
        {
            InputVector = value.Get<Vector2>();

            //Debug.Log(InputVector);
            
            PlayerAnimator.SetFloat(MovementXHash, InputVector.x);
            PlayerAnimator.SetFloat(MovementYHash, InputVector.y);
        }
        
        /// <summary>
        /// Get's notified when the player starts and ends running, Called by the PlayerInput component
        /// </summary>
        /// <param name="value"></param>
        public void OnRun(InputValue value)
        {
            Debug.Log(value.isPressed);
            PlayerController.IsRunning = value.isPressed;
            PlayerAnimator.SetBool(IsRunningHash, value.isPressed);
        }
        
        /// <summary>
        /// Get's notified when the player presses the jump key, Called by the PlayerInput component
        /// </summary>
        /// <param name="value"></param>
        public void OnJump(InputValue value)
        {
            if (PlayerController.IsJumping) return;

            PlayerNavMeshAgent.isStopped = true;
            PlayerNavMeshAgent.enabled = false;

            PlayerController.IsJumping = value.isPressed;
            PlayerAnimator.SetBool(IsJumpingHash, value.isPressed);
            
            PlayerRigidbody.AddForce((PlayerTransform.up + MoveDirection) * JumpForce, ForceMode.Impulse);

            InvokeRepeating(nameof(LandingCheck), JumpLandingCheckDelay, 0.1f);
        }

        private void LandingCheck()
        {
            if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 100f, JumpLayerMask))
            {
                Debug.Log(hit.distance);
                if (!(hit.distance < JumpThreshold) || !PlayerController.IsJumping) return;
                
                PlayerNavMeshAgent.enabled = true;
                PlayerNavMeshAgent.isStopped = false;

                PlayerController.IsJumping = false;
                PlayerAnimator.SetBool(IsJumpingHash, false);

                CancelInvoke(nameof(LandingCheck));
            }
        }


        private void Update()
        {
            if (PlayerController.IsJumping) return;

            MoveDirection = PlayerTransform.forward * InputVector.y + PlayerTransform.right * InputVector.x;

            float currentSpeed = PlayerController.IsRunning ? RunSpeed : WalkSpeed;

            Vector3 movementDirection = MoveDirection * (currentSpeed * Time.deltaTime);

            

            //if (!(InputVector.magnitude > 0)) MoveDirection = Vector3.zero;
            
            

            

            

            //PlayerTransform.position += movementDirection;
            PlayerNavMeshAgent.Move(movementDirection);
        }
    }
}
