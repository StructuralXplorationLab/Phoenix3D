using Phoenix3D.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix3D.Model.CrossSections
{
    public class UPESection : ICrossSection
    {
        public string TypeName { get; private set; } = "UPE";
        public string Name { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double WebThickness { get; private set; }
        public double FlangeThickness { get; private set; }
        public double CornerRadius { get; private set; }
        public double Area { get; private set; }
        public double Surface { get; private set; }
        public double cy { get; private set; }
        public double cz { get; private set; }
        public double Iy { get; private set; }
        public double Iz { get; private set; }
        public double iy { get; private set; }
        public double iz { get; private set; }
        public double Wy { get; private set; }
        public double Wz { get; private set; }
        public double Wypl { get; private set; }
        public double Wzpl { get; private set; }
        public double Avy { get; private set; }
        public double Avz { get; private set; }

        public double It { get; private set; }
        public double Wt { get; private set; }
        public List<int> PossibleCompounds { get; private set; } = new List<int>() { 1 };
        public List<(double, double)> Polygon { get; private set; }
        public double dy { get; private set; }
        public double dz { get; private set; }

        public UPESection(int Size = 80)
        {
            this.Name = "UPE " + Size.ToString();

            SetSectionProperties();
            SetPolygon();
        }

        public void SetSectionProperties()
        {
            bool found = false;
            string[] csvstring = Properties.Resources.UPESections.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < csvstring.Length; i++)
            {
                string L = csvstring[i];
                string[] parts = L.Split(';');

                if (parts[0] == this.Name)
                {
                    Height = double.Parse(parts[1], new System.Globalization.CultureInfo("en-US"));
                    Width = double.Parse(parts[2], new System.Globalization.CultureInfo("en-US"));
                    WebThickness = double.Parse(parts[3], new System.Globalization.CultureInfo("en-US"));
                    FlangeThickness = double.Parse(parts[4], new System.Globalization.CultureInfo("en-US"));
                    CornerRadius = double.Parse(parts[5], new System.Globalization.CultureInfo("en-US"));
                    Area = double.Parse(parts[6], new System.Globalization.CultureInfo("en-US"));
                    Surface = double.Parse(parts[7], new System.Globalization.CultureInfo("en-US"));
                    Iy = double.Parse(parts[8], new System.Globalization.CultureInfo("en-US"));
                    Wy = double.Parse(parts[9], new System.Globalization.CultureInfo("en-US"));
                    Wypl = double.Parse(parts[10], new System.Globalization.CultureInfo("en-US"));
                    iy = double.Parse(parts[11], new System.Globalization.CultureInfo("en-US"));
                    Avz = double.Parse(parts[12], new System.Globalization.CultureInfo("en-US"));
                    Iz = double.Parse(parts[13], new System.Globalization.CultureInfo("en-US"));
                    Wz = double.Parse(parts[14], new System.Globalization.CultureInfo("en-US"));
                    Wzpl = double.Parse(parts[15], new System.Globalization.CultureInfo("en-US"));
                    iz = double.Parse(parts[16], new System.Globalization.CultureInfo("en-US"));
                    It = double.Parse(parts[18], new System.Globalization.CultureInfo("en-US"));
                    Wt = double.Parse(parts[19], new System.Globalization.CultureInfo("en-US"));
                    Avy = Width * FlangeThickness * 2;

                    cy = Width / 2;
                    cz = Height / 2;
                    found = true;
                    break;
                }

            }
            if (!found)
            {
                throw new System.Exception("No UPE-Section with the given dimension could be found in the standard table");
            }
        }
        public void SetPolygon()
        {
            Polygon = new List<(double, double)>
            {
                (0 - FlangeThickness, Height - cz),
                (0 - FlangeThickness, cz - Height),
                (Width - WebThickness, cz - Height),
                (Width - WebThickness, FlangeThickness - cz),
                (0, FlangeThickness - cz),
                (0, cz - FlangeThickness),
                (Width - WebThickness, cz - FlangeThickness),
                (Width - WebThickness, Height - cz)
            };
            dy = Polygon.Select(i => i.Item1).Max() - Polygon.Select(i => i.Item1).Min();
            dz = Polygon.Select(i => i.Item2).Max() - Polygon.Select(i => i.Item2).Min();
        }
        public void SetPossibleCompounds(List<int> Compounds)
        {
            this.PossibleCompounds = Compounds;
        }
        public override string ToString() { return Name; }
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
                        double alphay = 0.21;
                        double alphaz = 0.34;
                        double ry = Math.Sqrt(Iy / Area);
                        double rz = Math.Sqrt(Iz / Area);
                        double lambda = Math.PI * Math.Sqrt(Material.E / Material.fc);
                        double lambda_bar_y = BucklingLength / ry / lambda;
                        double lambda_bar_z = BucklingLength / rz / lambda;
                        double psi_y = 0.5 * (1 + alphay * (lambda_bar_y - 0.2) + lambda_bar_y * lambda_bar_y);
                        double psi_z = 0.5 * (1 + alphaz * (lambda_bar_z - 0.2) + lambda_bar_z * lambda_bar_z);
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
