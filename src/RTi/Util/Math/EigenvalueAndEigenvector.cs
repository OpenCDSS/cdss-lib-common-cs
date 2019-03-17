using System;

// EigenvalueAndEigenvector - compute eigenvalues and eigenvectors by the Jacobi method

/* NoticeStart

CDSS Common Java Library
CDSS Common Java Library is a part of Colorado's Decision Support Systems (CDSS)
Copyright (C) 1994-2019 Colorado Department of Natural Resources

CDSS Common Java Library is free software:  you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CDSS Common Java Library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CDSS Common Java Library.  If not, see <https://www.gnu.org/licenses/>.

NoticeEnd */

/*
 * Compute eigenvalues and eigenvectors by the Jacobi method for symmetric
 *    matrices.  Currently, only Jacobi's Method is available.
 */

namespace RTi.Util.Math
{

	/// <summary>
	///  Currently, there are 2 ways to call the jacobi method.  One is static and
	///      the output matrix (eigenvectors) and vector (eigenvalues) must be created
	///      and passed in as parameters as well as the size of the input matrix.  The
	///      other is non-static.  An example of how to call that method follows:
	/// 
	///      int n=4;
	///      double[][] a = new double[n][n];
	///      a[0][0] = 4; a[0][1] = -30; a[0][2] = 60; a[0][3] = -35;
	///      a[1][0] = -30; a[1][1] = 300; a[1][2] = -675; a[1][3] = 420;
	///      a[2][0] = 60; a[2][1] = -675; a[2][2] = 1620; a[2][3] = -1050;
	///      a[3][0] = -35; a[3][1] = 420; a[3][2] = -1050; a[3][3] = 700;
	/// 
	///      EigenvalueAndEigenvector ee = new EigenvalueAndEigenvector( a );
	///      ee.jacobi();
	///      if ( ee.getStatus() != EigenvalueAndEigenvector.Status.CONVERGED )
	///          fail();  // or return or whatever
	///      double eigenvalues[] = ee.getEigenvalues();
	///      double eigenvectors[][] = ee.getEigenvectors();
	/// 
	/// 
	/// @author cen
	/// </summary>
	public class EigenvalueAndEigenvector
	{

	private double[][] _inputMatrix;
	private double[] _eigenvalues;
	private double[][] _eigenvectors;
	private Status _status;
	private int _size;

	public enum Status
	{
		CONVERGED,
		FAIL,
		NOT_CALCULATED
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public EigenvalueAndEigenvector(double[][] input) throws java.security.InvalidParameterException
	public EigenvalueAndEigenvector(double[][] input)
	{
		_inputMatrix = input;

		_size = _inputMatrix.Length;
		if (_size == 0)
		{
			throw new InvalidParameterException("Can't perform Eigenvalue/Eigenvector calculations on empty matrix.");
		}
		if (_inputMatrix[0].Length != _size)
		{
			throw new InvalidParameterException("Can't perform Eigenvalue/Eigenvector calculations on non n by n matrices.");
		}

		_eigenvalues = new double[_size];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: _eigenvectors = new double[_size][_size];
		_eigenvectors = RectangularArrays.RectangularDoubleArray(_size, _size);
		_status = Status.NOT_CALCULATED;

	}

	public virtual double[] getEigenvalues()
	{
		return _eigenvalues;
	}

	public virtual double[][] getEigenvectors()
	{
		return _eigenvectors;
	}

	public virtual int getSize()
	{
		return _size;
	}

	public virtual Status getStatus()
	{
		return _status;
	}

	/// <summary>
	///    David Garen  6/89
	/// 
	///    Eigenvalues and eigenvectors by the Jacobi method for symmetric
	///    matrices.
	/// 
	///    Computes all eigenvalues and eigenvectors of a real symmetric matrix
	///    a[0..n-1][0..n-1].  On output, elements of a above the diagonal are
	///    destroyed.  d[0..n-1] returns the eigenvalues of a.  v[0..n-1][0..n-1]
	///    is a matrix whose columns contain, on output, the normalized eigenvectors
	///    of a.
	/// 
	///    This program is slightly modified from the program in:
	///    Press, William H., Brian P. Flannery, Saul A. Teukolsky, and William T.
	///    Vetterling, Numerical Recipes in C:  The Art of Scientific Computing,
	///    Cambridge University Press, 1988, pp. 364-366.
	/// </summary>
	/// <param name="a"> input symmetric matrix </param>
	/// <param name="n"> number of variables </param>
	/// <param name="eigenvalues"> vector of eigenvalues </param>
	/// <param name="eigenvectors"> matrix of eigenvectors </param>
	/// <returns> status: 0 for normal completion, 1 for no convergence.
	/// @todo this really needs to change so that only a is input!!! </returns>
	public virtual void jacobi()
	{
		jacobi(_size);
	}

	public virtual void jacobi(int size)
	{
		// specifying size is provided in case the entire matrix should not be processed
		_size = Math.Min(size, _size);
		int status = jacobi(_inputMatrix, _size, _eigenvalues, _eigenvectors);

		_status = status == 0? Status.CONVERGED : Status.FAIL;
	}
	public static int jacobi(double[][] a, int n, double[] eigenvalues, double[][] eigenvectors)
	{
	   double c; // cosine of rotation angle
	   double g;
	   double h;
	   int imax = 50; // maximum number of iterations
	   double s; // sine of rotation angle
	   double sm; // sum of off-diagonal elements
	   double t;
	   double tau;
	   double theta;
	   double tresh;

	   // Allocate space for b and z vectors
	   double[] b = new double[n]; // work vector
	   double[] z = new double[n]; // work vector

	   // Initialize v to the identity matrix

	   for (int ip = 0; ip < n; ip++)
	   {
		  for (int iq = 0; iq < n; iq++)
		  {
			 eigenvectors[ip][iq] = 0.0;
		  }
		  eigenvectors[ip][ip] = 1.0;
	   }

	   // Initialize b and d to the diagonal of a and z to zero

	   for (int ip = 0; ip < n; ip++)
	   {
		  b[ip] = eigenvalues[ip] = a[ip][ip];
		  z[ip] = 0.0;
	   }

	   // Begin Jacobi iterations

	   for (int i = 1; i <= imax; i++)
	   {

		  // Sum off-diagonal elements

		  sm = 0.0;
		  for (int ip = 0; ip < n - 1; ip++)
		  {
			 for (int iq = ip + 1; iq < n; iq++)
			 {
				sm += Math.Abs(a[ip][iq]);
			 }
		  }

		  /* Algorithm completed if sm is zero.  Sort eigenvalues into
		     descending order, rearrange eigenvectors correspondingly,
		     and normal return. 
		   */

		  if (sm == 0.0)
		  {
			 sortEigenvaluesAndEigenvectors(eigenvalues, eigenvectors, n);
			 return (0);
		  }

		  if (i < 4)
		  {
			 tresh = 0.2 * sm / (n * n); // ... on the first three sweeps
		  }
		  else
		  {
			 tresh = 0.0; // ... thereafter
		  }

		  for (int ip = 0; ip < n - 1; ip++)
		  {
			 for (int iq = ip + 1; iq < n; iq++)
			 {
				g = 100.0 * Math.Abs(a[ip][iq]);

				/* After four sweeps, skip the rotation if the off-diagonal
				   element is small. */

				if (i > 4 && (Math.Abs(eigenvalues[ip]) + g) == Math.Abs(eigenvalues[ip]) && (Math.Abs(eigenvalues[iq]) + g) == Math.Abs(eigenvalues[iq]))
				{
				   a[ip][iq] = 0.0;
				}
				else if (Math.Abs(a[ip][iq]) > tresh)
				{
				   h = eigenvalues[iq] - eigenvalues[ip];
				   if ((Math.Abs(h) + g) == Math.Abs(h))
				   {
					  t = a[ip][iq] / h;
				   }
				   else
				   {
					  theta = 0.5 * h / a[ip][iq];
					  t = 1.0 / (Math.Abs(theta) + Math.Sqrt(1.0 + theta * theta));
					  if (theta < 0.0)
					  {
						 t = -t;
					  }
				   }
				   c = 1.0 / Math.Sqrt(1.0 + t * t);
				   s = t * c;
				   tau = s / (1.0 + c);
				   h = t * a[ip][iq];
				   z[ip] -= h;
				   z[iq] += h;
				   eigenvalues[ip] -= h;
				   eigenvalues[iq] += h;
				   a[ip][iq] = 0.0;

				   // Rotations for 0 <= j < ip

				   for (int j = 0; j < ip; j++)
				   {
					  rotate(a, j, ip, j, iq, s, tau);
				   }

				   // Rotations for ip < j < iq

				   for (int j = ip + 1; j < iq; j++)
				   {
					  rotate(a, ip, j, j, iq, s, tau);
				   }

				   // Rotations for q < j < n

				   for (int j = iq + 1; j < n; j++)
				   {
					  rotate(a, ip, j, iq, j, s, tau);
				   }

				   // Rotate eigenvectors

				   for (int j = 0; j < n; j++)
				   {
					  rotate(eigenvectors, j, ip, j, iq, s, tau);
				   }
				}
			 }
		  }

		  // Update d and reinintialze z.

		  for (int ip = 0; ip < n; ip++)
		  {
			 b[ip] += z[ip];
			 eigenvalues[ip] = b[ip];
			 z[ip] = 0.0;
		  }
	   }

	   return (1);
	}

	/*
	 *    sortEigenvaluesAndEigenvectors
	 *
	 *    David Garen  6/89
	 *
	 *    Sort eigenvalues into descending order and rearrange eigenvectors
	 *    correspondingly.  Straight insertion method used.
	 *
	 *    This program is from:
	 *    Press, William H., Brian P. Flannery, Saul A. Teukolsky, and William T.
	 *    Vetterling, Numerical Recipes in C:  The Art of Scientific Computing,
	 *    Cambridge University Press, 1988, p. 366.
	 *
	 * @param eigenvalues vector of eigenvalues to sort
	 * @param eigenvectors matrix of corresponding eigenvectors
	 * @param n number of variables
	 */
	private static void sortEigenvaluesAndEigenvectors(double[] d, double[][] v, int n)
	{
	   double p;
	   int k;

	   for (int i = 0; i < n - 1; i++)
	   {
		  p = d[k = i];
		  for (int j = i + 1; j < n; j++)
		  {
			 if (d[j] >= p)
			 {
				p = d[k = j];
			 }
		  }
		  if (k != i)
		  {
			 d[k] = d[i];
			 d[i] = p;
			 for (int j = 0; j < n; j++)
			 {
				p = v[j][i];
				v[j][i] = v[j][k];
				v[j][k] = p;
			 }
		  }
	   }
	}

	private static void rotate(double[][] a, int i, int j, int k, int l, double s, double tau)
	{
			double g = a[i][j];
			double h = a[k][l];
			a[i][j] = g - s * (h + g * tau);
			a[k][l] = h + s * (g - h * tau);
	}

	}

}