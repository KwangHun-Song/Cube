using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Utility;

namespace Cube {
    
    public class Cube2X2X2 : MonoBehaviour {
        // 각 큐브의 피스. X, Y, Z 순으로 소팅해서 지정해두어야 한다.
        [SerializeField] private CubePiece[] cubePieces;
        // RT는 RotationRoot의 줄임말. 이 축을 기준으로 회전한다.
        [SerializeField] private Transform verticalRT;
        [SerializeField] private Transform horizontalRT;
        [SerializeField] private Transform defaultRTransform;
        [SerializeField] private Transform x0RT;
        [SerializeField] private Transform x1RT;
        [SerializeField] private Transform y0RT;
        [SerializeField] private Transform y1RT;
        [SerializeField] private Transform z0RT;
        [SerializeField] private Transform z1RT;

        private static readonly Vector3Int OriginVerticalRT = new Vector3Int(150, 0, 0);
        private static readonly Vector3Int OriginHorizontalRT = new Vector3Int(0, 45, 0);

        public event Action OnClear;

        private readonly Stack<Input> inputStack = new Stack<Input>();

        public void Initialize(Action onClear) {
            OnClear = onClear;
            foreach (var cubePiece in cubePieces) {
                cubePiece.transform.SetParent(defaultRTransform);
                cubePiece.transform.localPosition = cubePiece.originLocalPosition;
                cubePiece.transform.localRotation = Quaternion.identity;
            }
            x0RT.localRotation = Quaternion.identity;
            x1RT.localRotation = Quaternion.identity;
            y0RT.localRotation = Quaternion.identity;
            y1RT.localRotation = Quaternion.identity;
            z0RT.localRotation = Quaternion.identity;
            z1RT.localRotation = Quaternion.identity;
            verticalRT.localRotation = Quaternion.Euler(OriginVerticalRT);
            horizontalRT.localRotation = Quaternion.Euler(OriginHorizontalRT);
            inputStack.Clear();
            
            UpdateCubePiecePoints();
        }

        public async UniTask RotateAsync(Input input, bool ignoreInputStack = false, bool immediately = false) {
            // 언두나 리셋을 위해서 인풋을 스택에 추가해둔다.
            if (ignoreInputStack == false) inputStack.Push(input);
            
            // 축 하나를 회전시켰는지, 전체를 회전시켰는지 구분해서 회전을 진행시킨다.
            if (Axis.SingleAxis.HasFlag(input.axis)) {
                await RotateSingleAxisAsync(input.axis, input.axisIndex, input.direction, immediately);
            } else if (Axis.WholeAxis.HasFlag(input.axis)) {
                await RotateWholeAsync(input.axis, input.direction, immediately);
            }

            if (IsClear()) {
                OnClear?.Invoke();
            }
        }

        public bool CanUndo(bool includeRandomShuffle = false) {
            if (includeRandomShuffle == false) {
                return inputStack.Any(input => input.inputType == InputType.User);
            }

            return inputStack.Any();
        }

        public async UniTask UndoLastInputAsync(bool includeRandomShuffle = false, bool immediately = false) {
            if (CanUndo(includeRandomShuffle) == false) return;
            var lastInput = inputStack.Pop();

            // 인풋의 방향만 반대로 해서 돌려준다.
            lastInput.direction = lastInput.direction == RotateDirection.Negative
                ? RotateDirection.Positive
                : RotateDirection.Negative;
            
            await RotateAsync(lastInput, true, immediately); // 인풋 스택에는 추가하지 않는다.
        }

        private bool IsClear() {
            return cubePieces
                .Select(piece => {
                    var eulerAngles = piece.transform.localRotation.eulerAngles;
                    return new Vector3Int(
                        Mathf.CeilToInt(eulerAngles.x),
                        Mathf.CeilToInt(eulerAngles.y),
                        Mathf.CeilToInt(eulerAngles.z)
                    );
                })
                .ItemsAreSame();
        }

        private void UpdateCubePiecePoints() {
            foreach (var cubePiece in cubePieces) {
                var pos = cubePiece.transform.localPosition;
                cubePiece.Point = new Vector3Int(
                    x: pos.z < 0 ? 0 : 1,
                    y: pos.x < 0 ? 0 : 1,
                    z: pos.y < 0 ? 1 : 0
                );
            }
            
            // 하이에라키상에 포인트대로 소팅되어서 보이도록 한다.
            Array.Sort(cubePieces);
            for (int i = 0; i < cubePieces.Length; i++) {
                cubePieces[i].transform.SetSiblingIndex(i);
            }
        }

        private async UniTask RotateSingleAxisAsync(Axis axis, int axisIndex, RotateDirection direction, bool immediately = false) {
            Vector3Int amount = default;
            Transform axisTfm = transform;
            Func<CubePiece, bool> GetPieceFunc = _ => false;
            
            switch (axis) {
                case Axis.X:
                    amount = Vector3Int.forward * (direction == RotateDirection.Negative ? -90 : 90);
                    axisTfm = axisIndex == 0 ? x0RT : x1RT;
                    GetPieceFunc = cubePiece => cubePiece.Point.x == axisIndex;
                    break;
                case Axis.Y:
                    amount = Vector3Int.right * (direction == RotateDirection.Negative ? -90 : 90);
                    axisTfm = axisIndex == 0 ? y0RT : y1RT;
                    GetPieceFunc = cubePiece => cubePiece.Point.y == axisIndex;
                    break;
                case Axis.Z:
                    amount = Vector3Int.up * (direction == RotateDirection.Negative ? -90 : 90);
                    axisTfm = axisIndex == 0 ? z0RT : z1RT;
                    GetPieceFunc = cubePiece => cubePiece.Point.z == axisIndex;
                    break;
            }
            
            var targetPieces = cubePieces.Where(GetPieceFunc).ToArray();
                    
            // 큐브 조각들을 회전 부모 트랜스폼으로 옮긴다.
            foreach (var targetPiece in targetPieces) 
                targetPiece.transform.SetParent(axisTfm, worldPositionStays:true);
            
            // 부모를 회전시킨다.
            if (immediately) {
                axisTfm.localRotation = Quaternion.Euler(axisTfm.localRotation.eulerAngles + amount);
            } else {
                await axisTfm.DOLocalRotate(amount, 0.3F, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack);
            }
            
            // 큐브 조각들을 원위치한다. 회전한 위치는 유지
            foreach (var targetPiece in targetPieces) 
                targetPiece.transform.SetParent(defaultRTransform, worldPositionStays:true);
            
            axisTfm.localRotation = Quaternion.Euler(NormalizeEulerAngles(axisTfm.localRotation.eulerAngles + amount));

            UpdateCubePiecePoints();
        }

        private async UniTask RotateWholeAsync(Axis axis, RotateDirection direction, bool immediately = false) {
            // 회전축이되는 부모 트랜스폼을 잡아서 회전시켜준다.
            Vector3Int rotationAmount = default;
            Transform rotationTransform = transform;

            switch (axis) {
                case Axis.Horizontal:
                    rotationAmount = Vector3Int.up * (direction == RotateDirection.Negative ? -90 : 90);
                    rotationTransform = horizontalRT;
                    break;
                case Axis.Vertical:
                    rotationAmount = Vector3Int.left * (direction == RotateDirection.Negative ? -180 : 180);
                    rotationTransform = verticalRT;
                    break;
            }

            if (immediately) {
                rotationTransform.localRotation = Quaternion.Euler(rotationTransform.localRotation.eulerAngles + rotationAmount);
            } else {
                await rotationTransform.DOLocalRotate(rotationAmount, 0.3F, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack);
            }
        }
        
        private Vector3 NormalizeEulerAngles(Vector3 eulerAngles) {
            eulerAngles.x = NormalizeAngle(eulerAngles.x);
            eulerAngles.y = NormalizeAngle(eulerAngles.y);
            eulerAngles.z = NormalizeAngle(eulerAngles.z);
            return eulerAngles;
        }

        private float NormalizeAngle(float angle) {
            while (angle > 360) angle -= 360;
            while (angle < -360) angle += 360;
            return angle;
        }
    }
}