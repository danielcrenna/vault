using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Qualm.Serialization;

namespace Qualm
{
    [DebuggerDisplay("{ToJson()}")]
    public struct Matrix : IEnumerable<Number[]>, IJson
    {
        private Number[,] _field;

        public int Rows { get; private set; }
        
        public int Columns { get; private set; }

        /// <summary>
        /// Gets the last column value of each row in the matrix.
        /// This is used in calculations that expect the final column
        /// to contain the feature space's outcomes or target variables.
        /// </summary>
        /// <returns></returns>
        public Number[] GetOutcomes()
        {
            var outcomes = new List<Number>(Rows);
            outcomes.AddRange(this.Select(row => row.Last()));
            return outcomes.ToArray();
        }

        /// <summary>
        /// Gets the most frequent outcome from this matrix's outcomes.
        /// Uses the <see cref="GetOutcomes" /> method to find outcomes.
        /// </summary>
        /// <returns></returns>
        public Number GetMajorityOutcome()
        {
            var outcomes = GetOutcomes();

            var votes = new Dictionary<Number, int>(outcomes.Length);

            foreach (var outcome in outcomes)
            {
                if (votes.ContainsKey(outcome))
                {
                    votes[outcome]++;
                }
                else
                {
                    votes.Add(outcome, 1);
                }
            }

            var nearest = votes.OrderByDescending(v => v.Value).First().Key;

            return nearest;
        }

        public Number[] this[int row]
        {
            get
            {
                CheckRowArgument(row);
                var columns = new List<Number>(Columns);
                for (var i = 0; i < Columns; i++ )
                {
                    columns.Add(_field[row, i]);
                }
                return columns.ToArray();
            }
        }

        public Number this[int row, int column]
        {
            get
            {
                CheckIndexArguments(row, column);
                return _field[row, column];
            }
            set
            {
                CheckIndexArguments(row, column);
                _field[row, column] = value;
            }
        }

        private void CheckIndexArguments(int row, int column)
        {
            CheckRowArgument(row);

            if (column < 0 || column > column - 1)
            {
                throw new ArgumentOutOfRangeException("column");
            }
        }

        private void CheckRowArgument(int row)
        {
            if (row < 0 || row > Rows - 1)
            {
                throw new ArgumentOutOfRangeException("row");
            }
        }

        public Matrix(IEnumerable<Number[]> values): this()
        {
            Rows = values.Count();
            Columns = values.First().Count();
            _field = new Number[Rows, Columns];

            var array = values.ToArray();

            for (var i = 0; i < Rows; i++)
            {
                var row = array[i];

                for (var j = 0; j < Columns; j++)
                {
                    _field[i, j] = row[j];
                }
            }
        }

        public Matrix(IEnumerable<double[]> values): this()
        {
            Rows = values.Count();
            Columns = values.First().Count();
            _field = new Number[Rows, Columns];

            var array = values.ToArray();

            for (var i = 0; i < Rows; i++)
            {
                var row = array[i];

                for (var j = 0; j < Columns; j++)
                {
                    _field[i, j] = row[j];
                }
            }
        }
        
        public Matrix(Number[,] values) : this()
        {
            Rows = values.GetLength(0);
            Columns = values.GetLength(1);

            _field = new Number[Rows, Columns];

            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Columns; j++)
                {
                    _field[i, j] = values[i, j];
                }
            }
        }

        public void AddRow(IEnumerable<Number> row)
        {
            var array = row.ToArray();

            if(Columns != 0 && array.Length != Columns)
            {
                throw new ArgumentOutOfRangeException("row", "Row must have the same number of columns as the target matrix");
            }

            Columns = array.Length;

            Rows += 1;

            var copy = _field;

            _field = new Number[Rows, Columns];

            for (var i = 0; i < copy.GetLength(0); i++)
            {
                for (var j = 0; j < copy.GetLength(1); j++)
                {
                    _field[i, j] = copy[i, j];
                }
            }

            for (var i = 0; i < array.Length; i++)
            {
                _field[Rows - 1, i] = array[i];
            }
        }

        public Matrix(int rows, int columns) : this()
        {
            Rows = rows;
            Columns = columns;
            _field = new Number[Rows, Columns];
        }
        
        public static Matrix operator +(Matrix a, Matrix b)
        {
            return Add(a, b);
        }

        public static Matrix Add(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
            {
                throw new ArgumentException("Matrix dimensions mismatch");
            }

            var result = new Matrix(a.Rows, a.Columns);

            for (var i = 0; i < result.Rows; i++)
            {
                for (var j = 0; j < result.Columns; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }

            return result;
        }

        public IEnumerator<Number[]> GetEnumerator()
        {
            for(var i = 0; i < Rows; i++)
            {
                var row = this[i];
                yield return row;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("[");

            var count = 0;
            foreach(var row in this)
            {
                sb.Append("[");
                for (var i = 0; i < row.Length; i++)
                {
                    var number = ((double)row[i]).ToString("0.00");
                    sb.Append(number);
                    if(i < row.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append("]");

                count++;
                if(count < Rows)
                {
                    sb.Append(", ");
                }
            }

            sb.Append("]");
            return sb.ToString();
        }
    }
}
