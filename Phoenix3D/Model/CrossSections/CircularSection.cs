using Phoenix3D.Model.Materials;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Phoenix3D.Model.CrossSections
{
    
    public class CircularSection : ICrossSection
    {
        public string TypeName { get; private set; } = "CS";
        public string Name { get; private set; }

        public double Diameter;
        public double Area { get; private set; }
        public double Iy { get; private set; }
        public double Iz { get; private set; }
        public double iy { get; private set; }
        public double iz { get; private set; }
        public double It { get; set; }
        public double Wy { get; private set; }
        public double Wz { get; private set; }
        public double Wt { get; private set; }
        public double Avy { get; private set; }
        public double Avz { get; private set; }
        public double dy { get; private set; }
        public double dz { get; private set; }
        public double cy { get; private set; }
        public double cz { get; private set; }

        public List<int> PossibleCompounds { get; private set; }
        public List<(double, double)> Polygon { get; private set; }

        public CircularSection(double D = 0.05)
        {
            Name = "CS" + Math.Round(1000 * D, 2).ToString();
            Diameter = D;
            this.Area = 0;
            this.Iy = 0;
            this.Iz = 0;
            this.It = 0;
            this.Wy = 0;
            this.Wz = 0;
            this.Wt = 0;
            this.dy = 0;
            this.dz = 0;
            this.cy = 0;
            this.cz = 0;
            this.Avy = 0;
            this.Avz = 0;
            this.iy = 0;
            this.iz = 0;
            PossibleCompounds = new List<int>() { 1 };
            Polygon = new List<(double, double)>();
            SetSectionProperties();
            SetPolygon();
        }

        public void SetSectionProperties()
        {
            Area = Math.PI * Diameter * Diameter / 4;
            Iy = Iz = Math.PI * Diameter * Diameter * Diameter * Diameter / 4;
            iy = iz = Math.Sqrt(Iy / Area);
            It = Math.PI * Diameter * Diameter * Diameter * Diameter / 2;
            Wy = Wz = Math.PI * Diameter * Diameter * Diameter / 4;
            Wt = Math.PI * Diameter * Diameter * Diameter / 2;
            Avy = Avz = Area;
        }

        public void SetPolygon()
        {
            int n = 16;
            double r = Diameter / 2;
            for (int i = 0; i < n; i++)
            {
                Polygon.Add((r * Math.Cos(i * Math.PI * 2 / (n + 0)), r * Math.Sin(i * Math.PI * 2 / (n + 0))));
            }
            dy = Polygon.Select(i => i.Item1).Max() - Polygon.Select(i => i.Item1).Min();
            dz = Polygon.Select(i => i.Item2).Max() - Polygon.Select(i => i.Item2).Min();
            cy = Diameter / 2;
            cz = Diameter / 2;
        }

        public void SetPossibleCompounds(List<int> Compounds)
        {
            this.PossibleCompounds = Compounds;
        }
        public override string ToString()
        {
            return Name;
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
            switch (Material.Type)
            {
                case MaterialType.Empty: return new List<double>() { 0 };
                
                case MaterialType.Metal:
                    {
                        switch (BucklingType)
                        {
                            case BucklingType.Off:
                                {
                                    return new List<double>() { -Area * Material.fc / Material.gamma_0 };
                                }
                            case BucklingType.Euler:
                                {
                                    double Euler_y = -Math.PI * Math.PI * Material.E * Iy / BucklingLength / BucklingLength / Material.gamma_1;
                                    double Euler_z = -Math.PI * Math.PI * Material.E * Iz / BucklingLength / BucklingLength / Material.gamma_1;

                                    return new List<double>() { Euler_y, Euler_z };
                                }
                            case BucklingType.Eurocode:
                                {
                                    double alpha = 0.21;
                                    double ry = Math.Sqrt(Iy / Area);
                                    double rz = Math.Sqrt(Iz / Area);
                                    double lambda = Math.PI * Math.Sqrt(Material.E / Material.fc);
                                    double lambda_bar_y = BucklingLength / ry / lambda;
                                    double lambda_bar_z = BucklingLength / rz / lambda;
                                    double psi_y = 0.5 * (1 + alpha * (lambda_bar_y - 0.2) + lambda_bar_y * lambda_bar_y);
                                    double psi_z = 0.5 * (1 + alpha * (lambda_bar_z - 0.2) + lambda_bar_z * lambda_bar_z);
                                    double xi_y = Math.Min(1, 1 / (psi_y + Math.Sqrt(psi_y * psi_y - lambda_bar_y * lambda_bar_y)));
                                    double xi_z = Math.Min(1, 1 / (psi_z + Math.Sqrt(psi_z * psi_z - lambda_bar_z * lambda_bar_z)));
                                    double N_buck_y = -xi_y * Area * Material.fc / Material.gamma_1;
                                    double N_buck_z = -xi_z * Area * Material.fc / Material.gamma_1;
                                    return new List<double>() { N_buck_y, N_buck_z };
                                }
                            default:
                                {
                                    return new List<double>() { -Area * Material.fc / Material.gamma_0 };
                                }
                        }
                    }
                case MaterialType.Timber:
                    {
                        switch (BucklingType)
                        {
                            case BucklingType.Off:
                                {
                                    return new List<double>() { -Area * Material.fc / Material.gamma_0 };
                                }
                            case BucklingType.Euler:
                                {
                                    double Euler_y = -Math.PI * Math.PI * Material.E * Iy / BucklingLength / BucklingLength / Material.gamma_1;
                                    double Euler_z = -Math.PI * Math.PI * Material.E * Iz / BucklingLength / BucklingLength / Material.gamma_1;

                                    return new List<double>() { -Area * Material.fc / Material.gamma_0, Euler_y, Euler_z };
                                }
                            case BucklingType.Eurocode:
                                {
                                    // SOLID TIMBER!
                                    double beta_c = 0.2;
                                    double E_005 = Material.E * 2 / 3;

                                    double lambda_y = BucklingLength / iy;
                                    double lambda_z = BucklingLength / iz;

                                    double lambda_rel_y = lambda_y / Math.PI * Math.Sqrt(Material.fc / E_005);
                                    double lambda_rel_z = lambda_z / Math.PI * Math.Sqrt(Material.fc / E_005);

                                    double ky = 0.5 * (1 + beta_c * (lambda_rel_y - 0.3) + lambda_rel_y * lambda_rel_y);
                                    double kz = 0.5 * (1 + beta_c * (lambda_rel_z - 0.3) + lambda_rel_z * lambda_rel_z);

                                    double kcy = 1 / (ky + Math.Sqrt(ky * ky - lambda_rel_y * lambda_rel_y));
                                    double kcz = 1 / (kz + Math.Sqrt(kz * kz - lambda_rel_z * lambda_rel_z));

                                    double N_buck_y = -kcy * Material.kmod * Area * Material.fc / Material.gamma_1;
                                    double N_buck_z = -kcz * Material.kmod * Area * Material.fc / Material.gamma_1;

                                    return new List<double>() { N_buck_y, N_buck_z };
                                }
                            default:
                                {
                                    return new List<double>() { -Area * Material.fc / Material.gamma_0 };
                                }
                        }
                    }
                default:
                    {
                        return new List<double>() { -Area * Material.fc / Material.gamma_0 };
                    }
            }
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
