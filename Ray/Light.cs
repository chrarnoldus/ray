namespace Ray
{
    sealed class Light
    {
        public Light(Vector position, Vector color)
        {
            Position = position;
            Color = color;
        }

        public Vector Position { get; }

        public Vector Color { get; }
    }
}
