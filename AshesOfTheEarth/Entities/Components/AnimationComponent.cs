using AshesOfTheEarth.Graphics.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AshesOfTheEarth.Entities.Components
{
    public class AnimationComponent : IComponent
    {
        public SpriteSheet SpriteSheet { get; private set; }
        public Dictionary<string, AnimationData> Animations { get; private set; }
        public AnimationController Controller { get; private set; }

        // Helper pentru a obține dreptunghiul sursă curent
        public Rectangle CurrentSourceRectangle =>
            SpriteSheet != null && Controller != null
            ? SpriteSheet.GetSourceRectangle(Controller.CurrentSpriteSheetFrameIndex)
            : Rectangle.Empty;

        public AnimationComponent(SpriteSheet spriteSheet, Dictionary<string, AnimationData> animations)
        {
            SpriteSheet = spriteSheet ?? throw new ArgumentNullException(nameof(spriteSheet));
            Animations = animations ?? new Dictionary<string, AnimationData>();
            Controller = new AnimationController();

            // Poate redăm o animație default "Idle" la creare?
            PlayAnimation("Idle_Down"); // Presupunem că există
        }

        public void PlayAnimation(string animationName)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                
                return;
            }

            if (Animations.TryGetValue(animationName, out AnimationData newAnimationData))
            {
                // Debugging existent
                

                AnimationData currentAnimationData = Controller.CurrentAnimationData;

                // 1. Dacă animația cerută este DEJA cea curentă ȘI încă se redă (și nu e terminată, decât dacă e looping și a ajuns la capăt)
                //    atunci nu facem nimic, o lăsăm să continue.
                if (Controller.CurrentAnimationName == animationName && Controller.IsPlaying && !Controller.AnimationFinished)
                {
                    // System.Diagnostics.Debug.WriteLine($"[AnimComp] '{animationName}' is already playing and not finished. No change.");
                    return;
                }

                // 2. Dacă animația curentă este diferită de cea cerută:
                //    Verificăm dacă animația curentă (dacă există și se redă) este una non-looping.
                //    Dacă da, NU o întrerupem până nu se termină.
                //    EXCEPȚIE: Animațiile de Hurt/Dead ar trebui să poată întrerupe orice. (Adaugă această logică dacă e necesar)
                //    Excepție 2: Dacă noua animație este aceeași cu cea curentă și cea curentă s-a terminat (și e looping), o repornim.

                // Caz specific: dacă vrem să redăm aceeași animație care tocmai s-a terminat și era looping.
                if (Controller.CurrentAnimationName == animationName && !Controller.IsPlaying && Controller.AnimationFinished && newAnimationData.IsLooping)
                {
                    
                    Controller.Play(newAnimationData);
                    return;
                }

                // Caz specific: dacă animația curentă este non-looping, se redă și nu s-a terminat încă,
                // NU o întrerupe decât dacă noua animație este considerată de prioritate mai mare (ex: Hurt, Dead).
                // Pentru simplitate acum, dacă o animație non-looping rulează, o lăsăm să termine.
                if (currentAnimationData != null && !currentAnimationData.IsLooping && Controller.IsPlaying && !Controller.AnimationFinished)
                {
                    // Permite întreruperea doar dacă noua animație este explicit "Hurt" sau "Dead" (sau altele prioritare)
                    // Sau dacă noua animație este aceeași, dar vrem să o "resetăm" (mai rar).
                    bool allowInterrupt = false;
                    if (animationName.Contains("Hurt") || animationName.Contains("Dead")) // Sau folosește enum-uri/flag-uri de prioritate
                    {
                        allowInterrupt = true;
                    }
                    // Adaugă o condiție pentru a permite întreruperea unei animații non-looping dacă vrem explicit (ex: cancel attack)
                    // bool forcePlay = ... (un flag extern dacă e nevoie)


                    if (!allowInterrupt /* && !forcePlay */)
                    {
                        // System.Diagnostics.Debug.WriteLine($"[AnimComp] Preventing interrupt of running non-looping animation '{Controller.CurrentAnimationName}' by '{animationName}'.");
                        return;
                    }
                }

                // Dacă am ajuns aici, înseamnă că putem reda noua animație:
                // - Fie nu era nicio animație (Controller.CurrentAnimationName == null)
                // - Fie animația curentă s-a terminat (și nu era cea nouă care e looping)
                // - Fie animația curentă era diferită și era looping (deci poate fi întreruptă)
                // - Fie animația curentă era non-looping și a fost permisă întreruperea (ex: de Hurt)
                // - Fie noua animație este diferită de cea curentă.

                // System.Diagnostics.Debug.WriteLine($"[AnimComp] Proceeding to play new animation: '{animationName}'.");
                Controller.Play(newAnimationData);
            }
            else
            {
                
                // ... logica de fallback ...
                AnimationData fallbackIdle = Animations.Values.FirstOrDefault(a => a.Name.Contains("Idle"));
                if (fallbackIdle != null && Controller.CurrentAnimationName != fallbackIdle.Name)
                {
                    
                    Controller.Play(fallbackIdle);
                }
                else if (Controller.CurrentAnimationName != null)
                {
                    // System.Diagnostics.Debug.WriteLine($"[AnimComp] Fallback failed or already on Idle. Stopping controller.");
                    Controller.Stop();
                }
            }
        }

        // Pasează update-ul la controller
        public void Update(GameTime gameTime)
        {
            Controller?.Update(gameTime);
        }

    }
}