using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Bomb : MonoBehaviour
{

  [SerializeField] private float duration = 2;
  [SerializeField] private GameObject explosion;

  [SerializeField] private float radius = 5f;
  [SerializeField] private float force = 10f;
  [SerializeField] private LayerMask interactionLayer;

  private const string DefaultExplosionPrefabName = "BigExplosion";

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    Invoke(nameof(Explode), duration);
  }

  private void Awake()
  {
    // Fallback: zur Laufzeit aus Resources laden, wenn nichts zugewiesen ist.
    if (explosion == null)
    {
      var fromResources = Resources.Load<GameObject>(DefaultExplosionPrefabName);
      if (fromResources != null)
      {
        explosion = fromResources;
        Debug.Log($"Bomb: Loaded '{DefaultExplosionPrefabName}' from Resources as explosion prefab.", this);
      }
    }
  }

  private void OnValidate()
  {
    if (explosion == null)
    {
      Debug.LogWarning("Bomb: 'explosion' prefab is not assigned in the inspector.", this);
    }
  }

  public void Explode()
  {
    transform.localScale = Vector3.zero; 

    if (explosion != null)
    {
      Instantiate(explosion, transform.position, Quaternion.identity);
      Collider[] cols = Physics.OverlapSphere(transform.position, radius, interactionLayer);
      foreach(Collider col in cols)
      {
        var rb = col.attachedRigidbody;
        if (rb != null)
        {
          rb.AddExplosionForce(force, transform.position, radius, 1f, ForceMode.Impulse);
        }
      }
    }
    else
    {
      Debug.LogError("Bomb: Cannot instantiate explosion because no prefab is assigned.", this);
    }
  }
}
