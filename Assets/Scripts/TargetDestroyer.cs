using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Particles), typeof(TargetsGenerator))]
public class TargetDestroyer : MonoBehaviour
{

    [Header("Destroyed parts: in how many seconds  ")]
    [Range(1, 10)] [SerializeField] private float _destroyPartsTime = 5;
    [Range(0.01f, 10)] [SerializeField] private float _disableCollisionTime = 0.3f;

    [SerializeField] private Material _explosionZoneMaterial;


    private void OnValidate()
    {
        if (_disableCollisionTime > _destroyPartsTime)
            _disableCollisionTime = _destroyPartsTime - 0.01f;
    }

    public void Destroy(ATargetBehavior aTargetBehavior, Color targetColor)
    {
        if (aTargetBehavior.DestroyedTarget != null)
        {
            DestroyedTarget destroyedTarget = aTargetBehavior.DestroyedTarget;
            destroyedTarget.DisableCollisionOverTime = _disableCollisionTime;
            destroyedTarget.DeactivateOverTime = _destroyPartsTime;

            destroyedTarget.gameObject.SetActive(true);

            Transform destroyedTargetTransforme = destroyedTarget.transform;
            destroyedTargetTransforme.position = aTargetBehavior.transform.position;


            Rigidbody[] partsRigidbody = destroyedTarget.GetComponentsInChildren<Rigidbody>();

            MeshRenderer[] partsMesh = destroyedTarget.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < partsRigidbody.Length; i++)
            {
                partsRigidbody[i].velocity = Vector3.zero;
                partsRigidbody[i].AddExplosionForce(Random.Range(50, 120), Vector3.zero, Random.Range(1, 5));

                partsMesh[i].materials[destroyedTarget.ChangeableMaterialIndex].color = targetColor;
            }

            aTargetBehavior.DestroyedTarget = null;
        }
    }


    public void Explosion(ATargetBehavior aTargetBehavior, Color targetColor)
    {
        GameObject explosionZone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionZone.GetComponent<Collider>().isTrigger = true;

        Transform explosionZoneTransform = explosionZone.transform;
        explosionZoneTransform.position = aTargetBehavior.transform.position;

        explosionZone.GetComponent<MeshRenderer>().material = _explosionZoneMaterial;
        Material zoneMaterial = explosionZone.GetComponent<MeshRenderer>().material;
        zoneMaterial.color = targetColor;

        explosionZone.tag = "ExplosionZone";

        TargetBomb targetBomb = aTargetBehavior as TargetBomb;
        StartCoroutine(ScaleExplosionZoneOverTime(targetBomb, explosionZoneTransform, zoneMaterial));
        Destroy(explosionZone, targetBomb.ExplosionTimeInSec + 0.5f);
    }
    private IEnumerator ScaleExplosionZoneOverTime(TargetBomb targetBomb, Transform explosionZoneTransform, Material zoneMaterial)
    {
        float _currenExplosionTime = 0;

        Color zoneStartColor = zoneMaterial.color;
        Color zoneEndColor = new Color(zoneStartColor.r, zoneStartColor.g, zoneStartColor.b, 0);
        while (_currenExplosionTime < targetBomb.ExplosionTimeInSec)
        {
            _currenExplosionTime += Time.deltaTime;

            explosionZoneTransform.localScale = Vector3.Lerp(Vector3.one, targetBomb.ExplosionZoneSize, _currenExplosionTime / targetBomb.ExplosionTimeInSec);
            zoneMaterial.color = Color.Lerp(zoneStartColor, zoneEndColor, _currenExplosionTime / targetBomb.ExplosionTimeInSec);

            yield return null;
        }
    }
}
