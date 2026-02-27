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
            _view[col, row].Value = normalizedValue;
        }

        public string SetCellValueWithUndo(int col, int row, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            string oldValue = _model.SetCellWithUndo(col, row, normalizedValue);
            _view[col, row].Value = normalizedValue;
            return oldValue;
        }

        public void ApplyUndoCellValue(int col, int row, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            _model.ApplyUndoCell(col, row, normalizedValue);
            _view[col, row].Value = normalizedValue;
        }

        public void ApplyRedoCellValue(int col, int row, string value)
        {
            string normalizedValue = NormalizeCellValue(value);
            _model.ApplyRedoCell(col, row, normalizedValue);
            _view[col, row].Value = normalizedValue;
        }

        private string NormalizeCellValue(string value)
        {
            return value ?? "";
        }

    }
}
