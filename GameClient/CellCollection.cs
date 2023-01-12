using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace GameClient
{
    public class CellCollection : BindableBase
    {
        private IList<Cell> cells;
        public IList<Cell> Cells
        {
            get => cells;
            set
            {
                if (cells == value)
                    return;
                SetProperty(ref cells, value);
            }
        }

        public CellCollection() => Cells = new List<Cell>(Enumerable
            .Range(0, 25)
            .Select(i => new Cell()));
    }
}
