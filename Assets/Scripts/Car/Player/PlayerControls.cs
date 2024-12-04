using Cinemachine;
using Destructors;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapons;

namespace Car.Player
{
    public class PlayerControls : MonoBehaviour
    {
        public static PlayerControls Instance;
        
        [Header("References")]
        public CarMovement playerCarMovement;
        public GunWeapon gunWeapon;
        public CinemachineVirtualCamera virtualCamera;
        public PauseManager pauseManager;
        public RadioManager radioManager;
        
        // Input Actions
        public PlayerInput playerInput;
        private InputAction _movementInput;
        private InputAction _driftInput;
        private InputAction _fireInput;
        private InputAction _pauseInput;
        private InputAction _showRadioInput;
        private InputAction _playRadioInput;
        private InputAction _nextRadioInput;
        private InputAction _previousRadioInput;
        
        // Input Values
        private Vector2 _movementValue;
        
        private void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }
            //Car controls
            _movementInput = playerInput.actions["Movement"];
            _driftInput = playerInput.actions["Drift"];
            _fireInput = playerInput.actions["Fire"];
            //Pause controls (Handles Escape key)
            _pauseInput = playerInput.actions["Pause"];
            //Radio controls (V, B, N, M keys)
            _showRadioInput = playerInput.actions["Show Radio"];
            _playRadioInput = playerInput.actions["Play Radio"];
            _nextRadioInput = playerInput.actions["Next Track"];
            _previousRadioInput = playerInput.actions["Previous Track"];
        }

        private void FixedUpdate()
        {
            // Movement
            _movementValue = _movementInput.ReadValue<Vector2>();
            
            playerCarMovement.Move(_movementValue.y, _movementValue.x);
            playerCarMovement.Turn(_movementValue.y, _movementValue.x);
            // Fire
            
            if (gunWeapon && _fireInput.IsPressed())
            {
                print("okay now shoot!");
                gunWeapon.Fire();
            }
        }

        private void Update()
        {
            // Drifting
            if (!gunWeapon)
            {
                gunWeapon = playerCarMovement.colliderRb.GetComponentInChildren<GunWeapon>();
            }
            if (_driftInput.WasPressedThisFrame())
            {
                playerCarMovement.StartDrift((int)Mathf.Sign(_movementValue.x));
            }
            else if (_driftInput.WasReleasedThisFrame())
            {
                playerCarMovement.UnDrift();
            }

            if (_pauseInput.WasPressedThisFrame())
            {
                ManagePause();
            }

            if (_showRadioInput.WasPressedThisFrame())
            {
                radioManager.ShowRadio();
            }
            if (_playRadioInput.WasPressedThisFrame())
            {
                radioManager.PausePlay();
            }
            if (_nextRadioInput.WasPressedThisFrame())
            {
                radioManager.PlayNextTrack();
            }
            if (_previousRadioInput.WasPressedThisFrame())
            {
                radioManager.PlayPreviousTrack();
            }
        }

        public void ManagePause(){
            pauseManager.PauseInput();
        }

        public void AssignCar(GameObject car)
        {
            playerCarMovement = car.GetComponent<CarMovement>();
            
            virtualCamera.LookAt = playerCarMovement.transform;
            virtualCamera.Follow = playerCarMovement.transform;
            
            gunWeapon = playerCarMovement.colliderRb.GetComponentInChildren<GunWeapon>();
            if (gunWeapon)
            {
                foreach (MeshFilter meshFilter in gunWeapon.GetComponentsInChildren<MeshFilter>())
                {
                    car.GetComponent<CarDestruction>().AddMesh(meshFilter);
                }
            }
        }
        
        public void SetMovementControls(bool setEnabled)
        {
            if (setEnabled)
            {
                _movementInput.Enable();
                _driftInput.Enable();
                _fireInput.Enable();
                _showRadioInput.Enable();
                _playRadioInput.Enable();
                _nextRadioInput.Enable();
                _previousRadioInput.Enable();
            }
            else
            {
                _movementInput.Disable();
                _driftInput.Disable();
                _fireInput.Disable();
                _showRadioInput.Disable();
                _playRadioInput.Disable();
                _nextRadioInput.Disable();
                _previousRadioInput.Disable();
            }
        }
    }
}