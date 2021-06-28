using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Maze
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] 
        private float jumpHeight = 0.1f;    

        [SerializeField]
        public float gravity = -5f;

        [SerializeField]
        float speed = 01.0f; 


        [Header("References")]
        [SerializeField] 
        CharacterController cc;

        [SerializeField] 
        Transform cameraTransform;

        [SerializeField] 
        Animator animator = null;

        [SerializeField] 
        LayerMask groundLayer;

        [SerializeField]
        Text myText;

        [SerializeField]
        Material winnerMaterial;


        OrbitCamera camera;

        private NetworkVariableString myString = new NetworkVariableString("my name");
        private NetworkVariableColor myColor = new NetworkVariableColor();

        private string myName = "";

        // Test fly cam script
        float shiftAdd = 1.5f; //multiplied by how long shift is held.  Basically running
        float maxShift = 03.0f; //Maximum speed when holdin gshift
        float camSens = 0.5f; //How sensitive it with mouse
        
        private Vector3 lastMouse =  new Vector3(255,300, 255); //kind of in the middle of the screen, rather than at the top (play)
        float pitch = 0f;

        private float totalRun  = 1.0f;
        float acceleration = 1f;

        public Vector3 velocity;
        private bool isGrounded;

        //Test
        public Button btn = null;
        public GameObject obj;

        Material m_Material;


        void Start()
        {
            if (!IsLocalPlayer)
            {
                cameraTransform.GetComponent<AudioListener>().enabled = false;
                cameraTransform.GetComponent<Camera>().enabled = false;
            }
            else
            {
                cc = GetComponent<CharacterController>();
                camera = GetComponentInChildren<OrbitCamera>();
                Cursor.lockState = CursorLockMode.Locked;
                Respawn();
                Debug.Log("MenuScript: "+ MenuScript.myName);
                SetNameServerRpc(MenuScript.myName);

                SetColorServerRpc(GetRandomColor());
                GetComponentInChildren<Renderer>().material.color = myColor.Value;

            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetNameServerRpc(string name)
        {
            myString.Value = name;
            myText.text = myString.Value;                
        }
        [ServerRpc(RequireOwnership = false)]
        public void SetColorServerRpc(Color color)
        {
            myColor.Value = color;
        }
        [ServerRpc(RequireOwnership = false)]
        public void SetMatServerRpc()
        {
            SetMatClientRpc();
        }        
        
        [ClientRpc]
        public void SetMatClientRpc()
        {
            GetComponentInChildren<Renderer>().material = winnerMaterial;

        }
        void Update()
        {
            Debug.Log(EventSystem.current.currentSelectedGameObject.name);


            myText.text = myString.Value;  
            GetComponentInChildren<Renderer>().material.color = myColor.Value;              
            if (IsLocalPlayer)
            {
                MovePlayer();
                //Look();
                // Assign the respawn pause button
                if(GameObject.FindWithTag("Respawn") && btn == null )
                {
                    btn = GameObject.FindWithTag("Respawn").GetComponent<Button>();
                    btn.onClick.AddListener(Respawn);
                }
            }
            else{
            }
        }

        void MovePlayer()
        {
            //Keyboard commands
            var p = GetBaseInput();

            // Gravity
            isGrounded = Physics.CheckSphere(transform.position, 0.1f, groundLayer, QueryTriggerInteraction.Ignore);
            if (isGrounded && velocity.y < 0){
                velocity.y = 0;
            }
            else{
                velocity.y += gravity * Time.deltaTime;
            }

            // Standing idle, walking, and running
            if (!Input.GetKey (KeyCode.LeftShift) && cc.velocity.magnitude > 0.2f){
                acceleration += 1f;
                if(acceleration >= 50f) acceleration =50f;
                animator.SetFloat("Speed", acceleration/100);
            }
            else if (Input.GetKey (KeyCode.LeftShift) && cc.velocity.magnitude > 0.2f){
                acceleration += 1f;
                if(acceleration >= 100) acceleration =100f;
                p  = p * totalRun * shiftAdd;
                p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
                // p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
                p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
                animator.SetFloat("Speed", acceleration/100);
            }
            else {
                acceleration -= 1f;
                if(acceleration <= 0) acceleration = 0f;
                animator.SetFloat("Speed", acceleration/100);
            }

            // Crouching
            if(!Input.GetKey("left ctrl")){
                animator.SetBool("Crouching", false);
                animator.SetBool("Sneaking", false);             
            }
            else if(Input.GetKey("left ctrl") ){
                if(cc.velocity.magnitude < 0.2f){
                    animator.SetBool("Sneaking", false);
                    animator.SetBool("Crouching", true);
                }
                else{
                    animator.SetBool("Crouching", true);
                    animator.SetBool("Sneaking", true);
                }
            }

            // Jumping, still and running
            if(isGrounded) { animator.SetBool("Jump", false);}
            if(Input.GetKeyDown("space") && isGrounded){
                animator.SetBool("Jump", true);
                velocity.y += Mathf.Sqrt(jumpHeight * -2 * gravity);
            }
            if(AnimatorIsPlaying("jump") || AnimatorIsPlaying("jumpRun"))
            {
                animator.SetBool("Jump", false);
            }

            // Apply speed
            p *= speed;

            // Create a copy of camera transform
            Transform tempTrans = cameraTransform;

            // Change the  forward vector of the copy so that its level
            tempTrans.forward = new Vector3(tempTrans.forward.x,0,tempTrans.forward.z);


            // Debug.Log("temptrans forward: " +tempTrans.forward);
            // Debug.Log( "Vector3 Before move: " + tempTrans.TransformDirection(new Vector3 (Mathf.Clamp(p.x, p.x*0.5f,p.x*0.5f), velocity.y, p.z)  )  * Time.deltaTime);

            // Change the local movement to translate to where the camera is looking
            Vector3 tempMove = tempTrans.TransformDirection(new Vector3 (Mathf.Clamp(p.x, p.x*0.5f,p.x*0.5f), velocity.y, p.z)  )  * Time.deltaTime;


            // Debug.Log( "Vector3 After move: " + tempMove);

            tempMove = new Vector3( tempMove.x, tempMove.y, tempMove.z);


            // if (Input.GetKey (KeyCode.Mouse1) && cc.velocity.magnitude < 0.2f ){
            //     animator.SetBool("Fighting", true);
            // }
            // if (!Input.GetKey (KeyCode.Mouse1) || cc.velocity.magnitude > 0.2f){
            //     animator.SetBool("Fighting", false);
            // }

            if(Input.GetKey (KeyCode.Mouse1))
            {
                animator.SetBool("Fighting", true);
                transform.rotation = Quaternion.LookRotation(tempTrans.forward);
            }
            else
            {
                if(p.x==0 && p.y==0 && p.z==0){}
                else{
                    animator.SetBool("Fighting", false);
                    transform.rotation = Quaternion.LookRotation(tempTrans.TransformDirection(new Vector3 (p.x,0,p.z) ));
                }
            }

            if(!Input.GetKey (KeyCode.Mouse1))
            {
                animator.SetBool("Fighting", false);
            }
            
            // if(p.x==0 && p.y==0 && p.z==0){}
            // else{
            //     animator.SetBool("Fighting", false);
            //     transform.rotation = Quaternion.LookRotation(tempTrans.TransformDirection(new Vector3 (p.x,0,p.z) ));
            // }
            // Send the movement vector to character controller
            cc.Move(tempMove);

            // Leave the player looking in the last direction they last moved
            // if(p.x==0 && p.y==0 && p.z==0){}
            // else{
            //     transform.rotation = Quaternion.LookRotation(tempTrans.TransformDirection(new Vector3 (p.x,0,p.z) ));
            // }


            if(transform.position.y < -10)
            {
                Respawn();
            }


        }

        Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
            Vector3 p_Velocity = new Vector3(0,0,0);
            if (Input.GetKey (KeyCode.W)){
                p_Velocity += new Vector3(0, 0 , 1);
            }
            if (Input.GetKey (KeyCode.S)){
                p_Velocity += new Vector3(0, 0 , -1);
            }
            if (Input.GetKey (KeyCode.A)){
                p_Velocity += new Vector3(-1, 0 , 0);
            }
            if (Input.GetKey (KeyCode.D)){
                p_Velocity += new Vector3(1, 0 , 0);
            }

            return p_Velocity;
        }
      
        public void Respawn() {
            // Spawn in start area
            cc.enabled = false;
            
            GetComponentInChildren<Renderer>().material = m_Material;

            SetColorServerRpc(GetRandomColor());
            GetComponentInChildren<Renderer>().material.color = myColor.Value;
            transform.position = new Vector3 (Random.Range(15,18), 1, Random.Range(-2,-6));
            cc.enabled = true;        
        }

        bool AnimatorIsPlaying(string stateName){
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        public void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Debug.Log("Hit " + hit.collider.tag);
            //text.text = transform.position + "" + hit.gameObject.tag + " " + hit.gameObject.name;
            if(hit.gameObject.tag != "Untagged") {
                if(hit.gameObject.tag ==("Win"))
                {
                    //GameObject.FindGameObjectWithTag("Renderer").GetComponent<MenuScript>().SpawnNewLevelServerRpc();
                    GameObject.FindGameObjectWithTag("Renderer").GetComponent<MazeRenderer>().SetMazeServerRpc();


                GetComponentInChildren<Renderer>().material = winnerMaterial;
                SetMatServerRpc();
                cc.enabled = false;
                transform.position = new Vector3 (Random.Range(15,18), 1, Random.Range(-2,-6));
                cc.enabled = true;   
                }
            }
        }

        private Color GetRandomColor()
        {
            return new Color(
                r:UnityEngine.Random.Range(0f, 1f),
                g:UnityEngine.Random.Range(0f, 1f),
                b:UnityEngine.Random.Range(0f, 1f)
            );
        }
    }
}
