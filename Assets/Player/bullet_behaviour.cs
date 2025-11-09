using Unity.Netcode;
using UnityEngine;

public class bullet_behaviour : NetworkBehaviour
{
  [Header("Bullet Settings")]
  public float lifetime = 10f;
  public int damage = 10;

  public override void OnNetworkSpawn()
  {
    if (IsServer)
    {
      Invoke(nameof(DespawnSelf), lifetime);
    }
  }

  private void DespawnSelf()
  {
    if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
    {
      NetworkObject.Despawn(true);
    }
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (!IsServer)
      return;

    var player = collision.collider.GetComponentInParent<FPS_Character_Controller>();
    if (player != null)
    {
      player.TakeDamage(damage);
    }

    DespawnSelf();
  }
}
