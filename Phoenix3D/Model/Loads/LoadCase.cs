using System;
using System.Collections.Generic;


namespace Phoenix3D.Model
{
    
    public class LoadCase
    {
        public int Number { get; set; } = -1;
        public string Name { get; set; } = "";
        public List<ILoad> Loads { get; private set; } = new List<ILoad>();
        public List<DisplacementBound> DisplacementBounds { get; private set; } = new List<DisplacementBound>();
        public double yg { get; set; } = 1.00;

        public LoadCase(string Name, double SelfWeightFactor = 1.00 )
        {
            if(Name == "")
            {
                throw new ArgumentException("LoadCase name cannot be empty");
            }
            else if(Name == "all")
            {
                throw new ArgumentException("LoadCase name cannot be 'all'. It is reserved to request the computation of all load cases");
            }
            else
                this.Name = Name;

            this.yg = SelfWeightFactor;
        }
        public void AddLoad(ILoad l)
        {
            Loads.Add(l);
        }
        public void AddDisplacementBound(DisplacementBound db)
        {
            if(DisplacementBounds.Contains(db))
            {
                DisplacementBounds.Remove(db);
                DisplacementBounds.Add(db);
            }
            else
            {
                DisplacementBounds.Add(db);
            }  
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if ((obj is null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                LoadCase LC = (LoadCase)obj;

                if (LC.Name == Name)
                    return true;
                else
                    return false;
            }
        }

        public override string ToString()
        {
            return Name.ToString();
        }
        public LoadCase Clone()
        {
            var new_lc = new LoadCase(Name, yg);
            new_lc.Number = Number;
            new_lc.Loads = new List<ILoad>();
            for (int i = 0; i < Loads.Count; ++i)
            {
                if (Loads[i] is PointLoad pl)
                    new_lc.Loads.Add(pl.Clone());
            }
            for (int i = 0; i < DisplacementBounds.Count; ++i)
                new_lc.DisplacementBounds.Add(DisplacementBounds[i].Clone());

            return new_lc;
        }
    }
}