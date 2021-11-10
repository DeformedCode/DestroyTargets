using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(TargetDestroyer), typeof(TargetsGenerator))]
public class Particles : MonoBehaviour
{
    [SerializeField] private ParticleSystem _destroyExplosionPrefab;
    [SerializeField] private ParticleSystem _hitPrefab;

    public void Hit(Vector3 position, Color color)
    {
        if (_hitPrefab != null)
        {
            ParticleSystem MainHit = Instantiate<ParticleSystem>(_hitPrefab);
            MainHit.transform.position = position;

            ConfiguringParticle(MainHit, color);
        }
    }

    public void DestroyExplosion(ATargetBehavior aTargetBehavior, Color color)
    {
        if (_destroyExplosionPrefab != null)
        {
            ParticleSystem Mainexplosion = Instantiate<ParticleSystem>(_destroyExplosionPrefab);
            Mainexplosion.transform.position = aTargetBehavior.transform.position;

            ConfiguringParticle(Mainexplosion, color);
        }
    }

    private void ConfiguringParticle(ParticleSystem mainParticle, Color color)
    {
        ParticleSystem[] ParticlesBuf = mainParticle.GetComponentsInChildren<ParticleSystem>();

        ParticleSystem[] Particles = new ParticleSystem[ParticlesBuf.Length + 1];
        ParticlesBuf.CopyTo(Particles, 0);
        Particles[Particles.Length - 1] = mainParticle;
        for (int i = 0; i < Particles.Length; i++)
        {
            Particles[i].Clear(true);
            var MainModule = Particles[i].main;
            MainModule.stopAction = ParticleSystemStopAction.Destroy;

            MainModule.startColor = color;

            mainParticle.GetComponent<ParticleSystemRenderer>().material.color = color;
        }

        mainParticle.Play(true);
    }
}
