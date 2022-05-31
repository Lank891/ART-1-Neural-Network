using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ART_1
{
    public static class ART1Network
    {
        /// <summary>
        /// Predicts in which cluster inputs are
        /// </summary>
        /// <param name="input">Testint Set</param>
        /// <param name="b">B matrix created in training</param>
        /// <returns>For each element in testing set, the list contains a cluster it was assigned to</returns>
        public static List<int> Predict(List<List<int>> input, List<List<double>> b)
        {
            List<int> predictions = new();

            foreach(var x in input)
            {
                List<double> outputs = CalculateOutputs(x, b);
                List<int> bestMatches = IndexesOfDescSortedVector(outputs);
                predictions.Add(bestMatches[0]);
            }
            
            return predictions;
        }
        
        /// <summary>
        /// Trains the network
        /// </summary>
        /// <param name="input">Training set</param>
        /// <param name="vigilanceParam">Vigilance parameter</param>
        /// <param name="b">B matrix, result of training</param>
        /// <param name="t">T matrix, result of training</param>
        public static void Train(List<List<int>> input, double vigilanceParam, out List<List<double>> b, out List<List<double>> t)
        {
            int attributes = input[0].Count;

            InitializeNetwork(attributes, out b, out t);

            List<List<double>> oldB;
            List<List<double>> oldT;

            do
            {
                oldB = DeepCopyMatrix(b);
                oldT = DeepCopyMatrix(t);

                TrainingEpoch(input, vigilanceParam, b, t);


            } while (!AreMatricesEqual(b, oldB) && !AreMatricesEqual(t, oldT));
        }
        
        /// <summary>
        /// One epoch of training, updates matrices
        /// </summary>
        private static void TrainingEpoch(List<List<int>> input, double vigilanceParam, List<List<double>> b, List<List<double>> t)
        {
            foreach(var x in input)
            {
                List<double> outputs = CalculateOutputs(x, b);
                List<int> bestMatches = IndexesOfDescSortedVector(outputs);

                double xSum = SumVector(x);

                bool matched = false;
                foreach(var matchIndex in bestMatches)
                {
                    double sSum = MultiplyVectors(x, t[matchIndex]);
                    
                    if(sSum / xSum >= vigilanceParam) // We match this input with given output
                    {
                        matched = true;
                        for(int i = 0; i < x.Count; i++)
                        {
                            b[matchIndex][i] = t[matchIndex][i] * x[i] / (0.5 + sSum);
                            t[matchIndex][i] *= x[i];
                        }
                        break;
                    }
                }
                
                // Add new output vector
                if(!matched)
                {
                    CreateNewOutput(x, out List<double> newBVector, out List<double> newTVector);
                    b.Add(newBVector);
                    t.Add(newTVector);
                }
            }
        }
        
        /// <summary>
        /// Generates vectors representing new output neuron that matches given input vector
        /// </summary>
        private static void CreateNewOutput(List<int> x, out List<double> bVector, out List<double> tVector)
        {
            tVector = new();
            foreach (var n in x)
                tVector.Add(n);

            double sSum = MultiplyVectors(x, tVector);

            bVector = new();
            for (int i = 0; i < x.Count; i++)
                bVector.Add(tVector[i] * x[i] / (0.5 + sSum));
        }
        
        /// <summary>
        /// For given vector v it returns a list of indexes, where 1st index is highest value in v, 2nd is second higest, ..., last index is the smallest value in v
        /// </summary>
        private static List<int> IndexesOfDescSortedVector(List<double> v)
        {
            List<(double value, int index)> valuesWithIndexes = new();
            
            for(int i = 0; i < v.Count; i++)
            {
                valuesWithIndexes.Add((v[i], i));
            }
            
            return valuesWithIndexes
                .OrderByDescending(item => item.value)
                .Select(item => item.index)
                .ToList();
        }
        
        /// <summary>
        /// Calculate outputs for given input vector and B matrix
        /// </summary>
        private static List<double> CalculateOutputs(List<int> x, List<List<double>> b)
        {
            List<double> result = new();
            foreach(var outputNeuron in b)
            {
                result.Add(MultiplyVectors(x, outputNeuron));
            }
            return result;
        }
        
        /// <summary>
        /// Multiplies int vector with double vector, returns 0 if sized don't match
        /// </summary>
        private static double MultiplyVectors(List<int> x, List<double> y)
        {
            if (x.Count != y.Count)
                return 0;
            double res = 0;
            for(int i = 0; i < x.Count; i++)
            {
                res += x[i] * y[i];
            }
            return res;
        }
        
        /// <summary>
        /// Returns sum of all values in the vector (like L1 norm but without absolute value)
        /// </summary>
        private static double SumVector(List<int> x)
        {
            double sum = 0;
            foreach (var n in x)
                sum += n;
            return sum;
        }
        
        /// <summary>
        /// Initializes matrices B and T at the beginning of the learning
        /// </summary>
        private static void InitializeNetwork(int attributes, out List<List<double>> b, out List<List<double>> t)
        {
            b = new List<List<double>>()
            {
                new List<double>()
            };

            t = new List<List<double>>()
            {
                new List<double>()
            };
            
            for(int i = 0; i < attributes; i++)
            {
                b[0].Add(1.0 / (attributes + 1));
                t[0].Add(1.0);
            }
            
        }
        
        /// <summary>
        /// Creates deep copy of matrix in a form of list of lists
        /// </summary>
        private static List<List<T>> DeepCopyMatrix<T>(List<List<T>> matrix)
        {
            List<List<T>> copiedMatrix = new();
            foreach(var row in matrix)
            {
                List<T> copiedRow = new();
                foreach(var n in row)
                {
                    copiedRow.Add(n);
                }
                copiedMatrix.Add(copiedRow);
            }
            return copiedMatrix;
        }
        
        /// <summary>
        /// Compares 2 matrices in form of a list of lists to check their equality
        /// </summary>
        private static bool AreMatricesEqual<T>(List<List<T>> A, List<List<T>> B)
        {
            if (A.Count != B.Count)
                return false;
            
            for(int i = 0; i < A.Count; i++)
            {
                var rowA = A[i];
                var rowB = B[i];

                if (rowA.Count != rowB.Count)
                    return false;
                
                for(int j = 0; j < rowA.Count; j++)
                {
                    if (!(rowA[j]?.Equals(rowB[j]) ?? false))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
