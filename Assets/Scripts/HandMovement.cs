using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMovement : MonoBehaviour
{
    [Header("Movement")]
    public float m_moveSpeed = 1.0f;

    [SerializeField] bool manual_height;

    public float targetHeight = 1.25f;


    [Header("Grabbing")]
    bool mouseDown = false;

    public Rigidbody m_rb;
    public FixedJoint m_joint;

    public float grabDistance;
    public LayerMask grabbable;

    public GameObject[] fingers;

    public Transform grabSpot;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (!manual_height) targetHeight = transform.position.y;
        if (!m_rb) m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredVel = Vector3.zero;

        float horiz = Input.GetAxis("Mouse X");
        float vert = Input.GetAxis("Mouse Y");

        if (Input.GetKey(KeyCode.Space))
        {
            desiredVel += Vector3.down * m_moveSpeed * 2;
        }
        else
        {
            desiredVel += Vector3.ClampMagnitude((new Vector3(transform.position.x, targetHeight, transform.position.z) - transform.position) * 10, m_moveSpeed);
        }

        desiredVel += new Vector3(horiz, 0, vert) * 10 * m_moveSpeed;

        m_rb.velocity = desiredVel;

        //if (Input.GetKey(KeyCode.W))
        //{
        //    desiredVel += Vector3.forward  * m_moveSpeed;
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    desiredVel += Vector3.back * m_moveSpeed;
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    desiredVel += Vector3.left * m_moveSpeed;
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    desiredVel += Vector3.right * m_moveSpeed;
        //}

        m_rb.velocity = desiredVel;


        float ratio = (Vector3.Angle(transform.forward, Vector3.forward) + Vector3.Angle(transform.up, Vector3.up)) / (360.0f * 2.0f);

        m_rb.AddTorque(Vector3.up * Vector3.Dot(transform.forward, Vector3.left) * ratio);
        m_rb.AddTorque(Vector3.right * Vector3.Dot(transform.forward, Vector3.up) * ratio);
        m_rb.AddTorque(Vector3.forward * Vector3.Dot(transform.right, Vector3.down) * ratio);

        if (Input.GetMouseButton(0))
        {
            if (!m_joint)
            {
                bool didHit = false;

                Collider[] hits = Physics.OverlapSphere(grabSpot.position, grabDistance, grabbable);
                foreach (Collider _hit in hits)
                {
                    Rigidbody hitBody = _hit.GetComponent<Rigidbody>();

                    if (hitBody)
                    {
                        m_joint = gameObject.AddComponent<FixedJoint>();
                        m_joint.connectedBody = hitBody;

                        HandCallback callback = hitBody.GetComponent<HandCallback>();

                        if (callback) callback.Grabbed();

                        didHit = true;
                        break;
                    }

                }

                if (didHit)
                {
                    foreach (GameObject _part in fingers)
                    {
                        _part.GetComponent<Collider>().enabled = false;
                    }
                }
            }

            if (!mouseDown)
            {
                GetComponent<Animation>().clip = GetComponent<Animation>().GetClip("Close");
                GetComponent<Animation>().Play();
            }
            mouseDown = true;
        }
        else
        {
            if (mouseDown)
            {
                if (m_joint)
                {
                    HandCallback callback = m_joint.connectedBody.transform.GetComponent<HandCallback>();
                    if (callback) callback.Release();
                    Destroy(m_joint);
                }
                foreach (GameObject _part in fingers)
                {
                    _part.GetComponent<Collider>().enabled = true;
                }

                GetComponent<Animation>().clip = GetComponent<Animation>().GetClip("Open");
                GetComponent<Animation>().Play();
            }

            mouseDown = false;
        }

    }
}
