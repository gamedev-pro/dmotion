using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DMotion.ComparisonTest
{
    public class AnimatorPerformanceComparisonUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Toggle useEntityToggle;
        [SerializeField] private TMP_InputField countField;
        [SerializeField] private TMP_InputField spacingField;

        [SerializeField] private AnimatorPerformanceComparisonAuthoring comparisonTestSpawner;

        private void Awake()
        {
            startButton.onClick.AddListener(OnStartClicked);
            useEntityToggle.isOn = true;
        }

        private void OnStartClicked()
        {
            int.TryParse(countField.text, out var count);
            float.TryParse(spacingField.text, out var spacing);
            
            comparisonTestSpawner.Count = count;
            comparisonTestSpawner.Spacing = spacing;
            comparisonTestSpawner.UseEntity = useEntityToggle.isOn;
            
            comparisonTestSpawner.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}