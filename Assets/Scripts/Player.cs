using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private float _startTime = 0;
    private GameObject _currentCube;
    private Collider _lastCollisionCollider;
    private Vector3 _cameraRelativePosition;
    private int _score = 0;
    // random direction control
    private bool _flag = false;
    private bool _isGameOver = false;
    private bool _landLock = false;

    public float Speed = 4f;
    public float MaxDistance = 3f;
    public float MaxTimePress = 1;
    public GameObject Cube;
    public GameObject Particle;
    public GameObject GameOverText;

    public Text ScoreText;

    public Transform Camera;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;

        // avoid falling down
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = Vector3.zero;

        // initialise the first cube
        _currentCube = Cube;
        // initialise the collider
        _lastCollisionCollider = _currentCube.GetComponent<Collider>();
        SpawnCube();

        // initialise the camera position
        _cameraRelativePosition = Camera.position - transform.position;

        // initalise the particle system
        Particle.SetActive(false);

        GameOverText.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        // exit or pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }
        // reload the scene
        if (_isGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            _isGameOver = false;
            _landLock = false;
            SceneManager.LoadScene("MainScene");
        }

        // press 'space'
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _startTime = Time.time;

            // emits particles
            Particle.SetActive(true);
        }
        // release 'space'
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _landLock = true;
            // maximum time of pressing
            var elapse = Time.time - _startTime > MaxTimePress ? MaxTimePress : Time.time - _startTime;
            _startTime = 0;
            OnJump(elapse);
            // release the cube
            _currentCube.transform.DOLocalMoveY(0.25f, 0.2f);
            // stop emitting particles
            Particle.SetActive(false);
        }
        // press the cube
        if (Input.GetKey(KeyCode.Space))
        {
            if (_currentCube.transform.localPosition.y >= 0)
            {
                _currentCube.transform.localPosition += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
            }
        }
    }

    // how far to bounce
    void OnJump(float elapse)
    {
        if (!_flag)
        {
            _rigidbody.AddForce(new Vector3(-1, 1.1f, 0) * elapse * Speed, ForceMode.Impulse);
        }
        else
        {
            _rigidbody.AddForce(new Vector3(0, 1.1f, -1) * elapse * Speed, ForceMode.Impulse);
        }
    }

    // size of cubes becomes smaller when the score++
    private double cubeSize()
    {
        return _score <= 5 ? 2 : 1.8 * Mathf.Sqrt(5) / Mathf.Sqrt(_score) + 0.2d;
    }

    // automaticly generate the cubes
    void SpawnCube()
    {
        var cube = Instantiate(Cube);
        if (!_flag)
        {
            cube.transform.position = _currentCube.transform.position + new Vector3(-Random.Range((float)(cubeSize() * 1.2 + 0.1), MaxDistance), 0, 0);
        }
        else
        {
            cube.transform.position = _currentCube.transform.position + new Vector3(0, 0, -Random.Range((float)(cubeSize() * 1.2 + 0.1), MaxDistance));
        }
        // random size
        var randomSize = Random.Range((float)(cubeSize() * 0.8), (float)(cubeSize() * 1.2));
        cube.transform.localScale = new Vector3(randomSize, 0.5f, randomSize);
        // ramdom color
        cube.GetComponent<Renderer>().material.color =
            new Color(Random.Range(0.5f, 1), Random.Range(0.8f, 1), Random.Range(0.8f, 1));

    }

    // override
    // check if the player lands on a cube and generate next cube
    void OnCollisionEnter(Collision collision)
    {
        // Game over if the player hits the ground
        if (collision.gameObject.name == "Ground")
        {
            GameOver();
        }

        if (_landLock && collision.gameObject.name.Contains("Cube")){
            if (collision.collider != _lastCollisionCollider)
            {
                _landLock = false;
                _lastCollisionCollider = collision.collider;
                _currentCube = collision.gameObject;
                _flag = Random.value > 0.5f;
                SpawnCube();
                MoveCamera();

                // add point
                AddPoint();
                ScoreText.text = _score.ToString();
            }
            else
            {
                // Gmae over if the player lands on the same cube
                GameOver();
            }
        }
    }

    void AddPoint()
    {
        _score++;
    }

    void GameOver()
    {
        GameOverText.SetActive(true);
        Time.timeScale = 0;
        _isGameOver = true;
    }
    void MoveCamera()
    {
        Camera.DOMove(transform.position + _cameraRelativePosition, 1);
    }
}
