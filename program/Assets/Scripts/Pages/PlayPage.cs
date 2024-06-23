using System;
using Cube;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Utility;
using Input = Cube.Input;

namespace Pages {
    public class PlayPage : MonoBehaviour {
        [SerializeField] private GameView gameView;
        [SerializeField] private GameObject curtain;
        [SerializeField] private TMP_Text systemMessage;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text moveCount;
        
        private DateTime? StartTime { get; set; }
        private int UsedMoveCount { get; set; } 

        private void Start() {
            InitUi();
            InvokeRepeating(nameof(RefreshTimer), 0, 1);
        }

        private void InitUi() {
            curtain.SetActive(true);
            systemMessage.text = "클릭해서\n새 게임 시작";
            StartTime = null;
            moveCount.text = "무브 : 0";
        }

        private void OnClear() {
            ClearAsync().Forget();

            async UniTask ClearAsync() {
                if (StartTime == null) return; // UI버튼으로 클리어를 트리거한 경우
                systemMessage.text = "Clear!";
                using (new ScreenLock()) 
                    await UniTask.Delay(TimeSpan.FromSeconds(2));
                
                InitUi();
            }
        }

        private void RefreshTimer() {
            if (StartTime == null) {
                timerText.text = "00:00";
                return;
            }
            
            var timeSpan = DateTime.Now - StartTime.Value;
            if (timeSpan.TotalMinutes < 60) {
                timerText.text =  timeSpan.ToString(@"mm\:ss");
            } else {
                timerText.text =  timeSpan.ToString(@"hh\:mm\:ss");
            }
        }

        private void RefreshMoveCount() {
            moveCount.text = $"무브 : {UsedMoveCount}";
        }

        #region EVENT

        
        public void OnClickCoverButton() {
            curtain.SetActive(false);
            systemMessage.text = "";
            OnClickNewGame();
        }

        public void OnClickNewGame() {
            gameView.StartNewGame(OnClear);
            StartTime = DateTime.Now;
            RefreshTimer();
            UsedMoveCount = 0;
            RefreshMoveCount();
        }

        public void OnClickRestart() {
            gameView.RestartGame();
            UsedMoveCount = 0;
            RefreshMoveCount();
        }

        public void OnClickUndo() {
            gameView.Undo();
            UsedMoveCount--;
            RefreshMoveCount();
        }

        public void OnClickAnswer() {
            ClickAnswerInternalAsync().Forget();
            
            async UniTask ClickAnswerInternalAsync() {
                StartTime = default;
                await gameView.ShowAnswerAsync();
                using (new ScreenLock()) 
                    await UniTask.Delay(TimeSpan.FromSeconds(2));
                
                InitUi();
            }
        }

        public void InputRotation(Input input) {
            gameView.InputRotation(input);
            UsedMoveCount++;
            RefreshMoveCount();
        }
        
        #endregion

    }
}
