﻿// Dxflib
// Vector.cs
// 
// ============================================================
// 
// Created: 2018-08-11
// Last Updated: 2018-09-01-6:23 PM
// By: Adam Renaud
// 
// ============================================================

using System;
using System.ComponentModel;
using Dxflib.Geometry;

namespace Dxflib.LinAlg
{
    /// <inheritdoc />
    /// <summary>
    ///     The Vector Class: Defines vector mathematics and functionality
    ///     for the dxf library
    /// </summary>
    public class Vector : GeoBase
    {
        private Vertex _vertex0;
        private Vertex _vertex1;

        /// <summary>
        ///     Default Constructor
        /// </summary>
        public Vector()
        {
            _vertex0 = new Vertex(0, 0);
            _vertex1 = new Vertex(1, 1, 1);
            UpdateGeometry();
            SubscribeToEvents();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor from another vector
        /// </summary>
        /// <param name="other"></param>
        public Vector(Vector other) : this(other.TailVertex, other.HeadVertex) { }

        /// <summary>
        ///     PositionVector Constructor: This will create a vector with its tail
        ///     at the origin and it's head at where the components are on the cartesian 3D space.
        /// </summary>
        /// <param name="startingVertex">The Starting Vertex for the position vector</param>
        /// <param name="endingVertex">The Ending Vertex for the position vector</param>
        public Vector(Vertex startingVertex, Vertex endingVertex)
        {
            _vertex0 = startingVertex;
            _vertex1 = endingVertex;
            UpdateGeometry();
            SubscribeToEvents();
        }

        /// <summary>
        ///     UnitVector Constructor
        /// </summary>
        /// <param name="xComponent">The x component of a unit vector</param>
        /// <param name="yComponent">The y component of a unit vector</param>
        /// <param name="zComponent">The z component of a unit vector</param>
        public Vector(double xComponent, double yComponent, double zComponent = 0)
        {
            _vertex0 = new Vertex(0, 0);
            _vertex1 = new Vertex(xComponent, yComponent, zComponent);
            UpdateGeometry();
            SubscribeToEvents();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Constructor from a line
        /// </summary>
        /// <param name="line"></param>
        public Vector(GeoLine line) : this(line.Vertex0, line.Vertex1) { }

        /// <summary>
        ///     The vector's x component
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        ///     The vector's y component
        /// </summary>
        public double Y { get; private set; }

        /// <summary>
        ///     The vector's z component
        /// </summary>
        public double Z { get; private set; }

        /// <summary>
        ///     The vector's length or magnitude
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        ///     The Head of the vector or the ending vector (Point of Application)
        /// </summary>
        public Vertex HeadVertex
        {
            get => _vertex1;
            set
            {
                _vertex1 = value;
                UpdateGeometry();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     The Tail of the vector: where the vector originates
        /// </summary>
        public Vertex TailVertex
        {
            get => _vertex0;
            set
            {
                _vertex0 = value;
                UpdateGeometry();
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     The 2D Slope of the vector (dY/dX)
        /// </summary>
        public double Slope => Y / X;

        /// <summary>
        ///     Translate a vertex to a new point by its tail
        /// </summary>
        /// <param name="newTail">The new tail vertex</param>
        public void Translate(Vertex newTail)
        {
            _vertex0 = newTail;
            _vertex1 = new Vertex(
                _vertex1.X + newTail.X,
                _vertex1.Y + newTail.Y,
                _vertex1.Z + newTail.Z);
            UpdateGeometry();
        }

        /// <summary>
        /// </summary>
        /// <param name="scaler"></param>
        public void Scale(double scaler)
        {
            HeadVertex = new Vertex(
                X * scaler + TailVertex.X,
                Y * scaler + TailVertex.Y,
                Z * scaler + TailVertex.Z);
        }

        /// <summary>
        ///     Rotate the vector about is tail
        /// </summary>
        /// <param name="angle">Rotation angle in radians</param>
        public void Rotate(double angle)
        {
            Transform(new Vector(Math.Cos(angle), Math.Sin(angle)),
                new Vector(-Math.Sin(angle), Math.Cos(angle)), new Vector(0, 0));
        }

        /// <summary>
        ///     Transform method transforms this vector using a new set of basis vectors
        /// </summary>
        /// <param name="newXBasis">The new basis for the x component</param>
        /// <param name="newYBasis">The new basis for the y component</param>
        /// <param name="newZBasis">The new basis for the z component</param>
        public void Transform(Vector newXBasis, Vector newYBasis, Vector newZBasis)
        {
            // Perform the transformation
            var tempX = newXBasis.X * X + newYBasis.X * Y + newZBasis.X * Z;
            var tempY = newXBasis.Y * X + newYBasis.Y * Y + newZBasis.Y * Z;
            var tempZ = newXBasis.Z * X + newYBasis.Z * Y + newZBasis.Z * Z;
            X = tempX;
            Y = tempY;
            Z = tempZ;

            // Reset the head position
            _vertex1 = new Vertex(_vertex0.X + X, _vertex0.Y + Y, _vertex0.Z + Z);

            // recalculate the length
            Length = GeoMath.Distance(_vertex0, _vertex1);
        }

        /// <summary>
        ///     Update the Geometry of the Vector
        /// </summary>
        /// <param name="command"></param>
        protected sealed override void UpdateGeometry(string command = "")
        {
            X = _vertex1.X - _vertex0.X;
            Y = _vertex1.Y - _vertex0.Y;
            Z = _vertex1.Z - _vertex0.Z;
            Length = GeoMath.Distance(_vertex0, _vertex1);
        }

        /// <summary>
        ///     Subscribing to vertex events
        /// </summary>
        private void SubscribeToEvents()
        {
            _vertex0.PropertyChanged += VertexOnPropertyChanged;
            _vertex1.PropertyChanged += VertexOnPropertyChanged;
        }

        private void VertexOnPropertyChanged(object sender, PropertyChangedEventArgs e) { UpdateGeometry(); }

        /// <summary>
        ///     Creates a new vector that is the unit vector of this vector
        /// </summary>
        /// <returns>A new Vector object</returns>
        public Vector ToUnitVector()
        {
            return new Vector(X / Length,
                Y / Length,
                Z / Length);
        }

        /// <summary>
        ///     The Mathematical Dot Product of two vectors
        /// </summary>
        /// <param name="other">The other vector in the dot product equation</param>
        /// <returns>A double value that is the dot product of other and this vector</returns>
        public double DotProduct(Vector other) { return other.X * X + other.Y * Y + other.Z * Z; }

        /// <summary>
        ///     The Angle between two vectors
        /// </summary>
        /// <param name="vec0">Vector 1</param>
        /// <param name="vec1">Vector 2</param>
        /// <returns>The angle between two vectors in radians</returns>
        public static double AngleBetweenVectors(Vector vec0, Vector vec1)
        {
            if ( Math.Abs(vec0.Length) < GeoMath.Tolerance || Math.Abs(vec1.Length) < GeoMath.Tolerance )
                throw new DivideByZeroException("Cannot Divide by zero (Angle Between Vectors)");
            return Math.Acos(vec0.DotProduct(vec1) / ( vec0.Length * vec1.Length ));
        }

        /// <summary>
        ///     The Cross product of this vector and the other vector (ThisVector x OtherVector)
        /// </summary>
        /// <param name="other">The other vector that is in the cross function</param>
        /// <returns>A new vector that is the resultant of the cross product</returns>
        public Vector CrossProduct(Vector other)
        {
            return new Vector(
                Y * other.Z - Z * other.Y,
                -( X * other.Z - other.X * Z ),
                X * other.Y - other.X * Y);
        }

        /// <summary>
        ///     The Addition overload operator for the vector class
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector operator +(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);
        }

        /// <summary>
        ///     The overloaded operator "-"
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns>A new vector that is the resultant of the subtraction</returns>
        public static Vector operator -(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z);
        }

        /// <summary>
        ///     The overloaded operator *
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="scaler"></param>
        /// <returns>A new vector that is the resultant of the scaler</returns>
        public static Vector operator *(Vector vec, double scaler)
        {
            return new Vector(vec.X * scaler, vec.Y * scaler, vec.Z * scaler);
        }

        /// <summary>
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="scaler"></param>
        /// <returns></returns>
        public static Vector operator *(double scaler, Vector vec) { return vec * scaler; }

        /// <summary>
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="scaler"></param>
        /// <returns></returns>
        public static Vector operator /(Vector vec, double scaler)
        {
            if ( Math.Abs(scaler) < GeoMath.Tolerance )
                throw new DivideByZeroException();

            return new Vector(vec.X / scaler, vec.Y / scaler, vec.Z / scaler);
        }
    }
}