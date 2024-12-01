using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleJobs.Demos {

  /// <summary>
  /// Particles in the SpatialHash demo are GameObjects, controlled by this MonoBehaviour.
  /// </summary>
  public class SpatialHashParticle : MonoBehaviour {

    SpatialHashDemo demo;

    public SpriteRenderer Renderer;
    public Gradient CollisionGradient;

    public Vector3 Scale;

    private void Update() {
      
      if(demo == false) {
        demo = GetComponentInParent<SpatialHashDemo>();
      }

      if(demo == false) { return; }

      int index = transform.GetSiblingIndex();

      if(demo.Particles.Length > index) {
        transform.position = demo.Particles[index].Position;
        transform.localScale = demo.Particles[index].Radius * Scale;

        Renderer.color = CollisionGradient.Evaluate(demo.Particles[index].CollisionTimer);
      }
      else {
        Destroy(gameObject);
      }

    }


  }
}