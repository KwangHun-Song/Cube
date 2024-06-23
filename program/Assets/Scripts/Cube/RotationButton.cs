using Pages;
using UnityEngine;
using UnityEngine.UI;

namespace Cube {
    [RequireComponent(typeof(Button))]
    public class RotationButton : MonoBehaviour {
        [SerializeField] private PlayPage playPage;
        [SerializeField] private Axis axis;
        [SerializeField] private int axisIndex;
        [SerializeField] private RotateDirection direction;

        private Button button;
        public Button Button => button ??= GetComponent<Button>();

        private void Awake() => Button.onClick.AddListener(OnClick);
        public void OnClick() => playPage.InputRotation(new Input(axis, axisIndex, direction));
    }
}