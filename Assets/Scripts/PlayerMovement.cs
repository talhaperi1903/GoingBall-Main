using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    private new Rigidbody rigidbody;
    public float moveSpeed;
    private bool onWater = false;
    public GameObject miniMapCamera;
    public TextMeshProUGUI warningText;
    private float waterTimer = 10;
    public GameObject sceneTransition;
    private GameObject selectedRuin;
    public GameObject levelController;
    private float speedMultiplier = 1;
    private Vector3 checkpoint = new Vector3(100,27.55f,-0);
    private int deathCount;
    private bool isSpawned = false;
    public TextMeshProUGUI livesText;
    public GameObject checkpointText;
    public GameObject deadScene;
    public GameObject finishScene;
    public GameObject quest;
    public GameObject respawnEffectPrefab;

    void Start()
    {
        rigidbody = GetComponentInChildren<Rigidbody>();
    }

    [Obsolete]
    private void Update()
    {
        if(miniMapCamera != null) miniMapCamera.transform.position = new Vector3(transform.position.x, 150, transform.position.z);

        if (onWater && waterTimer >= 0)
        {
            waterTimer -= Time.deltaTime;
            if(warningText != null) warningText.text = "Get out of water before " + (int)waterTimer + "!";
        }

        if (!onWater)
        {
            waterTimer = 10;
        }

        if (onWater && waterTimer < 0)
        {
            waterTimer = -0.25f;
            StartCoroutine(LoadAgain());
        }

        if (selectedRuin != null && Input.GetKeyDown(KeyCode.E))
        {
            selectedRuin.transform.GetChild(0).GetComponent<ParticleSystem>().loop = false;
            selectedRuin.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>().loop = false;
            selectedRuin.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().loop = false;
            selectedRuin.transform.GetChild(1).gameObject.SetActive(true);
            selectedRuin.transform.GetChild(2).gameObject.SetActive(true);
            selectedRuin.transform.tag = null;
            selectedRuin = null;
            levelController.GetComponent<LevelController>().cleanedRuinCount =
                levelController.GetComponent<LevelController>().cleanedRuinCount + 1;
        }
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (moveHorizontal != 0 || moveVertical != 0)
        {
            isSpawned = false;
        }

        if (isSpawned)
        {
            rigidbody.velocity = Vector3.zero;
        }

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rigidbody.AddForce(movement * moveSpeed * speedMultiplier);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            onWater = true;
            if(warningText != null) warningText.gameObject.SetActive(true);
        }
        
        if (other.CompareTag("Ruin"))
        {
            selectedRuin = other.gameObject;
        }
        
        if (other.CompareTag("Boost"))
        {
            speedMultiplier = 4;
        }
        
        if (other.CompareTag("Boost1"))
        {
            speedMultiplier = 6;
        }
        
        if (other.CompareTag("Checkpoint"))
        {
            checkpoint = transform.position;
            Destroy(other.GameObject());
            StartCoroutine(Checkpoint());
        }
        
        if (other.CompareTag("Dead"))
        {
            if(deathCount < 3)
            {
                deathCount = deathCount + 1;
                livesText.text = "Remaining Lives: " + (3 - deathCount).ToString() + "/3";
                rigidbody.velocity = Vector3.zero;
                transform.position = checkpoint;
                isSpawned = true;
                GameObject respawnEffect;
                respawnEffect = Instantiate(respawnEffectPrefab, checkpoint, Quaternion.identity);
            }
            else
            {
                speedMultiplier = 0;
                quest.SetActive(false);
                deadScene.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
        }
        
        if (other.CompareTag("Finish"))
        {
            speedMultiplier = 0;
            quest.SetActive(false);
            finishScene.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            onWater = false;
            if(warningText != null) warningText.gameObject.SetActive(false);
        }
        
        if (other.CompareTag("Ruin") && selectedRuin != null)
        {
            selectedRuin = null;
        }
        
        if (other.CompareTag("Boost") || other.CompareTag("Boost1"))
        {
            speedMultiplier = 1;
        }
    }

    public IEnumerator LoadAgain()
    {
        sceneTransition.GetComponent<Animator>().SetTrigger("Start");
        yield return new WaitForSeconds(1.25f);
        SceneManager.LoadScene(1);
    }

    public IEnumerator Checkpoint()
    {
        checkpointText.SetActive(true);
        yield return new WaitForSeconds(2);
        checkpointText.SetActive(false);
    }
}
