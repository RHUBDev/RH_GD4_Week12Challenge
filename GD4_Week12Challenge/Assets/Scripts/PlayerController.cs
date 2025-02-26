using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private Camera playerCamera;
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float runMultiplier = 2f;
    [SerializeField] float jumpForce = 2f;
    [SerializeField] float gravity = 6f;
    public float mouseSensitivity = 700f;
    [SerializeField] float lookXLimit = 85f;
    float rotationX = 0f;
    Vector3 moveDirection;
    CharacterController controller;
    float moveMultiplier = 1f;
    private float stamina = 100f;
    private float maxStamina = 100f;
    private float staminaDownRate = 20f;
    private float staminaUpRate = 20f;
    //public TMP_Text staminaText;
    //public RectTransform staminaImage;
    public GameObject web;
    private bool webbing = false;
    private bool webbed = false;
    private Vector3 webpos = Vector3.zero;
    private Vector3 webVector = Vector3.zero;
    private Vector3 extraMovement = Vector3.zero;
    private Vector3 startPos = Vector3.zero;
    private float webSpeed = 120f;
    private float webJourney = 0f;
    [SerializeField] private LayerMask layerMask;
    //[SerializeField] private Transform enemy;
    private int score = 0;
    //[SerializeField] private TMP_Text pointsText;
    //[SerializeField] private TMP_Text endText;
    //[SerializeField] private TMP_Text healthText;
    private int health = 3;
    public Volume volume;
    public ColorAdjustments CA;
    private VolumeParameter<Color> VP = new VolumeParameter<Color>();
    private float colourSpeed = 2f;
    private bool dead = false;
    private string levelName;
    [SerializeField] private Transform webpoint;
    [SerializeField] private Animation anim;
    private float webAnimTime = 0.5f;
    private float webAnimTimer = 0.5f;
    private Vector3 handRelativePos = Vector3.zero;
    private Quaternion handRelativeRot = Quaternion.identity;
    public Transform hand;
    public Transform camParent;
    private Vector3 webNorm = Vector3.zero;
    private Quaternion handEndRot = Quaternion.identity;
    public Transform webDummy;
    Collider collision1;
    Vector3 collision1Normal;
    Collider currentCollision;
    Vector3 currentCollisionNormal;
    private bool stoppedWeb = false;
    [SerializeField] private SkinnedMeshRenderer mesh;
    private bool grounded2 = false;
    private Vector3 webpos2 = Vector3.zero;
    private Vector3 webHandPos;
    private Quaternion webHandRot = Quaternion.identity;
    [SerializeField] private GameObject flames;
    private bool flying = false;
    private float extraStopTimer = 0f;
    private float extraStopTime = 2f;
    private float moveStopTimer = 0f;
    private float flyStopTimer = 0f;
    Vector3 flyMoveDirection;
    Vector3 flyMoveDirection2;
    private float flyMoveSpeed = 40f;
    private Vector3 tempExtraStop = Vector3.zero;
    private Vector3 tempMoveStop = Vector3.zero;
    private Vector3 tempFlyStop = Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lookXLimit = 85f;
        Time.timeScale = 1f;
        /////healthText.text = "Lives: " + health;
        controller = GetComponent<CharacterController>();
        playerCamera = Camera.main;

        //I googled how to reference ColorAdjustments
        volume.profile.TryGet<ColorAdjustments>(out CA);
        levelName = SceneManager.GetActiveScene().name;
        
        if (levelName != "SuperheroMenu")
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        handRelativePos = hand.localPosition;
        handRelativeRot = hand.localRotation;
        mesh.enabled = false;
        hand.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            if (levelName != "SuperheroMenu")
            {
                #region Movement

                float movementDirectionY = moveDirection.y;

                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                float horizontalInputRaw = Input.GetAxisRaw("Horizontal");
                float verticalInputRaw = Input.GetAxisRaw("Vertical");
                //Web key pressed
                if (Input.GetKeyDown(KeyCode.E))
                {
                    //if not already webbing
                    if (!webbing)
                    {
                        RaycastHit ray;

                        //cast a ray forwards in the camera direction
                        bool ahit = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out ray, 1000f, layerMask, QueryTriggerInteraction.Ignore);

                        if (ahit)
                        {
                            //end point of web
                            webpos = ray.point;
                            webpos2 = webpos;
                            RaycastHit ray2;
                            if (ray.normal.y < 0.5)
                            {
                                //the web vector
                                webVector = webpos - playerCamera.transform.position;
                            }
                            else
                            {
                                if (Physics.Raycast(controller.transform.position + Vector3.down * 1.2f, playerCamera.transform.forward, out ray2, 1000f, layerMask, QueryTriggerInteraction.Ignore))
                                {
                                    webpos2 = ray2.point;
                                    webVector = webpos2 - (controller.transform.position + Vector3.down * 1.2f);
                                }
                            }
                            Debug.Log("2.1");

                            bool doit = false;
                            if (currentCollision)
                            {
                                doit = Vector3.Dot(-currentCollisionNormal, webVector.normalized) < 0;
                            }
                            else
                            {
                                doit = true;
                            }
                                Debug.Log("Dot = " + Vector3.Dot(-currentCollisionNormal, webVector.normalized));
                            if (doit)//Vector3.Dot(-currentCollisionNormal, webVector) < 0)
                            {
                                Debug.Log("2.2");
                                
                                hand.localPosition = handRelativePos;
                                hand.localRotation = handRelativeRot;

                                webHandPos = hand.transform.position - playerCamera.transform.position;
                                webHandRot = hand.transform.rotation;
                                /////hand.SetParent(camParent);

                                hand.transform.position = playerCamera.transform.position + webHandPos;
                                hand.transform.rotation = webHandRot;

                               
                                //'webbed' is set to true after webbing is stopped by a collision or end of the web journey 
                                webbed = false;
                                flyMoveDirection2 = Vector3.zero;

                                flames.SetActive(false);

                                flying = false;

                                mesh.enabled = true;
                                hand.gameObject.SetActive(true);

                                stoppedWeb = false;

                                //webJourney will be increased to 1 later, used for position Lerp
                                webJourney = 0f;

                                //start position of the web
                                startPos = transform.position;//transform.position;

                                //extra movement will be the added velocity after stopping webbing mid-web
                                extraMovement = webVector.normalized * webSpeed;

                                //set webbing  boolean and set the web line object visible
                                webbing = true;
                                web.SetActive(true);

                                webNorm = ray.normal;

                                collision1 = ray.collider;
                                collision1Normal = ray.normal;
                            }
                        }
                    }
                    else
                    {
                        //if stopping webbing mid-web

                        //set all gravity Y direction speeds to 0
                        movementDirectionY = 0f;
                        moveDirection.y = 0f;

                        mesh.enabled = false;
                        hand.gameObject.SetActive(false);

                        //set extraMovement, I know we already called it higher up in the code though
                        extraMovement = webVector.normalized * webSpeed;

                        //webbing to false
                        webbing = false;

                        //don't set webbed to true cos we will be flying through the air (webbed keeps you stuck to a building after webbing)
                        webbed = false;

                        //hide web object
                        web.SetActive(false);

                        hand.SetParent(playerCamera.transform);

                        hand.localPosition = handRelativePos;
                        hand.localRotation = handRelativeRot;
                        //StartCoroutine(SetHandPos());
                    }
                }

                if (!flying)
                {
                    //set move direction from inputs
                    moveDirection = (horizontalInput * transform.right) + (verticalInput * transform.forward).normalized;

                    //and keep previous y move amount
                    moveDirection.y = movementDirectionY;
                }
                else
                {
                    flyMoveDirection = (horizontalInput * playerCamera.transform.right) + (verticalInput * playerCamera.transform.forward).normalized;
                    if(horizontalInputRaw != 0 || verticalInputRaw != 0)
                    {
                        flyMoveDirection2 = flyMoveDirection;
                    }
                }

                //if we are not webbing currently
                if (!webbing)
                {
                    //and if grounded OR recently webbed (stuck to building)
                    if (controller.isGrounded || webbed || stoppedWeb || grounded2)
                    {
                        //set extra movement and y falling to 0
                        extraMovement = Vector3.zero;

                        movementDirectionY = 0f;
                        moveDirection.y = 0f;

                        currentCollision = collision1;
                        currentCollisionNormal = collision1Normal;

                        //if we are webbed to building, but trying to move, stop the webbed bool
                        if (horizontalInput != 0 || verticalInput != 0)
                        {
                            webbed = false;
                            stoppedWeb = false;
                            if (!flames.activeInHierarchy)
                            {
                                mesh.enabled = false;
                                hand.gameObject.SetActive(false);
                            }
                        }

                        //Jumping
                        if (Input.GetButtonDown("Jump"))
                        {
                            webbed = false;
                            stoppedWeb = false;
                            grounded2 = false;
                            currentCollision = null;
                            moveDirection.y = jumpForce;
                        }
                        webAnimTimer = 0;

                        anim["Armature|palm"].time = anim["Armature|palm"].length;

                        if (webbed)
                        {
                            hand.position = webDummy.position;
                            hand.rotation = webDummy.rotation;
                        }

                    }
                    else if (!webbed && !stoppedWeb)
                    {
                        //if not grounded or webbed

                        //stop sprinting if sprint lifted key (but don't start sprint if mid air)
                        if (Input.GetKeyUp(KeyCode.LeftShift)) moveMultiplier = 1;

                        //add the move multiplier
                        moveDirection.x *= moveMultiplier;
                        moveDirection.z *= moveMultiplier;

                        if (!flying)
                        {
                            //apply gravity
                            moveDirection.y -= gravity * Time.deltaTime;
                        }

                        if (extraMovement != Vector3.zero)
                        {
                            //if we have extraMovement (flying with web velocity after stopped webbing mid air), subtract the input keys movement from it, so if player is moving against the extraMovement, the extraMovement won't increase again after stopping inputs.
                            extraMovement.x -= moveDirection.x * Time.deltaTime;
                            extraMovement.z -= moveDirection.z * Time.deltaTime;
                        }
                    }

                    if (!webbed && Input.GetKeyDown(KeyCode.Q))
                    {
                        if (!flames.activeInHierarchy)
                        {

                            hand.localPosition = handRelativePos;
                            hand.localRotation = handRelativeRot;
                            hand.gameObject.SetActive(true);
                            mesh.enabled = true;
                            flames.SetActive(true);
                        }
                        else
                        {
                            flames.SetActive(false);
                            mesh.enabled = false;
                            hand.gameObject.SetActive(false);
                        }
                    }

                    if (!webbing && !webbed && Input.GetKeyDown(KeyCode.R))
                    {
                        flying = !flying;
                        if (!flying)
                        {
                            extraMovement = flyMoveDirection2 * flyMoveSpeed;
                            flyMoveDirection2 = Vector3.zero;
                        }
                        else
                        {
                            tempMoveStop = moveDirection;
                            tempExtraStop = extraMovement;
                        }
                    }

                    //if sprinting
                    if (moveMultiplier > 1)
                    {
                        //reduce stamina over time, to 0
                        if (stamina > 0)
                        {
                            stamina -= Time.deltaTime * staminaDownRate;
                        }
                        else
                        {
                            stamina = 0;
                            //if stamina is 0, stop sprinting
                            moveMultiplier = 1;
                        }
                    }
                    else
                    {
                        //if not sprinting, regain stamina, up to maxStamina
                        if (stamina < maxStamina)
                        {
                            stamina += Time.deltaTime * staminaUpRate;
                        }
                        else
                        {
                            stamina = maxStamina;
                        }
                    }

                    //this time, floor the stamina float, otherwise it might never truly be 0
                    int visibleStamina = Mathf.FloorToInt(stamina);
                    //set stamina text
                    /////staminaText.text = "Stamina: " + visibleStamina;
                    //and set the stamina bar to the right length
                    /////staminaImage.sizeDelta = new Vector2(6 * visibleStamina, 20);
                    //move the player, including extraMovement

                    controller.Move(moveDirection * moveSpeed * Time.deltaTime + extraMovement * Time.deltaTime + flyMoveDirection2 * flyMoveSpeed * Time.deltaTime);
                }
                else
                {
                    //if we ARE webbing currently

                    hand.transform.position = playerCamera.transform.position + webHandPos;
                    hand.transform.rotation = webHandRot;

                    Debug.Log("webpos = " + webpos);
                    //set the web object position to mid-way, scale to the length of the web vector, and rotation to LookAt the webpos (web destination)
                    web.transform.position = webpoint.position + (webpos - webpoint.position) / 2;
                    web.transform.localScale = new Vector3(0.014f, 0.014f, (webpos - webpoint.position).magnitude);
                    web.transform.LookAt(webpos);

                    if (webAnimTimer < webAnimTime)
                    {
                        webAnimTimer += Time.deltaTime;
                        anim.Play("Armature|palm");
                    }
                    else
                    {
                        anim.Stop();
                    }
                    //increase webJourney to 1, so it takes the time to travel length of webVector at webSpeed
                    webJourney += (Time.deltaTime * webSpeed) / webVector.magnitude;
                    if (webJourney < 1)
                    {
                        //move the player controller with Lerp, from startPos to the destination
                        controller.transform.position = Vector3.Lerp(startPos, startPos + webVector, webJourney);
                    }
                    else
                    {
                        //if journey over, turn off web and set 'webbed', etc.
                        web.SetActive(false);
                        movementDirectionY = 0f;
                        moveDirection.y = 0f;
                        extraMovement = Vector3.zero;
                        webbing = false;
                        webbed = true;

                        currentCollision = collision1;
                        currentCollisionNormal = collision1Normal;

                        if (webNorm.y < 0.5)
                        {
                            Debug.Log("0.1");
                            StartCoroutine(WebbedHand());
                        }
                        else
                        {
                            webbed = false;
                            stoppedWeb = true;
                            mesh.enabled = false;
                            hand.gameObject.SetActive(false);
                            hand.localPosition = handRelativePos;
                            hand.localRotation = handRelativeRot;

                            RaycastHit ray;
                            if (Physics.Raycast(playerCamera.transform.position + Vector3.down * 0.1f, Vector3.down, out ray, 2.5f, layerMask, QueryTriggerInteraction.Ignore))
                            {
                                Debug.Log("3");
                                controller.transform.position = ray.point + Vector3.up * 1.4f;
                            }
                        }
                    }
                }

                if (flying)
                {
                    gravity = 0;

                    currentCollision = null;

                    if (extraMovement != Vector3.zero)
                    {
                        extraStopTimer += Time.deltaTime / extraStopTime;
                        extraMovement = Vector3.Lerp(tempExtraStop, Vector3.zero, extraStopTimer);
                    }

                    if (moveDirection != Vector3.zero)
                    {
                        moveStopTimer += Time.deltaTime / extraStopTime;
                        moveDirection = Vector3.Lerp(tempMoveStop, Vector3.zero, moveStopTimer);
                    }

                    if (horizontalInputRaw == 0 && verticalInputRaw == 0)
                    {
                        if (flyMoveDirection2 != Vector3.zero)
                        {
                            flyStopTimer += Time.deltaTime / extraStopTime;
                            flyMoveDirection2 = Vector3.Lerp(tempFlyStop, Vector3.zero, flyStopTimer);
                        }
                    }
                    else
                    {
                        tempFlyStop = flyMoveDirection2;

                        flyStopTimer = 0;
                    }
                }
                else
                {
                    gravity = 6;
                    extraStopTimer = 0;
                    moveStopTimer = 0;
                    flyStopTimer = 0;
                }

                #endregion

                #region Rotation

                //rotate the player and camera (Akshat knows this, he wrote it hehe)
                transform.Rotate(Vector3.up * mouseSensitivity * Time.deltaTime * Input.GetAxis("Mouse X"));

                rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

                #endregion
            }
        }
        if (levelName != "MenuScene")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene("MenuScene");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.3f)
        {
            grounded2 = true;
        }

        if (extraMovement != Vector3.zero)
        {
            extraMovement = extraMovement - Vector3.Project(extraMovement, -collision.contacts[0].normal);
        }

        bool doit;
        if (currentCollision)
        {
            doit = Vector3.Dot(-currentCollisionNormal, webVector) >= 0;
        }
        else
        {
            doit = true;
        }
        //if we hit something while webbing, stop webbing (it could be the destination object; we will never actually reach the destination because the Lerp goes all the way to the centre of the characterController at the moment)
        if (webbing && doit)//((Vector3.Dot(-currentCollisionNormal, webVector) >= 0)))// || !(Vector3.Dot(-currentCollisionNormal, webVector) >= 0 && currentCollision == collision.collider)))
        {
            moveDirection.y = 0f;
            web.SetActive(false);
            extraMovement = Vector3.zero;
            webbing = false;

            hand.SetParent(playerCamera.transform);

            collision1 = collision.collider;
            collision1Normal = collision.contacts[0].normal;

            //if (!controller.isGrounded)
            //{
            if (collision.contacts[0].normal.y<0.5)
            {
                Debug.Log("0.2");
                webbed = true;
                //collision1 = collision.collider;
                StartCoroutine(WebbedHand());
            }
            else
            {
                stoppedWeb = true;
                mesh.enabled = false;
                hand.gameObject.SetActive(false);
                hand.localPosition = handRelativePos;
                hand.localRotation = handRelativeRot;
                //StartCoroutine(SetHandPos());
                controller.transform.position = collision.contacts[0].point + Vector3.up * 1.4f;
            }
        }
    }

    public void AddScore(int scoreToAdd)
    {
        //Add Score after killing a bad guy
        score += scoreToAdd;

        /////pointsText.text = "Score: " + score;
    }

    public void TakeDamage()
    {
        if (!dead)
        {
            //Take damage if enemy hit us
            health--;
            ///// healthText.text = "Lives: " + health;
            if (health <= 0)
            {
                dead = true;
                if (levelName == "MyScene")
                {
                    int highScore = PlayerPrefs.GetInt("HighScore", 0);
                    if (score > highScore)
                    {
                        /////endText.text = "Congrats!\nYou scored: " + score + "\nOld score: " + highScore;
                        PlayerPrefs.SetInt("HighScore", score);
                    }
                    else
                    {
                        /////endText.text = "You scored: " + score + "\nHigh score: " + highScore;
                    }
                }
                StartCoroutine(DoDie());
            }
        }
    }

    IEnumerator DoDie()
    {
        //if dead, make screen go red, then restart
        float val = 1f;
        if (CA != null)
        {
            VP.value = Color.white;
            while (VP.value.b > 0)
            {
                val -= Time.deltaTime * colourSpeed;
                VP.value = Color.Lerp(Color.white, Color.red, 1 - val);
                CA.colorFilter.SetValue(VP);
                yield return null;
            }
            VP.value = Color.red;
            //googled how to set ColorAdjustments, but did the Lerp myself
            CA.colorFilter.SetValue(VP);
        }

        if (levelName != "SuperheroMenu")
        {
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    IEnumerator WebbedHand()
    {
        Vector3 handStartPos = hand.position;
        Vector3 handEndPos = webpos;
        Vector3 newNorm = webNorm;
     

        RaycastHit ray;
        if (Physics.Raycast(playerCamera.transform.position, -newNorm, out ray, 1f, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("1.1");
            newNorm = ray.normal;
            handEndPos = ray.point;
        }

        float diffMag = (handEndPos - playerCamera.transform.position).magnitude;
        if (diffMag < 0.6f)
        {
            Debug.Log("1.2");
            controller.transform.position = controller.transform.position - (newNorm * (diffMag - 0.6f));
        }

        Quaternion handStartRot = hand.rotation;
        webDummy.rotation = Quaternion.FromToRotation(-transform.up, -newNorm);
        webDummy.position = handEndPos;

        float angle = Vector3.SignedAngle(-webVector, webDummy.forward, -newNorm);
        webDummy.rotation *= Quaternion.Euler(0, angle, 0);
        handEndRot = webDummy.rotation;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3f;
            hand.position = Vector3.Lerp(handStartPos, handEndPos, t);
            hand.rotation = Quaternion.Lerp(handStartRot, handEndRot, t);
            yield return null;
        }
        hand.position = handEndPos;
        hand.rotation = handEndRot;
        anim["Armature|palm"].time = anim["Armature|palm"].length;
    }
    private void OnCollisionStay(Collision collision)
    {

        collision1 = collision.collider;
        collision1Normal = collision.contacts[0].normal;
        currentCollision = collision.collider;
        currentCollisionNormal = collision.contacts[0].normal;
        Debug.Log("currentCollision = " + currentCollision.name);
        
        //Debug.Log("currentCollisionNormal = " + currentCollisionNormal);
    }
    private void OnCollisionExit(Collision collision)
    {
        if (currentCollision == collision.collider)
        {
            currentCollision = null;
        }
        grounded2 = false;

        /*if (collision1 == collision.collider)
        {
            collision1 = null;
        }*/
    }

    IEnumerator SetHandPos()
    {
        yield return new WaitForNextFrameUnit();
        hand.localPosition = handRelativePos;
        hand.localRotation = handRelativeRot;
    }
}
