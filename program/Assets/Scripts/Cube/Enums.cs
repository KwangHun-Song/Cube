using System;

namespace Cube {
    [Flags]
    public enum Axis {
        None = 0,
        X = 1 << 0, 
        Y = 1 << 1, 
        Z = 1 << 2,
        Horizontal = 1 << 3,
        Vertical = 1 << 4,
        
        SingleAxis = X | Y | Z,
        WholeAxis = Horizontal | Vertical
    }
    
    public enum RotateDirection { Negative, Positive }
    public enum InputType { RandomShuffle, User }
    public enum CubePieceDirection { Up, Down, East, West, South, North }
    public enum CubePieceColor { None, Red, Yellow, Green, Blue, Purple, White }
}