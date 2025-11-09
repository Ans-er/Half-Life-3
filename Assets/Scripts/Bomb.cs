using UnityEngine;
using Unity.Netcode;

public class Bomb : NetworkBehaviour
{

  [SerializeField] private float duration = 2;
  [SerializeField] private GameObject explosion;

  [SerializeField] private float radius = 5f;
  [SerializeField] private float force = 10f;
  [SerializeField] private LayerMask interactionLayer;
  [SerializeField] private int damage = 50;

  private const string DefaultExplosionPrefabName = "BigExplosion";

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    if (IsServer)
    {
      Invoke(nameof(Explode), duration);
    }
  }

  private void Awake()
  {
    if (explosion == null)
    {
      var fromResources = Resources.Load<GameObject>(DefaultExplosionPrefabName);
      if (fromResources != null)
      {
        explosion = fromResources;
      }
    }
  }

  public void Explode()
  {
    if (!IsServer) return;

    transform.localScale = Vector3.zero; 

    if (explosion != null)
    {
      SpawnExplosionClientRpc(transform.position);
    }
    Collider[] cols = Physics.OverlapSphere(transform.position, radius, interactionLayer);
    foreach (Collider col in cols)
    {
      var rb = col.attachedRigidbody;
      if (rb != null)
      {
        rb.AddExplosionForce(force, transform.position, radius, 1f, ForceMode.Impulse);
      }
      var player = col.GetComponentInParent<FPS_Character_Controller>();
      if (player != null)
      {
        player.TakeDamage(damage);
      }
    }

    var playersAll = FindObjectsOfType<FPS_Character_Controller>();
    foreach (var p in playersAll)
    {
      if (Vector3.Distance(p.transform.position, transform.position) <= radius)
      {
        p.TakeDamage(damage);
      }
    }

    if (NetworkObject != null && NetworkObject.IsSpawned)
    {
      NetworkObject.Despawn(true);
    }
  }

  [ClientRpc]
  private void SpawnExplosionClientRpc(Vector3 position)
  {
    if (explosion != null)
    {
      Instantiate(explosion, position, Quaternion.identity);
    }
  }
}
