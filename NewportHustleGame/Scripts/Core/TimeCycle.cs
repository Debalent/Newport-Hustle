using UnityEngine;
using System;

namespace NewportHustle.Core
{
    public class TimeCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        public float dayLengthInMinutes = 24f; // Real minutes for a full day
        public float currentTime = 12f; // Start at noon (12:00)
        public int currentDay = 1;
        public bool isPaused = false;
        
        [Header("Visual Settings")]
        public Gradient sunColor;
        public Gradient fogColor;
        public AnimationCurve sunIntensityCurve;
        public AnimationCurve fogDensityCurve;
        
        [Header("Newport Weather")]
        public WeatherType currentWeather = WeatherType.Clear;
        public float weatherChangeChance = 0.1f; // Per game hour
        
        // Time constants
        private const float SECONDS_PER_REAL_MINUTE = 60f;
        private const float HOURS_PER_DAY = 24f;
        
        // Calculated values
        private float timeMultiplier;
        private float timeOfDay; // 0-1 value representing time of day
        
        // Lighting references
        private Light sunLight;
        private RenderSettings renderSettings;
        
        // Events
        public static event Action<float> OnTimeChanged;
        public static event Action<int> OnDayChanged;
        public static event Action<TimeOfDayPeriod> OnTimeOfDayChanged;
        public static event Action<WeatherType> OnWeatherChanged;
        
        private TimeOfDayPeriod currentPeriod = TimeOfDayPeriod.Day;
        
        void Start()
        {
            InitializeTimeCycle();
        }
        
        void Update()
        {
            if (!isPaused && GameManager.Instance.currentGameState == GameState.Playing)
            {
                UpdateTime();
                UpdateVisuals();
                CheckWeatherChange();
            }
        }
        
        private void InitializeTimeCycle()
        {
            // Calculate time multiplier
            timeMultiplier = HOURS_PER_DAY / (dayLengthInMinutes * SECONDS_PER_REAL_MINUTE);
            
            // Find sun light
            sunLight = GameObject.FindGameObjectWithTag("SunLight")?.GetComponent<Light>();
            if (sunLight == null)
            {
                sunLight = FindObjectOfType<Light>();
            }
            
            // Set initial weather based on Newport, AR climate
            SetRandomWeatherForNewport();
        }
        
        private void UpdateTime()
        {
            currentTime += Time.deltaTime * timeMultiplier;
            
            if (currentTime >= HOURS_PER_DAY)
            {
                currentTime -= HOURS_PER_DAY;
                currentDay++;
                OnDayChanged?.Invoke(currentDay);
                
                // New day events
                HandleNewDay();
            }
            
            timeOfDay = currentTime / HOURS_PER_DAY;
            OnTimeChanged?.Invoke(currentTime);
            
            CheckTimeOfDayPeriod();
        }
        
        private void UpdateVisuals()
        {
            if (sunLight != null)
            {
                UpdateLighting();
            }
            UpdateAtmosphere();
        }
        
        private void UpdateLighting()
        {
            // Calculate sun rotation based on time
            float sunAngle = (timeOfDay - 0.25f) * 360f; // Sunrise at 6 AM
            sunLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, 170f, 0f);
            
            // Update sun intensity
            float intensity = sunIntensityCurve.Evaluate(timeOfDay);
            sunLight.intensity = intensity * GetWeatherIntensityMultiplier();
            
            // Update sun color
            sunLight.color = sunColor.Evaluate(timeOfDay);
        }
        
        private void UpdateAtmosphere()
        {
            // Update fog
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor.Evaluate(timeOfDay);
            RenderSettings.fogDensity = fogDensityCurve.Evaluate(timeOfDay) * GetWeatherFogMultiplier();
            
            // Update ambient lighting
            RenderSettings.ambientIntensity = Mathf.Clamp01(sunIntensityCurve.Evaluate(timeOfDay) + 0.2f);
        }
        
        private void CheckTimeOfDayPeriod()
        {
            TimeOfDayPeriod newPeriod = GetTimeOfDayPeriod();
            if (newPeriod != currentPeriod)
            {
                currentPeriod = newPeriod;
                OnTimeOfDayChanged?.Invoke(currentPeriod);
            }
        }
        
        private void CheckWeatherChange()
        {
            // Check for weather change every game hour
            if (Mathf.FloorToInt(currentTime) != Mathf.FloorToInt(currentTime - Time.deltaTime * timeMultiplier))
            {
                if (UnityEngine.Random.value < weatherChangeChance)
                {
                    ChangeWeather();
                }
            }
        }
        
        private void HandleNewDay()
        {
            Debug.Log($"Day {currentDay} in Newport");
            
            // Newport-specific daily events
            HandleNewportDailyEvents();
            
            // Reset some daily mechanics
            SetRandomWeatherForNewport();
        }
        
        private void HandleNewportDailyEvents()
        {
            // River activities are more active in the morning
            if (IsTimeOfDay(TimeOfDayPeriod.Morning))
            {
                // Riverfront area becomes more active
            }
            
            // Business hours simulation
            // Barber shops open during day
            // Spas have extended hours
            // Different NPC behavior based on time
        }
        
        private void SetRandomWeatherForNewport()
        {
            // Newport, AR weather patterns
            float rand = UnityEngine.Random.value;
            
            // Summer weather (more clear days)
            if (currentDay % 90 < 45) // Summer season
            {
                if (rand < 0.6f) currentWeather = WeatherType.Clear;
                else if (rand < 0.8f) currentWeather = WeatherType.Cloudy;
                else if (rand < 0.95f) currentWeather = WeatherType.Rain;
                else currentWeather = WeatherType.Storm;
            }
            // Winter weather
            else
            {
                if (rand < 0.4f) currentWeather = WeatherType.Clear;
                else if (rand < 0.7f) currentWeather = WeatherType.Cloudy;
                else if (rand < 0.9f) currentWeather = WeatherType.Rain;
                else currentWeather = WeatherType.Fog;
            }
            
            OnWeatherChanged?.Invoke(currentWeather);
        }
        
        private void ChangeWeather()
        {
            WeatherType[] possibleWeathers = { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Rain, WeatherType.Fog };
            WeatherType newWeather = possibleWeathers[UnityEngine.Random.Range(0, possibleWeathers.Length)];
            
            if (newWeather != currentWeather)
            {
                currentWeather = newWeather;
                OnWeatherChanged?.Invoke(currentWeather);
            }
        }
        
        private float GetWeatherIntensityMultiplier()
        {
            switch (currentWeather)
            {
                case WeatherType.Clear: return 1f;
                case WeatherType.Cloudy: return 0.7f;
                case WeatherType.Rain: return 0.4f;
                case WeatherType.Storm: return 0.3f;
                case WeatherType.Fog: return 0.5f;
                default: return 1f;
            }
        }
        
        private float GetWeatherFogMultiplier()
        {
            switch (currentWeather)
            {
                case WeatherType.Clear: return 1f;
                case WeatherType.Cloudy: return 1.2f;
                case WeatherType.Rain: return 1.5f;
                case WeatherType.Storm: return 2f;
                case WeatherType.Fog: return 3f;
                default: return 1f;
            }
        }
        
        // Public methods
        public TimeOfDayPeriod GetTimeOfDayPeriod()
        {
            if (currentTime >= 5f && currentTime < 12f) return TimeOfDayPeriod.Morning;
            else if (currentTime >= 12f && currentTime < 18f) return TimeOfDayPeriod.Day;
            else if (currentTime >= 18f && currentTime < 22f) return TimeOfDayPeriod.Evening;
            else return TimeOfDayPeriod.Night;
        }
        
        public bool IsTimeOfDay(TimeOfDayPeriod period)
        {
            return GetTimeOfDayPeriod() == period;
        }
        
        public string GetFormattedTime()
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
            string ampm = hours < 12 ? "AM" : "PM";
            
            if (hours == 0) hours = 12;
            else if (hours > 12) hours -= 12;
            
            return $"{hours:D2}:{minutes:D2} {ampm}";
        }
        
        public float GetCurrentTime() => currentTime;
        public int GetDaysPassed() => currentDay;
        public WeatherType GetCurrentWeather() => currentWeather;
        
        public void SetTime(float time)
        {
            currentTime = Mathf.Clamp(time, 0f, HOURS_PER_DAY);
        }
        
        public void SetDaysPassed(int days)
        {
            currentDay = Mathf.Max(1, days);
        }
        
        public void PauseTime() => isPaused = true;
        public void ResumeTime() => isPaused = false;
        
        // Newport-specific time events
        public bool IsBusinessHours()
        {
            return currentTime >= 8f && currentTime <= 20f; // 8 AM to 8 PM
        }
        
        public bool IsRushHour()
        {
            return (currentTime >= 7f && currentTime <= 9f) || (currentTime >= 17f && currentTime <= 19f);
        }
        
        public bool IsRiverHour()
        {
            // Best times for riverfront activities
            return currentTime >= 6f && currentTime <= 10f; // Early morning
        }
    }
    
    public enum TimeOfDayPeriod
    {
        Morning,    // 5 AM - 12 PM
        Day,        // 12 PM - 6 PM
        Evening,    // 6 PM - 10 PM
        Night       // 10 PM - 5 AM
    }
    
    public enum WeatherType
    {
        Clear,
        Cloudy,
        Rain,
        Storm,
        Fog
    }
}