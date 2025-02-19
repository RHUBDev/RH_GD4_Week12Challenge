using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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

                //Web key pressed
                if (Input.GetKeyDown(KeyCode.E))
                {
                    //if not already webbing
                    if (!webbing)
                    {
                        RaycastHit ray;

                        //cast a ray forwards in the camera direction
                        bool ahit = Physics.Raycast(controller.transform.position, playerCamera.transform.forward, out ray, 1000f, layerMask, QueryTriggerInteraction.Ignore);

                        if (ahit)
                        {
                            //'webbed' is set to true after webbing is stopped by a collision or end of the web journey 
                            webbed = false;

                            //webJourney will be increased to 1 later, used for position Lerp
                            webJourney = 0f;

                            //start position of the web
                            startPos = transform.position;//transform.position;

                            //end point of web
                            webpos = ray.point;

                            //the web vector
                            webVector = webpos - transform.position;

                            //extra movement will be the added velocity after stopping webbing mid-web
                            extraMovement = webVector.normalized * webSpeed;

                            //set webbing  boolean and set the web line object visible
                            webbing = true;
                            web.SetActive(true);
                        }
                    }
                    else
                    {
                        //if stopping webbing mid-web

                        //set all gravity Y direction speeds to 0
                        movementDirectionY = 0f;
                        moveDirection.y = 0f;

                        //set extraMovement, I know we already called it higher up in the code though
                        extraMovement = webVector.normalized * webSpeed;

                        //webbing to false
                        webbing = false;

                        //don't set webbed to true cos we will be flying through the air (webbed keeps you stuck to a building after webbing)
                        webbed = false;

                        //hide web object
                        web.SetActive(false);
                    }
                }

                //set move direction from inputs
                moveDirection = (horizontalInput * transform.right) + (verticalInput * transform.forward).normalized;

                //and keep previous y move amount
                moveDirection.y = movementDirectionY;

                //if we are not webbing currently
                if (!webbing)
                {
                    //and if grounded OR recently webbed (stuck to building)
                    if (controller.isGrounded || webbed)
                    {
                        //set extra movement and y falling to 0
                        extraMovement = Vector3.zero;

                        movementDirectionY = 0f;
                        moveDirection.y = 0f;

                        //I multiplied the moveDirection x and z here individually, as I thought it might multiply the gravity too otherwise
                        if (Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            moveMultiplier *= runMultiplier;
                        }
                        if (Input.GetKeyUp(KeyCode.LeftShift))
                        {
                            moveMultiplier = 1;
                        }
                        moveDirection.x *= moveMultiplier;
                        moveDirection.z *= moveMultiplier;

                        //if we are webbed to building, but trying to move, stop the webbed bool
                        if (horizontalInput != 0 || verticalInput != 0)
                        {
                            webbed = false;
                        }

                        //Jumping
                        if (Input.GetButtonDown("Jump"))
                        {
                            webbed = false;
                            moveDirection.y = jumpForce;
                        }
                    }
                    else if (!webbed)
                    {
                        //if not grounded or webbed

                        //stop sprinting if sprint lifted key (but don't start sprint if mid air)
                        if (Input.GetKeyUp(KeyCode.LeftShift)) moveMultiplier = 1;

                        //add the move multiplier
                        moveDirection.x *= moveMultiplier;
                        moveDirection.z *= moveMultiplier;

                        //apply gravity
                        moveDirection.y -= gravity * Time.deltaTime;

                        if (extraMovement != Vector3.zero)
                        {
                            //if we have extraMovement (flying with web velocity after stopped webbing mid air), subtract the input keys movement from it, so if player is moving against the extraMovement, the extraMovement won't increase again after stopping inputs.
                            extraMovement.x -= moveDirection.x * Time.deltaTime;
                            extraMovement.z -= moveDirection.z * Time.deltaTime;
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
                    controller.Move(moveDirection * moveSpeed * Time.deltaTime + extraMovement * Time.deltaTime);
                }
                else
                {
                    //if we ARE webbing currently

                    //set the web object position to mid-way, scale to the length of the web vector, and rotation to LookAt the webpos (web destination)
                    web.transform.position = webpoint.position + (webpos - webpoint.position) / 2;
                    web.transform.localScale = new Vector3(0.014f, 0.014f, (webpos - webpoint.position).magnitude);
                    web.transform.LookAt(webpos);

                    //increase webJourney to 1, so it takes the time to travel length of webVector at webSpeed
                    webJourney += (Time.deltaTime * webSpeed) / webVector.magnitude;
                    if (webJourney < 1)
                    {
                        //move the player controller with Lerp, from startPos to the destination
                        controller.transform.position = Vector3.Lerp(startPos, webpos, webJourney);
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
                    }
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
        if (levelName != "SuperheroMenu")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //SceneManager.LoadScene("SuperheroMenu");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if we hit something while webbing, stop webbing (it could be the destination object; we will never actually reach the destination because the Lerp goes all the way to the centre of the characterController at the moment)
        if (webbing)
        {
            moveDirection.y = 0f;
            web.SetActive(false);
            extraMovement = Vector3.zero;
            webbing = false;
            webbed = true;
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
}
