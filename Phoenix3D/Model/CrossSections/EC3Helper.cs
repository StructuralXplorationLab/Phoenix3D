using Phoenix3D.Model.Materials;

using System;
using System.Collections.Generic;


namespace Phoenix3D.Model.CrossSections
{
    public static class EC3Helper
    {
        public static List<double> QK3_Util_NoStability(ICrossSection CS, Steel Material, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            if(My == 0 || Mz == 0)
            {
                return QK3_uniaxial(CS, Material, Nx, Vy, Vz, My, Mz, Mt);
            }
            else
            {
                return QK3_biaxial(CS, Material, Nx, Vy, Vz, My, Mz, Mt);
            }
        }


        public static List<double> QK3_uniaxial(ICrossSection CS, Steel Material, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            List<double> util = new List<double>();

            double rho;

            if (Mz == 0)
            {
                double VPlz = CS.Avz * Material.ft / Math.Sqrt(3) / Material.gamma_0;
                if (Vz <= VPlz)
                {
                    rho = 0;
                }
                else
                {
                    rho = Math.Pow(2 * Vy / VPlz - 1, 2);
                }

                util.Add(Nx / CS.Area + My / CS.Wy);
                util.Add(Nx / CS.Area - My / CS.Wy);

                for (int i = 0; i < util.Count; i++)
                {
                    if (util[i] >= 0)
                    {
                        double fy_red = (1 - rho) * Material.ft;
                        util[i] /= fy_red / Material.gamma_0;
                    }
                    else
                    {
                        double fy_red = (1 - rho) * Material.fc;
                        util[i] /= -fy_red / Material.gamma_0;
                    }
                }
                return util;
            }

            else if (My == 0)
            {
                double VPly = CS.Avy * Material.ft / Math.Sqrt(3) / Material.gamma_0;
                if (Vy <= VPly)
                {
                    rho = 0;
                }
                else
                {
                    rho = Math.Pow(2 * Vy / VPly - 1, 2);
                }

                util.Add(Nx / (CS.Area - CS.Avy * rho) + Mz / (CS.Wz*(1-rho)));
                util.Add(Nx / (CS.Area - CS.Avy * rho) - Mz / (CS.Wz*(1-rho)));

                for (int i = 0; i < util.Count; i++)
                {
                    if (util[i] >= 0)
                    {
                        double fy_red = (1 - rho) * Material.ft;
                        util[i] /= fy_red / Material.gamma_0;
                    }
                    else
                    {
                        double fy_red = (1 - rho) * Material.fc;
                        util[i] /= -fy_red / Material.gamma_0;
                    }
                }
                return util;
            }
            else
            {
                return QK3_biaxial(CS, Material, Nx, Vy, Vz, My, Mz, Mt);
            }
        }

        public static List<double> QK3_biaxial(ICrossSection CS, Steel Material, double Nx, double Vy, double Vz, double My, double Mz, double Mt)
        {
            List<double> util = new List<double>();
            double sigma_N = Nx / CS.Area;
            double sigma_y = My / CS.Wy;
            double sigma_z = Mz / CS.Wz;

            if (Vy <= 0.5 * CS.Avy * Material.ft / Math.Sqrt(3) / Material.gamma_0 && Vz <= CS.Avz * Material.ft / Math.Sqrt(3) / Material.gamma_0)
            {
                util.Add(sigma_N + sigma_y + sigma_z);
                util.Add(sigma_N + sigma_y - sigma_z);
                util.Add(sigma_N - sigma_y + sigma_z);
                util.Add(sigma_N - sigma_y - sigma_z);

                for (int i = 0; i < 4; i++)
                {
                    if (util[i] >= 0)
                    {
                        util[i] /= Material.ft / Material.gamma_0;
                    }
                    else
                    {
                        util[i] /= -Material.fc / Material.gamma_0;
                    }
                }
                return util;
            }
            else
            {
                util.Add(sigma_N + sigma_y + sigma_z);
                util.Add(sigma_N + sigma_y - sigma_z);
                util.Add(sigma_N - sigma_y + sigma_z);
                util.Add(sigma_N - sigma_y - sigma_z);

                for (int i = 0; i < 4; i++)
                {
                    if (util[i] >= 0)
                    {
                        util[i] /= Material.ft / Material.gamma_0;
                    }
                    else
                    {
                        util[i] /= -Material.fc / Material.gamma_0;
                    }
                    util[i] = util[i] * util[i] + 3 * (Vy / CS.Avy / (Material.ft / Material.gamma_0)) * (Vy / CS.Avy / (Material.ft/Material.gamma_0)) + 3 * (Vz / CS.Avz / (Material.ft / Material.gamma_0)) * (Vy / CS.Avz / (Material.ft / Material.gamma_0));
                }
                return util;
            }

            
        }
    }
}
