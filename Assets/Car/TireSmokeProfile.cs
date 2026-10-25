using UnityEngine;

[CreateAssetMenu(menuName = "Racing/Tire Smoke")]
public class TireSmokeProfile : ScriptableObject
{
    public Material material;
    public float minSize = 0.35f;
    public float maxSize = 0.7f;
    public float minLifetime = 0.6f;
    public float maxLifetime = 1.1f;
    public float emission = 38;
    public float slipThreshold = 0.65f;
    public float linger = 0.35f;
    public void Apply(ParticleSystem particles)
    {
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.startLifetime = new ParticleSystem.MinMaxCurve(minLifetime, maxLifetime);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startColor = new Color(0.97f, 0.92f, 0.83f, 0.85f);
        main.gravityModifier = 0;
        main.maxParticles = 160;
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.08f;
        var emissionModule = particles.emission;
        emissionModule.rateOverTime = emission;
        emissionModule.enabled = false;
        var color = particles.colorOverLifetime;
        color.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(0.7f, 0.66f, 0.61f), 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(0.85f, 0.12f), new GradientAlphaKey(0, 1) });
        color.color = gradient;
        var size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1.5f, AnimationCurve.Linear(0, 0.5f, 1, 1));
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.sortingOrder = 5;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (Application.isPlaying) particles.Play();
    }
}
