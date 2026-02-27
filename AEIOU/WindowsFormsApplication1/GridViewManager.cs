using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AEIOU
{
    public class GridViewManager
    {
        private UndoManager _undoManager;
        private DataGridView _view;
        private string[,] _copyBuffer;              // DataGridView向けのコピーバッファを保持
        private Rect _copyRect;                     // コピー範囲を保持
        private Stack<OperationGroup> _groupStack;  // OperationGroupの入れ子対応
        private TimingSheetModel _model;

        public DataGridView View
        {
            get { return _view; }
            set { _view = value; }
        }

        public string[,] CopyBuffer
        {
            get { return _copyBuffer; }
            set { _copyBuffer = value; }
        }
        public Rect CopyRect
        {
            get { return _copyRect; }
            set { _copyRect = value; }
        }

        public TimingSheetModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public int ColumnCount
        {
            get
            {
                if (_model != null)
                {
                    return _model.ColumnCount;
                }

                return (_view != null) ? _view.ColumnCount : 0;
            }
        }

        public int RowCount
        {
            get
            {
                if (_model != null)
                {
                    return _model.RowCount;
                }

                return (_view != null) ? _view.RowCount : 0;
            }
        }

        public void InitializeWork(DataGridView view, TimingSheetModel model)
        {
            _view = view;
            _model = model;
            _copyBuffer = null;
            _undoManager = new UndoManager();
            _groupStack = new Stack<OperationGroup>();
        }

        public OperationGroup BeginGroup(String name)
        {
            var newGroup = new OperationGroup(name);
            _groupStack.Push(newGroup);
            return newGroup;
        }

        public void EndGroup()
        {
            if (_groupStack.Count > 0)
            {
                var completedGroup = _groupStack.Pop();

                if (_groupStack.Count == 0 && completedGroup.OperationCount > 0)
                {
                    _undoManager.PushOperation(completedGroup);
                }
                else if (_groupStack.Count > 0)
                {
                    _groupStack.Peek().AddOperation(completedGroup);
                }
            }
        }
        
        public void ExecuteOperation(GridViewOperation operation)
        {
            if (_groupStack.Count > 0)
            {
                _groupStack.Peek().AddOperation(operation);
            }
            else
            {
                _undoManager.PushOperation(operation);
            }
            operation.Execute(this);
        }

        public void Undo()
        {
            _undoManager.Undo(this);
        }

        public void Redo()
        {
            _undoManager.Redo(this);
        }

        public string GetCellValue(int col, int row)
        {
            return _model.GetCell(col, row);
        }

        public bool TryGetCellValue(int col, int row, out string value)
        {
            return _model.TryGetCell(col, row, out value);
        }

        public void SetCellValue(int col, int row, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            _model.SetCell(col, row, normalizedValue);
            SetCellDisplayValue(col, row, normalizedValue);
        }

        public string SetCellValueWithUndo(int col, int row, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            string oldValue = _model.SetCellWithUndo(col, row, normalizedValue);
            SetCellDisplayValue(col, row, normalizedValue);
            return oldValue;
        }

        public void ApplyUndoCellValue(int col, int row, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            _model.ApplyUndoCell(col, row, normalizedValue);
            SetCellDisplayValue(col, row, normalizedValue);
        }

        public void ApplyRedoCellValue(int col, int row, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            _model.ApplyRedoCell(col, row, normalizedValue);
            SetCellDisplayValue(col, row, normalizedValue);
        }

        public string GetHeaderValue(int col)
        {
            return _model.GetHeader(col);
        }

        public void SetHeaderValue(int col, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            _model.SetHeader(col, normalizedValue);
            SetHeaderDisplayValue(col, normalizedValue);
        }

        public void SetCellDisplayValue(int col, int row, string value)
        {
            if (_view == null)
            {
                return;
            }

            _view[col, row].Value = NormalizeCellValue(value);
        }

        public void SetHeaderDisplayValue(int col, string value)
        {
            if (_view == null)
            {
                return;
            }

            _view.Columns[col].HeaderText = NormalizeCellValue(value);
        }

        public string[,] GetRangeValues(Rect range)
        {
            ValidateRange(range);
            string[,] values = new string[range.Height, range.Width];
            for (int rowOffset = 0; rowOffset < range.Height; rowOffset++)
            {
                for (int colOffset = 0; colOffset < range.Width; colOffset++)
                {
                    values[rowOffset, colOffset] = GetCellValue(range.X + colOffset, range.Y + rowOffset);
                }
            }

            return values;
        }

        public void SetRangeValues(int startCol, int startRow, string[,] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            int rowCount = values.GetLength(0);
            int columnCount = values.GetLength(1);
            ValidateRange(new Rect(startCol, startRow, columnCount, rowCount));
            for (int rowOffset = 0; rowOffset < rowCount; rowOffset++)
            {
                for (int colOffset = 0; colOffset < columnCount; colOffset++)
                {
                    SetCellValue(startCol + colOffset, startRow + rowOffset, values[rowOffset, colOffset]);
                }
            }
        }

        public void ClearRangeValues(Rect range)
        {
            ValidateRange(range);
            for (int rowOffset = 0; rowOffset < range.Height; rowOffset++)
            {
                for (int colOffset = 0; colOffset < range.Width; colOffset++)
                {
                    SetCellValue(range.X + colOffset, range.Y + rowOffset, "");
                }
            }
        }


        private void ValidateRange(Rect range)
        {
            if (range.Width < 0 || range.Height < 0)
            {
                throw new ArgumentOutOfRangeException("range", "Range size must be non-negative.");
            }

            if (range.Width == 0 || range.Height == 0)
            {
                return;
            }

            ValidateCellIndex(range.X, range.Y);
            ValidateCellIndex(range.Right, range.Bottom);
        }

        private void ValidateCellIndex(int col, int row)
        {
            if (col < 0 || row < 0 || col >= ColumnCount || row >= RowCount)
            {
                throw new ArgumentOutOfRangeException("col,row", "Cell index is out of range.");
            }
        }

        private string NormalizeCellValue(string value)
        {
            return value ?? "";
        }

    }
}
