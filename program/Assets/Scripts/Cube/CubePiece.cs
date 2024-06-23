using System;
using UnityEngine;

namespace Cube {
    
    public class CubePiece : MonoBehaviour, IComparable<CubePiece> {
        [SerializeField] private Vector3Int point;
        [SerializeField] private MeshRenderer[] meshOfPlanes;
        

        public Vector3 originLocalPosition;
        public CubePieceColor this[CubePieceDirection direction] {
            get {
                var mesh = meshOfPlanes[(int)direction];
                var materialName = mesh.material.name.Replace(" (Instance)", "");
                if (Enum.TryParse(materialName, out CubePieceColor color)) {
                    return color;
                }

                return CubePieceColor.None;
            }
        }

        private void Awake() {
            originLocalPosition = transform.localPosition;
        }

        public Vector3Int Point {
            get => point;
            set {
                point = value;
                gameObject.name = $"Cube ({point.x}, {point.y}, {point.z})";
            }
        }

        public int CompareTo(CubePiece other) {
            if (other == null) return 1;
            
            var result = point.z.CompareTo(other.point.z);
            if (result != 0) return result;
            
            result = point.y.CompareTo(other.point.y);
            if (result != 0) return result;

            result = point.x.CompareTo(other.point.x);
            if (result != 0) return result;

            return result;
        }
    }

}