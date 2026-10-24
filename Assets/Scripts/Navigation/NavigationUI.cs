using System;
using UnityEngine;
using UnityEngine.UI;

namespace ARFireNavigation.Navigation
{
    public class NavigationUI : MonoBehaviour
    {
        [Header("Header Elements")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text trackingStatusText;
        [SerializeField] private Image trackingStatusBadge;

        [Header("Navigation Card Elements")]
        [SerializeField] private Text instructionText;
        [SerializeField] private Text distanceNextText;
        [SerializeField] private Text distanceTotalText;
        [SerializeField] private Text destinationLabelText;
        [SerializeField] private Slider progressSlider;

        [Header("Interactive Buttons")]
        [SerializeField] private Button startNavButton;
        [SerializeField] private Button restartNavButton;
        [SerializeField] private Button stopNavButton;

        [Header("Modal Panels")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private Text completionTitleText;
        [SerializeField] private Text completionMessageText;
        [SerializeField] private Button completionRestartButton;

        [SerializeField] private GameObject trackingAlertPanel;
        [SerializeField] private Text trackingAlertText;

        public event Action OnStartButtonClicked;
        public event Action OnRestartButtonClicked;
        public event Action OnStopButtonClicked;

        private void Awake()
        {
            if (startNavButton != null)
                startNavButton.onClick.AddListener(() => OnStartButtonClicked?.Invoke());

            if (restartNavButton != null)
                restartNavButton.onClick.AddListener(() => OnRestartButtonClicked?.Invoke());

            if (stopNavButton != null)
                stopNavButton.onClick.AddListener(() => OnStopButtonClicked?.Invoke());

            if (completionRestartButton != null)
                completionRestartButton.onClick.AddListener(() => OnRestartButtonClicked?.Invoke());

            if (completionPanel != null)
                completionPanel.SetActive(false);

            if (trackingAlertPanel != null)
                trackingAlertPanel.SetActive(false);
        }

        public void SetTrackingStatus(string status, Color badgeColor)
        {
            if (trackingStatusText != null) trackingStatusText.text = status;
            if (trackingStatusBadge != null) trackingStatusBadge.color = badgeColor;
        }

        public void SetInstruction(string instruction)
        {
            if (instructionText != null) instructionText.text = instruction;
        }

        public void SetMetrics(float distNext, float distTotal, float progress, string destLabel)
        {
            if (distanceNextText != null) distanceNextText.text = $"Next Turn: {distNext:F1} m";
            if (distanceTotalText != null) distanceTotalText.text = $"Total to Exit: {distTotal:F1} m";
            if (progressSlider != null) progressSlider.value = Mathf.Clamp01(progress);
            if (destinationLabelText != null && !string.IsNullOrEmpty(destLabel)) destinationLabelText.text = destLabel;
        }

        public void SetButtonStates(bool startEnabled, bool restartEnabled, bool stopEnabled)
        {
            if (startNavButton != null) startNavButton.interactable = startEnabled;
            if (restartNavButton != null) restartNavButton.interactable = restartEnabled;
            if (stopNavButton != null) stopNavButton.interactable = stopEnabled;
        }

        public void ShowCompletion(bool show, string exitLabel = "FIRE EXIT B")
        {
            if (completionPanel != null)
            {
                completionPanel.SetActive(show);
                if (show)
                {
                    if (completionTitleText != null) completionTitleText.text = "EVACUATION COMPLETE";
                    if (completionMessageText != null)
                        completionMessageText.text = $"You have arrived safely at designated {exitLabel}. Please follow further emergency personnel directions.";
                }
            }
        }

        public void ShowTrackingAlert(bool show, string message = "")
        {
            if (trackingAlertPanel != null)
            {
                trackingAlertPanel.SetActive(show);
                if (show && trackingAlertText != null)
                {
                    trackingAlertText.text = string.IsNullOrEmpty(message) ?
                        "AR tracking limited. Please slowly point your device camera at textured floor and wall surfaces." : message;
                }
            }
        }
    }
}
