using UnityEngine;

public class PlayerControls : MonoBehaviour
{
  [SerializeField] private static PlayerControls instance;

  [SerializeField] private GameObject ballPrefab;
  [SerializeField] private Transform hand;
  [SerializeField] private LayerMask interactionMask;
  [SerializeField][Range(0f, 100f)] private float maxDistance = 100f;

  public bool useGravityGun { get; set; }

  [SerializeField][Range(0f, 500f)] private float shootForce = 500f;
  [SerializeField]
  [Range(0f, 100f)] private float pullForce = 50f;

  private GameObject objInHand;

  public static PlayerControls Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindFirstObjectByType<PlayerControls>();
        if (instance == null)
        {
          GameObject playerControlsObject = new GameObject("PlayerControls");
          instance = playerControlsObject.AddComponent<PlayerControls>();
          DontDestroyOnLoad(playerControlsObject);
        }
      }
      return instance;
    }
  }
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    if (instance == null)
    {
      instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else if (instance != this)
    {
      Destroy(gameObject);
    }
  }

  // Update is called once per frame
  void Update()
  {
    Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward * 10f, Color.red);

    if (useGravityGun)
      GravityAction();
    else
    {
      if (objInHand != null)
      {
        objInHand.GetComponent<Rigidbody>().isKinematic = false;
        objInHand.transform.SetParent(null);
        objInHand = null;
      }
    }
  }

  public void TimeSlowdown()
  {
    if(Time.timeScale == 1f)
    {
      Time.timeScale = 0.1f;
      Debug.Log("Time slowed down");
    }
    else
    {
      Time.timeScale = 1f;
      Debug.Log("Time returned to normal");
    }
    Time.fixedDeltaTime *= Time.timeScale;
  }

  public void GravityAction()
  {
    RaycastHit hit;
    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance, interactionMask))
    {
      Debug.Log("Gravity action triggered on " + hit.collider.gameObject.name);
      if (objInHand == null)
      {
        if (Vector3.Distance(hand.position, hit.transform.position) < 1)
        {
          objInHand = hit.transform.gameObject;
          objInHand.transform.position = hand.position;
          objInHand.GetComponent<Rigidbody>().isKinematic = true;
          objInHand.transform.SetParent(hand);
        }
        else
        {
          Vector3 forceDir = (hand.position -hit.transform.position).normalized;
          hit.transform.GetComponent<Rigidbody>().AddForce(forceDir.normalized * pullForce, ForceMode.Impulse);
        }
      }
    }
    else
    {
      Debug.Log("No valid target for gravity action");
    }
  }

  public void Shoot()
  {
    Debug.Log("Shoot triggered");
    if (objInHand == null)
    {
      GameObject ball = Instantiate(ballPrefab, hand.position, Quaternion.identity);
      ball.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * shootForce, ForceMode.Impulse);
    }
    else
    {
      objInHand.transform.SetParent(null);
      objInHand.GetComponent<Rigidbody>().isKinematic = false;
      objInHand.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * shootForce, ForceMode.Impulse);
      objInHand = null;
    }
  }
}
