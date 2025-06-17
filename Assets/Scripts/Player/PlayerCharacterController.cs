using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public struct PlayerInput
    {
        public Vector2 Movement;
        public Quaternion Rotation;
        public bool JumpDown;
        public bool SprintDown;
    }

    public class PlayerCharacterController : NetworkBehaviour, ICharacterController
    {
        [SerializeField] private KinematicCharacterMotor motor;

        [Header("Stable Movement")] 
        public float maxStableMoveSpeed = 9f;
        public float stableMovementSharpness = 15f;
        public float maxSprintMoveSpeed = 12f;

        [Header("Air Movement")] 
        public float maxAirMoveSpeed = 15f;
        public float airAccelerationSpeed = 15f;
        public float drag = 0.1f;
        public Vector3 gravity = new Vector3(0, -30f, 0);

        [Header("Jumping")] 
        public bool allowJumpingWhenSliding = false;
        public float jumpUpSpeed = 10f;
        public float jumpScalableForwardSpeed = 0f;
        public float jumpPreGroundingGraceTime = 0f;
        public float jumpPostGroundingGraceTime = 0f;

        private PlayerInput _playerInput;
        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private Vector3 _internalVelocityAdd = Vector3.zero;

        private void Awake()
        {
            motor.CharacterController = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) return;
            // disable when not owner
            motor.enabled = false;
            enabled = false;
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            currentRotation = Quaternion.Euler(0, _playerInput.Rotation.eulerAngles.y, 0);
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            var friction = 1f;
            var moveVec = transform.forward * _playerInput.Movement.y + transform.right * _playerInput.Movement.x;
            moveVec.Normalize();

            if (Physics.Raycast(transform.position, Vector3.down, out var hit, 1))
            {
                friction = hit.collider.material.dynamicFriction;
            }

            // Ground movement
            if (motor.GroundingStatus.IsStableOnGround)
            {
                float currentVelocityMagnitude = currentVelocity.magnitude;
                Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

                // Reorient current velocity to slope
                currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                // Input reoriented to ground
                Vector3 inputRight = Vector3.Cross(moveVec, motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveVec.magnitude;

                // Desired move speed
                Vector3 desiredVelocity = reorientedInput * (_playerInput.SprintDown ? maxSprintMoveSpeed : maxStableMoveSpeed);

                if (moveVec.sqrMagnitude > 0f)
                {
                    // Add movement input gradually depending on friction
                    float controlResponsiveness = Mathf.Lerp(1f, 0.05f, 1f - friction); // less responsive when slippery

                    currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity,
                        controlResponsiveness * (1f - Mathf.Exp(-stableMovementSharpness * deltaTime)));
                }
                else
                {
                    // Only decelerate if friction > 0
                    if (friction > 0f)
                    {
                        float slowDownFactor = friction * (1f - Mathf.Exp(-stableMovementSharpness * deltaTime));
                        currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, slowDownFactor);
                    }
                    // If friction == 0, do nothing — let velocity persist
                }
            }
            // Air movement
            else
            {
                // Add move input
                if (moveVec.sqrMagnitude > 0f)
                {
                    Vector3 addedVelocity = moveVec * (airAccelerationSpeed * deltaTime);
                    Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

                    // Limit air velocity from inputs
                    if (currentVelocityOnInputsPlane.magnitude < maxAirMoveSpeed)
                    {
                        // clamp addedVel to make total vel not exceed max vel on inputs plane
                        Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity,
                            maxAirMoveSpeed);
                        addedVelocity = newTotal - currentVelocityOnInputsPlane;
                    }
                    else
                    {
                        // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                        if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                        {
                            addedVelocity =
                                Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                        }
                    }

                    // Prevent air-climbing sloped walls
                    if (motor.GroundingStatus.FoundAnyGround)
                    {
                        if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                        {
                            Vector3 perpenticularObstructionNormal = Vector3
                                .Cross(Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal),
                                    motor.CharacterUp).normalized;
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                        }
                    }

                    // Apply added velocity
                    currentVelocity += addedVelocity;
                }

                // Gravity
                currentVelocity += gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (drag * deltaTime)));
            }

            // Handle jumping
            _jumpedThisFrame = false;
            _timeSinceJumpRequested += deltaTime;
            if (_jumpRequested)
            {
                // See if we actually are allowed to jump
                if (!_jumpConsumed &&
                    ((allowJumpingWhenSliding
                         ? motor.GroundingStatus.FoundAnyGround
                         : motor.GroundingStatus.IsStableOnGround) ||
                     _timeSinceLastAbleToJump <= jumpPostGroundingGraceTime))
                {
                    // Calculate jump direction before ungrounding
                    Vector3 jumpDirection = motor.CharacterUp;
                    if (motor.GroundingStatus.FoundAnyGround && !motor.GroundingStatus.IsStableOnGround)
                    {
                        jumpDirection = motor.GroundingStatus.GroundNormal;
                    }

                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    motor.ForceUnground();

                    // Add to the return velocity and reset jump state
                    currentVelocity += (jumpDirection * jumpUpSpeed) -
                                       Vector3.Project(currentVelocity, motor.CharacterUp);
                    currentVelocity += (moveVec * jumpScalableForwardSpeed);
                    _jumpRequested = false;
                    _jumpConsumed = true;
                    _jumpedThisFrame = true;
                }
            }

            // Take into account additive velocity
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // Handle jump-related values
            {
                // Handle jumping pre-ground grace period
                if (_jumpRequested && _timeSinceJumpRequested > jumpPreGroundingGraceTime)
                {
                    _jumpRequested = false;
                }

                if (allowJumpingWhenSliding
                        ? motor.GroundingStatus.FoundAnyGround
                        : motor.GroundingStatus.IsStableOnGround)
                {
                    // If we're on a ground surface, reset jumping values
                    if (!_jumpedThisFrame)
                    {
                        _jumpConsumed = false;
                    }

                    _timeSinceLastAbleToJump = 0f;
                }
                else
                {
                    // Keep track of time since we were last able to jump (for grace period)
                    _timeSinceLastAbleToJump += deltaTime;
                }
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return !coll.CompareTag("Player");
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public void SetInput(in PlayerInput input)
        {
            _playerInput = input;

            if (input.JumpDown)
            {
                _timeSinceJumpRequested = 0f;
                _jumpRequested = true;
            }
        }
    }
}