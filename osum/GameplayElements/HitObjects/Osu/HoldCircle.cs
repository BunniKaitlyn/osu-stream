﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using OpenTK;

namespace osum.GameplayElements.HitObjects.Osu
{
    class HoldCircle : HitObjectSpannable
    {
        #region General & Timing

        private const float TEXT_SIZE = 0.8f;

        internal HoldCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, HitObjectSoundType soundType, double pathLength, int repeatCount, List<HitObjectSoundType> soundTypes)
            : base(hit_object_manager, pos, startTime, soundType, newCombo)
        {
            EndTime = StartTime + (int)(1000 * pathLength / m_HitObjectManager.VelocityAt(StartTime)) * repeatCount;

            Type = HitObjectType.Circle;

            Color4 white = Color4.White;

            SpriteApproachCircle = new pSprite(TextureManager.Load(OsuTexture.approachcircle), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderFwdPrio(StartTime - DifficultyManager.PreEmpt), false, white);
            //if (ShowApproachCircle && (Player.currentScore == null || !ModManager.CheckActive(Player.currentScore.enabledMods, Mods.Hidden)))
            SpriteCollection.Add(SpriteApproachCircle);

            SpriteHitCircle1 =
                new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(StartTime), false, white);
            SpriteCollection.Add(SpriteHitCircle1);
            //SpriteHitCircle1.TagNumeric = 1;
            DimCollection.Add(SpriteHitCircle1);


            SpriteHitCircle2 =
                new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.Gamefield512x384,
                            OriginTypes.Centre, ClockTypes.Audio, Position,
                            SpriteManager.drawOrderBwd(StartTime - (BeatmapManager.ShowOverlayAboveNumber ? 2 : 1)), false, Color4.White);
            SpriteCollection.Add(SpriteHitCircle2);
            DimCollection.Add(SpriteHitCircle2);
            SpriteHitCircleText = new pSpriteText("1", "default", 3, //SkinManager.Current.FontHitCircle, SkinManager.Current.FontHitCircleOverlap, 
                                                    FieldTypes.Gamefield512x384, OriginTypes.Centre,
                                                    ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(StartTime - (BeatmapManager.ShowOverlayAboveNumber ? 1 : 2)),
                                                    false, white);

            SpriteHitCircleText.ScaleScalar = TEXT_SIZE;

            if (ShowCircleText)
            {
                SpriteCollection.Add(SpriteHitCircleText);
                DimCollection.Add(SpriteHitCircleText);
            }

            SpriteApproachCircle.Transform(new Transformation(TransformationType.Fade, 0, 0.9F, 
                startTime - DifficultyManager.PreEmpt, Math.Min(startTime, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn * 2)));

            SpriteApproachCircle.Transform(new Transformation(TransformationType.Scale, 4, 1, 
                startTime - DifficultyManager.PreEmpt, startTime));

            SpriteHitCircle1.Transform(new Transformation(TransformationType.Fade, 0, 1,
                startTime - DifficultyManager.PreEmpt, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn));

            SpriteHitCircle2.Transform(new Transformation(TransformationType.Fade, 0, 1, 
                startTime - DifficultyManager.PreEmpt, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn));

            SpriteHitCircleText.Transform(new Transformation(TransformationType.Fade, 0, 1, 
                startTime - DifficultyManager.PreEmpt, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn));

            SpriteHitCircle1.Transform(new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50));

            SpriteHitCircle2.Transform(new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50));

            SpriteHitCircleText.Transform(new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50));

            //}
        }

        protected virtual bool ShowCircleText
        {
            get { return true; }
        }

        protected virtual bool ShowApproachCircle
        {
            get { return true; }
        }

        protected override ScoreChange HitAction()
        {
            int hitTime = Clock.AudioTime;
            int accuracy = Math.Abs(hitTime - StartTime);

            if (accuracy < DifficultyManager.HitWindow300)
                hitValue = ScoreChange.Hit300;
            else if (accuracy < DifficultyManager.HitWindow100)
                hitValue = ScoreChange.Hit100;
            else if (accuracy < DifficultyManager.HitWindow50)
                hitValue = ScoreChange.Hit50;
            else
                hitValue = ScoreChange.Miss;

            if (hitValue > 0)
                PlaySound();

            return hitValue;
        }

        internal override void HitAnimation(ScoreChange action)
        {
            if (action > 0)
            {
                //Fade out the actual hit circle
                Transformation circleScaleOut = new Transformation(TransformationType.Scale, 1.1F, 1.4F, 
                    Clock.Time, Clock.Time + DifficultyManager.FadeOut, EasingTypes.InHalf);

                Transformation textScaleOut = new Transformation(TransformationType.Scale, 1.1F * TEXT_SIZE, 1.4F * TEXT_SIZE,
                    Clock.Time, Clock.Time + DifficultyManager.FadeOut, EasingTypes.InHalf);
                
                Transformation circleFadeOut = new Transformation(TransformationType.Fade, 1, 0, 
                    Clock.Time, Clock.Time + DifficultyManager.FadeOut);

                //SpriteHitCircle1.Depth = SpriteManager.drawOrderFwd(StartTime + 1);
                SpriteHitCircle1.Transformations.Clear();
                SpriteHitCircle1.Clocking = ClockTypes.Game;
                SpriteHitCircle1.Transform(circleScaleOut);
                SpriteHitCircle1.Transform(circleFadeOut);

                //SpriteHitCircle2.Depth = SpriteManager.drawOrderFwd(StartTime + 2);
                SpriteHitCircle2.Transformations.Clear();
                SpriteHitCircle2.Clocking = ClockTypes.Game;
                SpriteHitCircle2.Transform(circleScaleOut);
                SpriteHitCircle2.Transform(circleFadeOut);

                //SpriteHitCircleText.Depth = SpriteManager.drawOrderFwd(StartTime + 2);
                SpriteHitCircleText.Transformations.Clear();
                SpriteHitCircleText.Clocking = ClockTypes.Game;
                SpriteHitCircleText.Transform(textScaleOut);
                SpriteHitCircleText.Transform(circleFadeOut);

                SpriteApproachCircle.Transformations.Clear();
            }
            else
            {
                foreach (pSprite p in SpriteCollection)
                    p.Transformations.Clear();
            }

            base.HitAnimation(action);
        }

        #endregion

        internal pSprite SpriteApproachCircle;
        internal pSprite SpriteHitCircle1;
        internal pSprite SpriteHitCircle2;
        internal pSpriteText SpriteHitCircleText;

        private int comboNumber;
        internal override int ComboNumber
        {
            get { return comboNumber; }
            set
            {
                if (value == comboNumber) return;

                if (value > 0)
                    SpriteHitCircleText.Text = value.ToString();
                else
                    SpriteHitCircleText.Text = string.Empty;

                comboNumber = value;
            }
        }

        internal override bool IsVisible
        {
            get
            {
                return Clock.AudioTime >= StartTime - DifficultyManager.PreEmpt &&
                     Clock.AudioTime <= EndTime + DifficultyManager.FadeOut;
            }
        }

        internal override Color4 Colour
        {
            get
            {
                return base.Colour;
            }
            set
            {
                SpriteHitCircle1.Colour = value;
                SpriteApproachCircle.Colour = value;
                
                base.Colour = value;
            }
        }

    }
}