using System;
using System.Windows.Forms;

namespace AEIOU
{
    public partial class Form1 : Form, IGridShortcutHandler
    {
        public void OnBackSpace()
        {
            if (isFirstEdit || setting.IsAlwaysAppend)
            {
                // Delete selection
                deleteRect_with_backspace(isCellEdit);
            }
            else
            {
                // 1 char delete logic
                for (int row = selectRange.Top; row <= selectRange.Bottom; row++)
                {
                    for (int col = selectRange.Left; col <= selectRange.Right; col++)
                    {
                        string str = GetCellValue(col, row);
                        if (!string.IsNullOrEmpty(str) && str.Length > 0 && str != setting.KaraCell)
                        {
                            str = str.Substring(0, str.Length - 1);
                            gridCellValueService.ApplyValue(new Rect(col, row, 1, 1), str);
                            isCellEdit = true;
                        }
                    }
                }
            }
        }

        public void OnEnter() { gridSelectionService.MoveDown(selectRange); }
        public void OnPageUp(int keyValue) { gridScrollService.ScrollVertical(-dataGridView1.DisplayedRowCount(true)); }
        public void OnPageDown(int keyValue) { gridScrollService.ScrollVertical(dataGridView1.DisplayedRowCount(true)); }
        public void OnHome() { selectRange = new Rect(0, selectRange.Y, selectRange.Width, selectRange.Height); dataGridView1.ClearSelection(); selectRange = gridSelectionService.MoveSelection(selectRange, 0, selectRange.Y); }
        public void OnLeftArrow(int keyValue) { gridSelectionService.MoveLeft(selectRange); }
        public void OnUpArrow(int keyValue) { gridSelectionService.MoveUp(selectRange); }
        public void OnRightArrow(int keyValue) { gridSelectionService.MoveRight(selectRange); }
        public void OnDownArrow(int keyValue) { gridSelectionService.MoveDown(selectRange); }
        
        public void OnInsert() 
        { 
            gridViewManager.BeginGroup("セル挿入");
            for (int i=selectRange.Left; i<=selectRange.Right; i++) {
                gridCellValueService.InsertEmptyCell(new Rect(i, selectRange.Y, 1, 1), "");
            }
            gridViewManager.EndGroup();
            selectRange = gridSelectionService.MoveDown(selectRange);
        }

        public void OnDelete(int keyValue) 
        { 
            gridViewManager.BeginGroup("セル削除");
            for (int i=selectRange.Left; i<=selectRange.Right; i++) {
                gridCellValueService.ApplyValue(new Rect(i, selectRange.Y, 1, 1), "");
            }
            gridViewManager.EndGroup();
            selectRange = gridSelectionService.MoveUp(selectRange);
        }

        public void OnNumberKey(int keyValue, int keyCode)
        {
            gridCellValueService.InsertNumber(selectRange, keyCode, isFirstEdit, setting.IsAlwaysAppend);
            isCellEdit = true;
            this.dataGridView1.ClearSelection();
            selectRange = gridSelectionService.MoveDown(selectRange);
        }

        public void OnJOrKKey(int keyCode)
        {
            int value = selectRange.Top;
            int col, row;
            if (keyCode == 74) // J
            {
                for (row = selectRange.Top - 1; row >= 0 && value == selectRange.Top; row--)
                {
                    for (col = selectRange.Left; col <= selectRange.Right; col++)
                    {
                        if (GetCellValue(col, row) == "") continue;
                        value = row;
                        break;
                    }
                }
            }
            if (keyCode == 75) // K
            {
                for (row = selectRange.Bottom + 1; row < setting.RowLength && value == selectRange.Top; row++)
                {
                    for (col = selectRange.Left; col <= selectRange.Right; col++)
                    {
                        if (GetCellValue(col, row) == "") continue;
                        value = row;
                        break;
                    }
                }
            }
            if (value != selectRange.Top)
            {
                this.dataGridView1.ClearSelection();
                selectRange = gridSelectionService.MoveSelection(selectRange, selectRange.X, value);
            }
        }

        public void OnMultiplyKey() { /* Range Multiply handling */ }
        public void OnAddKey() { gridCellValueService.IncrementValue(selectRange, setting.KaraCell); }
        public void OnSubtractKey() { gridCellValueService.DecrementValue(selectRange, setting.KaraCell); }
        public void OnDivideKey() { /* Range Divide handling */ }
        
        public void OnDecimalKey() 
        {
            gridCellValueService.InsertEmptyCell(selectRange, setting.KaraCell);
            this.dataGridView1.ClearSelection();
            if (!setting.IsKaraNoMove) {
                selectRange = gridSelectionService.MoveDown(selectRange);
            }
        }
    }
}
