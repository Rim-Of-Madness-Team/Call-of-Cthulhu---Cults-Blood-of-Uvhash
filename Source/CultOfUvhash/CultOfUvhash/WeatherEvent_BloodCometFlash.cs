﻿using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfUvhash
{
    public class WeatherEvent_BloodCometFlash : WeatherEvent
    {
        public WeatherEvent_BloodCometFlash(Map map) : base(map)
        {
            this.duration = Rand.Range(15, 60);
            this.shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
        }

        public override bool Expired
        {
            get
            {
                return this.age > this.duration;
            }
        }

        public override SkyTarget SkyTarget
        {
            get
            {
                return new SkyTarget(1f, WeatherEvent_BloodCometFlash.BloodCometFlashColors, 1f, 1f);
            }
        }

        public override Vector2? OverrideShadowVector
        {
            get
            {
                return new Vector2?(this.shadowVector);
            }
        }

        public override float SkyTargetLerpFactor
        {
            get
            {
                return this.LightningBrightness;
            }
        }

        protected float LightningBrightness
        {
            get
            {
                if (this.age <= 3)
                {
                    return (float)this.age / 3f;
                }
                return 1f - (float)this.age / (float)this.duration;
            }
        }

        public override void FireEvent()
        {
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(this.map);
        }

        public override void WeatherEventTick()
        {
            this.age++;
        }

        private int duration;

        private Vector2 shadowVector;

        private int age;

        private const int FlashFadeInTicks = 3;

        private const int MinFlashDuration = 15;

        private const int MaxFlashDuration = 60;

        private const float FlashShadowDistance = 5f;

        private static readonly SkyColorSet BloodCometFlashColors = new SkyColorSet(new Color(0.9f, 0.05f, 0.05f), new Color(0.784313738f, 0.8235294f, 0.847058833f), new Color(0.9f, 0.05f, 0.05f), 1.15f);
    }
}
