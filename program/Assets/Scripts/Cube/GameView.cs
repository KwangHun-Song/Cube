using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Cube {
    public class GameView : MonoBehaviour {
        [SerializeField] private Cube2X2X2 cube;
        
        private Axis[] AxesForShuffle { get; } = new[] { Axis.X, Axis.Y, Axis.X };
        private RotateDirection[] DirectionsForShuffle { get; } = new[] { RotateDirection.Negative, RotateDirection.Positive };

        public void StartNewGame(Action onClear) {
            ShuffleInternalAsync().Forget();

            async UniTask ShuffleInternalAsync() {
                using (new ScreenLock()) {
                    cube.Initialize(onClear);
                    var randomCount = Random.Range(2, 7);
                    var lastAxis = Axis.None;
                    for (int i = 0; i < randomCount; i++) {
                        var axis = AxesForShuffle.PickRandom();
                        
                        // 셔플시 축의 중복을 방지
                        if (axis == lastAxis) {
                            i--;
                            continue;
                        }
                        lastAxis = axis;
                        
                        var input = new Input(
                            axis,
                            Random.Range(0, 2),
                            DirectionsForShuffle.PickRandom(),
                            InputType.RandomShuffle
                        );
                        await cube.RotateAsync(input, immediately: true);
                    }
                }
            }
        }

        public void RestartGame() {
            RestartGameInternalAsync().Forget();

            async UniTask RestartGameInternalAsync() {
                using (new ScreenLock())
                    while (cube.CanUndo()) 
                        await cube.UndoLastInputAsync(immediately: true);
            }
        }

        public void Undo() {
            UndoInternalAsync().Forget();

            async UniTask UndoInternalAsync() {
                using (new ScreenLock()) 
                    await cube.UndoLastInputAsync();
            }
        }

        public async UniTask ShowAnswerAsync() {
            using (new ScreenLock()) {
                while (cube.CanUndo(includeRandomShuffle: false)) {
                    await cube.UndoLastInputAsync(immediately:true, includeRandomShuffle: false);
                }
                
                while (cube.CanUndo(includeRandomShuffle: true)) {
                    await cube.UndoLastInputAsync(includeRandomShuffle: true);
                }
            }
        }

        public void InputRotation(Input input) {
            InputRotationInternalAsync().Forget();

            async UniTask InputRotationInternalAsync() {
                using (new ScreenLock())
                    await cube.RotateAsync(input);
            }
        }
    }
}