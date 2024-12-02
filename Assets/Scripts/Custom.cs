using UnityEngine;

public class Custom : MonoBehaviour
{
    public static float Dot(Vector2 v1, Vector2 v2)
    {
        float v = (v1.magnitude < 0.05 || v2.magnitude < 0.05) ? 0f : Vector2.Dot(v1, v2);
        return v < 0 ? 0 : v;
    }
    public static float Dot(VectorPolar v1, VectorPolar v2)
    {
        float v = (v1.Magnitude < 0.05 || v2.Magnitude < 0.05) ? 0f : Vector2.Dot(v1.Direction, v2.Direction);
        return v < 0 ? 0 : v;
    }

    public class VectorPolar
    {
        public Vector2 Direction { get; private set; }
        public float Magnitude { get; private set; }
        public VectorPolar(Vector2 direction, float magnitude)
        {
            this.Magnitude = magnitude;
            this.Direction = direction.normalized;
            //Prevents Negative Magnitude (Note, currently not used as it may distort directionVectors)
            //this.Magnitude = magnitude >= 0 ? magnitude : -magnitude;
            //this.Direction = magnitude >= 0 ? direction.normalized : -direction.normalized;
        }
        public VectorPolar(Vector2 vector)
        {
            this.Magnitude = vector.magnitude;
            this.Direction = vector.normalized;
        }

        public Vector2 Vector2()
        {
            return this.Direction * this.Magnitude;
        }
        public void SetMagnitude(float m)
        {
            Magnitude = m;
            Direction = Direction;
            //Prevents Negative Magnitude (Note, currently not used as it may distort directionVectors)
            //Magnitude = m >= 0 ? m : - m;
            //Direction = m >= 0 ? Direction : - Direction;
        }
        public override string ToString()
        {
            return $"(Dir: {this.Direction}, Magnitude: {this.Magnitude})";
        }
    }
}
