namespace Cube {
    public struct Input {
        public Axis axis;
        public int axisIndex;
        public RotateDirection direction;
        public InputType inputType;
        public Input(Axis axis, int axisIndex, RotateDirection direction, InputType inputType = InputType.User) {
            this.axis = axis;
            this.axisIndex = axisIndex;
            this.direction = direction;
            this.inputType = inputType;
        }
    }
}