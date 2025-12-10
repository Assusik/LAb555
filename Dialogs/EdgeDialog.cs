using System;
using System.Windows.Forms;

namespace GraphEditorApp.Dialogs
{
    public class EdgeDialog : Form
    {
        public int EdgeWeight { get; private set; } = 1;
        public int EdgeCapacity { get; private set; } = 0;
        public bool IsDirected { get; private set; } = false;

        private NumericUpDown nudWeight;
        private NumericUpDown nudCapacity;
        private CheckBox cbDirected;
        private Button btnOk;
        private Button btnCancel;

        public EdgeDialog()
        {
            Text = "Параметры ребра";
            Width = 300;
            Height = 200;
            StartPosition = FormStartPosition.CenterParent;

            Label l1 = new Label { Left = 10, Top = 10, Text = "Вес (weight):" };
            nudWeight = new NumericUpDown { Left = 120, Top = 8, Width = 120, Minimum = 1, Maximum = 1000, Value = 1 };

            Label l2 = new Label { Left = 10, Top = 40, Text = "Пропускная (capacity):" };
            nudCapacity = new NumericUpDown { Left = 120, Top = 38, Width = 120, Minimum = 0, Maximum = 10000, Value = 0 };

            cbDirected = new CheckBox { Left = 10, Top = 70, Width = 200, Text = "Направленное ребро (directed)" };

            btnOk = new Button { Left = 40, Top = 110, Width = 80, Text = "OK", DialogResult = DialogResult.OK };
            btnCancel = new Button { Left = 140, Top = 110, Width = 80, Text = "Отмена", DialogResult = DialogResult.Cancel };

            Controls.AddRange(new Control[] { l1, nudWeight, l2, nudCapacity, cbDirected, btnOk, btnCancel });

            btnOk.Click += (s, e) =>
            {
                EdgeWeight = (int)nudWeight.Value;
                EdgeCapacity = (int)nudCapacity.Value;
                IsDirected = cbDirected.Checked;
                DialogResult = DialogResult.OK;
                Close();
            };
        }
    }
}
