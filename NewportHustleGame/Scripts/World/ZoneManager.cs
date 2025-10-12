using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NewportHustle.World
{
    public class ZoneManager : MonoBehaviour
    {
        [Header("Zone Settings")]
        public NewportZone currentZone = NewportZone.Downtown;
        public float zoneTransitionRange = 10f;
        
        [Header("Zone Data")]
        public List<ZoneData> zones = new List<ZoneData>();
        
        [Header("Street Network")]
        public List<StreetData> streets = new List<StreetData>();
        
        [Header("Points of Interest")]
        public List<PointOfInterest> pointsOfInterest = new List<PointOfInterest>();
        
        [Header("Street Characters")]
        public List<StreetCharacter> streetCharacters = new List<StreetCharacter>();
        
        // Events
        public System.Action<NewportZone, NewportZone> OnZoneChanged;
        public System.Action<string> OnStreetEntered;
        public System.Action<PointOfInterest> OnPOIDiscovered;
        
        // Internal tracking
        private NewportZone previousZone;
        private string currentStreet = "";
        private HashSet<string> discoveredPOIs = new HashSet<string>();
        
        void Start()
        {
            InitializeNewportMap();
            InitializeZones();
            InitializeStreets();
            InitializePOIs();
            InitializeStreetCharacters();
        }
        
        void Update()
        {
            CheckPlayerZone();
            CheckPlayerStreet();
            CheckPOIDiscovery();
        }
        
        private void InitializeNewportMap()
        {
            Debug.Log("Initializing Newport, Arkansas Map Layout");
            
            // Create zone data based on real Newport areas
            zones.Clear();
            
            // Downtown Newport
            zones.Add(new ZoneData
            {
                zoneName = "Downtown Newport",
                zoneType = NewportZone.Downtown,
                description = "The heart of Newport with historic Main Street, city hall, and traditional businesses",
                bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(200, 50, 200)),
                atmosphereMusic = "downtown_theme",
                ambientSounds = new string[] { "traffic", "people_talking", "downtown_bustle" },
                weatherEffects = true,
                safetyLevel = 0.9f
            });
            
            // White River Waterfront
            zones.Add(new ZoneData
            {
                zoneName = "White River Waterfront",
                zoneType = NewportZone.Riverfront,
                description = "Scenic waterfront along the White River with docks, fishing spots, and riverside businesses",
                bounds = new Bounds(new Vector3(-150, 0, 100), new Vector3(300, 30, 100)),
                atmosphereMusic = "river_theme",
                ambientSounds = new string[] { "water_flowing", "birds", "boat_horns", "fishing_activity" },
                weatherEffects = true,
                safetyLevel = 0.8f
            });
            
            // Residential Areas
            zones.Add(new ZoneData
            {
                zoneName = "Newport Residential",
                zoneType = NewportZone.Residential,
                description = "Quiet residential neighborhoods with tree-lined streets and family homes",
                bounds = new Bounds(new Vector3(150, 0, -100), new Vector3(250, 40, 200)),
                atmosphereMusic = "residential_theme",
                ambientSounds = new string[] { "suburban_quiet", "dogs_barking", "children_playing" },
                weatherEffects = true,
                safetyLevel = 0.95f
            });
            
            // Spa District
            zones.Add(new ZoneData
            {
                zoneName = "Spa District",
                zoneType = NewportZone.SpaDistrict,
                description = "Modern wellness area with upscale spas and health centers",
                bounds = new Bounds(new Vector3(-100, 0, -150), new Vector3(150, 45, 150)),
                atmosphereMusic = "spa_theme",
                ambientSounds = new string[] { "relaxing_music", "water_features", "peaceful_ambiance" },
                weatherEffects = true,
                safetyLevel = 0.85f
            });
            
            // Barber's Row
            zones.Add(new ZoneData
            {
                zoneName = "Barber's Row",
                zoneType = NewportZone.BarbersRow,
                description = "Historic section of Main Street known for traditional barbershops",
                bounds = new Bounds(new Vector3(50, 0, 50), new Vector3(100, 35, 80)),
                atmosphereMusic = "barber_theme",
                ambientSounds = new string[] { "scissors_cutting", "conversation", "old_radio" },
                weatherEffects = true,
                safetyLevel = 0.9f
            });
            
            // Diaz District
            zones.Add(new ZoneData
            {
                zoneName = "Diaz District",
                zoneType = NewportZone.DiazDistrict,
                description = "Cultural district near Diaz with diverse businesses and community centers",
                bounds = new Bounds(new Vector3(200, 0, 150), new Vector3(180, 40, 180)),
                atmosphereMusic = "diaz_theme",
                ambientSounds = new string[] { "cultural_music", "community_activity", "diverse_conversations" },
                weatherEffects = true,
                safetyLevel = 0.8f
            });
            
            // Jacksonport Area
            zones.Add(new ZoneData
            {
                zoneName = "Jacksonport Area",
                zoneType = NewportZone.JacksonportArea,
                description = "Historic area near Jacksonport with museums and Civil War sites",
                bounds = new Bounds(new Vector3(-200, 0, 200), new Vector3(160, 35, 160)),
                atmosphereMusic = "historic_theme",
                ambientSounds = new string[] { "historical_ambiance", "tour_groups", "educational_activity" },
                weatherEffects = true,
                safetyLevel = 0.75f
            });
        }
        
        private void InitializeZones()
        {
            foreach (var zone in zones)
            {
                Debug.Log($"Zone Initialized: {zone.zoneName} - {zone.description}");
            }
        }
        
        private void InitializeStreets()
        {
            streets.Clear();
            
            // Real Newport, AR streets
            
            // Main Street (Historic Downtown)
            streets.Add(new StreetData
            {
                streetName = "Main Street",
                streetType = StreetType.MainRoad,
                description = "Historic Main Street - the backbone of Newport commerce",
                startPoint = new Vector3(-100, 0, 0),
                endPoint = new Vector3(100, 0, 0),
                zones = new NewportZone[] { NewportZone.Downtown, NewportZone.BarbersRow },
                landmarks = new string[] { "City Hall", "First National Bank", "Newport Theatre", "Lackey Tamale Shop", "The Yella Store" }
            });
            
            // Front Street (Near River)
            streets.Add(new StreetData
            {
                streetName = "Front Street",
                streetType = StreetType.Waterfront,
                description = "Runs parallel to the White River waterfront",
                startPoint = new Vector3(-150, 0, 80),
                endPoint = new Vector3(150, 0, 80),
                zones = new NewportZone[] { NewportZone.Riverfront },
                landmarks = new string[] { "River Dock", "Newport Marina", "Riverside Park" }
            });
            
            // Hazel Street
            streets.Add(new StreetData
            {
                streetName = "Hazel Street",
                streetType = StreetType.Residential,
                description = "Quiet residential street with historic homes",
                startPoint = new Vector3(50, 0, -50),
                endPoint = new Vector3(50, 0, 100),
                zones = new NewportZone[] { NewportZone.Residential, NewportZone.Downtown },
                landmarks = new string[] { "Newport Elementary", "Community Center" }
            });
            
            // Walnut Street  
            streets.Add(new StreetData
            {
                streetName = "Walnut Street",
                streetType = StreetType.Commercial,
                description = "Commercial street with local businesses",
                startPoint = new Vector3(-50, 0, -100),
                endPoint = new Vector3(-50, 0, 100),
                zones = new NewportZone[] { NewportZone.Downtown, NewportZone.SpaDistrict },
                landmarks = new string[] { "Newport Library", "Chamber of Commerce" }
            });
            
            // Second Street
            streets.Add(new StreetData
            {
                streetName = "Second Street",
                streetType = StreetType.Residential,
                description = "Tree-lined residential street",
                startPoint = new Vector3(-80, 0, 20),
                endPoint = new Vector3(80, 0, 20),
                zones = new NewportZone[] { NewportZone.Residential },
                landmarks = new string[] { "Newport Park", "Historic Homes" }
            });
            
            // Remmel Street
            streets.Add(new StreetData
            {
                streetName = "Remmel Street",
                streetType = StreetType.Industrial,
                description = "Industrial area street",
                startPoint = new Vector3(100, 0, -80),
                endPoint = new Vector3(200, 0, -80),
                zones = new NewportZone[] { NewportZone.DiazDistrict },
                landmarks = new string[] { "Newport Industrial Park" }
            });
            
            // Highway 67 (Major thoroughfare)
            streets.Add(new StreetData
            {
                streetName = "Highway 67",
                streetType = StreetType.Highway,
                description = "Major highway connecting Newport to Little Rock and other cities",
                startPoint = new Vector3(-300, 0, -200),
                endPoint = new Vector3(300, 0, 200),
                zones = new NewportZone[] { NewportZone.DiazDistrict, NewportZone.JacksonportArea },
                landmarks = new string[] { "Newport City Limits", "Highway Welcome Sign" }
            });
            
            // McLain Street
            streets.Add(new StreetData
            {
                streetName = "McLain Street",
                streetType = StreetType.Residential,
                description = "Residential street in the Diaz area",
                startPoint = new Vector3(150, 0, 100),
                endPoint = new Vector3(250, 0, 200),
                zones = new NewportZone[] { NewportZone.DiazDistrict },
                landmarks = new string[] { "Diaz Community Center", "Local Market" }
            });
            
            // Jacksonport Road
            streets.Add(new StreetData
            {
                streetName = "Jacksonport Road",
                streetType = StreetType.Historic,
                description = "Historic road leading to Jacksonport State Park",
                startPoint = new Vector3(-150, 0, 150),
                endPoint = new Vector3(-250, 0, 250),
                zones = new NewportZone[] { NewportZone.JacksonportArea },
                landmarks = new string[] { "Jacksonport State Park", "Civil War Museum", "Historic Courthouse" }
            });
            
            // River Road
            streets.Add(new StreetData
            {
                streetName = "River Road",
                streetType = StreetType.Scenic,
                description = "Scenic road following the White River",
                startPoint = new Vector3(-200, 0, 50),
                endPoint = new Vector3(200, 0, 120),
                zones = new NewportZone[] { NewportZone.Riverfront },
                landmarks = new string[] { "Fishing Access Points", "River Overlooks", "Boat Launches" }
            });
        }
        
        private void InitializePOIs()
        {
            pointsOfInterest.Clear();
            
            // Historic Points
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "Newport Courthouse Square",
                description = "Historic courthouse in the heart of downtown Newport",
                position = new Vector3(0, 0, 0),
                type = POIType.Historic,
                zone = NewportZone.Downtown,
                discoverable = true,
                importance = POIImportance.Major
            });
            
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "Jacksonport State Park",
                description = "Historic Civil War site with museum and restored courthouse",
                position = new Vector3(-220, 0, 220),
                type = POIType.Historic,
                zone = NewportZone.JacksonportArea,
                discoverable = true,
                importance = POIImportance.Major
            });
            
            // Business Points
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "Thompson's Traditional Cuts",
                description = "Est. 1952 - Premier barbershop on Main Street",
                position = new Vector3(60, 0, 45),
                type = POIType.Business,
                zone = NewportZone.BarbersRow,
                discoverable = true,
                importance = POIImportance.Minor
            });
            
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "White River Wellness Spa",
                description = "Luxury spa offering Arkansas-style relaxation",
                position = new Vector3(-80, 0, -120),
                type = POIType.Business,
                zone = NewportZone.SpaDistrict,
                discoverable = true,
                importance = POIImportance.Minor
            });
            
            // Natural Points
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "White River Marina",
                description = "Full-service marina on the White River",
                position = new Vector3(-100, 0, 90),
                type = POIType.Recreation,
                zone = NewportZone.Riverfront,
                discoverable = true,
                importance = POIImportance.Minor
            });
            
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "Riverside Fishing Dock",
                description = "Popular fishing spot on the White River",
                position = new Vector3(50, 0, 110),
                type = POIType.Recreation,
                zone = NewportZone.Riverfront,
                discoverable = true,
                importance = POIImportance.Minor
            });
            
            // Cultural Points
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "Diaz Cultural Center",
                description = "Community center celebrating Newport's diverse heritage",
                position = new Vector3(180, 0, 170),
                type = POIType.Cultural,
                zone = NewportZone.DiazDistrict,
                discoverable = true,
                importance = POIImportance.Minor
            });
            
            // Authentic Newport Businesses
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "Lackey Tamale Shop",
                description = "Famous Newport tamale shop serving authentic homemade tamales. A true local institution!",
                position = new Vector3(-20, 0, 5),
                type = POIType.Restaurant,
                zone = NewportZone.Downtown,
                discoverable = true,
                importance = POIImportance.Major
            });
            
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "The Yella Store",
                description = "Distinctive yellow-colored tobacco shop that's been a Newport landmark for years.",
                position = new Vector3(15, 0, -10),
                type = POIType.Shop,
                zone = NewportZone.Downtown,
                discoverable = true,
                importance = POIImportance.Major
            });
            
            pointsOfInterest.Add(new PointOfInterest
            {
                name = "Jordan's Gas Station",
                description = "Popular local gas station known for friendly service and community gathering spot.",
                position = new Vector3(80, 0, 20),
                type = POIType.Service,
                zone = NewportZone.Downtown,
                discoverable = true,
                importance = POIImportance.Major
            });
        }
        
        private void InitializeStreetCharacters()
        {
            Debug.Log("Initializing Newport Street Characters - Snitches, Winos, and Bums");
            
            // The Holy Trinity of Small-Town Authenticity
            
            // Snitches - "Neighborhood Informants"
            streetCharacters.Add(new StreetCharacter
            {
                characterName = "Lil Snitch",
                characterType = StreetCharacterType.Snitch,
                spawnLocation = new Vector3(85, 0, 25), // Near Jordan's Gas Station
                dialogueLines = new string[] {
                    "I seen everything that happened over there",
                    "You didn't hear this from me but...",
                    "The police been asking about you",
                    "I keep my eyes open, you know what I'm saying"
                },
                vehicleType = "snitch_bmx",
                movementSpeed = 4f,
                canCallPolice = true,
                providesInformation = true,
                affectsReputation = true
            });
            
            streetCharacters.Add(new StreetCharacter
            {
                characterName = "Corner Watcher",
                characterType = StreetCharacterType.Snitch,
                spawnLocation = new Vector3(120, 0, -10), // Near The Yella Store
                dialogueLines = new string[] {
                    "I stay posted up, watching everything",
                    "Somebody needs to report these activities",
                    "This neighborhood needs more law and order",
                    "I got the police on speed dial"
                },
                vehicleType = "snitch_bmx",
                movementSpeed = 3.5f,
                canCallPolice = true,
                providesInformation = true
            });
            
            // Winos - "Neighborhood Philosophers"
            streetCharacters.Add(new StreetCharacter
            {
                characterName = "Wise Willie",
                characterType = StreetCharacterType.Wino,
                spawnLocation = new Vector3(100, 0, 50), // Park area
                dialogueLines = new string[] {
                    "Life's like riding a bike, sometimes you wobble",
                    "You know what your problem is? You moving too fast",
                    "Back in my day, we had time to appreciate things",
                    "This town ain't what it used to be, but it's still home"
                },
                vehicleType = "wino_cruiser",
                movementSpeed = 1.5f,
                providesWisdom = true,
                providesInformation = true
            });
            
            streetCharacters.Add(new StreetCharacter
            {
                characterName = "Philosophical Phil",
                characterType = StreetCharacterType.Wino,
                spawnLocation = new Vector3(60, 0, -20), // Near Lackey Tamale Shop
                dialogueLines = new string[] {
                    "Every tamale tells a story, you feel me?",
                    "Slow down and smell the roses, young blood",
                    "The best conversations happen on two wheels",
                    "Newport's got soul, you just gotta find it"
                },
                vehicleType = "wino_cruiser",
                movementSpeed = 1.8f,
                providesWisdom = true
            });
            
            // Bums - "Traveling Entrepreneurs"
            streetCharacters.Add(new StreetCharacter
            {
                characterName = "Shortcut Sam",
                characterType = StreetCharacterType.Bum,
                spawnLocation = new Vector3(140, 0, 30), // Alley behind businesses
                dialogueLines = new string[] {
                    "You got any spare change? Gas money, you know",
                    "I know a shortcut that'll save you 10 minutes",
                    "One man's trash is another man's treasure",
                    "This bike gets better mileage than your car"
                },
                vehicleType = "bum_mountain_bike",
                movementSpeed = 3f,
                knowsShortcuts = true,
                sellsItems = true,
                providesInformation = true
            });
            
            streetCharacters.Add(new StreetCharacter
            {
                characterName = "Recycling Rick",
                characterType = StreetCharacterType.Bum,
                spawnLocation = new Vector3(20, 0, 80), // Industrial area
                dialogueLines = new string[] {
                    "Everything's got value if you know how to see it",
                    "I been all over this town, know every back road",
                    "Help a brother out? I'll make it worth your while",
                    "These wheels have taken me places you wouldn't believe"
                },
                vehicleType = "bum_mountain_bike",
                movementSpeed = 2.8f,
                knowsShortcuts = true,
                sellsItems = true
            });
            
            Debug.Log($"Initialized {streetCharacters.Count} street characters");
        }
        
        private void CheckPlayerZone()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            NewportZone newZone = GetZoneAtPosition(playerPos);
            
            if (newZone != currentZone)
            {
                previousZone = currentZone;
                currentZone = newZone;
                OnZoneChanged?.Invoke(previousZone, currentZone);
                
                ZoneData zoneData = GetZoneData(currentZone);
                if (zoneData != null)
                {
                    Debug.Log($"Entered: {zoneData.zoneName} - {zoneData.description}");
                    GameManager.Instance.EnterNewportArea(zoneData.zoneName);
                }
            }
        }
        
        private void CheckPlayerStreet()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            string newStreet = GetStreetAtPosition(playerPos);
            
            if (!string.IsNullOrEmpty(newStreet) && newStreet != currentStreet)
            {
                currentStreet = newStreet;
                OnStreetEntered?.Invoke(currentStreet);
                
                StreetData streetData = GetStreetData(currentStreet);
                if (streetData != null)
                {
                    Debug.Log($"Now on: {streetData.streetName} - {streetData.description}");
                }
            }
        }
        
        private void CheckPOIDiscovery()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            Vector3 playerPos = player.transform.position;
            
            foreach (var poi in pointsOfInterest)
            {
                if (poi.discoverable && !discoveredPOIs.Contains(poi.name))
                {
                    float distance = Vector3.Distance(playerPos, poi.position);
                    if (distance <= 15f) // Discovery range
                    {
                        discoveredPOIs.Add(poi.name);
                        OnPOIDiscovered?.Invoke(poi);
                        Debug.Log($"Discovered: {poi.name} - {poi.description}");
                        
                        // Give small reward for discovery
                        GameManager.Instance.UpdatePlayerRespect(1f);
                    }
                }
            }
        }
        
        private NewportZone GetZoneAtPosition(Vector3 position)
        {
            foreach (var zone in zones)
            {
                if (zone.bounds.Contains(position))
                {
                    return zone.zoneType;
                }
            }
            return NewportZone.Downtown; // Default zone
        }
        
        private string GetStreetAtPosition(Vector3 position)
        {
            foreach (var street in streets)
            {
                // Simple distance check to street line
                float distanceToLine = DistanceToLine(position, street.startPoint, street.endPoint);
                if (distanceToLine <= 15f) // Street width tolerance
                {
                    return street.streetName;
                }
            }
            return "";
        }
        
        private float DistanceToLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDirection = (lineEnd - lineStart).normalized;
            Vector3 pointDirection = point - lineStart;
            float projection = Vector3.Dot(pointDirection, lineDirection);
            
            Vector3 closestPoint;
            if (projection <= 0)
                closestPoint = lineStart;
            else if (projection >= Vector3.Distance(lineStart, lineEnd))
                closestPoint = lineEnd;
            else
                closestPoint = lineStart + lineDirection * projection;
            
            return Vector3.Distance(point, closestPoint);
        }
        
        // Public access methods
        public ZoneData GetZoneData(NewportZone zone)
        {
            return zones.FirstOrDefault(z => z.zoneType == zone);
        }
        
        public StreetData GetStreetData(string streetName)
        {
            return streets.FirstOrDefault(s => s.streetName == streetName);
        }
        
        public List<PointOfInterest> GetPOIsInZone(NewportZone zone)
        {
            return pointsOfInterest.Where(poi => poi.zone == zone).ToList();
        }
        
        public List<string> GetDiscoveredPOIs()
        {
            return discoveredPOIs.ToList();
        }
        
        public string GetCurrentStreetName()
        {
            return currentStreet;
        }
        
        public NewportZone GetCurrentZone()
        {
            return currentZone;
        }
    }
    
    // Data structures
    [System.Serializable]
    public class ZoneData
    {
        public string zoneName;
        public NewportZone zoneType;
        public string description;
        public Bounds bounds;
        public string atmosphereMusic;
        public string[] ambientSounds;
        public bool weatherEffects;
        public float safetyLevel; // 0-1, affects random events
    }
    
    [System.Serializable]
    public class StreetData
    {
        public string streetName;
        public StreetType streetType;
        public string description;
        public Vector3 startPoint;
        public Vector3 endPoint;
        public NewportZone[] zones;
        public string[] landmarks;
    }
    
    [System.Serializable]
    public class PointOfInterest
    {
        public string name;
        public string description;
        public Vector3 position;
        public POIType type;
        public NewportZone zone;
        public bool discoverable;
        public POIImportance importance;
    }
    
    // Enums
    public enum NewportZone
    {
        Downtown,
        Riverfront,
        Residential,
        SpaDistrict,
        BarbersRow,
        DiazDistrict,
        JacksonportArea
    }
    
    public enum StreetType
    {
        MainRoad,
        Residential,
        Commercial,
        Industrial,
        Waterfront,
        Highway,
        Historic,
        Scenic
    }
    
    public enum POIType
    {
        Historic,
        Business,
        Recreation,
        Cultural,
        Natural,
        Government,
        Educational,
        Restaurant,
        Shop,
        Service
    }
    
    public enum POIImportance
    {
        Minor,
        Major,
        Critical
    }
    
    public enum StreetCharacterType
    {
        Snitch,          // Neighborhood informant
        Wino,            // Neighborhood philosopher  
        Bum,             // Traveling entrepreneur
        Citizen,         // Regular Newport resident
        BusinessOwner,   // Local business proprietor
        Student,         // Local school student
        Elder            // Senior community member
    }
    
    [System.Serializable]
    public class StreetCharacter
    {
        public string characterName;
        public StreetCharacterType characterType;
        public Vector3 spawnLocation;
        public string[] dialogueLines;
        public string vehicleType = "bicycle"; // Default to bicycle for authentic types
        public bool hasConsentToAppear = true;
        public string realWorldInspiration = "Community Member";
        
        [Header("Behavior Settings")]
        public float movementSpeed = 2f;
        public float interactionRange = 3f;
        public bool providesInformation = false;
        public bool affectsReputation = false;
        
        [Header("Game Impact")]
        public bool canCallPolice = false;        // Snitches
        public bool providesWisdom = false;       // Winos
        public bool knowsShortcuts = false;       // Bums
        public bool sellsItems = false;           // Various types
    }
}