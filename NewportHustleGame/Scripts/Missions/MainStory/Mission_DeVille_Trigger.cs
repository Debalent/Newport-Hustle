using UnityEngine;

namespace NewportHustle.Missions.MainStory
{
    /// <summary>
    /// Simple trigger to start the "Welcome to the DeVille District" mission
    /// when the player enters a trigger volume in the world.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Mission_DeVille_Trigger : MonoBehaviour
    {
        [Tooltip("Reference to the DeVille mission script to start.")]
        public Mission_DeVille_WelcomeToDistrict mission;

        [Tooltip("If true, this trigger can only fire once.")]
        public bool oneShot = true;

        private bool hasTriggered = false;

        private void Reset()
        {
            // Ensure collider is set as trigger by default
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (oneShot && hasTriggered)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (mission == null)
            {
                Debug.LogWarning("Mission_DeVille_Trigger has no mission assigned.");
                return;
            }

            mission.StartMission();
            hasTriggered = true;
        }
    }
}
