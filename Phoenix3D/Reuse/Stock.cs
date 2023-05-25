using System;
using System.Collections.Generic;
using System.Linq;


namespace Phoenix3D.Reuse
{
    
    public class Stock
    {
        public List<ElementGroup> ElementGroups { get; private set; } = new List<ElementGroup>();
        public SortStockElementsBy SortBy { get; private set; } = SortStockElementsBy.Off;
        public List<int> SortMap { get; private set; } = new List<int>();
        public Stock() : this(new List<ElementGroup>(), SortStockElementsBy.Off) { }

        public Stock(List<ElementGroup> ElementGroups) : this(ElementGroups, SortStockElementsBy.Off){}

        public Stock(List<ElementGroup> ElementGroups, SortStockElementsBy SortBy)
        {
            this.ElementGroups = ElementGroups;
            this.SortBy = SortBy;
            SortElementGroups(SortBy);
            SetElementGroupNumber();
        }

        public void AddElementGroup(ElementGroup ElementGroup)
        {
            this.ElementGroups.Add(ElementGroup);
            SortElementGroups(SortBy);
            SetElementGroupNumber();
        }
        public void InsertElementGroup(int Index, ElementGroup ElementGroup)
        {
            ElementGroups.Insert(Index, ElementGroup);
            SortElementGroups(SortBy);
            SetElementGroupNumber();
        }
        public void RemoveElementGroup(int Index)
        {
            this.ElementGroups.RemoveAt(Index);
            SortElementGroups(SortBy);
            SetElementGroupNumber();
        }
        private void SetElementGroupNumber()
        {
            for (int i = 0; i < ElementGroups.Count; ++i)
            {
                ElementGroups[i].SetNumber(i);
            }
        }
        public void ClearElementGroup()
        {
            this.ElementGroups.Clear();
            this.SortBy = SortStockElementsBy.Off;
            this.SortMap.Clear();
        }
        public void SortElementGroups(SortStockElementsBy sort)
        {
            this.SortBy = sort;

            switch(SortBy)
            {
                case SortStockElementsBy.Off: break;
                case SortStockElementsBy.Type:
                    {
                        SortMap.Clear();
                        var orderedZip = ElementGroups.Zip(Enumerable.Range(0, ElementGroups.Count), (x, y) => new { x, y })
                                .OrderBy(pair => pair.x.Type)
                                .ToList();
                        ElementGroups = orderedZip.Select(pair => pair.x).ToList();
                        SortMap = orderedZip.Select(pair => pair.y).ToList();
                        break;
                    }
                case SortStockElementsBy.ForceThenLength:
                    {
                        SortMap.Clear();
                        var orderedZip = ElementGroups.Zip(Enumerable.Range(0, ElementGroups.Count), (x, y) => new { x, y })
                                                .OrderBy(pair => pair.x.Type)
                                                .ThenBy(pair => Math.Abs(pair.x.CrossSection.Area))
                                                .ThenBy(pair => pair.x.Length)
                                                .ToList();
                        ElementGroups = orderedZip.Select(pair => pair.x).ToList();
                        SortMap = orderedZip.Select(pair => pair.y).ToList();
                        break;
                    }
                case SortStockElementsBy.LengthThenForce:
                    {
                        SortMap.Clear();
                        var orderedZip = ElementGroups.Zip(Enumerable.Range(0, ElementGroups.Count), (x, y) => new { x, y })
                                .OrderBy(pair => pair.x.Type)
                                .ThenBy(pair => pair.x.Length)
                                .ThenBy(pair => Math.Abs(pair.x.CrossSection.Area))
                                .ToList();
                        ElementGroups = orderedZip.Select(pair => pair.x).ToList();
                        SortMap = orderedZip.Select(pair => pair.y).ToList();
                        break;
                    }
            }
        }

        internal Stock ExtendStock()
        {
            List<ElementGroup> temp = new List<ElementGroup>();
            foreach(ElementGroup EG in ElementGroups)
            {
                for (int j = 0; j < EG.NumberOfElements; j++)
                    temp.Add(new ElementGroup(EG.Type, EG.Material, EG.CrossSection, EG.Length, 1, EG.CanBeCut));
            }
            return new Stock(temp);
        }
        internal Stock ReduceStock(Stock OriginalStock)
        {
            throw new NotImplementedException();
        }

        public void ResetAssignedMembers()
        {
            foreach (ElementGroup EG in ElementGroups)
            {
                EG.ResetAssignedMembers();
            }
        }

        public void ResetRemainLenghts()
        {
            foreach (ElementGroup EG in ElementGroups)
            {
                EG.ResetRemainLengths();
            }
        }
        public void ResetRemainLenghtsTemp()
        {
            foreach(ElementGroup EG in ElementGroups)
            {
                EG.ResetRemainLengthsTemp();
            }
        }
        public void ResetAlreadyCounted()
        {
            foreach (ElementGroup EG in ElementGroups)
            {
                EG.ResetAlreadyCounted();
            }
        }
        public void ResetNext()
        {
            foreach (ElementGroup EG in ElementGroups)
            {
                EG.ResetNext();
            }
        }
        public void ResetStacks()
        {
            foreach (ElementGroup EG in ElementGroups)
            {
                EG.ResetStack();
            }
        }
        public Stock Clone()
        {
            var new_stock = new Stock();
            new_stock.ElementGroups = new List<ElementGroup>();
            foreach (var element in ElementGroups)
                new_stock.ElementGroups.Add(element.Clone());
            new_stock.SortBy = SortBy;
            new_stock.SortMap = new List<int>();
            foreach (var i in SortMap)
                new_stock.SortMap.Add(i);
            new_stock.ResetAlreadyCounted();
            new_stock.ResetAssignedMembers();
            new_stock.ResetRemainLenghts();
            new_stock.ResetRemainLenghtsTemp();
            new_stock.ResetStacks();
            new_stock.ResetNext();

            return new_stock;
        }
    }
}
