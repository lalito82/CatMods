using Beam;
using Beam.Serialization.Json;
using UnityEngine;
using System.Reflection;

namespace CatsItems
{
    public class Cat_Bucket : InteractiveObject_FOOD, IBase, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical
    {
        public bool IsSalty { get { return this._isSalty; } }

        protected override void Awake()
        {
            base.Awake();

            Servings = 0;
            _waterPlane = gameObject.transform.Find("Bucket_Water");
            _fillPoint = gameObject.transform.Find("Bucket_FillPosition");

            _boilSound = GetComponentInChildren<AudioSource>();
            _boilSound.loop = true;
            _boilSound.rolloffMode = AudioRolloffMode.Custom;

            PlayerRegistry.LocalPlayer.Movement.Jumped += PlayerJumped;
        }

        private void Update()
        {
            bool flag = !LevelLoader.IsLoading();
			if (flag)
			{
				bool flag2 = base.Servings == 0;
				if (flag2)
				{
					bool activeInHierarchy = this._waterPlane.gameObject.activeInHierarchy;
					if (activeInHierarchy)
					{
						this._waterPlane.gameObject.SetActive(false);
					}
					this._isSalty = false;
				}
				else
				{
					bool flag3 = !this._waterPlane.gameObject.activeInHierarchy;
					if (flag3)
					{
						this._waterPlane.gameObject.SetActive(true);
					}
					float num = (float)base.Servings / base.OriginalServings;
					float num2 = 1f - (0.25f - 0.25f * num);
					this._waterPlane.localPosition = new Vector3(0f, -(0.24f * (1f - num)), 0f);
					this._waterPlane.localScale = new Vector3(num2, num2, num2);
				}
				bool flag4 = this._fillPoint.position.y < 0f && base.Servings != base.OriginalServings;
				if (flag4)
				{
					typeof(Cooking).GetField("_cookingHours", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(base.Cooking, 2f);
					base.Servings = OriginalServings;
					this._isSalty = true;
				}
				else
				{
					bool flag5 = !base.IsPickedUp;
					if (flag5)
					{
						float num3 = this.CheckTilt();
						bool flag6 = Singleton<AtmosphereStorm>.Instance.Rain > 0 && num3 > 0.5f && base.Servings < base.OriginalServings;
						if (flag6)
						{
							this._rainFill += Time.deltaTime;
							bool flag7 = this._rainFill >= 200f;
							if (flag7)
							{
								this.CollectRain();
							}
						}
					}
				}
				this.CheckWater();
			}

        public override void OnDestroy()
        {
            base.OnDestroy();
            PlayerRegistry.LocalPlayer.Movement.Jumped -= PlayerJumped;
        }

        public override string GetDisplayName()
        {
            CheckWater();
            return DisplayName;
        }

        public override bool InteractWithObject(IPlayer player, IBase obj)
        {
            InteractiveObject_FOOD waterContainer = obj as InteractiveObject_FOOD;

            if (waterContainer != null && !_isSalty && waterContainer.Servings < waterContainer.OriginalServings && waterContainer.Hydration > 0 && Servings > 0)
            {
                waterContainer.Servings++;
                Servings--;

                return true;
            }

            return false;
        }

        public override void Use()
        {
            if (Servings > 0 && !Owner.Movement.IsBusy)
            {
                float vomitChance = this.IsSalty ? 100f : 0f;
                Owner.Statistics.Eat(InteractiveType.FOOD_WATER_SKIN, MeatProvenance.Other, base.Calories, 0, 0, 0, base.HydrationPerServe, vomitChance);

                Servings--;
            }
        }

        public override void Hold(bool holding)
        {
            if (holding) StopBoiling();
            base.Hold(holding);
        }

        public override bool ValidatePrimary(IBase obj)
        {
            if (Servings > 0 && !Owner.Movement.IsBusy)
            {
                return base.ValidatePrimary(obj);
            }
            return false;
        }

        private void PlayerJumped()
        {
            bool flag = base.IsPickedUp && base.Servings > base.OriginalServings * 0.6f;
			if (flag)
			{
				int servings = base.Servings;
				base.Servings = servings - 1;
				bool flag2 = this._waterParticles != null;
				if (flag2)
				{
					this._waterParticles.Emit(4);
				}
			}


        private void CollectRain()
        {
            Servings++;
            _rainFill = 0;
        }

        public void CheckWater()
        {
            if (Servings > 0)
            {
                DisplayNamePrefixes.Remove("ITEM_DISPLAY_NAME_PREFIX_EMPTY");
                DisplayNamePrefixes.Remove(!_isSalty ? "Sea Water" : "Fresh Water");
                string prefix = this._isSalty ? "Sea Water" : "Fresh Water";
                DisplayNamePrefixes.AddOrIgnore(prefix, -2);
            }
            else
            {
                DisplayNamePrefixes.Remove("Sea Water");
                DisplayNamePrefixes.Remove("Fresh Water");
                DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -2);
            }
            base.OnDisplayNameChanged();
        }

        private float CheckTilt()
        {
            bool flag = num < 0.5f && base.Servings == base.OriginalServings;
			if (flag)
			{
				int servings = base.Servings;
				base.Servings = servings - 1;
				this._waterParticles.Emit(2);
			}
			else
			{
				bool flag2 = num < 0.45f && base.Servings >= base.OriginalServings * 0.8f;
				if (flag2)
				{
					int servings = base.Servings;
					base.Servings = servings - 1;
					this._waterParticles.Emit(2);
				}
				else
				{
					bool flag3 = num < 0.4f && base.Servings >= base.OriginalServings * 0.6f;
					if (flag3)
					{
						int servings = base.Servings;
						base.Servings = servings - 1;
						this._waterParticles.Emit(2);
					}
					else
					{
						bool flag4 = num < 0.35f && base.Servings >= base.OriginalServings * 0.4f;
						if (flag4)
						{
							int servings = base.Servings;
							base.Servings = servings - 1;
							this._waterParticles.Emit(2);
						}
						else
						{
							bool flag5 = num < 0.3f && base.Servings >= base.OriginalServings * 0.2f;
							if (flag5)
							{
								int servings = base.Servings;
								base.Servings = servings - 1;
								this._waterParticles.Emit(2);
								this.CheckWater();
							}
						}
					}
				}
			}
			return num;
        }

        public void BoilToFresh()
        {
            _isSalty = false;
            CheckWater();
        }

        public void StartBoiling()
        {
            if (!_audioHandleEnabled)
            {
                _boilAudioHandle = AudioManager.GetAudioPlayer().Play3D(_boilSound.clip, transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
                _audioHandleEnabled = true;
                Cooking.IsBeingCooked = true;
            }
        }

        public void StopBoiling()
        {
            AudioManager.GetAudioPlayer().Stop(_boilAudioHandle);
            _boilSound.Stop();

            _boilAudioHandle = AudioActorHandle.Empty;
            _audioHandleEnabled = false;
            Cooking.IsBeingCooked = false;
        }

        public override JObject Save()
        {
            JObject jobject = base.Save();

            jobject.AddField("IsSalty", _isSalty);
            jobject.AddField("RainFill", _rainFill);

            gameObject.SetActive(true);

            return jobject;
        }

        public override void Load(JObject data)
        {
            base.Load(data);

            JObject field = data.GetField("IsSalty");
            _isSalty = field.GetValue<bool>();

            JObject field2 = data.GetField("RainFill");
            _rainFill = field2.GetValue<float>();

            CheckWater();
        }

        private Transform _fillPoint;
        private Transform _waterPlane;

        private ParticleSystem _waterParticles;

        private bool _isSalty = false;
        private float _rainFill = 0;

        private AudioActorHandle _boilAudioHandle = AudioActorHandle.Empty;
        private AudioSource _boilSound = null;
        private bool _audioHandleEnabled = false;

        private const int SECONDS_TO_REFILL_RAIN = 200;
    }
}
