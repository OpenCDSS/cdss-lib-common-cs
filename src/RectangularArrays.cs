//----------------------------------------------------------------------------------------
//	Copyright © 2007 - 2019 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class includes methods to convert Java rectangular arrays (jagged arrays
//	with inner arrays of the same length).
//----------------------------------------------------------------------------------------
internal static class RectangularArrays
{
    public static double[][] RectangularDoubleArray(int size1, int size2)
    {
        double[][] newArray = new double[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new double[size2];
        }

        return newArray;
    }

    public static TS[][] RectangularTSArray(int size1, int size2)
    {
        TS[][] newArray = new TS[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new TS[size2];
        }

        return newArray;
    }

    public static string[][] RectangularStringArray(int size1, int size2)
    {
        string[][] newArray = new string[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new string[size2];
        }

        return newArray;
    }

    public static float[][] RectangularFloatArray(int size1, int size2)
    {
        float[][] newArray = new float[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new float[size2];
        }

        return newArray;
    }

    public static int[][] RectangularIntArray(int size1, int size2)
    {
        int[][] newArray = new int[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new int[size2];
        }

        return newArray;
    }

    public static double[] RectangularDoubleArray(int size1)
    {
        double[] newArray = new double[size1];

        return newArray;
    }

    public static string[] RectangularStringArray(int size1)
    {
        string[] newArray = new string[size1];

        return newArray;
    }
}