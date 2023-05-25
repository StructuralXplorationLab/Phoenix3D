using Phoenix3D.Model.Materials;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Phoenix3D.Model.CrossSections
{
    
    public class GenericSection : ICrossSection
    {
        public string TypeName { get; private set; } = "GS";
        public string Name { get; private set; }
        public double Area { get; private set; }
        public double Iy { get; private set; }
        public double Iz { get; private set; }
        public double Wy { get; private set; }
        public double Wz { get; private set; }
        public double It { get; private set; }
        public double Wt { get; private set; }
        public double Avy { get; private set; }
        public double Avz { get; private set; }
        
        public double dy { get; private set; }
        public double dz { get; private set; }
        public double cy { get; private set; }
        public double cz { get; private set; }
        public List<int> PossibleCompounds { get; private set; }
        public List<(double, double)> Polygon { get; private set; }

        public GenericSection(string Name, double Area, double Iy, double Iz, double Wy, double Wz, double Wt, double It, double Avy, double Avz)
        {
            this.Name = Name;
            this.Area = Area;
            this.Iy = Iy;
            this.Iz = Iz;
            this.Wy = Wy;
            this.Wz = Wz;
            this.Wt = Wt;
            this.It = It;
            this.Avy = Avy;
            this.Avz = Avz;
            CircularSection CS = new CircularSection(Math.Sqrt(4 * Area / Math.PI));
            Polygon = CS.Polygon;
            dy = Polygon.Select(i => i.Item1).Max() - Polygon.Select(i => i.Item1).Min();
            dz = Polygon.Select(i => i.Item2).Max() - Polygon.Select(i => i.Item2).Min();
            cy = dy / 2;
            cz = dz / 2;
            PossibleCompounds = new List<int>() { 1 };
        }
        public double GetTensionResistance(IMaterial Material)
        {
            switch (Material.Type)
            {
                case MaterialType.Empty: return 0;
                case MaterialType.Metal: return Area * Material.ft / Material.gamma_0;
                case MaterialType.Timber: return Material.kmod * Area * Material.ft / Material.gamma_0;
                default: return Area * Material.ft / Material.gamma_0;
            }
        }
        public List<double> GetBucklingResistance(IMaterial Material, BucklingType BucklingType, double BucklingLength)
        {
            return new List<double>() { -Material.fc * this.Area / Material.gamma_0 };
        }
        public void SetPolygon()
        {
            throw new NotImplementedException();
        }
        public void SetPossibleCompounds(List<int> Compounds)
        {
            throw new NotImplementedException();
        }
        public void SetSectionProperties()
        {
            throw new NotImplementedException();
        }

        public double GetUtilization(IMaterial Material, BucklingType BucklingType, double BucklingLength, bool Plastic, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            if (Nx >= 0 && My == 0 && Mz == 0)
            {
                return Nx / this.GetTensionResistance(Material);
            }
            else
            {
                switch (Material)
                {
                    case Steel Steel:
                        {
                            switch (Plastic)
                            {
                                case false:
                                    {
                                        switch (BucklingType)
                                        {
                                            case BucklingType.Off:
                                                {
                                                    return UtilizationElasticNoStability(Steel, Nx, Vy, Vz, My, Mz, Mt);
                                                }
                                            case BucklingType.Euler:
                                                {
                                                    return UtilizationElasticNoStability(Steel, Nx, Vy, Vz, My, Mz, Mt);
                                                }
                                            case BucklingType.Eurocode:
                                                {
                                                    throw new NotImplementedException("Eurocode steel bending and buckling not implemented");
                                                }
                                        }
                                        break;
                                    }
                                case true:
                                    {
                                        throw new NotImplementedException("Eurocode plastic beam utilization not implemented yet");
                                    }
                            }
                        }
                        break;
                    case Timber T: throw new NotImplementedException("HEA beams in timber do not exist!");
                }
                return 1e3;
            }
        }
        private double UtilizationElasticNoStability(Steel Steel, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            if ((My == 0 && Vz == 0) || (Mz == 0 && Vy == 0))
            {
                return UtilizationElasticNoStabilityUniaxial(Steel, Nx, Vy, Vz, My, Mz, Mt);
            }
            else
            {
                return UtilizationElasticNoStabilityBiaxial(Steel, Nx, Vy, Vz, My, Mz, Mt);
            }
        }
        private double UtilizationElasticNoStabilityUniaxial(Steel Steel, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            double util;
            double rho;
            if (Mz == 0 && Vy == 0)
            {
                double VPlz = Avz * Steel.ft / Math.Sqrt(3) / Steel.gamma_0;
                if (Math.Abs(Vz) <= 0.5 * VPlz)
                {
                    rho = 0;
                }
                else
                {
                    rho = Math.Min(0.9999, Math.Pow(2 * Math.Abs(Vz) / VPlz - 1, 2));
                }

                util = Math.Max(Math.Abs(Nx / Area + My / Wy), Math.Abs(Nx / Area - My / Wy));
                util /= (1 - rho) * Steel.fc / Steel.gamma_0;

                return util;
            }

            else if (My == 0 && Vz == 0)
            {
                double VPly = Avy * Steel.ft / Math.Sqrt(3) / Steel.gamma_0;
                if (Math.Abs(Vy) <= 0.5 * VPly)
                {
                    rho = 0;
                }
                else
                {
                    rho = Math.Min(0.9999, Math.Pow(2 * Math.Abs(Vy) / VPly - 1, 2));
                }

                util = Math.Max(Math.Abs(Nx / Area + My / Wy), Math.Abs(Nx / Area - My / Wy));
                util /= (1 - rho) * Steel.fc / Steel.gamma_0;

                return util;
            }
            else
            {
                return UtilizationElasticNoStabilityBiaxial(Steel, Nx, Vy, Vz, My, Mz, Mt);
            }


        }
        private double UtilizationElasticNoStabilityBiaxial(Steel Steel, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            double[] util_4 = new double[4];
            double sigma_N = Nx / Area;
            double sigma_y = My / Wy;
            double sigma_z = Mz / Wz;

            double tau_y = Vy / Avy;
            double tau_z = Vz / Avz;

            double VyPlRd = Avy * Steel.fc / Math.Sqrt(3) / Steel.gamma_0;
            double VzPlRd = Avz * Steel.fc / Math.Sqrt(3) / Steel.gamma_0;

            if (Math.Abs(Vy) <= 0.5 * VyPlRd && Math.Abs(Vz) <= VzPlRd)
            {
                util_4[0] = sigma_N + sigma_y + sigma_z;
                util_4[1] = sigma_N + sigma_y - sigma_z;
                util_4[2] = sigma_N - sigma_y + sigma_z;
                util_4[3] = sigma_N - sigma_y - sigma_z;

                for (int i = 0; i < util_4.Length; i++)
                {
                    if (util_4[i] >= 0)
                    {
                        util_4[i] /= Steel.ft / Steel.gamma_0;
                    }
                    else
                    {
                        util_4[i] /= -Steel.fc / Steel.gamma_0;
                    }
                    util_4[i] = Math.Abs(util_4[i]);
                }
                return util_4.Max();
            }
            else
            {
                util_4[0] = sigma_N + sigma_y + sigma_z;
                util_4[1] = sigma_N + sigma_y - sigma_z;
                util_4[2] = sigma_N - sigma_y + sigma_z;
                util_4[3] = sigma_N - sigma_y - sigma_z;

                for (int i = 0; i < util_4.Length; i++)
                {
                    if (util_4[i] >= 0)
                    {
                        util_4[i] /= Steel.ft / Steel.gamma_0;
                    }
                    else
                    {
                        util_4[i] /= -Steel.fc / Steel.gamma_0;
                    }
                    util_4[i] = util_4[i] * util_4[i] + 3 * (tau_y / Steel.ft / Steel.gamma_0) * (tau_y / Steel.ft / Steel.gamma_0) + 3 * (tau_z / Steel.ft / Steel.gamma_0) * (tau_z / Steel.ft / Steel.gamma_0);

                    util_4[i] = Math.Abs(util_4[i]);
                }

                return util_4.Max();
            }
        }
    }
}
