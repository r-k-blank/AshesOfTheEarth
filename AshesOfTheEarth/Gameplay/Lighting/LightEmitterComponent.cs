using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Gameplay.Lighting
{
    public class LightEmitterComponent : Entities.Components.IComponent
    {
        public float LightRadius { get; set; } = 100f;
        public float LightIntensity { get; set; } = 1.0f;
        public Color LightColor { get; set; } = Color.White;
        public bool IsActive { get; set; } = true;
        public float FlickerIntensity { get; set; } = 0.0f;
        public float FlickerSpeed { get; set; } = 1.0f;
        internal float CurrentFlickerOffset { get; set; } = 0f;


        public LightEmitterComponent(float radius, float intensity = 1.0f, Color? color = null, bool isActive = true, float flickerIntensity = 0.0f, float flickerSpeed = 1.0f)
        {
            LightRadius = radius;
            LightIntensity = MathHelper.Clamp(intensity, 0f, 1f);
            LightColor = color ?? Color.White;
            IsActive = isActive;
            FlickerIntensity = MathHelper.Clamp(flickerIntensity, 0f, 0.5f);
            FlickerSpeed = flickerSpeed;
        }
    }
}