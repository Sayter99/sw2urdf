﻿// <copyright file="CholeskyTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Factorization
{
    using System;
    using System.Numerics;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Generic.Factorization;
    using NUnit.Framework;

    /// <summary>
    /// Cholesky factorization tests for a dense matrix.
    /// </summary>
    public class CholeskyTests
    {
        /// <summary>
        /// Can factorize identity matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanFactorizeIdentity([Values(1, 10, 100)] int order)
        {
            var matrixI = DenseMatrix.Identity(order);
            var factorC = matrixI.Cholesky().Factor;

            Assert.AreEqual(matrixI.RowCount, factorC.RowCount);
            Assert.AreEqual(matrixI.ColumnCount, factorC.ColumnCount);

            for (var i = 0; i < factorC.RowCount; i++)
            {
                for (var j = 0; j < factorC.ColumnCount; j++)
                {
                    Assert.AreEqual(i == j ? Complex.One : Complex.Zero, factorC[i, j]);
                }
            }
        }

        /// <summary>
        /// Cholesky factorization fails with diagonal a non-positive definite matrix.
        /// </summary>
        [Test]
        public void CholeskyFailsWithDiagonalNonPositiveDefiniteMatrix()
        {
            var matrixI = DenseMatrix.Identity(8);
            matrixI[3, 3] = -4.0;
            Assert.Throws<ArgumentException>(() => matrixI.Cholesky());
        }

        /// <summary>
        /// Cholesky factorization fails with a non-square matrix.
        /// </summary>
        [Test]
        public void CholeskyFailsWithNonSquareMatrix()
        {
            var matrix = new DenseMatrix(3, 2);
            Assert.Throws<ArgumentException>(() => matrix.Cholesky());
        }

        /// <summary>
        /// Identity determinant is one.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void IdentityDeterminantIsOne([Values(1, 10, 100)] int order)
        {
            var matrixI = DenseMatrix.Identity(order);
            var factorC = matrixI.Cholesky();
            Assert.AreEqual(Complex.One, factorC.Determinant);
            Assert.AreEqual(Complex.Zero, factorC.DeterminantLn);
        }

        /// <summary>
        /// Can factorize a random square matrix.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanFactorizeRandomMatrix([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var matrixX = MatrixLoader.GenerateRandomPositiveDefiniteHermitianDenseMatrix(order);
            var chol = matrixX.Cholesky();
            var factorC = chol.Factor;

            // Make sure the Cholesky factor has the right dimensions.
            Assert.AreEqual(order, factorC.RowCount);
            Assert.AreEqual(order, factorC.ColumnCount);

            // Make sure the Cholesky factor is lower triangular.
            for (var i = 0; i < factorC.RowCount; i++)
            {
                for (var j = i + 1; j < factorC.ColumnCount; j++)
                {
                    Assert.AreEqual(Complex.Zero, factorC[i, j]);
                }
            }

            // Make sure the cholesky factor times it's transpose is the original matrix.
            var matrixXfromC = factorC * factorC.ConjugateTranspose();
            for (var i = 0; i < matrixXfromC.RowCount; i++)
            {
                for (var j = 0; j < matrixXfromC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixX[i, j], matrixXfromC[i, j], 8);
                }
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random vector (Ax=b).
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanSolveForRandomVector([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianDenseMatrix(order);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = MatrixLoader.GenerateRandomDenseVector(order);
            var x = chol.Solve(matrixB);

            Assert.AreEqual(matrixB.Count, x.Count);

            var matrixBReconstruct = matrixA * x;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                AssertHelpers.AlmostEqual(matrixB[i], matrixBReconstruct[i], 8);
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix (AX=B).
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="col">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomMatrix([Values(1, 2, 5, 10, 50, 100)] int row, [Values(1, 4, 8, 3, 10, 100)] int col)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianDenseMatrix(row);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(row, col);
            var matrixX = chol.Solve(matrixB);

            Assert.AreEqual(matrixB.RowCount, matrixX.RowCount);
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixB[i, j], matrixBReconstruct[i, j], 8);
                }
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }
        }

        /// <summary>
        /// Can solve for a random vector into a result vector.
        /// </summary>
        /// <param name="order">Matrix order.</param>
        [Test]
        public void CanSolveForRandomVectorWhenResultVectorGiven([Values(1, 2, 5, 10, 50, 100)] int order)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianDenseMatrix(order);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = MatrixLoader.GenerateRandomDenseVector(order);
            var matrixBCopy = matrixB.Clone();
            var x = new DenseVector(order);
            chol.Solve(matrixB, x);

            Assert.AreEqual(matrixB.Count, x.Count);

            var matrixBReconstruct = matrixA * x;

            // Check the reconstruction.
            for (var i = 0; i < order; i++)
            {
                AssertHelpers.AlmostEqual(matrixB[i], matrixBReconstruct[i], 8);
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }

            // Make sure b didn't change.
            for (var i = 0; i < order; i++)
            {
                Assert.AreEqual(matrixBCopy[i], matrixB[i]);
            }
        }

        /// <summary>
        /// Can solve a system of linear equations for a random matrix (AX=B) into a result matrix.
        /// </summary>
        /// <param name="row">Matrix row number.</param>
        /// <param name="col">Matrix column number.</param>
        [Test, Sequential]
        public void CanSolveForRandomMatrixWhenResultMatrixGiven([Values(1, 2, 5, 10, 50, 100)] int row, [Values(1, 4, 8, 3, 10, 100)] int col)
        {
            var matrixA = MatrixLoader.GenerateRandomPositiveDefiniteHermitianDenseMatrix(row);
            var matrixACopy = matrixA.Clone();
            var chol = matrixA.Cholesky();
            var matrixB = MatrixLoader.GenerateRandomDenseMatrix(row, col);
            var matrixBCopy = matrixB.Clone();
            var matrixX = new DenseMatrix(row, col);
            chol.Solve(matrixB, matrixX);

            Assert.AreEqual(matrixB.RowCount, matrixX.RowCount);
            Assert.AreEqual(matrixB.ColumnCount, matrixX.ColumnCount);

            var matrixBReconstruct = matrixA * matrixX;

            // Check the reconstruction.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixB[i, j], matrixBReconstruct[i, j], 8);
                }
            }

            // Make sure A didn't change.
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixACopy[i, j], matrixA[i, j]);
                }
            }

            // Make sure B didn't change.
            for (var i = 0; i < matrixB.RowCount; i++)
            {
                for (var j = 0; j < matrixB.ColumnCount; j++)
                {
                    Assert.AreEqual(matrixBCopy[i, j], matrixB[i, j]);
                }
            }
        }
    }
}
