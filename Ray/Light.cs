using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ray
{
    sealed class Light
    {
        readonly Vector position, color;

        public Light(Vector position, Vector color)
        {
            this.position = position;
            this.color = color;
        }

        public Vector Position
        {
            get { return position; }
        }

        public Vector Color
        {
            get { return color; }
        }
    }
}
